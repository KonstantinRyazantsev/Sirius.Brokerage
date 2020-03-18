using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Brokerage.Common.Domain.AppFeatureExample;

namespace Brokerage.WebApi
{
    // TODO: It's just an example
    [ApiController]
    [Route("api/app-feature-example")]
    public class AppFeatureExampleController : ControllerBase
    {
        private readonly ISendEndpointProvider _commandsSender;

        public AppFeatureExampleController(ISendEndpointProvider commandsSender)
        {
            _commandsSender = commandsSender;
        }

        [HttpPost("execute-something")]
        public async Task<IActionResult> ExecuteSomething()
        {
            await _commandsSender.Send(new ExecuteSomething());

            return Ok();
        }
    }
}
