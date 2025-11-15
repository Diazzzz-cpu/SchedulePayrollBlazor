using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("shifts")]
public class Shift
{
    [Key]
    [Column("shift_id")]
    public int Id { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("start_time")]
    public DateTime Start { get; set; }

    [Column("end_time")]
    public DateTime End { get; set; }

    [Column("group_name")]
    public string? GroupName { get; set; }
}
