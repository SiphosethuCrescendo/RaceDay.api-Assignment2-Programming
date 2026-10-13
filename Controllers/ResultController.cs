using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
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

        // POST: /api/results  (Organiser owner only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Upload([FromBody] CreateResultDto dto)
        {
            // Find the enrolment
            Enrolment enrolment = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentID);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found" });
            }

            // Check organiser owns the event
            if (enrolment.Category.Event.OrganiserID != CurrentUserId)
            {
                return Forbid();
            }

            // Check a result doesn't already exist
            Result existing = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == dto.EnrolmentID);

            if (existing != null)
            {
                return BadRequest(new { message = "A result already exists for this enrolment" });
            }

            // Create the result
            Result newResult = new Result();
            newResult.EnrolmentID = dto.EnrolmentID;
            newResult.FinishTime = dto.FinishTime;
            newResult.Position = dto.Position;
            newResult.GunTime = dto.GunTime;
            newResult.ChipTime = dto.ChipTime;
            newResult.Status = "Valid";

            _context.Results.Add(newResult);

            // Mark enrolment as Completed
            enrolment.Status = "Completed";

            await _context.SaveChangesAsync();

            return Ok(newResult);
        }

        // GET: /api/results/my  (Participant only)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> MyResults()
        {
            List<Result> results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                        .ThenInclude(c => c.Event)
                .Where(r => r.Enrolment.ParticipantID == CurrentUserId)
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/results/event/{eventId}  (public)
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> EventResults(int eventId)
        {
            List<Result> results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.Category.EventID == eventId)
                .ToListAsync();

            return Ok(results);
        }
    }
}