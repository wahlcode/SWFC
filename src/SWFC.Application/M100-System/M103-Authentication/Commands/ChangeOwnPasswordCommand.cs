namespace SWFC.Application.M100_System.M103_Authentication.Commands;

public sealed record ChangeOwnPasswordCommand(
    string CurrentPassword,
    string NewPassword);