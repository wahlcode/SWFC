using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.OrganizationUnits;

public sealed record GetOrganizationUnitByIdQuery(Guid Id);

public sealed class GetOrganizationUnitByIdValidator : ICommandValidator<GetOrganizationUnitByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetOrganizationUnitByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Organization unit id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetOrganizationUnitByIdPolicy : IAuthorizationPolicy<GetOrganizationUnitByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetOrganizationUnitByIdQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetOrganizationUnitByIdHandler : IUseCaseHandler<GetOrganizationUnitByIdQuery, OrganizationUnitDetailsDto>
{
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;

    public GetOrganizationUnitByIdHandler(IOrganizationUnitReadRepository organizationUnitReadRepository)
    {
        _organizationUnitReadRepository = organizationUnitReadRepository;
    }

    public async Task<Result<OrganizationUnitDetailsDto>> HandleAsync(
        GetOrganizationUnitByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (organizationUnit is null)
        {
            return Result<OrganizationUnitDetailsDto>.Failure(
                new Error(
                    "m102.organization_unit.not_found",
                    "Organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        string? parentName = null;
        string? parentCode = null;

        if (organizationUnit.ParentOrganizationUnitId.HasValue)
        {
            var parent = await _organizationUnitReadRepository.GetByIdAsync(
                organizationUnit.ParentOrganizationUnitId.Value,
                cancellationToken);

            if (parent is not null)
            {
                parentName = parent.Name.Value;
                parentCode = parent.Code.Value;
            }
        }

        var dto = new OrganizationUnitDetailsDto(
            organizationUnit.Id,
            organizationUnit.Name.Value,
            organizationUnit.Code.Value,
            organizationUnit.ParentOrganizationUnitId,
            parentName,
            parentCode,
            organizationUnit.IsActive);

        return Result<OrganizationUnitDetailsDto>.Success(dto);
    }
}
