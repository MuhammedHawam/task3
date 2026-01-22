namespace PartnersHub.InfraBase.Domain.Enums;

/// <summary>
/// Represents the status of an asset throughout its lifecycle
/// </summary>
public enum AssetStatuses : byte
{
    /// <summary>
    /// Asset is being drafted and not yet submitted
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Asset submitted by PC Contributor, pending PC Admin approval
    /// User Story: "Pending on PC admin checking"
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Asset accepted/approved by PC Admin, pending Infrabase Admin approval
    /// User Story: "Accepted from PC admin", "Pending on infrabase admin checking"
    /// </summary>
    AcceptedByPcAdmin = 2,

    /// <summary>
    /// Asset rejected by PC Admin
    /// User Story: "Rejected from PC admin"
    /// </summary>
    RejectedByPcAdmin = 3,

    /// <summary>
    /// Asset accepted/approved by Infrabase Admin (Final approval)
    /// User Story: "Infrabase Accepted", "Checked assets"
    /// </summary>
    AcceptedByInfrabase = 4,

    /// <summary>
    /// Asset rejected/returned for correction by Infrabase Admin
    /// User Story: "Infrabase rejected", "Return for correction"
    /// </summary>
    RejectedByInfrabase = 5,

    /// <summary>
    /// Asset returned by Infrabase Admin (moves back to pending)
    /// </summary>
    ReturnedByInfrabase = 6
}
