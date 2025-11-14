using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("time_logs")]
public class TimeLog
{
    [Key]
    [Column("time_log_id")]
    public int TimeLogId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [Column("clock_in")]
    public DateTime ClockIn { get; set; }

    [Column("clock_out")]
    public DateTime? ClockOut { get; set; }

    [Column("source")]
    public string? Source { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
}
