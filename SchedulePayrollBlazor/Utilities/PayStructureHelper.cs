using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Utilities;

public enum PayStructureType
{
    Unknown,
    Hourly,
    Fixed,
    Hybrid
}

public static class PayStructureHelper
{
    public static PayStructureType Determine(EmployeeCompensation? compensation)
    {
        if (compensation is null)
        {
            return PayStructureType.Unknown;
        }

        var hasHourly = compensation.HourlyRate.HasValue && compensation.HourlyRate.Value > 0;
        var hasFixed = compensation.FixedMonthlySalary.HasValue && compensation.FixedMonthlySalary.Value > 0;

        if (hasHourly && hasFixed)
        {
            return PayStructureType.Hybrid;
        }

        if (hasFixed && !hasHourly)
        {
            return PayStructureType.Fixed;
        }

        if (hasHourly || compensation.IsHourly)
        {
            return PayStructureType.Hourly;
        }

        return PayStructureType.Unknown;
    }

    public static string GetDisplayName(PayStructureType payStructure)
    {
        return payStructure switch
        {
            PayStructureType.Hourly => "Hourly",
            PayStructureType.Fixed => "Fixed",
            PayStructureType.Hybrid => "Hybrid",
            _ => "Not set"
        };
    }
}
