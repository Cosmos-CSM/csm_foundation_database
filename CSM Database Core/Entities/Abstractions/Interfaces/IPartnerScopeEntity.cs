namespace CSM_Database_Core.Entities.Abstractions.Interfaces;
/// <summary>
///      Represents an <see cref="IEntity"/> scope for a <see cref="IPartnerBridgeEntity"/>.
/// </summary>
public interface IPartnerScopeEntity
    : IEntity {
}

/// <summary>
///      Represents an <see cref="IEntity"/> scope for a <see cref="IPartnerBridgeEntity"/>.
/// </summary>
/// <typeparam name="TPartnerBridgeEntity">
///     Type of the <see cref="IPartnerBridgeEntity"/>.
/// </typeparam>
public interface IPartnerScopeEntity<TPartnerBridgeEntity>
    : IPartnerScopeEntity
    where TPartnerBridgeEntity : IPartnerBridgeEntity {

    /// <summary>
    ///     <typeparamref name="TPartnerBridgeEntity"/> data.
    /// </summary>
    public TPartnerBridgeEntity Bridge { get; set; }
}
