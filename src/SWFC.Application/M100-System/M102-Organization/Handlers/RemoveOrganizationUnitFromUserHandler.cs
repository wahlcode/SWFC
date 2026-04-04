using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class RemoveOrganizationUnitFromUserHandler : IUseCaseHandler<RemoveOrganizationUnitFromUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly IAuditService _auditService;

    public RemoveOrganizationUnitFromUserHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _organizationUnitReadRepository = organizationUnitReadRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        RemoveOrganizationUnitFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var user = await _userWriteRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.user.not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(command.OrganizationUnitId, cancellationToken);
        if (organizationUnit is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.organization_unit.not_found",
                    "Organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        user.RemoveOrganizationUnit(organizationUnit.Id);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "RemoveOrganizationUnitFromUser",
            entity: "UserOrganizationUnit",
            entityId: $"{user.Id}:{organizationUnit.Id}",
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: JsonSerializer.Serialize(new
            {
                UserId = user.Id,
                OrganizationUnitId = organizationUnit.Id,
                OrganizationUnitName = organizationUnit.Name.Value
            }),
            newValues: null,
            cancellationToken: cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}