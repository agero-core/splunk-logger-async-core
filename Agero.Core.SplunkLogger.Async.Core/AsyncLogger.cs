using System;
using Agero.Core.Checker;

namespace Agero.Core.SplunkLogger.Async.Core
{
    /// <summary>Asynchronous Splunk Logger</summary>
    public class AsyncLogger : IAsyncLogger
    {
        private readonly ILogger _logger;

        /// <summary>Constructor</summary>
        /// <param name="collectorUri">Splunk HTTP collector URL</param>
        /// <param name="authorizationToken">Splunk authorization token</param>
        /// <param name="applicationName">Unique application name</param>
        /// <param name="applicationVersion">Application version</param>
        /// <param name="timeout">Splunk HTTP collector timeout (milliseconds)</param>
        public AsyncLogger(Uri collectorUri, string authorizationToken, string applicationName, string applicationVersion, int timeout = 10000)
        {
            Check.ArgumentIsNull(collectorUri, nameof(collectorUri));
            Check.ArgumentIsNullOrWhiteSpace(authorizationToken, nameof(authorizationToken));
            Check.ArgumentIsNullOrWhiteSpace(applicationName, nameof(applicationName));
            Check.ArgumentIsNullOrWhiteSpace(applicationVersion, nameof(applicationVersion));
            Check.Argument(timeout > 0, "timeout > 0");

            _logger = new Logger(collectorUri, authorizationToken, applicationName, applicationVersion, timeout);
        }

        /// <summary>Number of items to be processed</summary>
        public int PendingLogCount => LogProcessingBackgroundService.PendingLogCount;

        /// <summary>Submits log to Splunk asynchronously</summary>
        /// <param name="type">Log type (Error, Info, etc.)</param>
        /// <param name="message">Log text message</param>
        /// <param name="data">Any object which will be serialized into JSON</param>
        /// <param name="correlationId">Any optional string which can correlate different logs</param>
        public void Log(string type, string message, object data = null, string correlationId = null)
        {
            Check.ArgumentIsNullOrWhiteSpace(type, nameof(type));
            Check.ArgumentIsNullOrWhiteSpace(message, nameof(message));
            
            LogProcessingBackgroundService.AddLogForProcessing(_logger, type, message, data, correlationId);
        }
    }
}
