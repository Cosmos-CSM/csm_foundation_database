using CSM_Database_Core.Depots.Models.Structs;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Database_Core.Depots.Models;

/// <summary>
///     [Record] for single <typeparamref name="TEntity"/> update operation.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> to update.
/// </typeparam>
public record UpdateInput<TEntity>
    where TEntity : IEntity {

    /// <summary>
    ///     [Entity] to update.
    /// </summary>
    public required TEntity Entity { get; init; }

    /// <summary>
    ///     Wheter the record should be created if it doesn't exist in the database.
    /// </summary>
    public bool Create { get; init; } = false;

    /// <summary>
    ///     <see cref="Entity"/> relations to update.
    /// <code>
    ///     Key: Represents the entity relation name.
    ///     Value:
    ///         Key:   Represents the target relation name (used when the target entity
    ///                relation has several relations of the same Type, if not can be let as empty).
    ///         Value: Entity relation instructions.
    /// </code>
    /// </summary>
    /// <remarks>
    ///     Only use this to update relations that are collections in the source <see cref="Entity"/>. Be careful since the logic won't care about
    ///     the targt relation is dependency or dependant, it will update it anywat.
    /// </remarks>
    public IDictionary<string, IDictionary<string, RelationUpdate[]>> Relations { get; set; } = new Dictionary<string, IDictionary<string, RelationUpdate[]>>();
}
