using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly IEnrolmentService _enrolmentService;

        public EnrolmentsController(IEnrolmentService enrolmentService)
        {
            _enrolmentService = enrolmentService;
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

        // POST: /api/enrolments  (Participant only)
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            try
            {
                Enrolment enrolment = await _enrolmentService.EnrolAsync(dto, CurrentUserId);
                return Ok(enrolment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /api/enrolments/my  (Participant only)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> MyEnrolments()
        {
            List<Enrolment> enrolments = await _enrolmentService
                .GetParticipantEnrolmentsAsync(CurrentUserId);
            return Ok(enrolments);
        }

        // GET: /api/enrolments/event/{eventId}  (Organiser owner only)
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> EventEnrolments(int eventId)
        {
            try
            {
                List<Enrolment> enrolments = await _enrolmentService
                    .GetEventEnrolmentsAsync(eventId, CurrentUserId);
                return Ok(enrolments);
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }

        // PUT: /api/enrolments/{id}/status  (Organiser owner only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                bool updated = await _enrolmentService
                    .UpdateStatusAsync(id, status, CurrentUserId);
                if (updated == false)
                {
                    return NotFound(new { message = "Enrolment not found" });
                }
                return Ok(new { message = "Status updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}