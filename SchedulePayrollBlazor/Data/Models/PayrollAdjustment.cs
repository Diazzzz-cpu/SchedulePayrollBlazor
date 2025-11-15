using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payroll_adjustments")]
public class PayrollAdjustment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("payroll_entry_id")]
    public int PayrollEntryId { get; set; }

    [ForeignKey(nameof(PayrollEntryId))]
    public PayrollEntry PayrollEntry { get; set; } = default!;

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
}
