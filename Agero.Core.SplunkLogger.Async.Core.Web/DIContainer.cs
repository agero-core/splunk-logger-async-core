using System;
using System.IO;
using System.Reflection;
using Agero.Core.DIContainer;
using Agero.Core.Lazy;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Core.Web
{
    public class DIContainer
    {
        private static readonly SyncLazy<IContainer> _container =
            new SyncLazy<IContainer>(CreateContainer);

        private static readonly AsyncLoggerTestsSetup AsyncLoggerSetup =
            JsonConvert.DeserializeObject<AsyncLoggerTestsSetup>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "logger-settings.json")));

        private static IContainer CreateContainer()
        {
            var container = ContainerFactory.Create();

            container.RegisterFactoryMethod<IAsyncLogger>(c =>
                    new AsyncLogger
                    (
                        collectorUri: new Uri(AsyncLoggerSetup.SplunkCollectorUrl),
                        authorizationToken: AsyncLoggerSetup.AuthenticationToken,
                        applicationName: "WebTest",
                        applicationVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        timeout: 10000
                    ),
                Lifetime.PerContainer
            );

            return container;
        }

        public static IContainer Instance => _container.Value;
    }
}
