using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("employee_components")]
public class EmployeeComponent
{
    [Key]
    [Column("employee_component_id")]
    public int EmployeeComponentId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [Column("pay_component_id")]
    public int PayComponentId { get; set; }

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Required]
    [Column("active")]
    public bool Active { get; set; } = true;

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [ForeignKey(nameof(PayComponentId))]
    public PayComponent? PayComponent { get; set; }
}
