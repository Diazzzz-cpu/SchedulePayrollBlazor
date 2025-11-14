using System;
using System.ComponentModel.DataAnnotations;

namespace SchedulePayrollBlazor.Services.Models;

public class AdminUserFormModel
{
    public int? UserId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";

    [MaxLength(160)]
    public string? Department { get; set; }
        = string.Empty;

    [MaxLength(160)]
    public string? JobTitle { get; set; }
        = string.Empty;

    [MaxLength(60)]
    public string EmploymentType { get; set; } = "FullTime";

    public DateTime? StartDate { get; set; }
        = null;

    [MaxLength(160)]
    public string? Location { get; set; }
        = string.Empty;

    public string? Password { get; set; }
        = string.Empty;

    public bool IsEmployeeRole => !string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
}

public class AdminUserSummary
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? EmploymentType { get; set; }
    public DateTime? StartDate { get; set; }
    public string? Location { get; set; }
}
