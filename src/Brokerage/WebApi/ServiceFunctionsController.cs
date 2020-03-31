using System.Threading.Tasks;
using Brokerage.Common.ServiceFunctions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.WebApi
{
    [ApiController]
    [Route("api/service-functions")]
    public class ServiceFunctionsController : ControllerBase
    {
        private readonly ISendEndpointProvider _commandsSender;

        public ServiceFunctionsController(ISendEndpointProvider commandsSender)
        {
            _commandsSender = commandsSender;
        }

        [HttpPost("publish-account-requisites")]
        public async Task<IActionResult> PublishAccountRequisites()
        {
            await _commandsSender.Send(new PublishAccountRequisites());

            return Ok();
        }
    }
}
