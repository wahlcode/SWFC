namespace SWFC.Application.M200_Business.M204_Inventory.Commands;

public sealed record ReleaseStockReservationCommand(
    Guid ReservationId,
    string Reason);