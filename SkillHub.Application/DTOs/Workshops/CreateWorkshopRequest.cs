
using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Workshops;

public class CreateWorkshopRequest
{
    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int InstructorId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    [Range(1, 10000)]
    public int Capacity { get; set; }
}
