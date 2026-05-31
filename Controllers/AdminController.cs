using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        // Only Admin role can access
        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok("Admin dashboard data.");
        }

        // Policy-based authorization
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("reports")]
        public IActionResult Reports()
        {
            return Ok("Sensitive reports.");
        }
    }
}