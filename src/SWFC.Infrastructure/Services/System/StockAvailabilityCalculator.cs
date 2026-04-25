using System.Collections;
using System.Reflection;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;

namespace SWFC.Infrastructure.Services.System;

public sealed class StockAvailabilityCalculator : IInventoryAvailabilityCalculator
{
    public StockAvailabilityResult Calculate(StockEntity stock)
    {
        if (stock is null)
        {
            throw new ArgumentNullException(nameof(stock));
        }

        var quantityOnHand = ReadInt(stock, "QuantityOnHand");
        var reservedQuantity = ReadReservedQuantity(stock);

        return new StockAvailabilityResult(
            QuantityOnHand: quantityOnHand,
            ReservedQuantity: reservedQuantity,
            AvailableQuantity: Math.Max(0, quantityOnHand - reservedQuantity));
    }

    private static int ReadReservedQuantity(StockEntity stock)
    {
        var reservationsProperty = stock.GetType().GetProperty(
            "Reservations",
            BindingFlags.Instance | BindingFlags.Public);

        if (reservationsProperty?.GetValue(stock) is not IEnumerable reservations)
        {
            return 0;
        }

        var sum = 0;

        foreach (var reservation in reservations)
        {
            if (reservation is null)
            {
                continue;
            }

            if (!IsReservationActive(reservation))
            {
                continue;
            }

            sum += ReadInt(reservation, "ReservedQuantity", "Quantity", "Amount");
        }

        return sum;
    }

    private static bool IsReservationActive(object reservation)
    {
        var statusProperty = reservation.GetType().GetProperty(
            "Status",
            BindingFlags.Instance | BindingFlags.Public);

        var statusValue = statusProperty?.GetValue(reservation)?.ToString();

        if (string.IsNullOrWhiteSpace(statusValue))
        {
            return true;
        }

        return !string.Equals(statusValue, "Released", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(statusValue, "Consumed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(statusValue, "Cancelled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(statusValue, "Canceled", StringComparison.OrdinalIgnoreCase);
    }

    private static int ReadInt(object source, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = source.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(source);

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return checked((int)longValue);
            }

            if (value is short shortValue)
            {
                return shortValue;
            }

            if (value is byte byteValue)
            {
                return byteValue;
            }

            if (value is null)
            {
                continue;
            }

            if (int.TryParse(value.ToString(), out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }
}
