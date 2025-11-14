using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payroll_lines")]
public class PayrollLine
{
    [Key]
    [Column("payroll_line_id")]
    public int PayrollLineId { get; set; }

    [Required]
    [Column("payroll_run_id")]
    public int PayrollRunId { get; set; }

    [Required]
    [Column("pay_component_id")]
    public int PayComponentId { get; set; }

    [Required]
    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Required]
    [Column("rate")]
    public decimal Rate { get; set; }

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [ForeignKey(nameof(PayrollRunId))]
    public PayrollRun? PayrollRun { get; set; }

    [ForeignKey(nameof(PayComponentId))]
    public PayComponent? PayComponent { get; set; }
}
