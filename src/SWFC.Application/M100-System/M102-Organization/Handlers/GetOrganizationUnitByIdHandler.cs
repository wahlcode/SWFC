using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

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
        var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(command.OrganizationUnitId, cancellationToken);

        if (organizationUnit is null)
        {
            return Result<OrganizationUnitDetailsDto>.Failure(
                new Error(
                    "m102.organization_unit.not_found",
                    "Organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        SWFC.Domain.M100_System.M102_Organization.Entities.OrganizationUnit? parentOrganizationUnit = null;

        if (organizationUnit.ParentOrganizationUnitId.HasValue)
        {
            parentOrganizationUnit = await _organizationUnitReadRepository.GetByIdAsync(
                organizationUnit.ParentOrganizationUnitId.Value,
                cancellationToken);
        }

        var dto = new OrganizationUnitDetailsDto(
            organizationUnit.Id,
            organizationUnit.Name.Value,
            organizationUnit.Code.Value,
            organizationUnit.ParentOrganizationUnitId,
            parentOrganizationUnit?.Name.Value,
            parentOrganizationUnit?.Code.Value);

        return Result<OrganizationUnitDetailsDto>.Success(dto);
    }
}