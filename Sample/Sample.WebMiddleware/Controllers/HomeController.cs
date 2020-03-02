using System.Net;
using Microsoft.AspNetCore.Mvc;
using SampleApi.AppScopeStates;
using ScopeState;
using ScopeState.Imp;

namespace SampleApi.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly IScopeStateAccessor<AppScopeState> _appScopeStateAccessor;
        private readonly IScopeStateAccessor<BasicScopeState> _basicScopeStateAccessor;

        public HomeController(IScopeStateAccessor<AppScopeState> appScopeStateAccessor, IScopeStateAccessor<BasicScopeState> basicScopeStateAccessor)
        {
            _appScopeStateAccessor = appScopeStateAccessor;
            _basicScopeStateAccessor = basicScopeStateAccessor;
        }

        [Route("")]
        public IActionResult Get()
        {
            return StatusCode((int) HttpStatusCode.OK, new
                                                       {
                                                           AppScopeStateCultureName = _appScopeStateAccessor.ScopeState.Culture.Name,
                                                           AppScopeStateTraceId = _appScopeStateAccessor.ScopeState.TraceId,
                                                           BasicScopeStateTraceId = _basicScopeStateAccessor.ScopeState.TraceId
                                                       });
        }
    }
}