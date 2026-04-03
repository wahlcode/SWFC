using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class ReleaseStockReservationHandler : IUseCaseHandler<ReleaseStockReservationCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockReservationWriteRepository _stockReservationWriteRepository;
    private readonly IAuditService _auditService;

    public ReleaseStockReservationHandler(
        ICurrentUserService currentUserService,
        IStockReservationWriteRepository stockReservationWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _stockReservationWriteRepository = stockReservationWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        ReleaseStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _stockReservationWriteRepository.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
        {
            return Result<bool>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"StockReservation '{command.ReservationId}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            reservation.Id,
            reservation.StockId,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            reservation.AuditInfo.CreatedAtUtc,
            reservation.AuditInfo.CreatedBy,
            reservation.AuditInfo.LastModifiedAtUtc,
            reservation.AuditInfo.LastModifiedBy
        });

        reservation.Release(changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            reservation.Id,
            reservation.StockId,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            reservation.AuditInfo.CreatedAtUtc,
            reservation.AuditInfo.CreatedBy,
            reservation.AuditInfo.LastModifiedAtUtc,
            reservation.AuditInfo.LastModifiedBy,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "ReleaseStockReservation",
            entity: "StockReservation",
            entityId: reservation.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _stockReservationWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}