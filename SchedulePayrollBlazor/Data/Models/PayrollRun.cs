using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payrollrun")]
public class PayrollRun
{
    [Key]
    [Column("payrollrun_id")]
    public int PayrollRunId { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("payperiod_id")]
    public int? PayPeriodId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Pending";

    [Column("gross_pay")]
    public decimal GrossPay { get; set; }

    [Column("total_deductions")]
    public decimal TotalDeductions { get; set; }

    [Column("net_pay")]
    public decimal NetPay { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(PayPeriodId))]
    public PayPeriod? PayPeriod { get; set; }
}
