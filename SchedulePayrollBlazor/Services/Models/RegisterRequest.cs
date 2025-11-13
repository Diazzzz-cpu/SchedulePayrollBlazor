using System;
using System.ComponentModel.DataAnnotations;

namespace SchedulePayrollBlazor.Services.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("Admin|Employee", ErrorMessage = "Role must be Admin or Employee.")]
    public string Role { get; set; } = "Employee";

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public string EmploymentType { get; set; } = "FullTime";

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string Location { get; set; } = string.Empty;
}
