using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Agero.Core.SplunkLogger.Helpers;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;


namespace Agero.Core.SplunkLogger.Async.Core
{
    /// <summary>
    /// Exposes Background Service Implementation
    /// </summary>
    public class LoggerProcessor : BackgroundService
    {
        private ILoggerBackgroundTaskQueue _taskQueue => LoggerAsync.LoggerBackgroundTaskQueue;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    LogTrace(ex.ToString());
                }
            }
        }

        private void LogTrace(string exceptionMessage)
        {
            Check.ArgumentIsNullOrWhiteSpace(exceptionMessage, "exceptionMessage");

            try
            {
                var splunkFailedLog =
                    new
                    {
                        message = "Request to Splunk failed.",
                        url = "",
                        applicationName = "",
                        applicationVersion = "",
                        system = LoggerHelper.GetSystemData(),
                        data = exceptionMessage
                    };

                Trace.WriteLine(JsonConvert.SerializeObject(splunkFailedLog), "ERROR");
            }
            catch
            {
                // ignored
            }
        }
    }
}
