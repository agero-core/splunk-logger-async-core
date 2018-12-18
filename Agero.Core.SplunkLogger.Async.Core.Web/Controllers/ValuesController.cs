using Microsoft.AspNetCore.Mvc;

namespace Agero.Core.SplunkLogger.Async.Core.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : BaseController
    {
        // GET api/values
        [HttpGet]
        public int Get()
        {
            var logger = Container.Get<IAsyncLogger>();

            for (var i = 0; i < 100; i++)
                logger.Log("Error", $"Error {i}");

            return PendingCount();
        }

        [HttpGet("PendingCount")]
        public int PendingCount() => ((AsyncLogger) Container.Get<IAsyncLogger>()).PendingLogCount;
    }
}
