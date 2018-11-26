using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Agero.Core.DIContainer;
using Agero.Core.SplunkLogger.Helpers;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;


namespace Agero.Core.SplunkLogger.Async.Core
{
    /// <summary>
    /// Exposes Background Service Implementation
    /// </summary>
    public class LoggerProcessor : BackgroundService, ILoggerProcessor
    {
        private static readonly BlockingCollection<LogItem> _logQueue = LoggerAsync.LogQueue;

        private static IReadOnlyContainer _container => DIContainer.Instance;

        private const int QueueReadTimeout = 1000;

        private CancellationTokenSource _shutdown =
            new CancellationTokenSource();

        private Task _backgroundTask;

        private readonly ILogger _logger;

        public LoggerProcessor(IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            TaskQueue = taskQueue;
            _logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Queued Hosted Service is starting.");

            _backgroundTask = Task.Run(BackgroundProceessing);

            return Task.CompletedTask;
        }

        private async Task BackgroundProceessing()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                    try
                    {
                        if (!_logQueue.TryTake(out var logItem, millisecondsTimeout: QueueReadTimeout))
                            continue;

                        _container.Get<ILogger>().Log(logItem.Type, logItem.Message, logItem.Data, logItem.CorrelationId);
                    }
                    catch (Exception exception)
                    {
                        LogTrace(exception.ToString());
                    }
                }
        }

        /*protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!_logQueue.TryTake(out var logItem, millisecondsTimeout: QueueReadTimeout))
                            continue;

                        _container.Get<ILogger>().Log(logItem.Type, logItem.Message, logItem.Data, logItem.CorrelationId);
                    }
                    catch (Exception exception)
                    {
                        LogTrace(exception.ToString());
                    }
                }
            });

          return Task.CompletedTask;
        }*/

        public Task StopAsync(CancellationToken cancellationToken)
        {
           // _logger.LogInformation("Queued Hosted Service is stopping.");

            _shutdown.Cancel();

            return Task.WhenAny(_backgroundTask,
                Task.Delay(Timeout.Infinite, cancellationToken));
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
