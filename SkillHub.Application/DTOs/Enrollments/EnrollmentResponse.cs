namespace SkillHub.Application.DTOs.Enrollments;

public class EnrollmentResponse
{
    public int Id { get; set; }
    public int WorkshopId { get; set; }
    public string WorkshopTitle { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledByUserId { get; set; }
}
