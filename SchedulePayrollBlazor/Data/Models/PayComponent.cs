using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("pay_components")]
public class PayComponent
{
    [Key]
    [Column("pay_component_id")]
    public int PayComponentId { get; set; }

    [Required]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("kind")]
    public string Kind { get; set; } = string.Empty;

    [Column("default_rate")]
    public decimal? DefaultRate { get; set; }

    public ICollection<EmployeeComponent> EmployeeComponents { get; set; } = new List<EmployeeComponent>();
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
