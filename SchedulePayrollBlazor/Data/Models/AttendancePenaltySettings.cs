using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("attendance_penalty_settings")]
public class AttendancePenaltySettings
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("late_penalty_per_minute", TypeName = "decimal(18,4)")]
    public decimal LatePenaltyPerMinute { get; set; }

    [Column("undertime_penalty_per_minute", TypeName = "decimal(18,4)")]
    public decimal UndertimePenaltyPerMinute { get; set; }

    [Column("absence_full_day_multiplier", TypeName = "decimal(18,4)")]
    public decimal AbsenceFullDayMultiplier { get; set; }

    [Column("overtime_bonus_per_minute", TypeName = "decimal(18,4)")]
    public decimal OvertimeBonusPerMinute { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
