using Reddit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reddit.Data;
using Reddit.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Reddit.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            TokenService tokenService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.Now.AddDays(TokenService.RefreshTokenExpirationDays)
            };

            var result = await _userManager.CreateAsync(user, request.Password!);

            if (result.Succeeded)
            {
                request.Password = ""; // Clear sensitive data before returning
                return CreatedAtAction(nameof(Register), new { email = request.Email }, request);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !(await _userManager.CheckPasswordAsync(user, request.Password)))
            {
                return BadRequest("Invalid credentials");
            }

            var accessToken = _tokenService.CreateToken(user);

            return Ok(new AuthResponse
            {
                Username = user.UserName,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = user.RefreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid client request");
            }

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(tokenModel.Email);

            // Validate the user and the refresh token
            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest("Invalid refresh token or token expired");
            }

            // Generate new tokens
            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update user's refresh token and its expiry time
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(TokenService.RefreshTokenExpirationDays);

            // Persist changes to the database
            await _userManager.UpdateAsync(user);

            // Return new tokens to the client
            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

    }
}
