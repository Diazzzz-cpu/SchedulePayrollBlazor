using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class PayrollService : IPayrollService
{
    private const string BonusType = "Bonus";
    private const string DeductionType = "Deduction";
    private const string EarningKind = "Earning";
    private const string DeductionKind = "Deduction";
    private const string BaseCode = "BASE";
    private const string LateCode = "LATE";
    private const string UndertimeCode = "UNDERTIME";
    private const string AbsenceCode = "ABSENCE";
    private const string OvertimeCode = "OT";

    private readonly AppDbContext _db;
    private readonly IAttendanceService _attendanceService;
    private readonly IAttendanceSettingsService _attendanceSettingsService;

    public PayrollService(AppDbContext db, IAttendanceService attendanceService, IAttendanceSettingsService attendanceSettingsService)
    {
        _db = db;
        _attendanceService = attendanceService;
        _attendanceSettingsService = attendanceSettingsService;
    }

    public Task<EmployeeCompensation?> GetCompensationForEmployeeAsync(int employeeId)
    {
        return _db.EmployeeCompensations
            .Include(ec => ec.Employee)
            .FirstOrDefaultAsync(ec => ec.EmployeeId == employeeId);
    }

    public async Task UpsertCompensationAsync(EmployeeCompensation compensation)
    {
        ArgumentNullException.ThrowIfNull(compensation);

        var existing = await _db.EmployeeCompensations
            .FirstOrDefaultAsync(ec => ec.EmployeeId == compensation.EmployeeId);

        if (existing is null)
        {
            _db.EmployeeCompensations.Add(compensation);
        }
        else
        {
            existing.IsHourly = compensation.IsHourly;
            existing.HourlyRate = compensation.HourlyRate;
            existing.FixedMonthlySalary = compensation.FixedMonthlySalary;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PayrollPeriod> CreatePayrollPeriodAsync(string name, DateTime start, DateTime end)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Payroll period name is required.", nameof(name));
        }

        if (start > end)
        {
            throw new ArgumentException("Start date cannot be later than end date.", nameof(start));
        }

        var period = new PayrollPeriod
        {
            Name = name.Trim(),
            StartDate = start,
            EndDate = end,
            CreatedAt = DateTime.UtcNow
        };

        _db.PayrollPeriods.Add(period);
        await _db.SaveChangesAsync();

        return period;
    }

    public async Task<List<PayrollEntry>> GeneratePayrollForPeriodAsync(int payrollPeriodId)
    {
        var period = await _db.PayrollPeriods
            .FirstOrDefaultAsync(pp => pp.Id == payrollPeriodId)
            ?? throw new InvalidOperationException("Unable to locate payroll period.");

        var periodStartDate = period.StartDate.Date;
        var periodEndExclusive = period.EndDate.Date.AddDays(1);

        var relevantLogsEmployeeIds = await _db.TimeLogs
            .Where(t => t.ClockIn >= periodStartDate && t.ClockIn < periodEndExclusive)
            .Select(t => t.EmployeeId)
            .Distinct()
            .ToListAsync();

        var relevantShifts = await _db.Shifts
            .Where(s => s.Start < periodEndExclusive && s.End > period.StartDate)
            .ToListAsync();

        var shiftsByEmployee = relevantShifts
            .GroupBy(s => s.EmployeeId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var employeesWithActivity = new HashSet<int>(shiftsByEmployee.Keys);
        foreach (var employeeId in relevantLogsEmployeeIds)
        {
            employeesWithActivity.Add(employeeId);
        }

        var employees = await _db.Employees
            .Include(e => e.User)
            .Include(e => e.Compensation)
            .Where(e => employeesWithActivity.Contains(e.EmployeeId))
            .ToListAsync();

        var existingEntries = await _db.PayrollEntries
            .Include(pe => pe.PayrollLines)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .ToListAsync();

        var settings = await _attendanceSettingsService.GetOrCreateAsync();

        var employeeComponents = await _db.EmployeeComponents
            .Include(ec => ec.PayComponent)
            .Where(ec => employeesWithActivity.Contains(ec.EmployeeId) && ec.Active)
            .ToListAsync();

        var componentsByEmployee = employeeComponents
            .GroupBy(ec => ec.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var entriesWithoutShifts = existingEntries
            .Where(pe => !employeesWithActivity.Contains(pe.EmployeeId))
            .ToList();

        if (entriesWithoutShifts.Count > 0)
        {
            _db.PayrollEntries.RemoveRange(entriesWithoutShifts);
        }

        foreach (var employee in employees)
        {
            var employeeId = employee.EmployeeId;

            var existingEntry = existingEntries.FirstOrDefault(pe => pe.EmployeeId == employeeId);

            var attendanceSummary = await _attendanceService.GetSummaryForEmployeeAsync(
                employeeId,
                DateOnly.FromDateTime(period.StartDate),
                DateOnly.FromDateTime(period.EndDate));

            var totalHours = Math.Round(attendanceSummary.TotalRenderedHours, 2, MidpointRounding.AwayFromZero);
            var hourlyRate = CalculateHourlyRate(employee.Compensation);
            var basePay = Math.Round(hourlyRate * totalHours, 2, MidpointRounding.AwayFromZero);

            var hasAttendanceImpact = totalHours > 0
                || attendanceSummary.FullDayAbsences > 0
                || attendanceSummary.TotalOvertimeMinutes > 0
                || attendanceSummary.TotalLateMinutes > 0
                || attendanceSummary.TotalUndertimeMinutes > 0;

            if (!hasAttendanceImpact && !componentsByEmployee.ContainsKey(employeeId))
            {
                continue;
            }

            var entry = existingEntry;

            if (entry is null)
            {
                entry = new PayrollEntry
                {
                    PayrollPeriodId = payrollPeriodId,
                    EmployeeId = employeeId,
                    TotalHoursWorked = totalHours,
                    BasePay = basePay,
                    TotalDeductions = 0m,
                    TotalBonuses = 0m,
                    NetPay = basePay,
                    CalculatedAt = DateTime.UtcNow
                };

                _db.PayrollEntries.Add(entry);
                existingEntries.Add(entry);
            }
            else
            {
                entry.TotalHoursWorked = totalHours;
                entry.BasePay = basePay;
                entry.CalculatedAt = DateTime.UtcNow;
            }

            entry.PayrollLines ??= new List<PayrollLine>();
            RemoveAutoGeneratedLines(entry);

            AddBasePayrollLine(entry, totalHours, hourlyRate, basePay);
            ApplyAttendanceLines(entry, attendanceSummary, settings, hourlyRate);
            ApplyComponentLines(entry, basePay, totalHours, componentsByEmployee.TryGetValue(employeeId, out var employeeComponentList) ? employeeComponentList : new List<EmployeeComponent>());
            RecalculateEntryTotals(entry);
        }

        await _db.SaveChangesAsync();

        return await _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.Compensation)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.PayrollLines)
                .ThenInclude(pl => pl.PayComponent)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .OrderBy(pe => pe.Employee == null ? string.Empty : pe.Employee.FullName)
            .ToListAsync();
    }

    public async Task<List<PayrollEntry>> ApplyFixedPayAsync(int payrollPeriodId, bool applyToFixed, bool applyToHybrid)
    {
        var entries = await _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.Compensation)
            .Include(pe => pe.PayrollLines)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .ToListAsync();

        foreach (var entry in entries)
        {
            var structure = PayStructureHelper.Determine(entry.Employee?.Compensation);

            if (structure == PayStructureType.Fixed && applyToFixed)
            {
                entry.BasePay = CalculateBasePay(entry.Employee?.Compensation, entry.TotalHoursWorked, includeFixedComponent: true);
                UpdateBaseLine(entry);
                RecalculateEntryTotals(entry);
            }
            else if (structure == PayStructureType.Hybrid && applyToHybrid)
            {
                entry.BasePay = CalculateBasePay(entry.Employee?.Compensation, entry.TotalHoursWorked, includeFixedComponent: true);
                UpdateBaseLine(entry);
                RecalculateEntryTotals(entry);
            }
        }

        await _db.SaveChangesAsync();

        return await GetPayrollEntriesForPeriodAsync(payrollPeriodId);
    }

    public Task<PayrollEntry?> GetPayrollEntryAsync(int payrollEntryId)
    {
        return _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.Compensation)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.PayrollLines)
                .ThenInclude(pl => pl.PayComponent)
            .FirstOrDefaultAsync(pe => pe.Id == payrollEntryId);
    }

    public Task<List<PayrollEntry>> GetPayrollEntriesForPeriodAsync(int payrollPeriodId)
    {
        return _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.Compensation)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.PayrollLines)
                .ThenInclude(pl => pl.PayComponent)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .OrderBy(pe => pe.Employee == null ? string.Empty : pe.Employee.FullName)
            .ToListAsync();
    }

    public async Task AddAdjustmentAsync(int payrollEntryId, string type, string label, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Adjustment type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Adjustment label is required.", nameof(label));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Adjustment amount must be greater than zero.", nameof(amount));
        }

        var entry = await _db.PayrollEntries
            .Include(pe => pe.PayrollLines)
            .FirstOrDefaultAsync(pe => pe.Id == payrollEntryId)
            ?? throw new InvalidOperationException("Unable to find payroll entry for adjustment.");

        var normalizedType = NormalizeAdjustmentType(type);
        var kind = string.Equals(normalizedType, DeductionType, StringComparison.OrdinalIgnoreCase)
            ? DeductionKind
            : EarningKind;

        entry.PayrollLines ??= new List<PayrollLine>();
        entry.PayrollLines.Add(CreatePayrollLine(
            payrollEntryId,
            code: "MANUAL",
            description: label.Trim(),
            kind: kind,
            quantity: 1m,
            rate: Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            amount: Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            isAutoGenerated: false));

        RecalculateEntryTotals(entry);

        await _db.SaveChangesAsync();
    }

    public async Task RemoveAdjustmentAsync(int adjustmentId)
    {
        var line = await _db.PayrollLines
            .FirstOrDefaultAsync(pl => pl.PayrollLineId == adjustmentId)
            ?? throw new InvalidOperationException("Unable to find payroll line to remove.");

        if (line.IsAutoGenerated)
        {
            throw new InvalidOperationException("Auto-generated payroll lines cannot be removed manually.");
        }

        var entry = await _db.PayrollEntries
            .Include(pe => pe.PayrollLines)
            .FirstOrDefaultAsync(pe => pe.Id == line.PayrollEntryId)
            ?? throw new InvalidOperationException("Unable to find payroll entry for adjustment removal.");

        _db.PayrollLines.Remove(line);
        entry.PayrollLines.Remove(line);

        RecalculateEntryTotals(entry);

        await _db.SaveChangesAsync();
    }
    private void ApplyAttendanceLines(
        PayrollEntry entry,
        AttendancePeriodSummary summary,
        AttendancePenaltySettings settings,
        decimal hourlyRate)
    {
        if (summary.TotalLateMinutes > 0 && settings.LatePenaltyPerMinute > 0)
        {
            var amount = Math.Round(summary.TotalLateMinutes * settings.LatePenaltyPerMinute, 2, MidpointRounding.AwayFromZero);
            entry.PayrollLines.Add(CreatePayrollLine(
                entry.Id,
                LateCode,
                "Late penalty",
                DeductionKind,
                summary.TotalLateMinutes,
                settings.LatePenaltyPerMinute,
                amount,
                isAutoGenerated: true));
        }

        if (summary.TotalUndertimeMinutes > 0 && settings.UndertimePenaltyPerMinute > 0)
        {
            var amount = Math.Round(summary.TotalUndertimeMinutes * settings.UndertimePenaltyPerMinute, 2, MidpointRounding.AwayFromZero);
            entry.PayrollLines.Add(CreatePayrollLine(
                entry.Id,
                UndertimeCode,
                "Undertime penalty",
                DeductionKind,
                summary.TotalUndertimeMinutes,
                settings.UndertimePenaltyPerMinute,
                amount,
                isAutoGenerated: true));
        }

        if (summary.FullDayAbsences > 0 && settings.AbsenceFullDayMultiplier > 0 && hourlyRate > 0)
        {
            var absentHours = summary.Days
                .Where(d => d.IsAbsent && d.ScheduledHours > 0)
                .Sum(d => d.ScheduledHours);

            if (absentHours > 0)
            {
                var amount = Math.Round(absentHours * hourlyRate * settings.AbsenceFullDayMultiplier, 2, MidpointRounding.AwayFromZero);
                entry.PayrollLines.Add(CreatePayrollLine(
                    entry.Id,
                    AbsenceCode,
                    "Absence deduction",
                    DeductionKind,
                    absentHours,
                    hourlyRate,
                    amount,
                    isAutoGenerated: true));
            }
        }

        if (summary.TotalOvertimeMinutes > 0 && settings.OvertimeBonusPerMinute > 0)
        {
            var amount = Math.Round(summary.TotalOvertimeMinutes * settings.OvertimeBonusPerMinute, 2, MidpointRounding.AwayFromZero);
            entry.PayrollLines.Add(CreatePayrollLine(
                entry.Id,
                OvertimeCode,
                "Overtime bonus",
                EarningKind,
                summary.TotalOvertimeMinutes,
                settings.OvertimeBonusPerMinute,
                amount,
                isAutoGenerated: true));
        }
    }

    private static void ApplyComponentLines(
        PayrollEntry entry,
        decimal basePay,
        decimal totalHours,
        List<EmployeeComponent> components)
    {
        foreach (var component in components)
        {
            if (component.PayComponent is null)
            {
                continue;
            }

            var payComponent = component.PayComponent;
            var rate = component.Amount > 0 ? component.Amount : payComponent.DefaultAmount ?? 0m;

            decimal amount = payComponent.CalculationType switch
            {
                "FixedAmount" => rate,
                "PercentageOfBase" => Math.Round(basePay * rate, 2, MidpointRounding.AwayFromZero),
                "PerHour" => Math.Round(totalHours * rate, 2, MidpointRounding.AwayFromZero),
                _ => 0m
            };

            if (amount <= 0)
            {
                continue;
            }

            var kind = string.Equals(payComponent.ComponentType, DeductionType, StringComparison.OrdinalIgnoreCase)
                ? DeductionKind
                : EarningKind;

            var label = string.IsNullOrWhiteSpace(payComponent.Name)
                ? payComponent.Code
                : payComponent.Name;

            entry.PayrollLines.Add(CreatePayrollLine(
                entry.Id,
                payComponent.Code,
                label,
                kind,
                payComponent.CalculationType == "PercentageOfBase" ? 1m : totalHours,
                payComponent.CalculationType == "PercentageOfBase" ? rate : rate,
                amount,
                isAutoGenerated: true,
                payComponentId: payComponent.PayComponentId));
        }
    }

    private static PayrollLine CreatePayrollLine(
        int payrollEntryId,
        string code,
        string description,
        string kind,
        decimal quantity,
        decimal rate,
        decimal amount,
        bool isAutoGenerated,
        int? payComponentId = null)
    {
        return new PayrollLine
        {
            PayrollEntryId = payrollEntryId,
            PayComponentId = payComponentId,
            Code = code,
            Description = description,
            Kind = kind,
            Quantity = Math.Round(quantity, 2, MidpointRounding.AwayFromZero),
            Rate = Math.Round(rate, 2, MidpointRounding.AwayFromZero),
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            IsAutoGenerated = isAutoGenerated
        };
    }

    private static decimal CalculateHourlyRate(EmployeeCompensation? compensation)
    {
        if (compensation is null)
        {
            return 0m;
        }

        if (compensation.HourlyRate.HasValue && compensation.HourlyRate.Value > 0)
        {
            return compensation.HourlyRate.Value;
        }

        if (compensation.FixedMonthlySalary.HasValue && compensation.FixedMonthlySalary.Value > 0)
        {
            // Assumes a standard 160-hour working month when an hourly rate is not provided explicitly.
            return Math.Round(compensation.FixedMonthlySalary.Value / 160m, 2, MidpointRounding.AwayFromZero);
        }

        return 0m;
    }

    private static decimal CalculateBasePay(EmployeeCompensation? compensation, decimal totalHours, bool includeFixedComponent)
    {
        var structure = PayStructureHelper.Determine(compensation);
        var hourlyRate = compensation?.HourlyRate ?? 0m;
        var fixedSalary = includeFixedComponent ? compensation?.FixedMonthlySalary ?? 0m : 0m;

        var basePay = structure switch
        {
            PayStructureType.Hourly => totalHours * hourlyRate,
            PayStructureType.Hybrid => (totalHours * hourlyRate) + fixedSalary,
            PayStructureType.Fixed => fixedSalary,
            _ => totalHours * hourlyRate
        };

        return Math.Round(basePay, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeAdjustmentType(string type)
    {
        return string.Equals(type, DeductionType, StringComparison.OrdinalIgnoreCase)
            ? DeductionType
            : BonusType;
    }

    private void RemoveAutoGeneratedLines(PayrollEntry entry)
    {
        if (entry.PayrollLines is null || entry.PayrollLines.Count == 0)
        {
            return;
        }

        var autoLines = entry.PayrollLines.Where(pl => pl.IsAutoGenerated).ToList();

        if (autoLines.Count == 0)
        {
            return;
        }

        _db.PayrollLines.RemoveRange(autoLines);
        entry.PayrollLines = entry.PayrollLines.Except(autoLines).ToList();
    }

    private void UpdateBaseLine(PayrollEntry entry)
    {
        entry.PayrollLines ??= new List<PayrollLine>();

        var baseLine = entry.PayrollLines.FirstOrDefault(l => string.Equals(l.Code, BaseCode, StringComparison.OrdinalIgnoreCase));
        var hourlyRate = entry.TotalHoursWorked > 0
            ? Math.Round(entry.BasePay / entry.TotalHoursWorked, 2, MidpointRounding.AwayFromZero)
            : 0m;

        if (baseLine is null)
        {
            AddBasePayrollLine(entry, entry.TotalHoursWorked, hourlyRate, entry.BasePay);
            return;
        }

        baseLine.Description = "Base pay (rendered hours)";
        baseLine.Kind = EarningKind;
        baseLine.Quantity = entry.TotalHoursWorked;
        baseLine.Rate = hourlyRate;
        baseLine.Amount = entry.BasePay;
        baseLine.IsAutoGenerated = true;
    }

    private static void AddBasePayrollLine(PayrollEntry entry, decimal totalHours, decimal hourlyRate, decimal basePay)
    {
        entry.PayrollLines.Add(CreatePayrollLine(
            entry.Id,
            BaseCode,
            "Base pay (rendered hours)",
            EarningKind,
            totalHours,
            hourlyRate,
            basePay,
            isAutoGenerated: true));
    }

    private static decimal SumLines(IEnumerable<PayrollLine> lines, string kind)
    {
        return lines
            .Where(l => string.Equals(l.Kind, kind, StringComparison.OrdinalIgnoreCase))
            .Sum(l => l.Amount);
    }

    private static decimal GetBaseAmount(IEnumerable<PayrollLine> lines)
    {
        return lines.FirstOrDefault(l => string.Equals(l.Code, BaseCode, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0m;
    }

    private static void RecalculateEntryTotals(PayrollEntry entry)
    {
        var lines = entry.PayrollLines ?? new List<PayrollLine>();

        var totalEarnings = Math.Round(SumLines(lines, EarningKind), 2, MidpointRounding.AwayFromZero);
        var totalDeductions = Math.Round(SumLines(lines, DeductionKind), 2, MidpointRounding.AwayFromZero);
        var baseAmount = Math.Round(GetBaseAmount(lines), 2, MidpointRounding.AwayFromZero);

        entry.BasePay = baseAmount;
        entry.TotalBonuses = Math.Max(0, Math.Round(totalEarnings - baseAmount, 2, MidpointRounding.AwayFromZero));
        entry.TotalDeductions = totalDeductions;
        entry.NetPay = Math.Round(totalEarnings - totalDeductions, 2, MidpointRounding.AwayFromZero);
        entry.CalculatedAt = DateTime.UtcNow;
    }
}
