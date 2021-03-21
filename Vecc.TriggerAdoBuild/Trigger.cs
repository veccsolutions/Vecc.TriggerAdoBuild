using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Vecc.TriggerAdoBuild
{
    public static class Trigger
    {
        [FunctionName("Trigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting");
                var organization = req.Query["organization"];
                var project = req.Query["project"];
                var identifier = req.Query["identifier"];
                var pat = req.Query["pat"];

                log.LogInformation("Got query parameters");

                var existingInsance = await client.GetStatusAsync("hello123");
                if (string.IsNullOrWhiteSpace(organization) ||
                    string.IsNullOrWhiteSpace(project) ||
                    string.IsNullOrWhiteSpace(identifier) ||
                    string.IsNullOrWhiteSpace(pat))
                {
                    log.LogWarning("Invalid request: organization: '{@organization}' project: '{@project}' identifier: '{@identifier}'", organization, project, identifier);
                    return new BadRequestResult();
                }

                log.LogInformation("Checkint to see if we need to start");
                if (existingInsance == null ||
                    existingInsance.RuntimeStatus == OrchestrationRuntimeStatus.Completed ||
                    existingInsance.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                    existingInsance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
                {
                    log.LogInformation("Starting");

                    using var stream = new MemoryStream();
                    req.Body.CopyTo(stream);
                    log.LogInformation("Copying stream bytes");
                    var body = Encoding.UTF8.GetString(stream.ToArray());
                    log.LogInformation("Converted to string");
                    var request = JsonConvert.DeserializeObject<Request>(body);
                    log.LogInformation("Parsed body");
                    await client.StartNewAsync("AdoTrigger", "hello123", new TriggerState
                    {
                        AccessToken = pat,
                        BuildDefinitionId = identifier,
                        Organization = organization,
                        Project = project,
                        Request = request
                    });
                    log.LogInformation("Started single trigger");
                }
                else
                {
                    log.LogInformation("Already running, no need to start again.");
                }

                return new OkResult();
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Something bad happened");
                throw;
            }
        }
    }
}
