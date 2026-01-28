namespace PartnersHub.InfraBase.Domain.Enums;

/// <summary>
/// Represents the various funding models available.
/// </summary>
public enum FundingModels
{
    /// <summary>
    /// The project is fully funded by private sources.
    /// </summary>
    FullySelfFunded = 0,

    /// <summary>
    /// The project is funded through a partnership between public and private sectors.
    /// </summary>
    PublicPrivatePartnership = 1,

    /// <summary>
    /// The project is fully funded by the government.
    /// </summary>
    FullyGovernmentFunded = 2,

    /// <summary>
    /// The project is funded through a joint venture.
    /// </summary>
    JointVenture = 3,

    /// <summary>
    /// N/A value
    /// </summary>
    //NA= 4

}