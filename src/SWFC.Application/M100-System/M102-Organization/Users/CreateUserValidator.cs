using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed class CreateUserValidator : ICommandValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        Require(result, "IdentityKey", command.IdentityKey);
        Require(result, "Username", command.Username);
        Require(result, "DisplayName", command.DisplayName);
        Require(result, "FirstName", command.FirstName);
        Require(result, "LastName", command.LastName);
        Require(result, "EmployeeNumber", command.EmployeeNumber);
        Require(result, "BusinessEmail", command.BusinessEmail);
        Require(result, "BusinessPhone", command.BusinessPhone);
        Require(result, "Plant", command.Plant);
        Require(result, "Location", command.Location);
        Require(result, "Team", command.Team);
        Require(result, "JobFunction", command.JobFunction);
        Require(result, "Reason", command.Reason);

        if (command.PrimaryOrganizationUnitId == Guid.Empty)
        {
            result.Add("PrimaryOrganizationUnitId", "PrimaryOrganizationUnitId is required.");
        }

        if (command.CostCenterId.HasValue && command.CostCenterId.Value == Guid.Empty)
        {
            result.Add("CostCenterId", "CostCenterId is invalid.");
        }

        if (command.ShiftModelId.HasValue && command.ShiftModelId.Value == Guid.Empty)
        {
            result.Add("ShiftModelId", "ShiftModelId is invalid.");
        }

        return Task.FromResult(result);
    }

    private static void Require(ValidationResult result, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(field, $"{field} is required.");
        }
    }
}
