using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ApiProject.Dtos.Account;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

       
        public AccountController(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager, EmailService emailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var allowedClientRoles = new[] { "User", "Seller" };
            var requestedClientRole = (registerDto.Role ?? "User").Trim();
            if (!allowedClientRoles.Contains(requestedClientRole))
            {
                return BadRequest("Invalid role requested.");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Address = registerDto.Address,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var identityRole = requestedClientRole == "Seller" ? "Seller" : "User";
            var roleAddResult = await _userManager.AddToRoleAsync(user, identityRole);
            
            if (!roleAddResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(roleAddResult.Errors);
            }

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || user.IsDeleted) return Unauthorized(new { message = "EmailNotFound" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized(new { message = "InvalidPassword" });
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Token = await _tokenService.CreateToken(user, loginDto.RememberMe),
                Email = user.Email,
                FullName = user.FullName,
                Role = roles,
                RememberMe = loginDto.RememberMe
            });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = $"http://localhost:4200/reset-password?email={model.Email}&token={Uri.EscapeDataString(token)}";

            var body = $"<p>Click here to reset your password:</p><a href='{link}'>Reset Password</a>";

            await _emailService.SendEmailAsync(model.Email, "Reset Password", body);

            return Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest();
            var decodedToken = Uri.UnescapeDataString(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }
    }
}
