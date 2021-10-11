using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TT.Deliveries.Business.Services;

namespace TT.Deliveries.Web.Api.Controllers
{
    /// <summary>
    /// This class should be disregarded and is used purely for Token generation and debugging.
    /// </summary>
    /// <remarks>
    /// Neither the built-in Role, nor Policy will work with the JWT created.
    /// </remarks>
    [Route("[Controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TokenController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public TokenController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string role) => Content(await _authenticationService.GenerateTokenAsync(role));

        [Authorize]
        [HttpGet("Diagnose")]
        public IActionResult Diagnose()
        {
            var x = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "User");
            var a = User.IsInRole("User");
            var b = User.IsInRole("user");
            return Json(User, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
        }

        [Authorize(Roles = "Partner")]
        [HttpGet("Partner")]
        public IActionResult PartnerAccess() => Ok();

        //[Authorize(Roles = "User")]
        //[Authorize("User")]
        [HttpGet("User")]
        public async Task<IActionResult> UserAccess()
        {
            if (await _authenticationService.IsInRoleAsync(User, "User"))
            {
                return Ok();
            }

            return Unauthorized();
        }
    }
}