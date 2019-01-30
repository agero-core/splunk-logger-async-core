using System.Collections.Concurrent;
using System.Threading;
using Agero.Core.Checker;

namespace Agero.Core.SplunkLogger.Async.Core
{
    internal static class LogQueue
    {
        public const int QUEUE_READ_TIMEOUT_IN_MILLISECONDS = 1000;
        
        private static readonly BlockingCollection<LogItem> _queue  = new BlockingCollection<LogItem>();

        public static void Add(LogItem logItem)
        {
            Check.ArgumentIsNull(logItem, nameof(logItem));
            
            _queue.Add(logItem);
        }

        public static int Count => _queue.Count;

        public static bool TryTake(out LogItem logItem, CancellationToken cancellationToken) =>
            _queue.TryTake(out logItem, millisecondsTimeout: QUEUE_READ_TIMEOUT_IN_MILLISECONDS, cancellationToken: cancellationToken);
    }
}