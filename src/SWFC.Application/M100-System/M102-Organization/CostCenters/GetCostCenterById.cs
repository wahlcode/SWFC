using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.CostCenters;

public sealed record GetCostCenterByIdQuery(Guid Id);

public sealed class GetCostCenterByIdValidator : ICommandValidator<GetCostCenterByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetCostCenterByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Cost center id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetCostCenterByIdPolicy : IAuthorizationPolicy<GetCostCenterByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetCostCenterByIdQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetCostCenterByIdHandler : IUseCaseHandler<GetCostCenterByIdQuery, CostCenterDetailsDto>
{
    private readonly ICostCenterReadRepository _costCenterReadRepository;

    public GetCostCenterByIdHandler(ICostCenterReadRepository costCenterReadRepository)
    {
        _costCenterReadRepository = costCenterReadRepository;
    }

    public async Task<Result<CostCenterDetailsDto>> HandleAsync(
        GetCostCenterByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var costCenter = await _costCenterReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (costCenter is null)
        {
            return Result<CostCenterDetailsDto>.Failure(
                new Error(
                    "m102.cost_center.not_found",
                    "Cost center was not found.",
                    ErrorCategory.NotFound));
        }

        string? parentName = null;
        string? parentCode = null;

        if (costCenter.ParentCostCenterId.HasValue)
        {
            var parent = await _costCenterReadRepository.GetByIdAsync(
                costCenter.ParentCostCenterId.Value,
                cancellationToken);

            if (parent is not null)
            {
                parentName = parent.Name.Value;
                parentCode = parent.Code.Value;
            }
        }

        var dto = new CostCenterDetailsDto(
            costCenter.Id,
            costCenter.Name.Value,
            costCenter.Code.Value,
            costCenter.ParentCostCenterId,
            parentName,
            parentCode,
            costCenter.ValidFrom,
            costCenter.IsActive);

        return Result<CostCenterDetailsDto>.Success(dto);
    }
}