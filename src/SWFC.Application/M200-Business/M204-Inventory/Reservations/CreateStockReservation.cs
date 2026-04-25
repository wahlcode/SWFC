using SWFC.Domain.M200_Business.M204_Inventory.Errors;
using System.Text.Json;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Reservations;

public sealed record CreateStockReservationCommand(
    Guid StockId,
    int Quantity,
    string Note,
    string Reason,
    InventoryTargetType? TargetType,
    string? TargetReference);

public sealed class CreateStockReservationValidator : ICommandValidator<CreateStockReservationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.StockId == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Stock id is required.");
        }

        if (command.Quantity <= 0)
        {
            result.Add(ValidationErrorCodes.Invalid, "Reservation quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(InventoryErrorCodes.ReasonRequired, "Reason is required.");
        }

        if (command.TargetType is null && !string.IsNullOrWhiteSpace(command.TargetReference))
        {
            result.Add(ValidationErrorCodes.Invalid, "TargetType is required when TargetReference is set.");
        }

        if (command.TargetType is not null && string.IsNullOrWhiteSpace(command.TargetReference))
        {
            result.Add(ValidationErrorCodes.Invalid, "TargetReference is required when TargetType is set.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateStockReservationPolicy : IAuthorizationPolicy<CreateStockReservationCommand>
{
    public AuthorizationRequirement GetRequirement(CreateStockReservationCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockreservation.create" });
    }
}

public sealed class CreateStockReservationHandler : IUseCaseHandler<CreateStockReservationCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockReservationWriteRepository _stockReservationWriteRepository;
    private readonly IAuditService _auditService;

    public CreateStockReservationHandler(
        ICurrentUserService currentUserService,
        IStockReservationWriteRepository stockReservationWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _stockReservationWriteRepository = stockReservationWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var stock = await _stockReservationWriteRepository.GetStockByIdAsync(command.StockId, cancellationToken);

        if (stock is null)
        {
            return Result<Guid>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Stock '{command.StockId}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.LocationId,
            stock.Bin,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity()
        });

        StockReservation reservation;

        try
        {
            reservation = StockReservation.Create(
                command.StockId,
                command.Quantity,
                command.Note,
                command.TargetType,
                command.TargetReference,
                changeContext);

            stock.AddReservation(reservation, changeContext);
        }
        catch (ValidationException)
        {
            return Result<Guid>.Failure(new Error(
                ValidationErrorCodes.Invalid,
                "Reservation quantity exceeds available stock or contains invalid data.",
                ErrorCategory.Validation));
        }

        await _stockReservationWriteRepository.AddAsync(reservation, cancellationToken);

        var newValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.LocationId,
            stock.Bin,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity(),
            ReservationId = reservation.Id,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            reservation.TargetType,
            reservation.TargetReference,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateStockReservation",
            entity: "StockReservation",
            entityId: reservation.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _stockReservationWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reservation.Id);
    }
}



