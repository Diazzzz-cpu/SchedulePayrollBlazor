using System;
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
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email)
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.LastName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.PasswordHash)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleName)
                  .HasMaxLength(100)
                  .IsRequired();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee");
            entity.HasKey(e => e.EmployeeId);

            entity.HasIndex(e => e.UserId)
                  .IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(160);
            entity.Property(e => e.JobTitle).HasMaxLength(160);
            entity.Property(e => e.EmploymentType)
                  .HasMaxLength(60)
                  .HasDefaultValue("FullTime");
            entity.Property(e => e.Location).HasMaxLength(160);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.StartDate)
                  .HasConversion(
                      v => v,
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                  .HasColumnType("date");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule");
            entity.HasKey(e => e.ScheduleId);

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

            entity.Property(e => e.Source)
                  .HasMaxLength(60)
                  .HasDefaultValue("Manual");

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Schedules)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.ToTable("payperiod");
            entity.HasKey(p => p.PayPeriodId);

            entity.Property(p => p.PeriodName)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(p => p.Status)
                  .HasMaxLength(40)
                  .HasDefaultValue("Open");

            entity.Property(p => p.StartDate)
                  .HasConversion(
                      v => v.ToDateTime(TimeOnly.MinValue),
                      v => DateOnly.FromDateTime(v))
                  .HasColumnType("date");

            entity.Property(p => p.EndDate)
                  .HasConversion(
                      v => v.ToDateTime(TimeOnly.MinValue),
                      v => DateOnly.FromDateTime(v))
                  .HasColumnType("date");
        });

        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("payrollrun");
            entity.HasKey(p => p.PayrollRunId);

            entity.Property(p => p.Status)
                  .HasMaxLength(40)
                  .HasDefaultValue("Pending");

            entity.Property(p => p.GrossPay)
                  .HasColumnType("decimal(12,2)")
                  .HasDefaultValue(0);

            entity.Property(p => p.TotalDeductions)
                  .HasColumnType("decimal(12,2)")
                  .HasDefaultValue(0);

            entity.Property(p => p.NetPay)
                  .HasColumnType("decimal(12,2)")
                  .HasDefaultValue(0);

            entity.Property(p => p.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.PayrollRuns)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PayPeriod)
                  .WithMany(pp => pp.PayrollRuns)
                  .HasForeignKey(p => p.PayPeriodId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
