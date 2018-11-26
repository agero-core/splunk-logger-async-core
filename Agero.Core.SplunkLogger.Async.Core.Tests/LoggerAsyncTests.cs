﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Core.Tests
{
    [TestClass]
    public class LoggerAsyncTests
    {
        private static LoggerAsyncTestsSetup _splunkCollectorInfo;

        [ClassInitialize]
        public static void LoggerTestsInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(@"logger-settings.json"), "The configuration file logger-settings.json needs to be setup. Please see https://github.com/agero-core/splunk-logger to set it up.");

            _splunkCollectorInfo = JsonConvert.DeserializeObject<LoggerAsyncTestsSetup>(File.ReadAllText(@"logger-settings.json"));
        }

        private static LoggerAsync CreateLogger(string collectorUrl)
        {
            Check.ArgumentIsNullOrWhiteSpace(collectorUrl, nameof(collectorUrl));

            return new LoggerAsync(
                collectorUri: new Uri(collectorUrl),
                authorizationToken: _splunkCollectorInfo.AuthenticationToken,
                applicationName: "Test",
                applicationVersion: "1.0.0.0",
                timeout: 10000);
        }

        private static void LogError(ILoggerAsync logger, int iterationCount = 10)
        {
            Check.ArgumentIsNull(logger, nameof(logger));
            Check.Argument(iterationCount > 0, "iterationCount > 0");

            for (var i = 0; i < iterationCount; i++)
            {
                logger.Log("Error", $"Error {i} from thread {i}");
                Console.WriteLine($"Thread - 1, iteration - {i}");
                Thread.Sleep(1);
            }
        }

        [TestMethod]
        [TestCategory("Ignore")]
        public void MultiThreading_Test_When_Invalid_Collector_Url()
        {
            using (var logger = CreateLogger("http://localhost/Wrong/"))
            {
                //Arrange
                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<LoggerProcessor>();
                        services.AddHostedService<LoggerProcessor>();
                    }).Build();

                builder.StartAsync();

                // Act
                LogError(logger, 100);
                Thread.Sleep(10_000);
                builder.StopAsync();

                // Assert
                Assert.AreEqual(0, logger.PendingLogCount);
            }

            Thread.Sleep(1500);
        }

        [TestMethod]
        [TestCategory("Ignore")]
        public void MultiThreading_Test_When_Valid_Collector_Url()
        {
            using (var logger = CreateLogger(_splunkCollectorInfo.SplunkCollectorUrl))
            {
                //Arrange
                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<LoggerProcessor>();
                    }).Build();

                builder.StartAsync();

                // Act
                LogError(logger, 3); 
                Thread.Sleep(10_000);
                builder.StopAsync();

                // Assert
                Assert.AreEqual(0, logger.PendingLogCount);
            }

            Thread.Sleep(1500);
        }
    }
}
