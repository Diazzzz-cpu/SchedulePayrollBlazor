using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ===== DB SETS =====
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== ROLE TABLE (role) =====
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");

            entity.HasKey(r => r.RoleId);

            entity.Property(r => r.RoleId)
                  .HasColumnName("role_id");

            entity.Property(r => r.RoleName)
                  .HasColumnName("role_name")
                  .HasMaxLength(100)
                  .IsRequired();
        });

        // ===== USERS TABLE (users) =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.Email)
                  .HasColumnName("email")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.HasIndex(e => e.Email)
                  .IsUnique();

            entity.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired();

            entity.Property(e => e.FirstName)
                  .HasColumnName("first_name")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.LastName)
                  .HasColumnName("last_name")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.RoleId)
                  .HasColumnName("role_id")
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // (optional) FK to role if you want EF to know about it
            entity.HasOne<Role>()
                  .WithMany()
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== EMPLOYEE TABLE (employee) =====
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee");

            entity.HasKey(e => e.EmployeeId);

            entity.Property(e => e.EmployeeId)
                  .HasColumnName("employee_id");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Department).HasMaxLength(120);
            entity.Property(e => e.JobTitle).HasMaxLength(160);
            entity.Property(e => e.EmploymentType)
                  .HasMaxLength(60)
                  .IsRequired()
                  .HasDefaultValue("FullTime");
            entity.Property(e => e.Location).HasMaxLength(160);
            entity.Property(e => e.StartDate)
                  .HasConversion(
                      v => v,
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== SCHEDULE TABLE (schedule) =====
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule");

            // use your real PK property here
            entity.HasKey(s => s.ScheduleId);

            entity.Property(s => s.ScheduleId)
                  .HasColumnName("schedule_id");

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

            entity.Property(e => e.EmployeeId)
                  .HasColumnName("employee_id");

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Schedules)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        // ===== PAYPERIOD TABLE (payperiod) =====
        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.ToTable("payperiod");

            entity.HasKey(p => p.PayPeriodId);

            entity.Property(p => p.PayPeriodId)
                  .HasColumnName("payperiod_id");

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


        // ===== PAYROLLRUN TABLE (payrollrun) =====
        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("payrollrun");

            entity.HasKey(p => p.PayrollRunId);

            entity.Property(p => p.PayrollRunId)
                  .HasColumnName("payrollrun_id");

            entity.Property(e => e.Status).HasMaxLength(40).HasDefaultValue("Pending");
            entity.Property(e => e.GrossPay).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.NetPay).HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.EmployeeId)
                  .HasColumnName("employee_id");

            entity.Property(e => e.PayPeriodId)
                  .HasColumnName("payperiod_id");

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