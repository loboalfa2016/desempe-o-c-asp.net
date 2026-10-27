
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Persistence;

namespace SkillHub.Infrastructure.Services;

public class WorkshopService : IWorkshopService
{
    private readonly ApplicationDbContext _context;

    public WorkshopService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkshopResponse>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;

        var workshops = await _context.Workshops
            .AsNoTracking()
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .Where(w => w.StartDateTime > now)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync();

        return workshops.Select(MapToResponse).ToList();
    }

    public async Task<WorkshopResponse?> GetByIdAsync(int id)
    {
        var now = DateTime.UtcNow;

        var workshop = await _context.Workshops
            .AsNoTracking()
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w =>
                w.Id == id && w.StartDateTime > now);

        return workshop is null ? null : MapToResponse(workshop);
    }

    public async Task<WorkshopResponse> CreateAsync(
        CreateWorkshopRequest request)
    {
        var now = DateTime.UtcNow;

        if (request.StartDateTime <= now)
        {
            throw new ArgumentException(
                "La fecha de inicio debe ser futura.");
        }

        if (request.EndDateTime <= request.StartDateTime)
        {
            throw new ArgumentException(
                "La fecha de finalización debe ser posterior al inicio.");
        }

        var code = request.Code.Trim();

        var codeExists = await _context.Workshops
            .AnyAsync(w => w.Code == code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Ya existe un taller con ese código.");
        }

        var instructorExists = await _context.Instructors
            .AnyAsync(i => i.Id == request.InstructorId);

        if (!instructorExists)
        {
            throw new ArgumentException(
                "El instructor indicado no existe.");
        }

        var workshop = new Workshop
        {
            Code = code,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            InstructorId = request.InstructorId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            Capacity = request.Capacity
        };

        _context.Workshops.Add(workshop);
        await _context.SaveChangesAsync();

        var createdWorkshop = await _context.Workshops
            .AsNoTracking()
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstAsync(w => w.Id == workshop.Id);

        return MapToResponse(createdWorkshop);
    }

    private static WorkshopResponse MapToResponse(Workshop workshop)
    {
        var activeEnrollments = workshop.Enrollments.Count(
            e => e.Status == EnrollmentStatus.Active);

        return new WorkshopResponse
        {
            Id = workshop.Id,
            Code = workshop.Code,
            Title = workshop.Title,
            Description = workshop.Description,
            InstructorName = workshop.Instructor.FullName,
            StartDateTime = workshop.StartDateTime,
            EndDateTime = workshop.EndDateTime,
            Capacity = workshop.Capacity,
            ActiveEnrollments = activeEnrollments,
            AvailableSeats = Math.Max(
                0, workshop.Capacity - activeEnrollments)
        };
    }
}
