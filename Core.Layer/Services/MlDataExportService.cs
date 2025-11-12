using Core.Layer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Layer.Services
{
    public class MlDataExportService
    {
        private readonly MentalHealthDbContext _dbContext;
        private readonly ILogger<MlDataExportService> _logger;
        public MlDataExportService(MentalHealthDbContext dbContext, ILogger<MlDataExportService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    //    public async Task<MlDataExportDto> GetUserDataForMlAsync(Guid userId, Guid sessionId)
    //    {
    //        _logger.LogInformation("Preparing ML export data for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

    //        var userAnswers = await _dbContext.UserAnswers
    //            .Where(ua => ua.UserId == userId.ToString() && ua.SessionId == sessionId.ToString())
    //            .Include(ua => ua.Question)
    //            .OrderBy(ua => ua.Question.Order)
    //            .ToListAsync();

    //        if (!userAnswers.Any())
    //        {
    //            _logger.LogWarning("No answers found for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
    //            return new MlDataExportDto
    //            {
    //                UserId = userId,
    //                SessionId = sessionId
    //            };
    //        }

    //        var answersDict = new Dictionary<string, string>();
    //        int qNumber = 1;
    //        foreach (var answer in userAnswers)
    //        {
    //            string val = answer.Answer?.Trim().ToLower() switch
    //            {
    //                "بله" => "1",
    //                "خیر" => "0",
    //                _ => answer.Answer ?? "0"
    //            };

    //            answersDict.Add($"Q{qNumber}", val);
    //            qNumber++;
    //        }

    //        var dto = new MlDataExportDto
    //        {
    //            UserId = userId,
    //            SessionId = sessionId,
    //            Answers = answersDict
    //        };

    //        _logger.LogInformation("ML export data prepared for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

    //        return dto;
    //    }
    //}
    }
}
