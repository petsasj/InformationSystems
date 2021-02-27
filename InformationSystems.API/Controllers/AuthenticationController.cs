using System.Threading.Tasks;
using InformationSystems.API.Models;
using InformationSystems.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InformationSystems.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _authenticationService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "VAT or password is incorrect" });

            return Ok(response);
        }
    }
}
