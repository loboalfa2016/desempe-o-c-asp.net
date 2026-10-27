

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public class WorkshopsController : ControllerBase
{
    private readonly IWorkshopService _workshopService;

    public WorkshopsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUpcoming()
    {
        var workshops = await _workshopService.GetUpcomingAsync();

        return Ok(workshops);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                error = "El identificador del taller debe ser mayor que cero."
            });
        }

        var workshop = await _workshopService.GetByIdAsync(id);

        if (workshop is null)
        {
            return NotFound(new
            {
                error = "No se encontró un taller futuro con ese identificador."
            });
        }

        return Ok(workshop);
    }

    [Authorize(Roles = "Employee")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkshopRequest request)
    {
        try
        {
            var workshop = await _workshopService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = workshop.Id },
                workshop);
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
