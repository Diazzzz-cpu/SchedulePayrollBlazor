using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task UpdateAsync(Employee employee, string? newPassword = null);
}
