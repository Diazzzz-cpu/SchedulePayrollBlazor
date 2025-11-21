using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public interface IPayComponentService
{
    Task<List<PayComponent>> GetAllAsync();
    Task<PayComponent?> GetByIdAsync(int id);
    Task<PayComponent> CreateAsync(PayComponent model);
    Task<PayComponent> UpdateAsync(PayComponent model);
    Task DeleteAsync(int id);
    Task<List<PayComponent>> EnsurePresetsAsync(CancellationToken cancellationToken = default);
}
