using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Interfaces;

public interface IWorkshopService
{
    Task<IReadOnlyList<WorkshopResponse>> GetUpcomingAsync();
    Task<WorkshopResponse?> GetByIdAsync(int id);
    Task<WorkshopResponse> CreateAsync(CreateWorkshopRequest request);
}