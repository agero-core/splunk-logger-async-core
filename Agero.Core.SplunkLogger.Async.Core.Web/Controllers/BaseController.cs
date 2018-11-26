using Agero.Core.DIContainer;
using Microsoft.AspNetCore.Mvc;

namespace Agero.Core.SplunkLogger.Async.Core.Web.Controllers
{
    public class BaseController : ControllerBase
    {
        protected static IReadOnlyContainer Container => DIContainer.Instance;
    }
}
