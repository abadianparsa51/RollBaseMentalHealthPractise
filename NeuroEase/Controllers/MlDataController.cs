using Core.Layer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeuroEase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MlDataController : ControllerBase
    {
        private readonly MlDataExportService _mlService;

        public MlDataController(MlDataExportService mlService)
        {
            _mlService = mlService;
        }

        //[HttpGet("{userId}/{sessionId}")]
        //public async Task<IActionResult> Get(Guid userId, Guid sessionId)
        //{
        //    var data = await _mlService.GetUserDataForMlAsync(userId, sessionId);
        //    return Ok(data);
        //}
    }
}
