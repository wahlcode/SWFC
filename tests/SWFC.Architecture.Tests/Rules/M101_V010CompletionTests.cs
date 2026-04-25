using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.Rules;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M101_V010CompletionTests
{
    [Fact]
    public void M101_Result_Error_Handling_Should_Enforce_Documented_Invariants()
    {
        // Result / Error Handling
        var success = Result.Success();
        var failure = Result.Failure(new Error("m101.failure", "Failure", ErrorCategory.Technical));

        Assert.True(success.IsSuccess);
        Assert.False(success.IsFailure);
        Assert.Equal(Error.None, success.Error);

        Assert.True(failure.IsFailure);
        Assert.Equal("m101.failure", failure.Error.Code);
    }

    [Fact]
    public void M101_Guards_Should_Reject_Invalid_Input()
    {
        // Guards
        Assert.Throws<ValidationException>(() => Guard.AgainstNull(null, "Value"));
        Assert.Throws<ValidationException>(() => Guard.AgainstNullOrWhiteSpace(" ", "Value"));
        Assert.Throws<ValidationException>(() => Guard.AgainstMaxLength("toolong", 3, "Value"));
    }

    [Fact]
    public void M101_ValueObjects_Basis_Primitives_Globale_ID_Grundlagen_Zeit_Timestamp_Basis_UTC_And_Einheiten_Basis_Should_Work()
    {
        // ValueObjects / Basis-Primitives
        // Globale ID-Grundlagen
        var systemId = SystemId.New();
        var parsedId = SystemId.Parse(systemId.ToString());

        Assert.NotEqual(Guid.Empty, systemId.Value);
        Assert.Equal(systemId.Value, parsedId.Value);

        // Zeit-/Timestamp-Basis (UTC)
        var localTime = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Local);
        var timestamp = UtcTimestamp.From(localTime);

        Assert.Equal(DateTimeKind.Utc, timestamp.UtcDateTime.Kind);

        // Einheiten-Basis
        var unit = MeasurementUnit.Create("  kWh  ");

        Assert.Equal("kWh", unit.Value);
    }

    [Fact]
    public void M101_Audit_Basisfelder_Should_Be_Represented_By_AuditInfo()
    {
        // Audit-Basisfelder
        var createdAtUtc = new DateTime(2026, 4, 25, 10, 30, 0, DateTimeKind.Utc);
        var auditInfo = new AuditInfo(createdAtUtc, "system", createdAtUtc.AddMinutes(5), "developer");

        Assert.Equal(createdAtUtc, auditInfo.CreatedAtUtc);
        Assert.Equal("system", auditInfo.CreatedBy);
        Assert.Equal(createdAtUtc.AddMinutes(5), auditInfo.LastModifiedAtUtc);
        Assert.Equal("developer", auditInfo.LastModifiedBy);
    }
}
