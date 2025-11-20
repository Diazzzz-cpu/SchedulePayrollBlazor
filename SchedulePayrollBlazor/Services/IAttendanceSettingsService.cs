using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public interface IAttendanceSettingsService
{
    Task<AttendancePenaltySettings> GetOrCreateAsync();
    Task<AttendancePenaltySettings> UpdateAsync(AttendancePenaltySettings settings);
}
