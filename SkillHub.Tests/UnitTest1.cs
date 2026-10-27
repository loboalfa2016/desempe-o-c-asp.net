
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Tests;

public class EnrollmentTests
{
    [Fact]
    public void NewEnrollment_ShouldStartAsActive()
    {
        var enrollment = new Enrollment();

        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Null(enrollment.CancelledAt);
        Assert.Null(enrollment.CancelledByUserId);
    }

    [Fact]
    public void NewEnrollment_ShouldSetEnrolledAtToCurrentUtcTime()
    {
        var beforeCreation = DateTime.UtcNow;

        var enrollment = new Enrollment();

        var afterCreation = DateTime.UtcNow;

        Assert.InRange(
            enrollment.EnrolledAt,
            beforeCreation,
            afterCreation);
    }
}
