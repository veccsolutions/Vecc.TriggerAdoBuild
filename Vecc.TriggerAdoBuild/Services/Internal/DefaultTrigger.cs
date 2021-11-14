using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vecc.TriggerAdoBuild.Models;

namespace Vecc.TriggerAdoBuild.Services.Internal
{
    public class DefaultTrigger : ITrigger
    {
        private readonly ConcurrentQueue<Blob> _triggerQueue;
        private readonly ILogger<Blob> _logger;
        private Task _runner;

        public DefaultTrigger(ILogger<Blob> logger)
        {
            this._triggerQueue = new ConcurrentQueue<Blob>();
            this._logger = logger;
            this._runner = Run();
        }

        public Task QueueItemAsync(Blob blob)
        {
            this._triggerQueue.Enqueue(blob);
            return Task.CompletedTask;
        }

        public Task Run()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (_triggerQueue.TryDequeue(out var blob))
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
                            this._logger.LogInformation("Triggered succesfully");
                        }
                        catch (Exception exception)
                        {
                            this._logger.LogError(exception, "Error posting to URL: {url}", blob.Url);
                        }
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            });
        }
    }
}
