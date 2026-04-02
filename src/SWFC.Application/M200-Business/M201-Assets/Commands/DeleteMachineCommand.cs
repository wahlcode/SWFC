namespace SWFC.Application.M200_Business.M201_Assets.Commands;

public sealed record DeleteMachineCommand(
    Guid Id,
    string Reason);