using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("role_pay_components")]
public class RolePayComponent
{
    [Key]
    [Column("role_pay_component_id")]
    public int RolePayComponentId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("pay_component_id")]
    public int PayComponentId { get; set; }

    [Column("default_rate_override")]
    public decimal? DefaultRateOverride { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public Role Role { get; set; } = default!;
    public PayComponent PayComponent { get; set; } = default!;
}
