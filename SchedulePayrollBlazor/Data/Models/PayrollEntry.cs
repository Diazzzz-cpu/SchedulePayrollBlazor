using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payroll_entries")]
public class PayrollEntry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("payroll_period_id")]
    public int PayrollPeriodId { get; set; }

    [ForeignKey(nameof(PayrollPeriodId))]
    public PayrollPeriod PayrollPeriod { get; set; } = default!;

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = default!;

    [Column("total_hours_worked", TypeName = "decimal(18,2)")]
    public decimal TotalHoursWorked { get; set; }

    [Column("base_pay", TypeName = "decimal(18,2)")]
    public decimal BasePay { get; set; }

    [Column("total_deductions", TypeName = "decimal(18,2)")]
    public decimal TotalDeductions { get; set; }

    [Column("total_bonuses", TypeName = "decimal(18,2)")]
    public decimal TotalBonuses { get; set; }

    [Column("net_pay", TypeName = "decimal(18,2)")]
    public decimal NetPay { get; set; }

    [Column("calculated_at")]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
    public ICollection<PayrollAdjustment> Adjustments { get; set; } = new List<PayrollAdjustment>();
}
