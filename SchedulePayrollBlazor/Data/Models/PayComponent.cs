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
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("kind")]
    public string ComponentType { get; set; } = "Earning";

    [Column("calculation_type")]
    public string CalculationType { get; set; } = "FixedAmount";

    [Column("default_rate")]
    public decimal? DefaultAmount { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeComponent> EmployeeComponents { get; set; } = new List<EmployeeComponent>();
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
