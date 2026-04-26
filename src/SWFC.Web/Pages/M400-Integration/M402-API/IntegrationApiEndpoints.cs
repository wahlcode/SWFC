using Microsoft.AspNetCore.Http.HttpResults;
using SWFC.Application.M200_Business.M201_Assets.Machines;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Infrastructure.M400_Integration.M402_API;

namespace SWFC.Web.Pages.M400_Integration.M402_API;

public static class IntegrationApiEndpoints
{
    public static IEndpointRouteBuilder MapM402IntegrationApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1")
            .RequireAuthorization()
            .WithTags("M402 API");

        group.MapGet("/integration/modules", (IntegrationApiCatalog catalog) =>
            Results.Ok(catalog.GetEndpoints()));

        group.MapGet("/assets/machines", async (
            IExecutionPipeline<GetVisibleMachinesQuery, IReadOnlyList<MachineListItem>> pipeline,
            CancellationToken cancellationToken) =>
        {
            var result = await pipeline.ExecuteAsync(new GetVisibleMachinesQuery(), cancellationToken);
            return ToHttpResult(result);
        });

        group.MapGet("/inventory/items", async (
            IExecutionPipeline<GetInventoryItemsQuery, IReadOnlyList<InventoryItemListItem>> pipeline,
            CancellationToken cancellationToken) =>
        {
            var result = await pipeline.ExecuteAsync(new GetInventoryItemsQuery(), cancellationToken);
            return ToHttpResult(result);
        });

        group.MapPost("/energy/readings", async (
            CreateEnergyReadingApiRequest request,
            IExecutionPipeline<CreateEnergyReadingCommand, Guid> pipeline,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateEnergyReadingCommand(
                request.MeterId,
                request.Date,
                request.Value,
                request.Source,
                request.CapturedByUserId,
                request.CaptureContext,
                request.RfidTag,
                request.RfidExceptionReason,
                request.OfflineCaptureId,
                request.CapturedOfflineAtUtc,
                request.SyncedAtUtc,
                request.PlausibilityStatus,
                request.PlausibilityNote,
                request.Reason);

            var result = await pipeline.ExecuteAsync(command, cancellationToken);
            return ToHttpResult(result);
        });

        group.MapPut("/energy/readings/{id:guid}", async (
            Guid id,
            UpdateEnergyReadingApiRequest request,
            IExecutionPipeline<UpdateEnergyReadingCommand, Guid> pipeline,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateEnergyReadingCommand(
                id,
                request.Date,
                request.Value,
                request.Source,
                request.CapturedByUserId,
                request.CaptureContext,
                request.RfidTag,
                request.RfidExceptionReason,
                request.OfflineCaptureId,
                request.CapturedOfflineAtUtc,
                request.SyncedAtUtc,
                request.PlausibilityStatus,
                request.PlausibilityNote,
                request.Reason);

            var result = await pipeline.ExecuteAsync(command, cancellationToken);
            return ToHttpResult(result);
        });

        return endpoints;
    }

    private static Results<Ok<T>, ProblemHttpResult> ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return TypedResults.Ok(result.Value);
        }

        return TypedResults.Problem(
            result.Error.Message,
            statusCode: result.Error.Category == ErrorCategory.Security ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest,
            title: result.Error.Code);
    }
}

public sealed record CreateEnergyReadingApiRequest(
    Guid MeterId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    string? CapturedByUserId,
    string? CaptureContext,
    string? RfidTag,
    string? RfidExceptionReason,
    Guid? OfflineCaptureId,
    DateTime? CapturedOfflineAtUtc,
    DateTime? SyncedAtUtc,
    EnergyReadingPlausibilityStatus PlausibilityStatus,
    string? PlausibilityNote,
    string Reason);

public sealed record UpdateEnergyReadingApiRequest(
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    string? CapturedByUserId,
    string? CaptureContext,
    string? RfidTag,
    string? RfidExceptionReason,
    Guid? OfflineCaptureId,
    DateTime? CapturedOfflineAtUtc,
    DateTime? SyncedAtUtc,
    EnergyReadingPlausibilityStatus PlausibilityStatus,
    string? PlausibilityNote,
    string Reason);
