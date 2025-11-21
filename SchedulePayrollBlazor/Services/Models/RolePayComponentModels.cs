namespace SchedulePayrollBlazor.Services.Models;

public sealed class RolePayComponentDefinition
{
    public int PayComponentId { get; set; }
    public decimal? DefaultRateOverride { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class EmployeeComponentApplyResult
{
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
}

public sealed class BulkAssignComponentRequest
{
    public int PayComponentId { get; set; }
    public decimal? CustomRate { get; set; }
}

public sealed class BulkAssignResult
{
    public int EmployeesConsidered { get; set; }
    public int AssignmentsCreated { get; set; }
    public int AssignmentsSkippedExisting { get; set; }
}
