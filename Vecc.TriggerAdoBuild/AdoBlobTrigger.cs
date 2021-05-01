using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;

namespace Vecc.TriggerAdoBuild
{
    public static class AdoBlobTrigger
    {
        [FunctionName("AdoBlobTrigger")]
        public static void Run(
            [QueueTrigger("build-trigger", Connection = "ConnectionStrings:blobconnection")] string queuedItem,
            ILogger log)
        {
            log.LogInformation("QueuedItem: {queuedItem}", queuedItem);
            var blob = JsonSerializer.Deserialize<BlobObject>(queuedItem);
            log.LogInformation("Kicking off build: {url}", blob.Url);
            var encodedPat = Convert.ToBase64String(Encoding.UTF8.GetBytes($"something:{blob.Pat}"));
            var webRequest = System.Net.WebRequest.CreateHttp(blob.Url);
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", $"Basic {encodedPat}");
            webRequest.Method = "POST";
            webRequest.ContentLength = 0;
            using var webResponse = webRequest.GetResponse();
        }
    }
}
