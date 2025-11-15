using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public interface IPayrollService
{
    Task<EmployeeCompensation?> GetCompensationForEmployeeAsync(int employeeId);
    Task UpsertCompensationAsync(EmployeeCompensation compensation);

    Task<PayrollPeriod> CreatePayrollPeriodAsync(string name, DateTime start, DateTime end);

    Task<List<PayrollEntry>> GeneratePayrollForPeriodAsync(int payrollPeriodId);
    Task<PayrollEntry?> GetPayrollEntryAsync(int payrollEntryId);
    Task<List<PayrollEntry>> GetPayrollEntriesForPeriodAsync(int payrollPeriodId);

    Task AddAdjustmentAsync(int payrollEntryId, string type, string label, decimal amount);
    Task RemoveAdjustmentAsync(int adjustmentId);
}
