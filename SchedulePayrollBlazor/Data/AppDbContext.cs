using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(32).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Department).HasMaxLength(120);
            entity.Property(e => e.JobTitle).HasMaxLength(160);
            entity.Property(e => e.EmploymentType).HasMaxLength(60).IsRequired().HasDefaultValue("FullTime");
            entity.Property(e => e.Location).HasMaxLength(160);
            entity.Property(e => e.StartDate).HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.ShiftDate)
                  .HasConversion(
                      v => v.ToDateTime(TimeOnly.MinValue),
                      v => DateOnly.FromDateTime(v))
                  .HasColumnType("date");

            entity.Property(e => e.StartTime)
                  .HasConversion(
                      v => v.ToTimeSpan(),
                      v => TimeOnly.FromTimeSpan(v))
                  .HasColumnType("time(6)");

            entity.Property(e => e.EndTime)
                  .HasConversion(
                      v => v.ToTimeSpan(),
                      v => TimeOnly.FromTimeSpan(v))
                  .HasColumnType("time(6)");

            entity.Property(e => e.Source).HasMaxLength(60).HasDefaultValue("Manual");

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Schedules)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.Property(e => e.PeriodName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(40).HasDefaultValue("Open");
            entity.Property(e => e.StartDate)
                  .HasConversion(
                      v => v.ToDateTime(TimeOnly.MinValue),
                      v => DateOnly.FromDateTime(v))
                  .HasColumnType("date");
            entity.Property(e => e.EndDate)
                  .HasConversion(
                      v => v.ToDateTime(TimeOnly.MinValue),
                      v => DateOnly.FromDateTime(v))
                  .HasColumnType("date");
        });

        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(40).HasDefaultValue("Pending");
            entity.Property(e => e.GrossPay).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.NetPay).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.PayrollRuns)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PayPeriod)
                  .WithMany(p => p.PayrollRuns)
                  .HasForeignKey(e => e.PayPeriodId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
