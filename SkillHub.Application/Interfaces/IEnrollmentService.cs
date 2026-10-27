
using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponse> CreateAsync(
        string participantId,
        int workshopId);

    Task<IReadOnlyList<EnrollmentResponse>> GetMineAsync(
        string participantId);

    Task<EnrollmentResponse> CancelAsync(
        string participantId,
        int enrollmentId);
}
