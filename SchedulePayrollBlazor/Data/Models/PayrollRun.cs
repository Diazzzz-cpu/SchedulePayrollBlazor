using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payroll_runs")]
public class PayrollRun
{
    [Key]
    [Column("payroll_run_id")]
    public int PayrollRunId { get; set; }

    [Required]
    [Column("pay_period_id")]
    public int PayPeriodId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("gross_pay")]
    public decimal GrossPay { get; set; }

    [Column("total_deductions")]
    public decimal TotalDeductions { get; set; }

    [Column("net_pay")]
    public decimal NetPay { get; set; }

    // This is what the UI expects
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PayPeriodId))]
    public PayPeriod? PayPeriod { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
