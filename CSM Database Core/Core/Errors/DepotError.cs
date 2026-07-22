using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core;
using CSM_Foundation_Core.Errors.Abstractions.Bases;
using CSM_Foundation_Core.Errors.Models;

namespace CSM_Database_Core.Core.Errors;

/// <summary>
///     Represents the <see cref="DepotError{TEntity}"/> trigger events.
/// </summary>
public enum DepotErrorEvents {
    /// <summary>
    ///     Used when a searched <see cref="IEntity"/> wasn't found.
    /// </summary>
    UNFOUND,

    /// <summary>
    ///     Used when at an Update operation the <see cref="IEntity"/> given has <see cref="IEntity.Id"/> 0
    ///     (wich usually means a new entity creation) but <seealso cref="UpdateInput{TEntity}.Create"/> is set to false.
    /// </summary>
    CREATE_DISABLED,

    /// <summary>
    ///     Event when at an <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> operation, target entity relation 
    ///     references mismatches found references.
    /// </summary>
    RELATIONS_UPDATE_MISMATCH_TARGET_REFS,

    /// <summary>
    ///     Event when at an <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> operation, a relation 
    ///     updates is configured for multi target relation references with one of them as the default (string.Empty) reference configuration.
    /// </summary>
    RELATIONS_UPDATE_TARGETMULTIREFERENCE_CANNOTHAVEDEFAULT,
}

/// <summary>
///     Represents an exception at <see cref="IDepot{TEntity}"/> operation events.
/// </summary>
public class DepotError<TEntity>
    : ErrorBase<DepotErrorEvents>
    where TEntity : IEntity {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="event">
    ///     The <see cref="DepotErrorEvents"/> that triggered the exception.
    /// </param>
    /// <param name="searchContext">
    ///     Depot transaction search argument.
    /// </param>
    /// <param name="exception">
    ///     When the trigger events includes a caught framework exception.
    /// </param>
    /// <param name="feedback">
    ///     Whether the exception event has specific user feedback messages to display.
    /// </param>
    public DepotError(
            DepotErrorEvents @event,
            string searchContext = "",
            Exception? exception = null,
            ErrorFeedback[]? feedback = null
        )
        : base(
                $"[Depot]: ({@event})", @event, exception, feedback,
                new Dictionary<string, object?> {
                    {
                        "Entity",
                        typeof(TEntity).Name
                    },
                    {
                        "Search",
                        searchContext
                    }
                }
            ) {
    }

    /// <inheritdoc/>
    protected override Dictionary<DepotErrorEvents, string> BuildAdviseContext() {
        return new Dictionary<DepotErrorEvents, string> {
            {
                DepotErrorEvents.UNFOUND,
                $"({typeof(TEntity).Name}) not found"
            },
            {
                DepotErrorEvents.CREATE_DISABLED,
                $"{ Constants.Messages.DEFAULT_USER_ADVISE }"
            }
        };
    }
}