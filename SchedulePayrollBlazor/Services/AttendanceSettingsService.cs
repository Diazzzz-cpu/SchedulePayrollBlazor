using System;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public class AttendanceSettingsService : IAttendanceSettingsService
{
    private readonly AppDbContext _db;

    public AttendanceSettingsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AttendancePenaltySettings> GetOrCreateAsync()
    {
        var settings = await _db.AttendancePenaltySettings.FirstOrDefaultAsync();

        if (settings is not null)
        {
            return settings;
        }

        settings = new AttendancePenaltySettings
        {
            LatePenaltyPerMinute = 0m,
            UndertimePenaltyPerMinute = 0m,
            AbsenceFullDayMultiplier = 0m,
            OvertimeBonusPerMinute = 0m,
            UpdatedAt = DateTime.UtcNow
        };

        _db.AttendancePenaltySettings.Add(settings);
        await _db.SaveChangesAsync();

        return settings;
    }

    public async Task<AttendancePenaltySettings> UpdateAsync(AttendancePenaltySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var existing = await GetOrCreateAsync();

        existing.LatePenaltyPerMinute = settings.LatePenaltyPerMinute;
        existing.UndertimePenaltyPerMinute = settings.UndertimePenaltyPerMinute;
        existing.AbsenceFullDayMultiplier = settings.AbsenceFullDayMultiplier;
        existing.OvertimeBonusPerMinute = settings.OvertimeBonusPerMinute;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return existing;
    }
}
