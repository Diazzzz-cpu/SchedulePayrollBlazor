using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("pay_periods")]
public class PayPeriod
{
    [Key]
    [Column("pay_period_id")]
    public int PayPeriodId { get; set; }

    [Required]
    [Column("period_name")]
    public string PeriodName { get; set; } = string.Empty;

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
