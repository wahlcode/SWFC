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

public sealed record CreateOrganizationUnitCommand(
    string Name,
    string Code,
    Guid? ParentId,
    string Reason);

public sealed class CreateOrganizationUnitValidator : ICommandValidator<CreateOrganizationUnitCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateOrganizationUnitPolicy : IAuthorizationPolicy<CreateOrganizationUnitCommand>
{
    public AuthorizationRequirement GetRequirement(CreateOrganizationUnitCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class CreateOrganizationUnitHandler : IUseCaseHandler<CreateOrganizationUnitCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationUnitWriteRepository _organizationUnitWriteRepository;
    private readonly IAuditService _auditService;

    public CreateOrganizationUnitHandler(
        ICurrentUserService currentUserService,
        IOrganizationUnitWriteRepository organizationUnitWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _organizationUnitWriteRepository = organizationUnitWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingOrganizationUnit = await _organizationUnitWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingOrganizationUnit is not null)
        {
            return Result<Guid>.Failure(
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
                return Result<Guid>.Failure(
                    new Error(
                        "m102.organization_unit.parent.not_found",
                        "Parent organization unit was not found.",
                        ErrorCategory.NotFound));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var name = OrganizationUnitName.Create(command.Name);
        var code = OrganizationUnitCode.Create(command.Code);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var organizationUnit = OrganizationUnit.Create(
            name,
            code,
            command.ParentId,
            changeContext);

        await _organizationUnitWriteRepository.AddAsync(organizationUnit, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "CreateOrganizationUnit",
            entity: "OrganizationUnit",
            entityId: organizationUnit.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                organizationUnit.Id,
                Name = organizationUnit.Name.Value,
                Code = organizationUnit.Code.Value,
                ParentId = organizationUnit.ParentOrganizationUnitId,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _organizationUnitWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(organizationUnit.Id);
    }
}
