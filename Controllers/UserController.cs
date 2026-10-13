using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static RaceDay.Core.DTOs.UserDtos;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                {
                    return 0;
                }
                return int.Parse(idClaim.Value);
            }
        }

        // GET: /api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            User user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            UserDto dto = new UserDto();
            dto.UserID = user.UserID;
            dto.Email = user.Email;
            dto.FullName = user.FullName;
            dto.PhoneNumber = user.PhoneNumber;
            dto.Role = user.Role;

            return Ok(dto);
        }

        // PUT: /api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            User user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated" });
        }
    }
}