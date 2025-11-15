using System;
using System.ComponentModel.DataAnnotations;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Components.Pages.Models;

public class EmployeeEditModel
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; }
        = true;

    [DataType(DataType.Password)]
    [StringLength(100)]
    public string? NewPassword { get; set; }
        = null;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
        = null;
}
