using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<PayComponent> PayComponents => Set<PayComponent>();
    public DbSet<EmployeeComponent> EmployeeComponents => Set<EmployeeComponent>();
    public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<EmployeeCompensation> EmployeeCompensations => Set<EmployeeCompensation>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<AttendancePenaltySettings> AttendancePenaltySettings => Set<AttendancePenaltySettings>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<RolePayComponent> RolePayComponents => Set<RolePayComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ROLES
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(r => r.RoleId);

            entity.Property(r => r.RoleId).HasColumnName("role_id");
            entity.Property(r => r.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        });

        // USERS
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);

            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(u => u.RoleId).HasColumnName("role_id").IsRequired();
            entity.Property(u => u.CreatedAt)
      .HasColumnName("created_at");

            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // EMPLOYEES
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.EmployeeId);

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EMPLOYEE PROFILE
        modelBuilder.Entity<EmployeeProfile>(entity =>
        {
            entity.ToTable("employee_profiles");
            entity.HasKey(p => p.EmployeeId);

            entity.Property(p => p.EmployeeId).HasColumnName("employee_id");
            entity.Property(p => p.UserId).HasColumnName("user_id");
            entity.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(p => p.BirthDate).HasColumnName("birthdate");
            entity.Property(p => p.Address).HasColumnName("address").HasMaxLength(255);
            entity.Property(p => p.HireDate).HasColumnName("hire_date");
            entity.Property(p => p.Status).HasColumnName("status").HasMaxLength(50);

            entity.HasOne(p => p.Employee)
                  .WithOne(e => e.Profile)
                  .HasForeignKey<EmployeeProfile>(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PAY COMPONENTS
        modelBuilder.Entity<PayComponent>(entity =>
        {
            entity.ToTable("pay_components");
            entity.HasKey(pc => pc.PayComponentId);

            entity.Property(pc => pc.PayComponentId).HasColumnName("pay_component_id");
            entity.Property(pc => pc.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(pc => pc.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(pc => pc.ComponentType).HasColumnName("kind").HasMaxLength(50).IsRequired();
            entity.Property(pc => pc.CalculationType).HasColumnName("calculation_type").HasMaxLength(50);
            entity.Property(pc => pc.DefaultAmount).HasColumnName("default_rate").HasColumnType("decimal(18,2)");
            entity.Property(pc => pc.IsActive).HasColumnName("is_active");
        });

        // ROLE PAY COMPONENTS
        modelBuilder.Entity<RolePayComponent>(entity =>
        {
            entity.ToTable("role_pay_components");
            entity.HasKey(rp => rp.RolePayComponentId);

            entity.Property(rp => rp.RolePayComponentId).HasColumnName("role_pay_component_id");
            entity.Property(rp => rp.RoleId).HasColumnName("role_id");
            entity.Property(rp => rp.PayComponentId).HasColumnName("pay_component_id");
            entity.Property(rp => rp.DefaultRateOverride).HasColumnName("default_rate_override").HasColumnType("decimal(18,2)");
            entity.Property(rp => rp.IsActive).HasColumnName("is_active");

            entity.HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.PayComponent)
                .WithMany()
                .HasForeignKey(rp => rp.PayComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EMPLOYEE COMPONENTS
        modelBuilder.Entity<EmployeeComponent>(entity =>
        {
            entity.ToTable("employee_components");
            entity.HasKey(ec => ec.EmployeeComponentId);

            entity.Property(ec => ec.EmployeeComponentId).HasColumnName("employee_component_id");
            entity.Property(ec => ec.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(ec => ec.PayComponentId).HasColumnName("pay_component_id").IsRequired();
            entity.Property(ec => ec.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(ec => ec.Active).HasColumnName("active").IsRequired();

            entity.HasOne(ec => ec.Employee)
                  .WithMany(e => e.EmployeeComponents)
                  .HasForeignKey(ec => ec.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ec => ec.PayComponent)
                  .WithMany(pc => pc.EmployeeComponents)
                  .HasForeignKey(ec => ec.PayComponentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeCompensation>(entity =>
        {
            entity.ToTable("employee_compensation");
            entity.HasKey(ec => ec.EmployeeId);

            entity.Property(ec => ec.EmployeeId).HasColumnName("employee_id");
            entity.Property(ec => ec.IsHourly).HasColumnName("is_hourly");
            entity.Property(ec => ec.HourlyRate).HasColumnName("hourly_rate").HasColumnType("decimal(18,2)");
            entity.Property(ec => ec.FixedMonthlySalary).HasColumnName("fixed_monthly_salary").HasColumnType("decimal(18,2)");

            entity.HasOne(ec => ec.Employee)
                  .WithOne(e => e.Compensation)
                  .HasForeignKey<EmployeeCompensation>(ec => ec.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PAY PERIODS
        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.ToTable("pay_periods");
            entity.HasKey(pp => pp.PayPeriodId);

            entity.Property(pp => pp.PayPeriodId).HasColumnName("pay_period_id");
            entity.Property(pp => pp.PeriodName).HasColumnName("period_name").HasMaxLength(100).IsRequired();
            entity.Property(pp => pp.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(pp => pp.EndDate).HasColumnName("end_date").IsRequired();
            entity.Property(pp => pp.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        });

        // PAYROLL RUNS
        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("payroll_runs");
            entity.HasKey(pr => pr.PayrollRunId);

            entity.Property(pr => pr.PayrollRunId).HasColumnName("payroll_run_id");
            entity.Property(pr => pr.PayPeriodId).HasColumnName("pay_period_id").IsRequired();
            entity.Property(pr => pr.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(pr => pr.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(pr => pr.GrossPay).HasColumnName("gross_pay").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(pr => pr.TotalDeductions).HasColumnName("total_deductions").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(pr => pr.NetPay).HasColumnName("net_pay").HasColumnType("decimal(18,2)").IsRequired();

            entity.HasOne(pr => pr.PayPeriod)
                  .WithMany(pp => pp.PayrollRuns)
                  .HasForeignKey(pr => pr.PayPeriodId);

            entity.HasOne(pr => pr.Employee)
                  .WithMany(e => e.PayrollRuns)
                  .HasForeignKey(pr => pr.EmployeeId);
        });

        // PAYROLL LINES
        modelBuilder.Entity<PayrollLine>(entity =>
        {
            entity.ToTable("payroll_lines");
            entity.HasKey(pl => pl.PayrollLineId);

            entity.Property(pl => pl.PayrollLineId).HasColumnName("payroll_line_id");
            entity.Property(pl => pl.PayrollRunId).HasColumnName("payroll_run_id").IsRequired();
            entity.Property(pl => pl.PayComponentId).HasColumnName("pay_component_id").IsRequired();
            entity.Property(pl => pl.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(pl => pl.Rate).HasColumnName("rate").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(pl => pl.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)").IsRequired();

            entity.HasOne(pl => pl.PayrollRun)
                  .WithMany(pr => pr.PayrollLines)
                  .HasForeignKey(pl => pl.PayrollRunId);

            entity.HasOne(pl => pl.PayComponent)
                  .WithMany(pc => pc.PayrollLines)
                  .HasForeignKey(pl => pl.PayComponentId);
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("payroll_periods");
            entity.HasKey(pp => pp.Id);

            entity.Property(pp => pp.Id).HasColumnName("id");
            entity.Property(pp => pp.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(pp => pp.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(pp => pp.EndDate).HasColumnName("end_date").IsRequired();
            entity.Property(pp => pp.CreatedAt).HasColumnName("created_at").IsRequired();
        });

        modelBuilder.Entity<PayrollEntry>(entity =>
        {
            entity.ToTable("payroll_entries");
            entity.HasKey(pe => pe.Id);

            entity.Property(pe => pe.Id).HasColumnName("id");
            entity.Property(pe => pe.PayrollPeriodId).HasColumnName("payroll_period_id");
            entity.Property(pe => pe.EmployeeId).HasColumnName("employee_id");
            entity.Property(pe => pe.TotalHoursWorked).HasColumnName("total_hours_worked").HasColumnType("decimal(18,2)");
            entity.Property(pe => pe.BasePay).HasColumnName("base_pay").HasColumnType("decimal(18,2)");
            entity.Property(pe => pe.TotalDeductions).HasColumnName("total_deductions").HasColumnType("decimal(18,2)");
            entity.Property(pe => pe.TotalBonuses).HasColumnName("total_bonuses").HasColumnType("decimal(18,2)");
            entity.Property(pe => pe.NetPay).HasColumnName("net_pay").HasColumnType("decimal(18,2)");
            entity.Property(pe => pe.CalculatedAt).HasColumnName("calculated_at").IsRequired();

            entity.HasOne(pe => pe.PayrollPeriod)
                  .WithMany(pp => pp.Entries)
                  .HasForeignKey(pe => pe.PayrollPeriodId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pe => pe.Employee)
                  .WithMany(e => e.PayrollEntries)
                  .HasForeignKey(pe => pe.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayrollAdjustment>(entity =>
        {
            entity.ToTable("payroll_adjustments");
            entity.HasKey(pa => pa.Id);

            entity.Property(pa => pa.Id).HasColumnName("id");
            entity.Property(pa => pa.PayrollEntryId).HasColumnName("payroll_entry_id");
            entity.Property(pa => pa.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            entity.Property(pa => pa.Label).HasColumnName("label").HasMaxLength(200).IsRequired();
            entity.Property(pa => pa.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(pa => pa.Source).HasColumnName("source").HasMaxLength(100);
            entity.Property(pa => pa.IsAutoGenerated).HasColumnName("is_auto_generated").IsRequired();

            entity.HasOne(pa => pa.PayrollEntry)
                  .WithMany(pe => pe.Adjustments)
                  .HasForeignKey(pa => pa.PayrollEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttendancePenaltySettings>(entity =>
        {
            entity.ToTable("attendance_penalty_settings");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.LatePenaltyPerMinute).HasColumnName("late_penalty_per_minute").HasColumnType("decimal(18,4)");
            entity.Property(a => a.UndertimePenaltyPerMinute).HasColumnName("undertime_penalty_per_minute").HasColumnType("decimal(18,4)");
            entity.Property(a => a.AbsenceFullDayMultiplier).HasColumnName("absence_full_day_multiplier").HasColumnType("decimal(18,4)");
            entity.Property(a => a.OvertimeBonusPerMinute).HasColumnName("overtime_bonus_per_minute").HasColumnType("decimal(18,4)");
            entity.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        });

        // SCHEDULES
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedules");
            entity.HasKey(s => s.ScheduleId);

            entity.Property(s => s.ScheduleId).HasColumnName("schedule_id");
            entity.Property(s => s.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(s => s.ShiftDate).HasColumnName("shift_date").IsRequired();
            entity.Property(s => s.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(s => s.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(s => s.Source).HasColumnName("source").HasMaxLength(50);

            entity.HasOne(s => s.Employee)
                  .WithMany(e => e.Schedules)
                  .HasForeignKey(s => s.EmployeeId);
        });

        // TIME LOGS
        modelBuilder.Entity<TimeLog>(entity =>
        {
            entity.ToTable("time_logs");
            entity.HasKey(t => t.TimeLogId);

            entity.Property(t => t.TimeLogId).HasColumnName("time_log_id");
            entity.Property(t => t.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(t => t.ClockIn).HasColumnName("clock_in").IsRequired();
            entity.Property(t => t.ClockOut).HasColumnName("clock_out");
            entity.Property(t => t.Source).HasColumnName("source").HasMaxLength(50);

            entity.HasOne(t => t.Employee)
                  .WithMany(e => e.TimeLogs)
                  .HasForeignKey(t => t.EmployeeId);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("shifts");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id).HasColumnName("shift_id");
            entity.Property(s => s.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(s => s.EmployeeName).HasColumnName("employee_name").HasMaxLength(200).IsRequired();
            entity.Property(s => s.Start).HasColumnName("start_time").IsRequired();
            entity.Property(s => s.End).HasColumnName("end_time").IsRequired();
            entity.Property(s => s.GroupName).HasColumnName("group_name").HasMaxLength(150);
        });
    }
}
