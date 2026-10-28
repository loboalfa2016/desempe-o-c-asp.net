
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Services;

namespace SkillHub.Tests;

public class EnrollmentServiceTests
{
    [Fact]
    public async Task CreateAsync_WithEmptyParticipantId_ThrowsArgumentException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .Options;

        await using var context = new ApplicationDbContext(options);
        var service = new EnrollmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync("", 1));
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesActiveEnrollment()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SKILLHUB_TEST_CONNECTION");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Configura SKILLHUB_TEST_CONNECTION para ejecutar esta prueba.");

        var connectionBuilder =
            new NpgsqlConnectionStringBuilder(connectionString);

        Assert.True(
            string.Equals(
                connectionBuilder.Database,
                "skillhub_test",
                StringComparison.OrdinalIgnoreCase),
            "La prueba solo puede ejecutarse contra skillhub_test.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var instructorEmail = $"t-{Guid.NewGuid():N}@example.com";
        var workshopCode = $"T-{Guid.NewGuid():N}"[..30];
        var participantId = $"test-{Guid.NewGuid():N}";

        int instructorId = 0;
        int workshopId = 0;

        try
        {
            await using (var seedContext =
                new ApplicationDbContext(options))
            {
                var instructor = new Instructor
                {
                    FullName = "Instructor de prueba",
                    Email = instructorEmail,
                    Specialty = "Pruebas automatizadas"
                };

                seedContext.Instructors.Add(instructor);
                await seedContext.SaveChangesAsync();

                instructorId = instructor.Id;

                var now = DateTime.UtcNow;

                var workshop = new Workshop
                {
                    Code = workshopCode,
                    Title = "Taller de prueba automatizada",
                    Description = "Taller temporal para pruebas.",
                    InstructorId = instructorId,
                    StartDateTime = now.AddDays(7),
                    EndDateTime = now.AddDays(7).AddHours(2),
                    Capacity = 5
                };

                seedContext.Workshops.Add(workshop);
                await seedContext.SaveChangesAsync();

                workshopId = workshop.Id;
            }

            await using (var testContext =
                new ApplicationDbContext(options))
            {
                var service = new EnrollmentService(testContext);

                var result = await service.CreateAsync(
                    participantId,
                    workshopId);

                Assert.True(result.Id > 0);
                Assert.Equal(workshopId, result.WorkshopId);
                Assert.Equal("Taller de prueba automatizada", result.WorkshopTitle);
                Assert.Equal("Active", result.Status);
                Assert.Null(result.CancelledAt);
            }
        }
        finally
        {
            await using var cleanupContext =
                new ApplicationDbContext(options);

            if (workshopId > 0)
            {
                var enrollments = await cleanupContext.Enrollments
                    .Where(e => e.WorkshopId == workshopId)
                    .ToListAsync();

                cleanupContext.Enrollments.RemoveRange(enrollments);

                var workshop = await cleanupContext.Workshops
                    .FirstOrDefaultAsync(w => w.Id == workshopId);

                if (workshop is not null)
                {
                    cleanupContext.Workshops.Remove(workshop);
                }
            }

            if (instructorId > 0)
            {
                var instructor = await cleanupContext.Instructors
                    .FirstOrDefaultAsync(i => i.Id == instructorId);

                if (instructor is not null)
                {
                    cleanupContext.Instructors.Remove(instructor);
                }
            }

            await cleanupContext.SaveChangesAsync();
        }
    }
}
