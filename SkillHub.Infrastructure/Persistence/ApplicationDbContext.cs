using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Instructor>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.FullName).IsRequired().HasMaxLength(150);
            entity.Property(i => i.Email).IsRequired().HasMaxLength(256);
            entity.Property(i => i.Specialty).IsRequired().HasMaxLength(150);
            entity.HasIndex(i => i.Email).IsUnique();
        });

        builder.Entity<Workshop>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Code).IsRequired().HasMaxLength(30);
            entity.Property(w => w.Title).IsRequired().HasMaxLength(150);
            entity.Property(w => w.Description).IsRequired();
            entity.HasIndex(w => w.Code).IsUnique();

            entity.HasOne(w => w.Instructor)
                .WithMany(i => i.Workshops)
                .HasForeignKey(w => w.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParticipantId).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.Enrollments)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ParticipantId, e.WorkshopId });
        });
    }
}
