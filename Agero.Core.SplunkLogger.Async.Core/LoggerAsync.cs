﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using Agero.Core.Checker;
using Agero.Core.DIContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Core
{
    /// <summary>Logger which supports asynchronous message submit</summary>
    public class LoggerAsync : ILoggerAsync
    {
        private const int QueueReadTimeout = 1000;

        internal static readonly BlockingCollection<LogItem> LogQueue = new BlockingCollection<LogItem>();

        private static readonly IContainer Container = DIContainer.Instance;

        /// <summary>Constructor</summary>
        /// <param name="collectorUri">Splunk HTTP collector URL</param>
        /// <param name="authorizationToken">Splunk authorization token</param>
        /// <param name="applicationName">Unique application name</param>
        /// <param name="applicationVersion">Application version</param>
        /// <param name="timeout">Splunk HTTP collector timeout</param>
        /// 
        public LoggerAsync(Uri collectorUri, string authorizationToken, string applicationName, string applicationVersion, int timeout = 10000)
        {
            Check.ArgumentIsNull(collectorUri, nameof(collectorUri));
            Check.ArgumentIsNullOrWhiteSpace(authorizationToken, nameof(authorizationToken));
            Check.ArgumentIsNullOrWhiteSpace(applicationName, nameof(applicationName));
            Check.ArgumentIsNullOrWhiteSpace(applicationVersion, nameof(applicationVersion));
            Check.Argument(timeout > 0, "timeout > 0");

            if(Container.Contains<ILogger>())
                Container.Remove<ILogger>();

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddSingleton<ILogger>();

                    #region snippet3
                    services.AddHostedService<QueuedHostedService>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                    #endregion
                })
                .UseConsoleLifetime()
                .Build();

            Container.RegisterInstance<ILogger>(new Logger(collectorUri, authorizationToken, applicationName, applicationVersion, timeout));
        }

        /// <summary>Number of items to be processed</summary>
        public int PendingLogCount => LogQueue.Count;

        /// <summary>Submits log to Splunk</summary>
        /// <param name="type">Log type (Error, Info, etc.)</param>
        /// <param name="message">Log text message</param>
        /// <param name="data">Any object which serialized into JSON</param>
        /// <param name="correlationId">Correlation ID for synchronizing different messages</param>
        /// <remarks>If submitting to Splunk fails then log is submitted to Windows Event Log</remarks>
        public void Log(string type, string message, object data = null, string correlationId = null) => LogQueue.Add(new LogItem(type, message, data, correlationId));

        /// <summary>Disposes current object</summary>
        public void Dispose()
        {
            //
        }
    }
}