using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

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
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

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

        var name = OrganizationUnitName.Create(command.Name);
        var code = OrganizationUnitCode.Create(command.Code);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var organizationUnit = OrganizationUnit.Create(
            name,
            code,
            command.ParentOrganizationUnitId,
            changeContext);

        await _organizationUnitWriteRepository.AddAsync(organizationUnit, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
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
                organizationUnit.ParentOrganizationUnitId,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _organizationUnitWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(organizationUnit.Id);
    }
}