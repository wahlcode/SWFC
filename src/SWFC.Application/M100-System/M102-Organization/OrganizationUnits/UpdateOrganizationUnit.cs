using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;

namespace SWFC.Application.M100_System.M102_Organization.OrganizationUnits;

public sealed record UpdateOrganizationUnitCommand(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    bool IsActive,
    string Reason);

public sealed class UpdateOrganizationUnitValidator : ICommandValidator<UpdateOrganizationUnitCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateOrganizationUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Organization unit id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Organization unit name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            result.Add("Code", "Organization unit code is required.");
        }

        if (command.ParentId.HasValue && command.ParentId.Value == Guid.Empty)
        {
            result.Add("ParentId", "Parent id is invalid.");
        }

        if (command.ParentId.HasValue && command.ParentId.Value == command.Id)
        {
            result.Add("ParentId", "An organization unit cannot be its own parent.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateOrganizationUnitPolicy : IAuthorizationPolicy<UpdateOrganizationUnitCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateOrganizationUnitCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class UpdateOrganizationUnitHandler : IUseCaseHandler<UpdateOrganizationUnitCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationUnitWriteRepository _organizationUnitWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateOrganizationUnitHandler(
        ICurrentUserService currentUserService,
        IOrganizationUnitWriteRepository organizationUnitWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _organizationUnitWriteRepository = organizationUnitWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateOrganizationUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var organizationUnit = await _organizationUnitWriteRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (organizationUnit is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.organization_unit.not_found",
                    "Organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        var existingByCode = await _organizationUnitWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingByCode is not null && existingByCode.Id != command.Id)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.organization_unit.code.exists",
                    "An organization unit with the same code already exists.",
                    ErrorCategory.Conflict));
        }

        if (command.ParentId.HasValue)
        {
            var parent = await _organizationUnitWriteRepository.GetByIdAsync(
                command.ParentId.Value,
                cancellationToken);

            if (parent is null)
            {
                return Result<bool>.Failure(
                    new Error(
                        "m102.organization_unit.parent.not_found",
                        "Parent organization unit was not found.",
                        ErrorCategory.NotFound));
            }

            if (await CreatesCycleAsync(
                    organizationUnit.Id,
                    command.ParentId.Value,
                    cancellationToken))
            {
                return Result<bool>.Failure(
                    new Error(
                        "m102.organization_unit.parent.cycle",
                        "Parent organization unit would create a cycle.",
                        ErrorCategory.Validation));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var oldValues = JsonSerializer.Serialize(new
        {
            organizationUnit.Id,
            Name = organizationUnit.Name.Value,
            Code = organizationUnit.Code.Value,
            ParentId = organizationUnit.ParentOrganizationUnitId,
            organizationUnit.IsActive
        });

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        organizationUnit.UpdateDetails(
            OrganizationUnitName.Create(command.Name),
            OrganizationUnitCode.Create(command.Code),
            command.ParentId,
            changeContext);

        if (command.IsActive)
        {
            organizationUnit.Activate(changeContext);
        }
        else
        {
            organizationUnit.Deactivate(changeContext);
        }

        var newValues = JsonSerializer.Serialize(new
        {
            organizationUnit.Id,
            Name = organizationUnit.Name.Value,
            Code = organizationUnit.Code.Value,
            ParentId = organizationUnit.ParentOrganizationUnitId,
            organizationUnit.IsActive,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "UpdateOrganizationUnit",
            entity: "OrganizationUnit",
            entityId: organizationUnit.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _organizationUnitWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private async Task<bool> CreatesCycleAsync(
        Guid organizationUnitId,
        Guid parentId,
        CancellationToken cancellationToken)
    {
        var currentParentId = parentId;

        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == organizationUnitId)
            {
                return true;
            }

            var currentParent = await _organizationUnitWriteRepository.GetByIdAsync(
                currentParentId,
                cancellationToken);

            if (currentParent?.ParentOrganizationUnitId is null)
            {
                return false;
            }

            currentParentId = currentParent.ParentOrganizationUnitId.Value;
        }

        return false;
    }
}
