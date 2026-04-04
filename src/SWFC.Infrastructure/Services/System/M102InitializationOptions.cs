namespace SWFC.Infrastructure.Services.System;

public sealed class M102InitializationOptions
{
    public string AdminRoleName { get; set; } = "Admin";
    public string DeveloperIdentityKey { get; set; } = "local-admin";
    public string DeveloperDisplayName { get; set; } = "Developer";
    public bool CreateRootOrganizationUnit { get; set; } = true;
    public string RootOrganizationUnitCode { get; set; } = "ROOT";
    public string RootOrganizationUnitName { get; set; } = "ROOT";
}