using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;

namespace SWFC.Application.M100_System.M102_Organization.CostCenters;

public sealed record UpdateCostCenterCommand(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    DateOnly ValidFrom,
    bool IsActive,
    string? Reason);

public sealed class UpdateCostCenterValidator : ICommandValidator<UpdateCostCenterCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateCostCenterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Cost center id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Cost center name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            result.Add("Code", "Cost center code is required.");
        }

        if (command.ParentId.HasValue && command.ParentId.Value == Guid.Empty)
        {
            result.Add("ParentId", "Parent id is invalid.");
        }

        if (command.ParentId.HasValue && command.ParentId.Value == command.Id)
        {
            result.Add("ParentId", "A cost center cannot be its own parent.");
        }

        if (command.ValidFrom == default)
        {
            result.Add("ValidFrom", "Valid from is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateCostCenterPolicy : IAuthorizationPolicy<UpdateCostCenterCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateCostCenterCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class UpdateCostCenterHandler : IUseCaseHandler<UpdateCostCenterCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICostCenterWriteRepository _costCenterWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateCostCenterHandler(
        ICurrentUserService currentUserService,
        ICostCenterWriteRepository costCenterWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _costCenterWriteRepository = costCenterWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateCostCenterCommand command,
        CancellationToken cancellationToken = default)
    {
        var costCenter = await _costCenterWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (costCenter is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.cost_center.not_found",
                    "Cost center was not found.",
                    ErrorCategory.NotFound));
        }

        var existingByCode = await _costCenterWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingByCode is not null && existingByCode.Id != command.Id)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.cost_center.code.exists",
                    "A cost center with the same code already exists.",
                    ErrorCategory.Conflict));
        }

        if (command.ParentId.HasValue)
        {
            var parent = await _costCenterWriteRepository.GetByIdAsync(
                command.ParentId.Value,
                cancellationToken);

            if (parent is null)
            {
                return Result<bool>.Failure(
                    new Error(
                        "m102.cost_center.parent.not_found",
                        "Parent cost center was not found.",
                        ErrorCategory.NotFound));
            }

            if (await CreatesCycleAsync(costCenter.Id, command.ParentId.Value, cancellationToken))
            {
                return Result<bool>.Failure(
                    new Error(
                        "m102.cost_center.parent.cycle",
                        "Parent cost center would create a cycle.",
                        ErrorCategory.Validation));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var reason = string.IsNullOrWhiteSpace(command.Reason)
            ? "Cost center updated"
            : command.Reason.Trim();

        var changeContext = ChangeContext.Create(securityContext.UserId, reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            costCenter.Id,
            Name = costCenter.Name.Value,
            Code = costCenter.Code.Value,
            ParentId = costCenter.ParentCostCenterId,
            costCenter.ValidFrom,
            costCenter.IsActive
        });

        costCenter.UpdateDetails(
            CostCenterName.Create(command.Name),
            CostCenterCode.Create(command.Code),
            command.ParentId,
            command.ValidFrom,
            changeContext);

        if (command.IsActive)
        {
            costCenter.Activate(changeContext);
        }
        else
        {
            costCenter.Deactivate(changeContext);
        }

        var newValues = JsonSerializer.Serialize(new
        {
            costCenter.Id,
            Name = costCenter.Name.Value,
            Code = costCenter.Code.Value,
            ParentId = costCenter.ParentCostCenterId,
            costCenter.ValidFrom,
            costCenter.IsActive,
            Reason = reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "UpdateCostCenter",
            entity: "CostCenter",
            entityId: costCenter.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _costCenterWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private async Task<bool> CreatesCycleAsync(
        Guid costCenterId,
        Guid parentId,
        CancellationToken cancellationToken)
    {
        var currentParentId = parentId;

        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == costCenterId)
            {
                return true;
            }

            var parent = await _costCenterWriteRepository.GetByIdAsync(currentParentId, cancellationToken);

            if (parent?.ParentCostCenterId is null)
            {
                return false;
            }

            currentParentId = parent.ParentCostCenterId.Value;
        }

        return false;
    }
}