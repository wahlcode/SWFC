namespace SWFC.Application.M200_Business.M201_Assets.Commands;

public sealed record CreateMachineCommand(
    string Name,
    string Reason);