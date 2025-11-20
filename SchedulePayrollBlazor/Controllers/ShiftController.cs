using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulePayrollBlazor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shift>>> GetAsync(
        [FromQuery] DateTime? weekStart,
        [FromQuery] DateTime? weekEnd,
        [FromQuery] int? employeeId,
        [FromQuery] string? group,
        [FromQuery] string? searchText)
    {
        var rangeStart = weekStart ?? StartOfWeek(DateTime.Today);
        var rangeEnd = weekEnd ?? rangeStart.AddDays(7);
        if (rangeEnd <= rangeStart)
        {
            rangeEnd = rangeStart.AddDays(7);
        }

        int? effectiveEmployeeId = null;
        if (User.IsInRole("Admin"))
        {
            effectiveEmployeeId = employeeId;
        }
        else
        {
            effectiveEmployeeId = GetEmployeeId();
            if (effectiveEmployeeId is null)
            {
                return Forbid();
            }
        }

        var normalizedGroup = string.Equals(group, "All", StringComparison.OrdinalIgnoreCase)
            ? null
            : group;

        var shifts = await _shiftService.GetShiftsAsync(
            rangeStart,
            rangeEnd,
            effectiveEmployeeId,
            normalizedGroup,
            searchText);

        return Ok(shifts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Shift>> GetByIdAsync(int id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin"))
        {
            var employeeId = GetEmployeeId();
            if (employeeId is null || employeeId.Value != shift.EmployeeId)
            {
                return Forbid();
            }
        }

        return Ok(shift);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Shift>> CreateAsync([FromBody] ShiftUpsertRequest request)
    {
        ValidateRequest(request);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var shift = new Shift
        {
            EmployeeId = request.EmployeeId,
            EmployeeName = request.EmployeeName ?? string.Empty,
            Start = request.Start,
            End = request.End,
            GroupName = request.GroupName
        };

        try
        {
            var created = await _shiftService.InsertShiftAsync(shift);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Shift>> UpdateAsync(int id, [FromBody] ShiftUpsertRequest request)
    {
        ValidateRequest(request);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var shift = new Shift
        {
            Id = id,
            EmployeeId = request.EmployeeId,
            EmployeeName = request.EmployeeName ?? string.Empty,
            Start = request.Start,
            End = request.End,
            GroupName = request.GroupName
        };

        try
        {
            var updated = await _shiftService.UpdateShiftAsync(shift);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var deleted = await _shiftService.DeleteShiftAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private void ValidateRequest(ShiftUpsertRequest request)
    {
        if (request.EmployeeId <= 0)
        {
            ModelState.AddModelError(nameof(request.EmployeeId), "Employee is required.");
        }

        if (request.Start >= request.End)
        {
            ModelState.AddModelError(nameof(request.End), "End time must be after the start time.");
        }
    }

    private int? GetEmployeeId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Sunday) % 7;
        return date.Date.AddDays(-diff);
    }

    public sealed record ShiftUpsertRequest
    {
        public int EmployeeId { get; init; }
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public string? GroupName { get; init; }
        public string? EmployeeName { get; init; }
    }
}
