using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vecc.TriggerAdoBuild
{
    public static class AdoTrigger
    {
        private static DateTime _lastUpdated = DateTime.MinValue;

        [FunctionName("AdoTrigger")]
        public static async Task EntityTrigger([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                log.LogInformation("In AdoTrigger");

                var kickOffBuild = true;
                string itemId;
                DateTime updatedAt;
                var state = context.GetInput<TriggerState>();
                var request = state.Request;
                var organization = state.Organization;
                var project = state.Project;
                var buildDefinitionId = state.BuildDefinitionId;
                var accessToken = state.AccessToken;

                if (request.Page != null)
                {
                    itemId = "page-" + request.Page.Current.Id;
                    updatedAt = request.Page.Current.Updated_At;
                }
                else if (request.Post != null)
                {
                    itemId = "post-" + request.Post.Current.Id;
                    updatedAt = request.Post.Current.Updated_At;
                }
                else if (request.Tag != null)
                {
                    itemId = "tag-" + request.Tag.Current.Id;
                    updatedAt = request.Tag.Current.Updated_At;
                }
                else
                {
                    itemId = "NOT FOUND";
                    updatedAt = DateTime.UtcNow;
                }

                log.LogInformation("Id: {id} Updated_At: {updated_at}", itemId, request.Page?.Current.Updated_At ?? request.Post?.Current.Updated_At ?? DateTime.MinValue);

                // Not sure about this, it takes 30 seconds for my environment, There's probably some better way than this, but this works for me.
                if (_lastUpdated.AddSeconds(30) >= updatedAt)
                {
                    log.LogInformation("Duplicate request detected. Skipping.");
                    kickOffBuild = false;
                }

                if (kickOffBuild)
                {
                    _lastUpdated = DateTime.UtcNow;
                    log.LogInformation($@"Kicking off build pipeline {organization}\{project}\{buildDefinitionId} at {DateTime.UtcNow}");
                    var url = $"https://dev.azure.com/{organization}/{project}/_apis/build/builds?definitionId={buildDefinitionId}&api-version=6.1-preview.6";
                    var encodedPat = Convert.ToBase64String(Encoding.UTF8.GetBytes($"something:{accessToken}"));
                    var httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedPat}");

                    var response = await httpClient.PostAsync(url, new ObjectContent(typeof(object), new object(), new System.Net.Http.Formatting.JsonMediaTypeFormatter()));
                    response.EnsureSuccessStatusCode();
                    log.LogInformation($"Finished kick off at {DateTime.UtcNow}");
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, "Error kicking off the build. Sad.");
            }
        }
    }
}
