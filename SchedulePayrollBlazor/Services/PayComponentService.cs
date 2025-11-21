using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public class PayComponentService : IPayComponentService
{
    private readonly AppDbContext _dbContext;

    public PayComponentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PayComponent>> GetAllAsync()
    {
        return await _dbContext.PayComponents
            .AsNoTracking()
            .OrderBy(pc => pc.Name)
            .ToListAsync();
    }

    public async Task<PayComponent?> GetByIdAsync(int id)
    {
        return await _dbContext.PayComponents
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.PayComponentId == id);
    }

    public async Task<PayComponent> CreateAsync(PayComponent model)
    {
        ValidateModel(model);
        await EnsureUniqueAsync(model);

        model.Name = model.Name.Trim();
        model.Code = model.Code.Trim();
        model.ComponentType = string.IsNullOrWhiteSpace(model.ComponentType) ? "Earning" : model.ComponentType.Trim();
        model.CalculationType = string.IsNullOrWhiteSpace(model.CalculationType) ? "FixedAmount" : model.CalculationType.Trim();

        _dbContext.PayComponents.Add(model);
        await _dbContext.SaveChangesAsync();
        return model;
    }

    public async Task<PayComponent> UpdateAsync(PayComponent model)
    {
        ValidateModel(model);

        var existing = await _dbContext.PayComponents.FirstOrDefaultAsync(pc => pc.PayComponentId == model.PayComponentId)
            ?? throw new InvalidOperationException("Pay component not found.");

        await EnsureUniqueAsync(model, model.PayComponentId);

        existing.Name = model.Name.Trim();
        existing.Code = model.Code.Trim();
        existing.ComponentType = string.IsNullOrWhiteSpace(model.ComponentType) ? "Earning" : model.ComponentType.Trim();
        existing.CalculationType = string.IsNullOrWhiteSpace(model.CalculationType) ? "FixedAmount" : model.CalculationType.Trim();
        existing.DefaultAmount = model.DefaultAmount;
        existing.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _dbContext.PayComponents.FirstOrDefaultAsync(pc => pc.PayComponentId == id)
            ?? throw new InvalidOperationException("Pay component not found.");

        _dbContext.PayComponents.Remove(existing);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<PayComponent>> EnsurePresetsAsync(CancellationToken cancellationToken = default)
    {
        var presetDefinitions = new List<PayComponent>
        {
            new()
            {
                Code = "SSS",
                Name = "SSS contribution",
                ComponentType = "Deduction",
                CalculationType = "PercentageOfBase",
                DefaultAmount = 0.045m,
                IsActive = true
            },
            new()
            {
                Code = "PHIC",
                Name = "PhilHealth",
                ComponentType = "Deduction",
                CalculationType = "PercentageOfBase",
                DefaultAmount = 0.03m,
                IsActive = true
            },
            new()
            {
                Code = "HDMF",
                Name = "Pag-IBIG",
                ComponentType = "Deduction",
                CalculationType = "PercentageOfBase",
                DefaultAmount = 0.01m,
                IsActive = true
            },
            new()
            {
                Code = "TAX",
                Name = "Withholding tax",
                ComponentType = "Deduction",
                CalculationType = "PercentageOfBase",
                DefaultAmount = 0.10m,
                IsActive = true
            },
            new()
            {
                Code = "ND",
                Name = "Night differential",
                ComponentType = "Earning",
                CalculationType = "PerHour",
                DefaultAmount = 20m,
                IsActive = true
            }
        };

        var results = new List<PayComponent>();
        var added = false;

        foreach (var preset in presetDefinitions)
        {
            var normalizedCode = preset.Code.Trim().ToLower();
            var existing = await _dbContext.PayComponents
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.Code.ToLower() == normalizedCode, cancellationToken);

            if (existing is not null)
            {
                results.Add(existing);
                continue;
            }

            ValidateModel(preset);
            await EnsureUniqueAsync(preset);

            preset.Name = preset.Name.Trim();
            preset.Code = preset.Code.Trim();
            preset.ComponentType = string.IsNullOrWhiteSpace(preset.ComponentType) ? "Earning" : preset.ComponentType.Trim();
            preset.CalculationType = string.IsNullOrWhiteSpace(preset.CalculationType) ? "FixedAmount" : preset.CalculationType.Trim();

            _dbContext.PayComponents.Add(preset);
            results.Add(preset);
            added = true;
        }

        if (added)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return results;
    }

    private static void ValidateModel(PayComponent model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Code))
        {
            throw new ArgumentException("Code is required.");
        }
    }

    private async Task EnsureUniqueAsync(PayComponent model, int? excludeId = null)
    {
        var name = model.Name.Trim();
        var code = model.Code.Trim();
        var normalizedName = name.ToLower();
        var normalizedCode = code.ToLower();

        var hasDuplicateName = await _dbContext.PayComponents
            .AsNoTracking()
            .AnyAsync(pc => (!excludeId.HasValue || pc.PayComponentId != excludeId.Value) && pc.Name.ToLower() == normalizedName);

        if (hasDuplicateName)
        {
            throw new InvalidOperationException("A pay component with the same name already exists.");
        }

        var hasDuplicateCode = await _dbContext.PayComponents
            .AsNoTracking()
            .AnyAsync(pc => (!excludeId.HasValue || pc.PayComponentId != excludeId.Value) && pc.Code.ToLower() == normalizedCode);

        if (hasDuplicateCode)
        {
            throw new InvalidOperationException("A pay component with the same code already exists.");
        }
    }
}
