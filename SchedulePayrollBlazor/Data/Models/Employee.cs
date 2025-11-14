using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("employees")]
public class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    // Link to users table
    [Column("user_id")]
    public int UserId { get; set; }

    // Main display name for dashboards/lists
    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("department")]
    public string? Department { get; set; }

    [Column("job_title")]
    public string? JobTitle { get; set; }

    // e.g. "FullTime", "PartTime"
    [Column("employment_type")]
    public string? EmploymentType { get; set; }

    // Nullable so we can safely use HasValue / ??
    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    // Used by AdminUserService, Profile, AdminDashboard
    [Column("location")]
    public string? Location { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation to User
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    // Navigation to EmployeeProfile (used in AppDbContext)
    public EmployeeProfile? Profile { get; set; }

    public ICollection<EmployeeComponent> EmployeeComponents { get; set; } = new List<EmployeeComponent>();
    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}
