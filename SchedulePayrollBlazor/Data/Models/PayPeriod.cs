using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

[Table("payperiod")]
public class PayPeriod
{
    [Key]
    [Column("payperiod_id")]
    public int PayPeriodId { get; set; }

    [Column("period_name")]
    public string PeriodName { get; set; } = string.Empty;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Open";

    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
