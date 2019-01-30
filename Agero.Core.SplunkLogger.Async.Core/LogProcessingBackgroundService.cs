using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Microsoft.Extensions.Hosting;

namespace Agero.Core.SplunkLogger.Async.Core
{
    /// <summary>Background service for log processing</summary>
    public class LogProcessingBackgroundService : BackgroundService
    {
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (LogQueue.Count == 0)
                        await Task.Delay(millisecondsDelay: LogQueue.QUEUE_READ_TIMEOUT_IN_MILLISECONDS, cancellationToken: stoppingToken);
                    
                    if (!LogQueue.TryTake(out var logItem, stoppingToken))
                        continue;

                    var success = await logItem.Logger.LogAsync(logItem.Type, logItem.Message, logItem.Data, logItem.CorrelationId); 
                    
                    Check.Assert(success, "Log submit failed.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString(), "ERROR");
                }
            }
        }
    }
}
