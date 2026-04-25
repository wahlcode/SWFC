using SWFC.Application.M800_Security.M801_Access;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M803_Visibility.AccessRules;

public sealed record GetAccessRulesByTargetQuery(
    AccessTargetType TargetType,
    string TargetId);

public sealed class GetAccessRulesByTargetValidator : ICommandValidator<GetAccessRulesByTargetQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetAccessRulesByTargetQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.TargetId))
        {
            result.Add("TargetId", "Target id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetAccessRulesByTargetPolicy : IAuthorizationPolicy<GetAccessRulesByTargetQuery>
{
    public AuthorizationRequirement GetRequirement(GetAccessRulesByTargetQuery request)
        => new(requiredPermissions: new[] { "machine.read" });
}

public sealed class GetAccessRulesByTargetHandler : IUseCaseHandler<GetAccessRulesByTargetQuery, IReadOnlyList<AccessRuleListItemDto>>
{
    private readonly IAccessRuleReadRepository _readRepository;

    public GetAccessRulesByTargetHandler(IAccessRuleReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyList<AccessRuleListItemDto>>> HandleAsync(
        GetAccessRulesByTargetQuery request,
        CancellationToken cancellationToken = default)
    {
        var rules = await _readRepository.GetByTargetsAsync(
            new[] { new AccessRuleTargetRef(request.TargetType, request.TargetId) },
            cancellationToken);

        var items = rules
            .OrderBy(x => x.SubjectType)
            .ThenBy(x => x.SubjectId, StringComparer.OrdinalIgnoreCase)
            .Select(x => new AccessRuleListItemDto(
                x.Id,
                x.TargetType,
                x.TargetId,
                x.SubjectType,
                x.SubjectId,
                x.Mode,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<AccessRuleListItemDto>>.Success(items);
    }
}
