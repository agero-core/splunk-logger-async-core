using System;
using System.IO;
using System.Reflection;
using Agero.Core.Checker;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Core.Web.Controllers
{
    [Route("logs")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        private static readonly AsyncLogger _logger = new Func<AsyncLogger>(() =>
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "logger-settings.json");

            Check.Assert(System.IO.File.Exists(path), "The configuration file logger-settings.json needs to be setup. Please see https://github.com/agero-core/splunk-logger-async-core to set it up.");

            var setup = JsonConvert.DeserializeObject<AsyncLoggerTestsSetup>(System.IO.File.ReadAllText(path));

            return new AsyncLogger(
                collectorUri: new Uri(setup.SplunkCollectorUrl),
                authorizationToken: setup.AuthenticationToken,
                applicationName: "WebTest",
                applicationVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                timeout: 10000
            );
        })();
        
        /// <remarks>POST /logs</remarks>
        [Route("")]
        [HttpPost]
        public int CreateLogs()
        {
            for (var i = 0; i < 100; i++)
                _logger.Log("Error", $"Error {i}");

            return GetPendingCount();
        }

        /// <remarks>GET /logs/pending</remarks>
        [Route("pending")]
        [HttpGet]
        public int GetPendingCount() => _logger.PendingLogCount;
    }
}
