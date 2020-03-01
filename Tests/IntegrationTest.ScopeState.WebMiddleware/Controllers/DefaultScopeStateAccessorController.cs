using System.Net;
using Microsoft.AspNetCore.Mvc;
using ScopeState;
using ScopeState.Imp;

namespace IntegrationTest.ScopeState.WebMiddleware.Controllers
{
    [Route("default-scope-state-accessor")]
    public class DefaultScopeStateAccessorController : ControllerBase
    {
        private readonly IScopeStateAccessor<BasicScopeState> _basicScopeStateAccessor;

        public DefaultScopeStateAccessorController(IScopeStateAccessor<BasicScopeState> basicScopeStateAccessor)
        {
            _basicScopeStateAccessor = basicScopeStateAccessor;
        }

        [Route("trace-id")]
        public IActionResult Get()
        {
            string traceId = _basicScopeStateAccessor.ScopeState.TraceId;

            return StatusCode((int) HttpStatusCode.OK, traceId);
        }
    }
}