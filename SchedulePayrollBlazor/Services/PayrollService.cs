using Microsoft.EntityFrameworkCore;
using System.Linq;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public class PayrollService : IPayrollService
{
    private const string BonusType = "Bonus";
    private const string DeductionType = "Deduction";

    private readonly AppDbContext _db;

    public PayrollService(AppDbContext db)
    {
        _db = db;
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

        var compensations = await _db.EmployeeCompensations
            .Include(ec => ec.Employee)
                .ThenInclude(e => e.User)
            .ToListAsync();

        if (compensations.Count == 0)
        {
            return new List<PayrollEntry>();
        }

        var relevantShifts = await _db.Shifts
            .Where(s => s.Start >= period.StartDate && s.End <= period.EndDate)
            .ToListAsync();

        var shiftsByEmployee = relevantShifts
            .GroupBy(s => s.EmployeeId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var existingEntries = await _db.PayrollEntries
            .Include(pe => pe.Adjustments)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .ToListAsync();

        foreach (var compensation in compensations)
        {
            if (compensation.Employee is null)
            {
                continue;
            }

            var employeeId = compensation.EmployeeId;

            shiftsByEmployee.TryGetValue(employeeId, out var employeeShifts);

            decimal totalHours = 0m;

            if (employeeShifts is not null)
            {
                foreach (var shift in employeeShifts)
                {
                    if (shift.End <= shift.Start)
                    {
                        continue;
                    }

                    var duration = (decimal)(shift.End - shift.Start).TotalHours;
                    totalHours += Math.Round(duration, 2, MidpointRounding.AwayFromZero);
                }
            }

            var basePay = compensation.IsHourly
                ? totalHours * (compensation.HourlyRate ?? 0m)
                : compensation.FixedMonthlySalary ?? 0m;

            basePay = Math.Round(basePay, 2, MidpointRounding.AwayFromZero);

            var entry = existingEntries.FirstOrDefault(pe => pe.EmployeeId == employeeId);

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

            RecalculateEntryTotals(entry);
        }

        await _db.SaveChangesAsync();

        return await _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.Adjustments)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .OrderBy(pe => pe.Employee.FullName)
            .ToListAsync();
    }

    public Task<PayrollEntry?> GetPayrollEntryAsync(int payrollEntryId)
    {
        return _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.Adjustments)
            .FirstOrDefaultAsync(pe => pe.Id == payrollEntryId);
    }

    public Task<List<PayrollEntry>> GetPayrollEntriesForPeriodAsync(int payrollPeriodId)
    {
        return _db.PayrollEntries
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.User)
            .Include(pe => pe.PayrollPeriod)
            .Include(pe => pe.Adjustments)
            .Where(pe => pe.PayrollPeriodId == payrollPeriodId)
            .OrderBy(pe => pe.Employee.FullName)
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
            .Include(pe => pe.Adjustments)
            .FirstOrDefaultAsync(pe => pe.Id == payrollEntryId)
            ?? throw new InvalidOperationException("Unable to find payroll entry for adjustment.");

        var normalizedType = NormalizeAdjustmentType(type);

        entry.Adjustments.Add(new PayrollAdjustment
        {
            PayrollEntryId = payrollEntryId,
            Type = normalizedType,
            Label = label.Trim(),
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero)
        });

        RecalculateEntryTotals(entry);

        await _db.SaveChangesAsync();
    }

    public async Task RemoveAdjustmentAsync(int adjustmentId)
    {
        var adjustment = await _db.PayrollAdjustments
            .FirstOrDefaultAsync(pa => pa.Id == adjustmentId)
            ?? throw new InvalidOperationException("Unable to find adjustment to remove.");

        var entry = await _db.PayrollEntries
            .Include(pe => pe.Adjustments)
            .FirstOrDefaultAsync(pe => pe.Id == adjustment.PayrollEntryId)
            ?? throw new InvalidOperationException("Unable to find payroll entry for adjustment removal.");

        _db.PayrollAdjustments.Remove(adjustment);

        RecalculateEntryTotals(entry);

        await _db.SaveChangesAsync();
    }

    private static string NormalizeAdjustmentType(string type)
    {
        return string.Equals(type, DeductionType, StringComparison.OrdinalIgnoreCase)
            ? DeductionType
            : BonusType;
    }

    private static decimal SumAdjustments(IEnumerable<PayrollAdjustment> adjustments, string type)
    {
        return adjustments
            .Where(a => string.Equals(a.Type, type, StringComparison.OrdinalIgnoreCase))
            .Sum(a => a.Amount);
    }

    private static void RecalculateEntryTotals(PayrollEntry entry)
    {
        var adjustments = entry.Adjustments ?? new List<PayrollAdjustment>();

        entry.TotalDeductions = Math.Round(SumAdjustments(adjustments, DeductionType), 2, MidpointRounding.AwayFromZero);
        entry.TotalBonuses = Math.Round(SumAdjustments(adjustments, BonusType), 2, MidpointRounding.AwayFromZero);

        entry.NetPay = Math.Round(entry.BasePay - entry.TotalDeductions + entry.TotalBonuses, 2, MidpointRounding.AwayFromZero);
        entry.CalculatedAt = DateTime.UtcNow;
    }
}
