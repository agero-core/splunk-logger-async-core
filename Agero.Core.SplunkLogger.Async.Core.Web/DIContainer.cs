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

        private static readonly LoggerAsyncTestsSetup _loggerSetup =
            JsonConvert.DeserializeObject<LoggerAsyncTestsSetup>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "logger-settings.json")));

        private static IContainer CreateContainer()
        {
            var container = ContainerFactory.Create();

            container.RegisterFactoryMethod<ILoggerAsync>(c =>
                    new LoggerAsync
                    (
                        collectorUri: new Uri(_loggerSetup.SplunkCollectorUrl),
                        authorizationToken: _loggerSetup.AuthenticationToken,
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
