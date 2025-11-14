using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("employee_profiles")]
public class EmployeeProfile
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("birthdate")]
    public DateTime? BirthDate { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("hire_date")]
    public DateTime? HireDate { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
