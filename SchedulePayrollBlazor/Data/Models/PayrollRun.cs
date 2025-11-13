using System;

namespace SchedulePayrollBlazor.Data.Models;

public class PayrollRun
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int? PayPeriodId { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal GrossPay { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal NetPay { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;

    public PayPeriod? PayPeriod { get; set; }
}
