using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.Machines;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineReadRepository : IMachineReadRepository
{
    private readonly AppDbContext _dbContext;

    public MachineReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MachineListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var machines = await _dbContext.Machines
            .AsNoTracking()
            .Select(x => new MachineRow(
                x.Id,
                x.Name.Value,
                x.InventoryNumber.Value,
                x.Location.Value,
                x.Status.Value,
                x.Manufacturer.Value,
                x.Model.Value,
                x.SerialNumber.Value,
                x.ParentMachineId,
                x.ParentMachine != null ? x.ParentMachine.Name.Value : null,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToListAsync(cancellationToken);

        var childrenLookup = machines
            .Where(x => x.ParentMachineId.HasValue)
            .GroupBy(x => x.ParentMachineId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());

        var roots = machines
            .Where(x => !x.ParentMachineId.HasValue)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ordered = new List<MachineListItem>();

        foreach (var root in roots)
        {
            AppendTree(root, 0, childrenLookup, ordered);
        }

        return ordered;
    }

    public async Task<MachineDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var machine = await _dbContext.Machines
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                Name = x.Name.Value,
                InventoryNumber = x.InventoryNumber.Value,
                Location = x.Location.Value,
                Status = x.Status.Value,
                Manufacturer = x.Manufacturer.Value,
                Model = x.Model.Value,
                SerialNumber = x.SerialNumber.Value,
                Description = x.Description.Value,
                x.ParentMachineId,
                ParentMachineName = x.ParentMachine != null ? x.ParentMachine.Name.Value : null,
                x.OrganizationUnitId,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (machine is null)
        {
            return null;
        }

        string? organizationUnitName = null;

        if (machine.OrganizationUnitId.HasValue)
        {
            organizationUnitName = await _dbContext.OrganizationUnits
                .AsNoTracking()
                .Where(x => x.Id == machine.OrganizationUnitId.Value)
                .Select(x => x.Name.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var childRows = await _dbContext.Machines
            .AsNoTracking()
            .Where(x => x.ParentMachineId == id)
            .Select(x => new
            {
                x.Id,
                Name = x.Name.Value,
                InventoryNumber = x.InventoryNumber.Value,
                Status = x.Status.Value
            })
            .ToListAsync(cancellationToken);

        var children = childRows
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new MachineChildListItemDto(
                x.Id,
                x.Name,
                x.InventoryNumber,
                x.Status))
            .ToList();

        return new MachineDetailsDto(
            machine.Id,
            machine.Name,
            machine.InventoryNumber,
            machine.Location,
            machine.Status,
            machine.Manufacturer,
            machine.Model,
            machine.SerialNumber,
            machine.Description,
            machine.ParentMachineId,
            machine.ParentMachineName,
            machine.OrganizationUnitId,
            organizationUnitName,
            children,
            machine.CreatedAtUtc,
            machine.CreatedBy,
            machine.LastModifiedAtUtc,
            machine.LastModifiedBy);
    }

    public async Task<IReadOnlyList<MachineSelectionOptionDto>> GetSelectionOptionsAsync(Guid? excludeMachineId = null, CancellationToken cancellationToken = default)
    {
        var machines = await _dbContext.Machines
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                Name = x.Name.Value,
                InventoryNumber = x.InventoryNumber.Value,
                x.ParentMachineId
            })
            .ToListAsync(cancellationToken);

        if (excludeMachineId.HasValue)
        {
            var excludedIds = new HashSet<Guid> { excludeMachineId.Value };
            var queue = new Queue<Guid>();
            queue.Enqueue(excludeMachineId.Value);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();

                var children = machines
                    .Where(x => x.ParentMachineId == currentId)
                    .Select(x => x.Id)
                    .ToList();

                foreach (var childId in children)
                {
                    if (excludedIds.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            machines = machines
                .Where(x => !excludedIds.Contains(x.Id))
                .ToList();
        }

        var rows = machines
            .Select(x => new SelectionRow(x.Id, x.Name, x.InventoryNumber, x.ParentMachineId))
            .ToList();

        var childrenLookup = rows
            .Where(x => x.ParentMachineId.HasValue)
            .GroupBy(x => x.ParentMachineId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());

        var roots = rows
            .Where(x => !x.ParentMachineId.HasValue)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ordered = new List<MachineSelectionOptionDto>();

        foreach (var root in roots)
        {
            AppendSelectionTree(root, 0, childrenLookup, ordered);
        }

        return ordered;
    }

    public async Task<IReadOnlyList<OrganizationUnitSelectionOptionDto>> GetOrganizationUnitSelectionOptionsAsync(CancellationToken cancellationToken = default)
    {
        var organizationUnits = await _dbContext.OrganizationUnits
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                Name = x.Name.Value
            })
            .ToListAsync(cancellationToken);

        return organizationUnits
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new OrganizationUnitSelectionOptionDto(
                x.Id,
                x.Name))
            .ToList();
    }

    private static void AppendTree(
        MachineRow current,
        int level,
        IReadOnlyDictionary<Guid, List<MachineRow>> childrenLookup,
        ICollection<MachineListItem> output)
    {
        var hasChildren = childrenLookup.ContainsKey(current.Id);

        output.Add(new MachineListItem(
            current.Id,
            current.Name,
            current.InventoryNumber,
            current.Location,
            current.Status,
            current.Manufacturer,
            current.Model,
            current.SerialNumber,
            current.ParentMachineId,
            current.ParentMachineName,
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

    private static void AppendSelectionTree(
        SelectionRow current,
        int level,
        IReadOnlyDictionary<Guid, List<SelectionRow>> childrenLookup,
        ICollection<MachineSelectionOptionDto> output)
    {
        output.Add(new MachineSelectionOptionDto(
            current.Id,
            current.Name,
            current.InventoryNumber,
            level));

        if (!childrenLookup.TryGetValue(current.Id, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            AppendSelectionTree(child, level + 1, childrenLookup, output);
        }
    }

    private sealed record MachineRow(
        Guid Id,
        string Name,
        string InventoryNumber,
        string Location,
        string Status,
        string Manufacturer,
        string Model,
        string SerialNumber,
        Guid? ParentMachineId,
        string? ParentMachineName,
        DateTime CreatedAtUtc,
        string CreatedBy,
        DateTime? LastModifiedAtUtc,
        string? LastModifiedBy);

    private sealed record SelectionRow(
        Guid Id,
        string Name,
        string InventoryNumber,
        Guid? ParentMachineId);
}

