namespace SWFC.Application.M100_System.M102_Organization.ShiftModels;

public sealed record ShiftModelDetailsDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public sealed record ShiftModelListItem(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public sealed record ShiftModelSelectionOptionDto(
    Guid Id,
    string Name,
    string Code);
