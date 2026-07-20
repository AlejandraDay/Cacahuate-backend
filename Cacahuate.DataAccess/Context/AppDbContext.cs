using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Therapist> Therapists => Set<Therapist>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TherapistAvailability> TherapistAvailabilities => Set<TherapistAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TherapistRating> TherapistRatings => Set<TherapistRating>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormAssignment> FormAssignments => Set<FormAssignment>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormAnswer> FormAnswers => Set<FormAnswer>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Therapist>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.User)
                .WithOne(u => u.Therapist)
                .HasForeignKey<Therapist>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Parent>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Parent)
                .WithMany(par => par.Patients)
                .HasForeignKey(p => p.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TherapistAvailability>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Therapist)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasConversion<string>();
            e.HasOne(a => a.Therapist)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Parent)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Availability)
                .WithMany(av => av.Appointments)
                .HasForeignKey(a => a.AvailabilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TherapistRating>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.AppointmentId).IsUnique();
            e.HasOne(r => r.Appointment)
                .WithOne(a => a.Rating)
                .HasForeignKey<TherapistRating>(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Therapist)
                .WithMany(t => t.Ratings)
                .HasForeignKey(r => r.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Parent)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FormTemplate>(e => e.HasKey(t => t.Id));

        modelBuilder.Entity<FormField>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Type).HasConversion<string>();
            e.HasOne(f => f.FormTemplate)
                .WithMany(t => t.Fields)
                .HasForeignKey(f => f.FormTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FormAssignment>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.FormTemplate)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.FormTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FormSubmission>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.FormAssignmentId, s.AppointmentId }).IsUnique();
            e.HasOne(s => s.FormAssignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.FormAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Appointment)
                .WithMany(a => a.FormSubmissions)
                .HasForeignKey(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Therapist)
                .WithMany()
                .HasForeignKey(s => s.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FormAnswer>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.FormSubmission)
                .WithMany(s => s.Answers)
                .HasForeignKey(a => a.FormSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.FormField)
                .WithMany(f => f.Answers)
                .HasForeignKey(a => a.FormFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
