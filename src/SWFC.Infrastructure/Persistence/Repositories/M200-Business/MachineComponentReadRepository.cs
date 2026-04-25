using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.MachineComponents;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineComponentReadRepository : IMachineComponentReadRepository
{
    private readonly AppDbContext _dbContext;

    public MachineComponentReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MachineComponentListItemDto>> GetByMachineAsync(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.MachineComponents
            .AsNoTracking()
            .Where(x => x.MachineId == machineId)
            .Select(x => new ComponentRow(
                x.Id,
                x.MachineId,
                x.MachineComponentAreaId,
                x.ParentMachineComponentId,
                x.Name.Value,
                x.Description.Value,
                x.IsActive,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToListAsync(cancellationToken);

        var childrenLookup = rows
            .Where(x => x.ParentMachineComponentId.HasValue)
            .GroupBy(x => x.ParentMachineComponentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());

        var roots = rows
            .Where(x => !x.ParentMachineComponentId.HasValue)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ordered = new List<MachineComponentListItemDto>();

        foreach (var root in roots)
        {
            AppendTree(root, 0, childrenLookup, ordered);
        }

        return ordered;
    }

    private static void AppendTree(
        ComponentRow current,
        int level,
        IReadOnlyDictionary<Guid, List<ComponentRow>> childrenLookup,
        ICollection<MachineComponentListItemDto> output)
    {
        var hasChildren = childrenLookup.ContainsKey(current.Id);

        output.Add(new MachineComponentListItemDto(
            current.Id,
            current.MachineId,
            current.MachineComponentAreaId,
            current.ParentMachineComponentId,
            current.Name,
            current.Description,
            current.IsActive,
            level,
            hasChildren,
            current.CreatedAtUtc,
            current.CreatedBy,
            current.LastModifiedAtUtc,
            current.LastModifiedBy));

        if (!hasChildren)
        {
            return;
        }

        foreach (var child in childrenLookup[current.Id])
        {
            AppendTree(child, level + 1, childrenLookup, output);
        }
    }

    private sealed record ComponentRow(
        Guid Id,
        Guid MachineId,
        Guid? MachineComponentAreaId,
        Guid? ParentMachineComponentId,
        string Name,
        string Description,
        bool IsActive,
        DateTime CreatedAtUtc,
        string CreatedBy,
        DateTime? LastModifiedAtUtc,
        string? LastModifiedBy);
}
