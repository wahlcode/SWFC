namespace SWFC.Infrastructure.M100_System.M107_SetupDeployment;

public sealed class M107SetupOptions
{
    public const string SectionName = "Initialization";

    public string AdminRoleName { get; set; } = "Admin";
    public string SuperAdminRoleName { get; set; } = "SuperAdmin";
    public string LegacyDeveloperRoleName { get; set; } = "Developer";
    public string LegacyDeveloperIdentityKey { get; set; } = "local-admin";
    public string SuperAdminIdentityKey { get; set; } = "stephan.wahl@dbw.de";
    public string SuperAdminDisplayName { get; set; } = "Stephan Wahl";
    public string SuperAdminFirstName { get; set; } = "Stephan";
    public string SuperAdminLastName { get; set; } = "Wahl";
    public string SuperAdminEmployeeNumber { get; set; } = "SWAHL";
    public string SuperAdminBusinessEmail { get; set; } = "Stephan.wahl@dbw.de";
    public string SuperAdminBusinessPhone { get; set; } = "n/a";
    public string SuperAdminPlant { get; set; } = "Default Plant";
    public string SuperAdminLocation { get; set; } = "Default Location";
    public string SuperAdminTeam { get; set; } = "Administration";
    public string SuperAdminCostCenter { get; set; } = "0001";
    public string SuperAdminShift { get; set; } = "Day";
    public string SuperAdminJobFunction { get; set; } = "Super Administration";
    public bool CreateRootOrganizationUnit { get; set; } = true;
    public string RootOrganizationUnitCode { get; set; } = "ROOT";
    public string RootOrganizationUnitName { get; set; } = "ROOT";
}
