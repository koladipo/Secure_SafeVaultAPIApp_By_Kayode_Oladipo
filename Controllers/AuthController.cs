using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SafeVaultAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SafeVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
       private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        // ==========================
        // Register User
        // ==========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
                return BadRequest("Email already exists.");

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email
            };

            var result =
                await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign role
            var roleResult =
                await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
{
    return BadRequest(roleResult.Errors);
}

            if (!roleResult.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to assign role.",
                    Errors = roleResult.Errors
                });
            }

            _logger.LogInformation(
                $"User registered successfully: {model.Email}");

            return Ok(new
            {
                Message = "User registered successfully."
            });
        }

        // ==========================
        // Login User
        // ==========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user =
                await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning(
                    $"Login failed. User not found: {model.Email}");

                return Unauthorized("Invalid credentials.");
            }

            var passwordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    model.Password);

            if (!passwordValid)
            {
                _logger.LogWarning(
                    $"Login failed. Invalid password: {model.Email}");

                return Unauthorized("Invalid credentials.");
            }

            var roles = await _userManager.GetRolesAsync(user);

return Ok(new
{
    email = user.Email,
    roles = roles
});
        }

        // ==========================
        // Generate JWT Token
        // ==========================
        private async Task<string> GenerateJwtToken(ApplicationUser user)
{
    var roles = await _userManager.GetRolesAsync(user);

    _logger.LogInformation(
        $"Roles for {user.Email}: {string.Join(", ", roles)}");

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
    };

    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

    var creds = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var expires = DateTime.UtcNow.AddMinutes(
        Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]));

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: expires,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
    }
}