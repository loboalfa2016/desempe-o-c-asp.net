
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Persistence;

namespace SkillHub.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentResponse> CreateAsync(
        string participantId,
        int workshopId)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            throw new ArgumentException(
                "El participante no es válido.");
        }

        if (workshopId <= 0)
        {
            throw new ArgumentException(
                "El identificador del taller no es válido.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        var now = DateTime.UtcNow;

        var workshop = await _context.Workshops
            .FirstOrDefaultAsync(w => w.Id == workshopId);

        if (workshop is null)
        {
            throw new KeyNotFoundException(
                "No se encontró el taller indicado.");
        }

        if (workshop.StartDateTime <= now)
        {
            throw new InvalidOperationException(
                "No puedes inscribirte en un taller que ya comenzó.");
        }

        var hasActiveEnrollment = await _context.Enrollments
            .AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.WorkshopId == workshopId &&
                e.Status == EnrollmentStatus.Active);

        if (hasActiveEnrollment)
        {
            throw new InvalidOperationException(
                "Ya tienes una inscripción activa en este taller.");
        }

        var activeEnrollments = await _context.Enrollments
            .CountAsync(e =>
                e.WorkshopId == workshopId &&
                e.Status == EnrollmentStatus.Active);

        if (activeEnrollments >= workshop.Capacity)
        {
            throw new InvalidOperationException(
                "El taller no tiene cupos disponibles.");
        }

        var hasScheduleConflict = await _context.Enrollments
            .AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.Status == EnrollmentStatus.Active &&
                e.WorkshopId != workshopId &&
                e.Workshop.StartDateTime < workshop.EndDateTime &&
                e.Workshop.EndDateTime > workshop.StartDateTime);

        if (hasScheduleConflict)
        {
            throw new InvalidOperationException(
                "Ya tienes otra inscripción activa que coincide con el horario de este taller.");
        }

        var enrollment = new Enrollment
        {
            ParticipantId = participantId,
            WorkshopId = workshopId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = now
        };

        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        enrollment.Workshop = workshop;

        return MapToResponse(enrollment);
    }

    public async Task<IReadOnlyList<EnrollmentResponse>> GetMineAsync(
        string participantId)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            throw new ArgumentException(
                "El participante no es válido.");
        }

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Workshop)
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(MapToResponse).ToList();
    }

    public async Task<EnrollmentResponse> CancelAsync(
        string participantId,
        int enrollmentId)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            throw new ArgumentException(
                "El participante no es válido.");
        }

        if (enrollmentId <= 0)
        {
            throw new ArgumentException(
                "El identificador de la inscripción no es válido.");
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Workshop)
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.ParticipantId == participantId);

        if (enrollment is null)
        {
            throw new KeyNotFoundException(
                "No se encontró la inscripción indicada.");
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            throw new InvalidOperationException(
                "La inscripción ya está cancelada.");
        }

        if (enrollment.Workshop.StartDateTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "No puedes cancelar la inscripción de un taller que ya comenzó.");
        }

        enrollment.Status = EnrollmentStatus.Cancelled;
        enrollment.CancelledAt = DateTime.UtcNow;
        enrollment.CancelledByUserId = participantId;

        await _context.SaveChangesAsync();

        return MapToResponse(enrollment);
    }

    private static EnrollmentResponse MapToResponse(
        Enrollment enrollment)
    {
        return new EnrollmentResponse
        {
            Id = enrollment.Id,
            WorkshopId = enrollment.WorkshopId,
            WorkshopTitle = enrollment.Workshop.Title,
            StartDateTime = enrollment.Workshop.StartDateTime,
            EndDateTime = enrollment.Workshop.EndDateTime,
            Status = enrollment.Status.ToString(),
            EnrolledAt = enrollment.EnrolledAt,
            CancelledAt = enrollment.CancelledAt,
            CancelledByUserId = enrollment.CancelledByUserId
        };
    }
}
