using CSM_Database_Core.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CSM_Database_Core.Abstractions.Bases;

/// <summary>
///     Represents a <see cref="IDatabase"/> design time factory.
/// </summary>
/// <typeparam name="TDatabase">
///     Type of the <see cref="IDatabase"/> to build.
/// </typeparam>
public class DatabaseDesignerBase<TDatabase>
    : IDesignTimeDbContextFactory<TDatabase>
    where TDatabase : DbContext, IDatabase, new() {

    /// <inheritdoc/>
    public virtual TDatabase CreateDbContext(string[] args) {
        ConsoleUtils.Warning(
            "Designing database using a design factory",
            new Dictionary<string, object?> {
                { "DesignFactory", GetType().FullName },
                { "Database", typeof(TDatabase).FullName  },
            }
        );

        return new TDatabase();
    }
}
