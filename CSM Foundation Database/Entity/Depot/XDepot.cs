using CSM_Foundation.Database;

using CSM_Foundation_Core;
using CSM_Foundation_Core.Exceptions;
using CSM_Foundation_Core.Exceptions.Models;

namespace CSM_Foundation_Database.Entity.Depot;

/// <summary>
///     Represents the <see cref="XDepot{TEntity}"/> trigger events.
/// </summary>
public enum XDepotEvents {
    /// <summary>
    ///     Used when a searched <see cref="IEntity"/> wasn't found.
    /// </summary>
    UNFOUND,

    /// <summary>
    ///     Usedn when at an Update operation the <see cref="IEntity"/> given has <see cref="IEntity.Id"/> 0
    ///     (wich usually means a new entity creation) but <seealso cref="UpdateInput.Create"/> is set to false.
    /// </summary>
    CREATE_DISABLED,
}

/// <summary>
///     Represents an exception at <see cref="IDepot{TEntity}"/> operation events.
/// </summary>
public class XDepot<TEntity>
    : BException<XDepotEvents>
    where TEntity : IEntity {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="event">
    ///     The <see cref="XDepotEvents"/> that triggered the exception.
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
    public XDepot(
            XDepotEvents @event,
            string searchContext = "",
            Exception? exception = null,
            ExceptionFeedback[]? feedback = null
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

    protected override Dictionary<XDepotEvents, string> BuildAdviseContext() {
        return new Dictionary<XDepotEvents, string> {
            {
                XDepotEvents.UNFOUND,
                $"({typeof(TEntity).Name}) not found"
            },
            {
                XDepotEvents.CREATE_DISABLED,
                $"{ Constants.Messages.DEFAULT_USER_ADVISE }"
            }
        };
    }
}