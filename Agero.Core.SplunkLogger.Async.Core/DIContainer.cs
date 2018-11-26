using Agero.Core.DIContainer;
using Agero.Core.Lazy;

namespace Agero.Core.SplunkLogger.Async.Core
{
    internal class DIContainer
    {
        private static readonly SyncLazy<IContainer> Container = new SyncLazy<IContainer>(CreateContainer);

        private static IContainer CreateContainer()
        {
            var container = ContainerFactory.Create();

            return container;
        }

        public static IContainer Instance => Container.Value;
    }
}
