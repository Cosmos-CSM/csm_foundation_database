using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

namespace CSM_Database_Testing;

/// <summary>
///     Provide utils methods for entities bases drafting
/// </summary>
static public class BaseDraftUtils {

    /// <summary>
    ///     Gets a new random 16 length string.
    /// </summary>
    static string Rnd => RandomUtils.String(16);

    /// <summary>
    ///     Drafts a <typeparamref name="TPartnerBridgeEntity"/> instance with random data.
    /// </summary>
    /// <typeparam name="TPartnerBridgeEntity">
    ///     Type of the entity to draft.
    /// </typeparam>
    /// <typeparam name="TInternalScope">
    ///     Type of the entity's internal scope to draft.
    /// </typeparam>
    /// <typeparam name="TExternalScope">
    ///     Type of the entity's external scope to draft.
    /// </typeparam>
    /// <param name="ref">
    ///     Default entity values.
    /// </param>
    /// <param name="draftInternal">
    ///     Whether the drafting utils should draft internal or external scope.
    /// </param>
    /// <returns>
    ///     A drafted <typeparamref name="TPartnerBridgeEntity"/> instance.
    /// </returns>
    static public TPartnerBridgeEntity PartnerBridgeEntity<TPartnerBridgeEntity, TInternalScope, TExternalScope>(TPartnerBridgeEntity? @ref = default, bool draftInternal = true)
        where TPartnerBridgeEntity : IPartnerBridgeEntity<TInternalScope, TExternalScope>, new()
        where TInternalScope : IPartnerScopeEntity<TPartnerBridgeEntity>, new()
        where TExternalScope : IPartnerScopeEntity<TPartnerBridgeEntity>, new() {

        @ref ??= Entity(@ref);

        if (draftInternal) {
            @ref.Internal = PartnerScopeEntity(@ref.Internal);
            @ref.Internal.Bridge = @ref;
        }

        if (!draftInternal) {
            @ref.External = PartnerScopeEntity(@ref.External);
            @ref.External.Bridge = @ref;
        }

        return @ref;
    }

    /// <summary>
    ///     Drafts a <typeparamref name="TPartnerScopeEntity"/> instance with random data.
    /// </summary>
    /// <typeparam name="TPartnerScopeEntity">
    ///     Type of the entity to draft.
    /// </typeparam>
    /// <param name="ref">
    ///     Default entity values.
    /// </param>
    /// <returns>
    ///     A drafted <typeparamref name="TPartnerScopeEntity"/> instance.
    /// </returns>
    static public TPartnerScopeEntity PartnerScopeEntity<TPartnerScopeEntity>(TPartnerScopeEntity? @ref = default)
        where TPartnerScopeEntity : IPartnerScopeEntity, new() {

        @ref = Entity(@ref);

        return @ref;
    }

    /// <summary>
    ///     Drafts an <typeparamref name="TActivableEntity"/> instance with random data. 
    /// </summary>
    /// <typeparam name="TActivableEntity">
    ///     Type of the entity to draft.
    /// </typeparam>
    /// <param name="ref">
    ///     Default values for the entity.
    /// </param>
    /// <returns>
    ///     A drafted <typeparamref name="TActivableEntity"/> instance.
    /// </returns>
    static public TActivableEntity ActivableEntity<TActivableEntity>(TActivableEntity? @ref = default)
        where TActivableEntity : IActivableEntity, new() {

        @ref ??= Entity(@ref);

        return @ref;
    }

    /// <summary>
    ///     Drafts a <typeparamref name="TNamedEntity"/> instance with random data.
    /// </summary>
    /// <typeparam name="TNamedEntity">
    ///     Type of the entity to draft.
    /// </typeparam>
    /// <param name="ref">
    ///     Default entity values.
    /// </param>
    /// <returns>
    ///     A drafted <typeparamref name="TNamedEntity"/> instance.
    /// </returns>
    static public TNamedEntity NamedEntity<TNamedEntity>(TNamedEntity? @ref = default)
        where TNamedEntity : INamedEntity, new() {

        @ref ??= Entity(@ref);

        if (string.IsNullOrWhiteSpace(@ref.Name)) {
            @ref.Name = $"{Rnd}_name";
        }

        if (string.IsNullOrWhiteSpace(@ref.Description)) {
            @ref.Description = $"drafted entity {DateTime.Now:t}";
        }

        return @ref;
    }

    /// <summary>
    ///     Drafts an <typeparamref name="TEntity"/> instance with random data.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     Type of the entity to draft.
    /// </typeparam>
    /// <param name="ref">
    ///     Default values for the entity.
    /// </param>
    /// <returns>
    ///     A drafted <typeparamref name="TEntity"/> instance.
    /// </returns>
    static public TEntity Entity<TEntity>(TEntity? @ref = default)
        where TEntity : IEntity, new() {

        @ref ??= new TEntity();

        return @ref;
    }
}
