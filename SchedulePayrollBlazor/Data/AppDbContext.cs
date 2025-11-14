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

        // ===== USERS TABLE =====
        modelBuilder.Entity<User>(entity =>
        {
            // physical table name in MySQL
            entity.ToTable("users");

            // primary key
            entity.HasKey(e => e.UserId);

            // columns mapping to your SQL
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
        });

        // ===== EMPLOYEE =====
        modelBuilder.Entity<Employee>(entity =>
        {
            // actual table name
            entity.ToTable("employee");

            // PK in C# + DB column name
            entity.HasKey(e => e.EmployeeId);

            entity.Property(e => e.EmployeeId)
                  .HasColumnName("employee_id");   // <-- PK column in MySQL

            // FK to users.user_id (1–1 relationship)
            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");       // <-- IMPORTANT: map to real column name

            entity.HasIndex(e => e.UserId).IsUnique();

            // these name/HR fields actually live elsewhere or don't exist in the
            // original employee table, so we tell EF NOT to expect columns for them
            entity.Ignore(e => e.FirstName);
            entity.Ignore(e => e.LastName);
            entity.Ignore(e => e.Department);
            entity.Ignore(e => e.JobTitle);
            entity.Ignore(e => e.EmploymentType);
            entity.Ignore(e => e.Location);
            entity.Ignore(e => e.StartDate);
            entity.Ignore(e => e.IsActive);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });




        // ===== SCHEDULE =====
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule");   // 

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

        // ===== PAY PERIOD =====
        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.ToTable("payperiod");  // 

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

        // ===== PAYROLL RUN =====
        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("payrollrun"); // 

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
