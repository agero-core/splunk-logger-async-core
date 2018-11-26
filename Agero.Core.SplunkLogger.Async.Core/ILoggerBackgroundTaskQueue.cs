using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agero.Core.SplunkLogger.Async.Core
{
    internal interface ILoggerBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
