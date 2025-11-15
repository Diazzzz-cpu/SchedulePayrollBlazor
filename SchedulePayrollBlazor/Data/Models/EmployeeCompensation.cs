using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("employee_compensation")]
public class EmployeeCompensation
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = default!;

    [Column("is_hourly")]
    public bool IsHourly { get; set; }

    [Column("hourly_rate", TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }

    [Column("fixed_monthly_salary", TypeName = "decimal(18,2)")]
    public decimal? FixedMonthlySalary { get; set; }
}
