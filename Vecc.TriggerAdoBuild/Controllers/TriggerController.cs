using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vecc.TriggerAdoBuild.Models;
using Vecc.TriggerAdoBuild.Services;

namespace Vecc.TriggerAdoBuild.Controllers
{
    [Route("api/trigger")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        private readonly ILogger<TriggerController> _logger;
        private readonly ITrigger _trigger;

        public TriggerController(ILogger<TriggerController> logger, ITrigger trigger)
        {
            this._logger = logger;
            this._trigger = trigger;
        }

        [HttpGet("")]
        [HttpPost("")]
        public async Task<IActionResult> Index([FromQuery] Blob blob)
        {
            this._logger.LogInformation("Queueing build for url: {url}", blob.Url);
            try
            {
                await this._trigger.QueueItemAsync(blob);
                this._logger.LogInformation("Queueing successful");
                return this.Ok();
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, "Error queueing blob: {url}", blob.Url);
                return this.StatusCode(500);
            }
        }
    }
}
