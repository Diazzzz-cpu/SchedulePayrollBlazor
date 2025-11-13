namespace SchedulePayrollBlazor.Data.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Employee";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee? Employee { get; set; }
}
