using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("schedule")]
public class Schedule
{
    [Key]
    [Column("schedule_id")]
    public int ScheduleId { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("shift_date")]
    public DateOnly ShiftDate { get; set; }

    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly EndTime { get; set; }

    [Column("source")]
    public string Source { get; set; } = "Manual";

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}
