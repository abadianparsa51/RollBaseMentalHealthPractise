// Controllers/DiagnosisController.cs
using Application.Layer.Commands;
using Application.Layer.Querrys;
using Core.Layer.Data;
using Core.Layer.Services; // برای DiagnosisIntegrationService
using Core.Model.Layer.Dto;
using Core.Model.Layer.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace NeuroEase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosisController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly MentalHealthDbContext _dbContext;
        private readonly MlDataExportService _mlService;
        private readonly DiagnosisIntegrationService _pythonService;

        public DiagnosisController(IMediator mediator, MentalHealthDbContext dbContext, DiagnosisIntegrationService pythonService, MlDataExportService mlService = null)
        {
            _mediator = mediator;
            _dbContext = dbContext;
            _pythonService = pythonService;
            _mlService = mlService;
        }

        // ==================== ASP.NET Core Evaluation ====================
        [HttpPost("evaluate")]
        public async Task<IActionResult> Evaluate([FromBody] UserAnswer answer)
        {
            if (answer == null)
                return BadRequest("پاسخ نامعتبر است.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("شناسه کاربر در توکن یافت نشد.");

            answer.UserId = userId;

            // گرفتن یا ایجاد SessionId
            var sessionId = HttpContext.Session.GetString("DiagnosisSessionId") ?? Guid.NewGuid().ToString();
            answer.SessionId = sessionId;
            HttpContext.Session.SetString("DiagnosisSessionId", sessionId);

            try
            {
                var diagnoses = await _mediator.Send(new EvaluateRulesCommand
                {
                    Answers = new List<UserAnswer> { answer }
                });

                var result = diagnoses.Select(d => new DiagnosisResult
                {
                    DiagnosisType = d,
                    DetailedResult = $"تشخیص: {d}. برای اطلاعات بیشتر با پزشک مشورت کنید."
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در ارزیابی: {ex.Message}");
            }
        }

        // ==================== Create Diagnosis Record ====================
        [HttpPost]
        public async Task<IActionResult> CreateDiagnosis([FromBody] DiagnosisCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var diagnosis = new Diagnosis
            {
                SessionId = dto.SessionId,
                Result = "",
                DiagnosticRuleId = dto.DiagnosticRuleId,
                Code = dto.Code,
                Title = dto.Title,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Diagnoses.Add(diagnosis);
            await _dbContext.SaveChangesAsync();

            return Ok(diagnosis);
        }

        // ==================== Get Rules ====================
        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            var rules = await _mediator.Send(new GetAllRulesQuery());
            return Ok(rules);
        }

        // ==================== Get Conditions ====================
        [HttpGet("conditions")]
        public async Task<IActionResult> GetConditions()
        {
            var conditions = await _mediator.Send(new GetAllConditionsQuery());
            return Ok(conditions);
        }

        // ==================== Get Next Question ====================
        [HttpGet("next")]
        public async Task<IActionResult> GetNextQuestion()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("شناسه کاربر در توکن یافت نشد.");

            try
            {
                var nextQuestion = await _mediator.Send(new GetNextQuestionQuery(userId));
                if (nextQuestion == null)
                    return Ok(new { Done = true, Message = "همه سوالات پاسخ داده شده‌اند." });

                return Ok(nextQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت سوال بعدی: {ex.Message}");
            }
        }

         //==================== Python Integration Endpoint ====================
        [HttpPost("python-diagnose")]
        public async Task<IActionResult> PythonDiagnose([FromBody] DiagnosisRequestDto request)
        {
            if (request == null || request.Answers == null || !request.Answers.Any())
                return BadRequest("داده‌های ورودی نامعتبر است.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("شناسه کاربر در توکن یافت نشد.");

            var sessionId = HttpContext.Session.GetString("DiagnosisSessionId") ?? Guid.NewGuid().ToString();
            HttpContext.Session.SetString("DiagnosisSessionId", sessionId);

            // اضافه کردن UserId و SessionId
            request.Answers["UserId"] = new List<string> { userId };
            request.Answers["SessionId"] = new List<string> { sessionId };

            // **اینجا تبدیل پاسخ‌های عددی به string**
            if (request.Answers.ContainsKey("question1"))
            {
                request.Answers["question1"] = request.Answers["question1"]
                                                .Select(x => x.ToString())
                                                .ToList();
            }

            try
            {
                var result = await _pythonService.GetDiagnosisAsync(request);
                if (result == null)
                {
                    return StatusCode(500, "خطا در ارتباط با Python Service");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در ارتباط با Python Service: {ex.Message}");
            }
        }
        //[HttpPost("python-diagnose")]
        //public async Task<IActionResult> PythonDiagnose()
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userId))
        //        return Unauthorized("شناسه کاربر در توکن یافت نشد.");

        //    // گرفتن یا ساختن SessionId
        //    var sessionId = HttpContext.Session.GetString("DiagnosisSessionId") ?? Guid.NewGuid().ToString();
        //    HttpContext.Session.SetString("DiagnosisSessionId", sessionId);

        //    try
        //    {
        //        // 🔹 مرحله ۱: جمع‌آوری داده‌ها از دیتابیس
        //        var mlData = await _mlService.GetUserDataForMlAsync(Guid.Parse(userId), Guid.Parse(sessionId));

        //        if (mlData.Answers == null || mlData.Answers.Count == 0)
        //            return BadRequest("هیچ پاسخی برای ارسال به ML وجود ندارد.");

        //        // 🔹 مرحله ۲: آماده‌سازی درخواست برای Python API
        //        var request = new DiagnosisRequestDto
        //        {
        //            //UserId = mlData.UserId,
        //            //SessionId = mlData.SessionId,
        //            Answers = mlData.Answers.ToDictionary(
        //            kvp => kvp.Key,
        //            kvp => new List<string> { kvp.Value } // تبدیل string → List<string>
        //             )
        //        };

        //        // 🔹 مرحله ۳: ارسال به Python
        //        var result = await _pythonService.GetDiagnosisAsync(request);

        //        if (result == null)
        //            return StatusCode(500, "خطا در ارتباط با Python Service");

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"خطا در پردازش داده‌ها یا ارتباط با Python: {ex.Message}");
        //    }
        //}
    }
}
