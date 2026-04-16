using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ApiProject.Models;
using ApiProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ApiProject.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        [HttpPut("users/{id}/block")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);
            return Ok(new { Message = "User restricted successfully.", User = user });
        }

        [HttpPut("users/{id}/reactivate")]
        public async Task<IActionResult> ReactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            user.IsDeleted = false;
            await _userManager.UpdateAsync(user);
            return Ok(new { Message = "User reactivated successfully.", User = user });
        }

        [HttpPost("users/admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Address = registerDto.Address,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var roleAddResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleAddResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return BadRequest(roleAddResult.Errors);
                }
                return Ok(new { Message = "Admin created successfully." });
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            var allowedRoles = new[] { "User", "Seller" };
            if (!allowedRoles.Contains(updateRoleDto.Role))
            {
                return BadRequest("Invalid role. Must be 'User' or 'Seller'.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Where(r => r == "User" || r == "Seller").ToList();
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            var result = await _userManager.AddToRoleAsync(user, updateRoleDto.Role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { Message = $"User role updated to {updateRoleDto.Role}." });
        }
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
