using SkillHub.Domain.Enums;

namespace SkillHub.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public string ParticipantId { get; set; } = string.Empty;
    public int WorkshopId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancelledByUserId { get; set; }

    public Workshop Workshop { get; set; } = null!;
}
