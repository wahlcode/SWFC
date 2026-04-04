namespace SWFC.Application.M200_Business.M204_Inventory.Commands;

public sealed record ConsumeStockReservationCommand(
    Guid StockReservationId,
    int Quantity,
    string Reason);