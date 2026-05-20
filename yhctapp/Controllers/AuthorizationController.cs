using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        public readonly IAuthorization _authorization;
        public AuthorizationController(IAuthorization authorization)
        {
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthorizationByUserId()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _authorization.GetAuthorizationByUserId(userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

}
