using System;
using System.Collections.Concurrent;
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
        private const int QUEUE_READ_TIMEOUT = 1000;
        
        private static readonly BlockingCollection<LogItem> _queue  = new BlockingCollection<LogItem>();

        internal static void AddLogForProcessing(ILogger logger, string type, string message, object data = null, string correlationId = null) => _queue.Add(new LogItem(logger, type, message, data, correlationId));
        
        internal static int PendingLogCount => _queue.Count;
        
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
                    if (!_queue.TryTake(out var logItem, millisecondsTimeout: QUEUE_READ_TIMEOUT))
                        continue;
                
                    Check.Assert(await logItem.Logger.LogAsync(logItem.Type, logItem.Message, logItem.Data, logItem.CorrelationId), "Log submit failed.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString(), "ERROR");
                }
            }
        }
    }
}
