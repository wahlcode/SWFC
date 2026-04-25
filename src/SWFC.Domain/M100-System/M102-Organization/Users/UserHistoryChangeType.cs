namespace SWFC.Domain.M100_System.M102_Organization.Users;

public enum UserHistoryChangeType
{
    Created = 1,
    MasterDataUpdated = 2,
    StatusChanged = 3,
    PrimaryOrganizationAssigned = 4,
    SecondaryOrganizationAdded = 5,
    SecondaryOrganizationRemoved = 6
}
