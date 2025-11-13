using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Employeecomponent> Employeecomponents { get; set; }

    public virtual DbSet<Paycomponent> Paycomponents { get; set; }

    public virtual DbSet<Payperiod> Payperiods { get; set; }

    public virtual DbSet<Payrollline> Payrolllines { get; set; }

    public virtual DbSet<Payrollrun> Payrollruns { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Timelog> Timelogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=SchedulePayroll_Demo;user id=blazoruser;password=Demo123!;sslmode=None", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("employee");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "Role_ID");

            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.Active).HasDefaultValueSql("'1'");
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.EmploymentClass)
                .HasDefaultValueSql("'FullTime'")
                .HasColumnType("enum('FullTime','PartTime')")
                .HasColumnName("Employment_Class");
            entity.Property(e => e.EmploymentType)
                .HasDefaultValueSql("'Hourly'")
                .HasColumnType("enum('Hourly','Salaried')")
                .HasColumnName("Employment_Type");
            entity.Property(e => e.HourlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("Hourly_Rate");
            entity.Property(e => e.MonthlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("Monthly_Rate");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employee_ibfk_1");
        });

        modelBuilder.Entity<Employeecomponent>(entity =>
        {
            entity.HasKey(e => e.EmployeeComponentId).HasName("PRIMARY");

            entity.ToTable("employeecomponent");

            entity.HasIndex(e => e.PayComponentId, "PayComponent_ID");

            entity.HasIndex(e => new { e.EmployeeId, e.PayComponentId }, "uq_emp_pc").IsUnique();

            entity.Property(e => e.EmployeeComponentId).HasColumnName("EmployeeComponent_ID");
            entity.Property(e => e.Active).HasDefaultValueSql("'1'");
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.PayComponentId).HasColumnName("PayComponent_ID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Employeecomponents)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("employeecomponent_ibfk_1");

            entity.HasOne(d => d.PayComponent).WithMany(p => p.Employeecomponents)
                .HasForeignKey(d => d.PayComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employeecomponent_ibfk_2");
        });

        modelBuilder.Entity<Paycomponent>(entity =>
        {
            entity.HasKey(e => e.PayComponentId).HasName("PRIMARY");

            entity.ToTable("paycomponent");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.PayComponentId).HasColumnName("PayComponent_ID");
            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.DefaultRate)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("'0.0000'")
                .HasColumnName("Default_Rate");
            entity.Property(e => e.Kind).HasColumnType("enum('Earning','Deduction')");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Payperiod>(entity =>
        {
            entity.HasKey(e => e.PayPeriodId).HasName("PRIMARY");

            entity.ToTable("payperiod");

            entity.HasIndex(e => e.PeriodName, "Period_Name").IsUnique();

            entity.Property(e => e.PayPeriodId).HasColumnName("PayPeriod_ID");
            entity.Property(e => e.EndDate).HasColumnName("End_Date");
            entity.Property(e => e.PeriodName)
                .HasMaxLength(60)
                .HasColumnName("Period_Name");
            entity.Property(e => e.StartDate).HasColumnName("Start_Date");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Open'")
                .HasColumnType("enum('Open','Locked','Paid')");
        });

        modelBuilder.Entity<Payrollline>(entity =>
        {
            entity.HasKey(e => e.PayrollLineId).HasName("PRIMARY");

            entity.ToTable("payrollline");

            entity.HasIndex(e => e.PayComponentId, "PayComponent_ID");

            entity.HasIndex(e => e.PayrollRunId, "idx_line_run");

            entity.Property(e => e.PayrollLineId).HasColumnName("PayrollLine_ID");
            entity.Property(e => e.Amount).HasPrecision(12, 2);
            entity.Property(e => e.PayComponentId).HasColumnName("PayComponent_ID");
            entity.Property(e => e.PayrollRunId).HasColumnName("PayrollRun_ID");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 4)
                .HasDefaultValueSql("'0.0000'");
            entity.Property(e => e.Rate)
                .HasPrecision(12, 6)
                .HasDefaultValueSql("'0.000000'");

            entity.HasOne(d => d.PayComponent).WithMany(p => p.Payrolllines)
                .HasForeignKey(d => d.PayComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payrollline_ibfk_2");

            entity.HasOne(d => d.PayrollRun).WithMany(p => p.Payrolllines)
                .HasForeignKey(d => d.PayrollRunId)
                .HasConstraintName("payrollline_ibfk_1");
        });

        modelBuilder.Entity<Payrollrun>(entity =>
        {
            entity.HasKey(e => e.PayrollRunId).HasName("PRIMARY");

            entity.ToTable("payrollrun");

            entity.HasIndex(e => e.EmployeeId, "Employee_ID");

            entity.HasIndex(e => new { e.PayPeriodId, e.Status }, "idx_run_period");

            entity.HasIndex(e => new { e.PayPeriodId, e.EmployeeId }, "uq_period_emp").IsUnique();

            entity.Property(e => e.PayrollRunId).HasColumnName("PayrollRun_ID");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.GrossPay)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("Gross_Pay");
            entity.Property(e => e.NetPay)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("Net_Pay");
            entity.Property(e => e.PayPeriodId).HasColumnName("PayPeriod_ID");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Calculated','Approved','Paid')");
            entity.Property(e => e.TotalDeductions)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("Total_Deductions");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrollruns)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("payrollrun_ibfk_2");

            entity.HasOne(d => d.PayPeriod).WithMany(p => p.Payrollruns)
                .HasForeignKey(d => d.PayPeriodId)
                .HasConstraintName("payrollrun_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("role");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.Code).HasMaxLength(40);
            entity.Property(e => e.Name).HasMaxLength(80);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PRIMARY");

            entity.ToTable("schedule");

            entity.HasIndex(e => new { e.EmployeeId, e.ShiftDate }, "idx_sched_emp_day");

            entity.HasIndex(e => new { e.EmployeeId, e.ShiftDate, e.StartTime, e.EndTime }, "uq_emp_day_time").IsUnique();

            entity.Property(e => e.ScheduleId).HasColumnName("Schedule_ID");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("End_Time");
            entity.Property(e => e.ShiftDate).HasColumnName("Shift_Date");
            entity.Property(e => e.Source)
                .HasDefaultValueSql("'Manual'")
                .HasColumnType("enum('Manual','Auto')");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("Start_Time");

            entity.HasOne(d => d.Employee).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("schedule_ibfk_1");
        });

        modelBuilder.Entity<Timelog>(entity =>
        {
            entity.HasKey(e => e.TimeLogId).HasName("PRIMARY");

            entity.ToTable("timelog");

            entity.HasIndex(e => new { e.EmployeeId, e.ClockIn }, "idx_timelog_emp_day");

            entity.Property(e => e.TimeLogId).HasColumnName("TimeLog_ID");
            entity.Property(e => e.ClockIn)
                .HasColumnType("datetime")
                .HasColumnName("Clock_In");
            entity.Property(e => e.ClockOut)
                .HasColumnType("datetime")
                .HasColumnName("Clock_Out");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");
            entity.Property(e => e.Source)
                .HasDefaultValueSql("'Web'")
                .HasColumnType("enum('Web','Kiosk','Admin')");

            entity.HasOne(d => d.Employee).WithMany(p => p.Timelogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("timelog_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
