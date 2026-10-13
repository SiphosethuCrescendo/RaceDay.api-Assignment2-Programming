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
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // Helper to get the current user's ID from the JWT token
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

        // GET: /api/events  (public)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            List<Event> events = await _eventService.GetAllAsync();
            return Ok(events);
        }

        // GET: /api/events/{id}  (public)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            Event foundEvent = await _eventService.GetByIdAsync(id);
            if (foundEvent == null)
            {
                return NotFound(new { message = "Event not found" });
            }
            return Ok(foundEvent);
        }

        // POST: /api/events  (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            try
            {
                Event newEvent = await _eventService.CreateAsync(dto, CurrentUserId);
                return Ok(newEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: /api/events/{id}  (Organiser owner only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
        {
            try
            {
                Event updated = await _eventService.UpdateAsync(id, dto, CurrentUserId);
                if (updated == null)
                {
                    return NotFound(new { message = "Event not found or you do not own it" });
                }
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: /api/events/{id}  (Organiser owner only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _eventService.DeleteAsync(id, CurrentUserId);
            if (deleted == false)
            {
                return NotFound(new { message = "Event not found or you do not own it" });
            }
            return Ok(new { message = "Event has been deleted" });
        }
    }
}