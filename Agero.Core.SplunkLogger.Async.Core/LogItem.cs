using Agero.Core.Checker;

namespace Agero.Core.SplunkLogger.Async.Core
{
    internal class LogItem
    {
        public LogItem(ILogger logger, string type, string message, object data = null, string correlationId = null)
        {
            Check.ArgumentIsNull(logger, nameof(logger));
            Check.ArgumentIsNullOrWhiteSpace(type, nameof(type));
            Check.ArgumentIsNullOrWhiteSpace(message, nameof(message));

            Logger = logger;
            Type = type;
            Message = message;
            Data = data;
            CorrelationId = correlationId;
        }
        
        public ILogger Logger { get; }
        
        public string Type { get; }

        public string Message { get; }

        public object Data { get; }

        public string CorrelationId { get; }
    }
}