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

public sealed record CreateCostCenterCommand(
    string Name,
    string Code,
    Guid? ParentId,
    DateOnly ValidFrom,
    string? Reason);

public sealed class CreateCostCenterValidator : ICommandValidator<CreateCostCenterCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateCostCenterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

        if (command.ValidFrom == default)
        {
            result.Add("ValidFrom", "Valid from is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateCostCenterPolicy : IAuthorizationPolicy<CreateCostCenterCommand>
{
    public AuthorizationRequirement GetRequirement(CreateCostCenterCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class CreateCostCenterHandler : IUseCaseHandler<CreateCostCenterCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICostCenterWriteRepository _costCenterWriteRepository;
    private readonly IAuditService _auditService;

    public CreateCostCenterHandler(
        ICurrentUserService currentUserService,
        ICostCenterWriteRepository costCenterWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _costCenterWriteRepository = costCenterWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateCostCenterCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingCostCenter = await _costCenterWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingCostCenter is not null)
        {
            return Result<Guid>.Failure(
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
                return Result<Guid>.Failure(
                    new Error(
                        "m102.cost_center.parent.not_found",
                        "Parent cost center was not found.",
                        ErrorCategory.NotFound));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var reason = string.IsNullOrWhiteSpace(command.Reason)
            ? "Cost center created"
            : command.Reason.Trim();

        var changeContext = ChangeContext.Create(securityContext.UserId, reason);

        var costCenter = CostCenter.Create(
            CostCenterName.Create(command.Name),
            CostCenterCode.Create(command.Code),
            command.ParentId,
            command.ValidFrom,
            changeContext);

        await _costCenterWriteRepository.AddAsync(costCenter, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "CreateCostCenter",
            entity: "CostCenter",
            entityId: costCenter.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                costCenter.Id,
                Name = costCenter.Name.Value,
                Code = costCenter.Code.Value,
                ParentId = costCenter.ParentCostCenterId,
                costCenter.ValidFrom,
                costCenter.IsActive,
                Reason = reason
            }),
            cancellationToken: cancellationToken);

        await _costCenterWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(costCenter.Id);
    }
}