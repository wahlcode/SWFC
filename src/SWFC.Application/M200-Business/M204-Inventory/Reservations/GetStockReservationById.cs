using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Reservations;

public sealed record GetStockReservationByIdQuery(Guid Id);

public sealed class GetStockReservationByIdValidator : ICommandValidator<GetStockReservationByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetStockReservationByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Reservation id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetStockReservationByIdPolicy : IAuthorizationPolicy<GetStockReservationByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockReservationByIdQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockreservation.read" });
    }
}

public sealed class GetStockReservationByIdHandler : IUseCaseHandler<GetStockReservationByIdQuery, StockReservationDetailsDto>
{
    private readonly IStockReservationReadRepository _stockReservationReadRepository;

    public GetStockReservationByIdHandler(IStockReservationReadRepository stockReservationReadRepository)
    {
        _stockReservationReadRepository = stockReservationReadRepository;
    }

    public async Task<Result<StockReservationDetailsDto>> HandleAsync(
        GetStockReservationByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _stockReservationReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (reservation is null)
        {
            return Result<StockReservationDetailsDto>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"StockReservation '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<StockReservationDetailsDto>.Success(reservation);
    }
}

