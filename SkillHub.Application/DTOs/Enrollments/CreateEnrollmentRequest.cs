using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Enrollments;

public class CreateEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int WorkshopId { get; set; }
}
