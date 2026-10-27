namespace SkillHub.Domain.Entities;

public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
}
