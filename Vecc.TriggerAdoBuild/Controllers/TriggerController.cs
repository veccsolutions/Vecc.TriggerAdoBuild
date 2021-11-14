using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vecc.TriggerAdoBuild.Models;

namespace Vecc.TriggerAdoBuild.Controllers
{
    [Route("api/trigger")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        private readonly ILogger<TriggerController> _logger;

        public TriggerController(ILogger<TriggerController> logger)
        {
            this._logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Blob blob)
        {
            this._logger.LogInformation("Triggering build on: {blob}", blob.Url);
            var encodedPat = Convert.ToBase64String(Encoding.UTF8.GetBytes($"something:{blob.Pat}"));
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, blob.Url);
            requestMessage.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedPat);
            try
            {
                await httpClient.SendAsync(requestMessage);
                return this.Ok();
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, "Error posting to URL: {url}", blob.Url);
                return this.StatusCode(500);
            }
        }
    }
}
