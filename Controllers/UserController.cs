using Microsoft.AspNetCore.Mvc;
using QuizAPI.Data;
using QuizAPI.Validators;
using QuizAPI.Models;
using QuizAPI.Services;
using QuizAPI.Utils;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuizAPI.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly TokenHelper _tokenHelper;
        private readonly IConfiguration _configuration;

        public UserController(
            ApplicationDbContext context,
            EmailService emailService,
            TokenHelper tokenHelper,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _tokenHelper = tokenHelper;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterValidator model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password
            };

            user.HashPassword();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate verification token
            var verificationToken = _tokenHelper.GenerateToken(user.Id.ToString(), "1440"); // 24 hours

            // Send verification email
            var verificationUrl = $"{_configuration["FrontendURL"]}/verify-email/{user.Id}/{verificationToken}";
            await _emailService.SendMailAsync(
                _configuration["Email:From"],
                user.Email,
                "Email Verification",
                "",
                $"Click <a href='{verificationUrl}'>here</a> to verify your email."
            );

            return Ok(new { message = "Registration successful. Please check your email to verify your account." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginValidator model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !user.MatchPassword(model.Password))
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            if (!user.IsVerified)
            {
                return BadRequest(new { message = "Please verify your email first" });
            }

            var tokeng = _tokenHelper.GenerateToken(user.Id.ToString(), "10080"); // 7 days

            return Ok(new
            {
                
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    token = tokeng
                }
            });
        }

        [HttpPost("send-reset-password-email")]
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] SendResetPasswordEmailValidator model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            var resetToken = _tokenHelper.GenerateToken(user.Id.ToString(), "60"); // 1 hour
            var resetUrl = $"{_configuration["FrontendURL"]}/reset-password/{user.Id}/{resetToken}";

            await _emailService.SendMailAsync(
                _configuration["Email:From"],
                user.Email,
                "Password Reset",
                "",
                $"Click <a href='{resetUrl}'>here</a> to reset your password."
            );

            return Ok(new { message = "Password reset email sent" });
        }

        [HttpPost("reset-password/{id}/{token}")]
        public async Task<IActionResult> ResetPassword(string id, string token, [FromBody] ResetPasswordValidator model)
        {
            try
            {
                var claims = _tokenHelper.VerifyToken(token);
                var tokenId = claims.FindFirst("id")?.Value;

                if (tokenId != id)
                {
                    return BadRequest(new { message = "Invalid token" });
                }

                var user = await _context.Users.FindAsync(Guid.Parse(id));
                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                user.Password = model.Password;
                user.HashPassword();
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password reset successful" });
            }
            catch
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }
        }

        [HttpGet("verify-email/{id}/{token}")]
        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            try
            {
                var claims = _tokenHelper.VerifyToken(token);
                var tokenId = claims.FindFirst("id")?.Value;

                if (tokenId != id)
                {
                    return BadRequest(new { message = "Invalid token" });
                }

                var user = await _context.Users.FindAsync(Guid.Parse(id));
                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                var tokeng = _tokenHelper.GenerateToken(user.Id.ToString(), "10080"); // 7 days

                return Ok(new
                {
                    message = "Email verified successfully",
                    user = new
                    {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                        token = tokeng
                    }
                });
                //return Ok(new { message = "Email verified successfully" });
            }
            catch
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }
        }
    }
}
