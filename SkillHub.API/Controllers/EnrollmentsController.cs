
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize(Roles = "Participant")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEnrollmentRequest request)
    {
        var participantId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(participantId))
        {
            return Unauthorized(new
            {
                error = "No se pudo identificar al participante."
            });
        }

        try
        {
            var enrollment = await _enrollmentService.CreateAsync(
                participantId,
                request.WorkshopId);

            return StatusCode(
                StatusCodes.Status201Created,
                enrollment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var participantId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(participantId))
        {
            return Unauthorized(new
            {
                error = "No se pudo identificar al participante."
            });
        }

        var enrollments = await _enrollmentService.GetMineAsync(
            participantId);

        return Ok(enrollments);
    }

    [HttpDelete("{enrollmentId:int}")]
    public async Task<IActionResult> Cancel(int enrollmentId)
    {
        var participantId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(participantId))
        {
            return Unauthorized(new
            {
                error = "No se pudo identificar al participante."
            });
        }

        try
        {
            var enrollment = await _enrollmentService.CancelAsync(
                participantId,
                enrollmentId);

            return Ok(enrollment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
