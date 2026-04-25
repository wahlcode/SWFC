using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.ShiftModels;

public sealed record GetShiftModelByIdQuery(Guid Id);

public sealed class GetShiftModelByIdValidator : ICommandValidator<GetShiftModelByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetShiftModelByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Shift model id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetShiftModelByIdPolicy : IAuthorizationPolicy<GetShiftModelByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetShiftModelByIdQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetShiftModelByIdHandler : IUseCaseHandler<GetShiftModelByIdQuery, ShiftModelDetailsDto>
{
    private readonly IShiftModelReadRepository _shiftModelReadRepository;

    public GetShiftModelByIdHandler(IShiftModelReadRepository shiftModelReadRepository)
    {
        _shiftModelReadRepository = shiftModelReadRepository;
    }

    public async Task<Result<ShiftModelDetailsDto>> HandleAsync(
        GetShiftModelByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var shiftModel = await _shiftModelReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (shiftModel is null)
        {
            return Result<ShiftModelDetailsDto>.Failure(
                new Error(
                    "m102.shift_model.not_found",
                    "Shift model was not found.",
                    ErrorCategory.NotFound));
        }

        var dto = new ShiftModelDetailsDto(
            shiftModel.Id,
            shiftModel.Name.Value,
            shiftModel.Code.Value,
            shiftModel.Description,
            shiftModel.IsActive);

        return Result<ShiftModelDetailsDto>.Success(dto);
    }
}
