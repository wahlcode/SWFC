param(
    [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = "Stop"

function Ensure-Dir {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Write-Utf8File {
    param(
        [string]$Path,
        [string]$Content
    )

    $dir = Split-Path $Path -Parent
    if ($dir) { Ensure-Dir $dir }

    $normalized = $Content.TrimStart("`r","`n")
    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
    Write-Host "written: $Path"
}

function Join-Repo {
    param([string]$RelativePath)
    return [System.IO.Path]::Combine($RepoRoot, $RelativePath)
}

# --------------------------------------------------------------------------------------
# PATHS
# --------------------------------------------------------------------------------------

$domainCommonBase         = Join-Repo "src/SWFC.Domain/Common/Base"
$domainCommonInterfaces   = Join-Repo "src/SWFC.Domain/Common/Interfaces"
$domainCommonRules        = Join-Repo "src/SWFC.Domain/Common/Rules"
$domainCommonExceptions   = Join-Repo "src/SWFC.Domain/Common/Exceptions"
$domainCommonResults      = Join-Repo "src/SWFC.Domain/Common/Results"
$domainCommonValueObjects = Join-Repo "src/SWFC.Domain/Common/ValueObjects"
$domainCommonErrors       = Join-Repo "src/SWFC.Domain/Common/Errors"

$appCommonValidation      = Join-Repo "src/SWFC.Application/Common/Validation"

$githubWorkflows          = Join-Repo ".github/workflows"
$githubRoot               = Join-Repo ".github"
$docsDevSecOps            = Join-Repo "docs/devsecops"

Ensure-Dir $domainCommonBase
Ensure-Dir $domainCommonInterfaces
Ensure-Dir $domainCommonRules
Ensure-Dir $domainCommonExceptions
Ensure-Dir $domainCommonResults
Ensure-Dir $domainCommonValueObjects
Ensure-Dir $domainCommonErrors
Ensure-Dir $appCommonValidation
Ensure-Dir $githubWorkflows
Ensure-Dir $docsDevSecOps

# --------------------------------------------------------------------------------------
# M801 - DOMAIN BASE
# --------------------------------------------------------------------------------------

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/SwfcException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public abstract class SwfcException : Exception
{
    protected SwfcException(string message) : base(message)
    {
    }

    protected SwfcException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/DomainException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public sealed class DomainException : SwfcException
{
    public DomainException(string message) : base(message)
    {
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/ValidationException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public sealed class ValidationException : SwfcException
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = Array.Empty<string>();
    }

    public ValidationException(string message, IReadOnlyCollection<string> errors)
        : base(message)
    {
        Errors = errors;
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/SecurityException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public sealed class SecurityException : SwfcException
{
    public SecurityException(string message) : base(message)
    {
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/ConflictException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public sealed class ConflictException : SwfcException
{
    public ConflictException(string message) : base(message)
    {
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Exceptions/NotFoundException.cs") @"
namespace SWFC.Domain.Common.Exceptions;

public sealed class NotFoundException : SwfcException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Results/ErrorCategory.cs") @"
namespace SWFC.Domain.Common.Results;

public enum ErrorCategory
{
    Validation = 1,
    Domain = 2,
    Security = 3,
    Conflict = 4,
    NotFound = 5,
    Technical = 6
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Results/Error.cs") @"
namespace SWFC.Domain.Common.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorCategory Category)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorCategory.Technical);
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Results/Result.cs") @"
namespace SWFC.Domain.Common.Results;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Results/ResultOfT.cs") @"
namespace SWFC.Domain.Common.Results;

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, Error.None);

    public static new Result<T> Failure(Error error) => new(default, false, error);
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/ValueObjects/ChangeContext.cs") @"
namespace SWFC.Domain.Common.ValueObjects;

public sealed class ChangeContext
{
    private ChangeContext(string userId, string reason)
    {
        UserId = userId;
        Reason = reason;
        ChangedAtUtc = DateTime.UtcNow;
    }

    public string UserId { get; }
    public string Reason { get; }
    public DateTime ChangedAtUtc { get; }

    public static ChangeContext Create(string userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        return new ChangeContext(userId.Trim(), reason.Trim());
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/ValueObjects/AuditInfo.cs") @"
namespace SWFC.Domain.Common.ValueObjects;

public sealed class AuditInfo
{
    public AuditInfo(
        DateTime createdAtUtc,
        string createdBy,
        DateTime? lastModifiedAtUtc = null,
        string? lastModifiedBy = null)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
        LastModifiedAtUtc = lastModifiedAtUtc;
        LastModifiedBy = lastModifiedBy;
    }

    public DateTime CreatedAtUtc { get; }
    public string CreatedBy { get; }
    public DateTime? LastModifiedAtUtc { get; }
    public string? LastModifiedBy { get; }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Errors/ErrorCodes.cs") @"
namespace SWFC.Domain.Common.Errors;

public static class ErrorCodes
{
    public static class General
    {
        public const string ValidationFailed = "GEN_VALIDATION_FAILED";
        public const string ContextRequired = "SEC_CONTEXT_REQUIRED";
        public const string Unauthorized = "SEC_UNAUTHORIZED";
        public const string NotFound = "GEN_NOT_FOUND";
        public const string Conflict = "GEN_CONFLICT";
        public const string Unexpected = "GEN_UNEXPECTED";
    }

    public static class Machine
    {
        public const string NameRequired = "MACHINE_NAME_REQUIRED";
        public const string NameTooLong = "MACHINE_NAME_TOO_LONG";
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Domain/Common/Rules/Guard.cs") @"
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.Common.Rules;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new ValidationException($"{name} must not be null.");
    }

    public static void AgainstNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} must not be empty.");
    }

    public static void AgainstMaxLength(string? value, int maxLength, string name)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > maxLength)
            throw new ValidationException($"{name} must not exceed {maxLength} characters.");
    }
}
"@

# --------------------------------------------------------------------------------------
# M801 - APPLICATION VALIDATION
# --------------------------------------------------------------------------------------

Write-Utf8File (Join-Repo "src/SWFC.Application/Common/Validation/ValidationError.cs") @"
namespace SWFC.Application.Common.Validation;

public sealed record ValidationError(string Code, string Message);
"@

Write-Utf8File (Join-Repo "src/SWFC.Application/Common/Validation/ValidationResult.cs") @"
namespace SWFC.Application.Common.Validation;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyCollection<ValidationError> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params ValidationError[] errors)
    {
        var result = new ValidationResult();
        result._errors.AddRange(errors);
        return result;
    }

    public void Add(string code, string message)
    {
        _errors.Add(new ValidationError(code, message));
    }
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Application/Common/Validation/ICommandValidator.cs") @"
using SWFC.Application.Common.Validation;

namespace SWFC.Application.Common.Validation;

public interface ICommandValidator<in TCommand>
{
    Task<ValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}
"@

Write-Utf8File (Join-Repo "src/SWFC.Application/Common/Validation/ValidatedHandler.cs") @"
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.Common.Validation;

public abstract class ValidatedHandler<TCommand, TResult>
{
    private readonly ICommandValidator<TCommand> _validator;

    protected ValidatedHandler(ICommandValidator<TCommand> validator)
    {
        _validator = validator;
    }

    public async Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));
            return Result<TResult>.Failure(
                new Error(
                    ErrorCodes.General.ValidationFailed,
                    message,
                    ErrorCategory.Validation));
        }

        return await HandleValidatedAsync(command, cancellationToken);
    }

    protected abstract Task<Result<TResult>> HandleValidatedAsync(TCommand command, CancellationToken cancellationToken);
}
"@

# --------------------------------------------------------------------------------------
# M804 - PULL REQUEST TEMPLATE
# --------------------------------------------------------------------------------------

Write-Utf8File (Join-Repo ".github/PULL_REQUEST_TEMPLATE.md") @"
## Summary
<!-- Briefly describe the change -->

## Changes
- 

## Task-ID
<!-- Example: M801.01 -->

## Modul
<!-- Example: M801 -->

## CORE / EXT
<!-- CORE or EXT -->

## Betroffene Layer
- Domain
- Application
- Infrastructure
- Web

## Kurzbeschreibung
<!-- One short paragraph -->

## Risiko / offene Punkte
- 

## Checkliste
- [ ] Build erfolgreich
- [ ] Tests erfolgreich
- [ ] Struktur korrekt
- [ ] Architektur eingehalten
- [ ] Keine Secrets
- [ ] Dokumentation aktualisiert
"@

# --------------------------------------------------------------------------------------
# M804 - DOCS
# --------------------------------------------------------------------------------------

Write-Utf8File (Join-Repo "docs/devsecops/M804-DevSecOps-Betriebsregeln.md") @"
# M804 – DevSecOps Betriebsregeln

## 1. Commit-Regeln

Erlaubte Formate:

- `feat(Mxxx.xx): description`
- `fix(Mxxx.xx): description`
- `chore(Mxxx.xx): description`
- `refactor(Mxxx.xx): description`
- `test(Mxxx.xx): description`

## 2. Branch-Regeln

Erlaubte Formate:

- `feature/Mxxx.xx-description`
- `fix/Mxxx.xx-description`
- `chore/Mxxx.xx-description`
- `refactor/Mxxx.xx-description`
- `test/Mxxx.xx-description`

## 3. Pull-Request-Regeln

Jeder PR muss enthalten:

- Task-ID
- Modul
- CORE / EXT
- Beschreibung
- Risiko
- betroffene Layer
- Checkliste

## 4. Pflicht-Checks

- Build
- Test
- Commit Message Check
- Branch Name Check
- Structure Check
- Architecture Check
- Secret Scan
- Dependency Check

## 5. Verhalten bei Fehlern

- Kein Merge bei roten Checks
- Keine Ausnahmen im Alltag
- Fehler zuerst beheben, dann erneut pushen
"@

# --------------------------------------------------------------------------------------
# M804 - WORKFLOWS
# --------------------------------------------------------------------------------------

Write-Utf8File (Join-Repo ".github/workflows/build.yml") @"
name: SWFC CI

on:
  pull_request:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build
"@

Write-Utf8File (Join-Repo ".github/workflows/commit-message-check.yml") @"
name: Commit Message Check

on:
  pull_request:

jobs:
  commit-message-check:
    runs-on: ubuntu-latest

    steps:
      - name: Validate commit messages
        uses: actions/github-script@v7
        with:
          script: |
            const regex = /^(feat|fix|chore|refactor|test)\(M\d{3,4}\.\d{2}\): .+/;
            const commits = await github.paginate(
              github.rest.pulls.listCommits,
              {
                owner: context.repo.owner,
                repo: context.repo.repo,
                pull_number: context.payload.pull_request.number
              }
            );

            const invalid = commits.filter(c => !regex.test(c.commit.message.split('\n')[0]));

            if (invalid.length > 0) {
              core.setFailed(
                'Invalid commit messages:\n' +
                invalid.map(c => `- ${c.sha.substring(0,7)} ${c.commit.message.split('\n')[0]}`).join('\n')
              );
            }
"@

Write-Utf8File (Join-Repo ".github/workflows/branch-name-check.yml") @"
name: Branch Name Check

on:
  pull_request:

jobs:
  branch-name-check:
    runs-on: ubuntu-latest

    steps:
      - name: Validate branch name
        run: |
          $branch = $env:GITHUB_HEAD_REF
          echo "Branch: $BRANCH"
          if [[ ! "$BRANCH" =~ ^(feature|fix|chore|refactor|test)/M[0-9]{3,4}\.[0-9]{2}-[a-z0-9-]+$ ]]; then
            echo "Invalid branch name: $BRANCH"
            exit 1
          fi
"@

Write-Utf8File (Join-Repo ".github/workflows/structure-check.yml") @"
name: Structure Check

on:
  pull_request:

jobs:
  structure-check:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Validate repository root
        run: |
          for dir in src tests docs build .github; do
            [ -d "$dir" ] || { echo "Missing root dir: $dir"; exit 1; }
          done

      - name: Reject task-id folders
        run: |
          if find src tests docs build -type d | grep -E '/M[0-9]{3,4}\.[0-9]{2}'; then
            echo "Task-ID folders are not allowed."
            exit 1
          fi
"@

Write-Utf8File (Join-Repo ".github/workflows/architecture-check.yml") @"
name: Architecture Check

on:
  pull_request:

jobs:
  architecture-check:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Validate project references
        shell: bash
        run: |
          DOMAIN='src/SWFC.Domain/SWFC.Domain.csproj'
          APP='src/SWFC.Application/SWFC.Application.csproj'
          INFRA='src/SWFC.Infrastructure/SWFC.Infrastructure.csproj'
          WEB='src/SWFC.Web/SWFC.Web.csproj'

          if grep -q 'SWFC.Infrastructure' "$DOMAIN"; then
            echo "Domain must not reference Infrastructure"
            exit 1
          fi

          if grep -q 'SWFC.Web' "$DOMAIN"; then
            echo "Domain must not reference Web"
            exit 1
          fi

          if grep -q 'SWFC.Infrastructure' "$APP"; then
            echo "Application must not reference Infrastructure"
            exit 1
          fi

          if grep -q 'SWFC.Web' "$APP"; then
            echo "Application must not reference Web"
            exit 1
          fi

          if ! grep -q 'SWFC.Domain' "$APP"; then
            echo "Application must reference Domain"
            exit 1
          fi

          if ! grep -q 'SWFC.Domain' "$INFRA"; then
            echo "Infrastructure must reference Domain"
            exit 1
          fi

          if ! grep -q 'SWFC.Application' "$INFRA"; then
            echo "Infrastructure must reference Application"
            exit 1
          fi

          if grep -q 'SWFC.Domain' "$WEB"; then
            echo "Web must not reference Domain directly"
            exit 1
          fi
"@

Write-Utf8File (Join-Repo ".github/workflows/secret-scan.yml") @"
name: Secret Scan

on:
  pull_request:

jobs:
  secret-scan:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Scan for obvious secrets
        shell: bash
        run: |
          if grep -RInE '(password\s*=\s*.+|apikey\s*=\s*.+|secret\s*=\s*.+|token\s*=\s*.+)' src tests docs .github --exclude-dir bin --exclude-dir obj; then
            echo "Potential secret detected."
            exit 1
          fi
"@

Write-Utf8File (Join-Repo ".github/workflows/dependency-check.yml") @"
name: Dependency Check

on:
  pull_request:

jobs:
  dependency-check:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Vulnerability check
        shell: bash
        run: |
          set -e
          projects=$(find src tests -name '*.csproj')
          for project in $projects; do
            echo "Checking $project"
            dotnet list "$project" package --vulnerable --include-transitive
          done
"@

Write-Utf8File (Join-Repo ".github/workflows/pr-template-check.yml") @"
name: PR Template Presence Check

on:
  pull_request:
    types: [opened, edited, synchronize]

jobs:
  pr-template-check:
    runs-on: ubuntu-latest

    steps:
      - name: Validate PR body
        uses: actions/github-script@v7
        with:
          script: |
            const body = context.payload.pull_request.body || '';
            const required = ['Task-ID', 'Modul', 'CORE / EXT', 'Beschreibung', 'Checkliste'];
            const missing = required.filter(x => !body.includes(x));
            if (missing.length > 0) {
              core.setFailed('PR body is missing required sections: ' + missing.join(', '));
            }
"@

# --------------------------------------------------------------------------------------
# DONE
# --------------------------------------------------------------------------------------

Write-Host ""
Write-Host "M801 and M804 setup completed."
Write-Host "Next steps:"
Write-Host "1. dotnet restore"
Write-Host "2. dotnet build"
Write-Host "3. dotnet test"