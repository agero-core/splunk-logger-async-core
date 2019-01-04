using System;
using System.IO;
using System.Threading;
using Agero.Core.Checker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Core.Tests
{
    [TestClass]
    public class AsyncLoggerTests
    {
        private static AsyncLoggerTestsSetup _splunkCollectorInfo;

        [ClassInitialize]
        public static void LoggerTestsInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(@"logger-settings.json"), "The configuration file logger-settings.json needs to be setup. Please see https://github.com/agero-core/splunk-logger to set it up.");

            _splunkCollectorInfo = JsonConvert.DeserializeObject<AsyncLoggerTestsSetup>(File.ReadAllText(@"logger-settings.json"));
        }

        private static AsyncLogger CreateLogger(string collectorUrl)
        {
            Check.ArgumentIsNullOrWhiteSpace(collectorUrl, nameof(collectorUrl));


            return new AsyncLogger(
                collectorUri: new Uri(collectorUrl),
                authorizationToken: _splunkCollectorInfo.AuthenticationToken,
                applicationName: "Test",
                applicationVersion: "1.0.0.0",
                timeout: 10000);
        }

        private static void LogError(IAsyncLogger logger, int iterationCount = 10)
        {
            Check.ArgumentIsNull(logger, nameof(logger));
            Check.Argument(iterationCount > 0, "iterationCount > 0");

            for (var i = 0; i < iterationCount; i++)
            {
                logger.Log("Error", $"Error {i} from LoggerProcessor");
                Thread.Sleep(1);
            }
        }

        [TestMethod]
        [TestCategory("Ignore")]
        public void MultiThreading_Test_When_Invalid_Collector_Url()
        {
            //Arrange
            var logger = CreateLogger("http://localhost/Wrong/");
            var builder = new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<LogProcessingBackgroundService>();
                        services.AddHostedService<LogProcessingBackgroundService>();
                    }).Build();

            builder.StartAsync();

            // Act
            LogError(logger, 10);
            Thread.Sleep(10_000);
            builder.StopAsync();

            // Assert
            Assert.AreEqual(0, logger.PendingLogCount);
        }

        [TestMethod]
        [TestCategory("Ignore")]
        public void MultiThreading_Test_When_Valid_Collector_Url()
        {
            //Arrange
            var logger = CreateLogger(_splunkCollectorInfo.SplunkCollectorUrl);
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<LogProcessingBackgroundService>();
                    services.AddHostedService<LogProcessingBackgroundService>();
                }).Build();

            builder.StartAsync();

            // Act
            LogError(logger, 10);
            Thread.Sleep(10_000);
            builder.StopAsync();

            // Assert
            Assert.AreEqual(0, logger.PendingLogCount);
        }
    }
}
