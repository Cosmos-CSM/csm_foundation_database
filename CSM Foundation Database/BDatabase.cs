using System.ComponentModel.DataAnnotations;
using System.Reflection;

using CSM_Foundation_Core;

using CSM_Foundation_Database.Entity.Bases;
using CSM_Foundation_Database.Models;
using CSM_Foundation_Database.Utilitites;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSM_Foundation.Database;

/// <summary>
///     Represents a CSM Database, wich configures and handles data management along business entities and 
///     data persistence.
/// </summary>
public interface IDatabase {

    /// <summary>
    ///     Validates database connection and configuration health.
    /// </summary>
    void ValidateHealth();
}

/// <summary>
///     Represents a CSM Database, wich configures and handles data management along business entities and
///     data persistence.
/// </summary>
/// <typeparam name="TDatabases">
///     <see cref="DbContext"/> implementation of the database handler.
/// </typeparam>
public abstract partial class BDatabase<TDatabases>
    : DbContext, IDatabase
    where TDatabases : DbContext {

    /// <summary>
    ///     Whether the database context has logs enabled at building time.
    /// </summary>
    readonly bool LogsOn = true;

    /// <summary>
    ///     ORM Connection data.
    /// </summary>
    protected readonly ConnectionOptions Connection;

    /// <summary>
    ///     Server sign identificator (needed for transactions and authorization processes).
    /// </summary>
    /// <remarks>
    ///     Must be strictly 5 char length
    /// </remarks>
    [StringLength(5, MinimumLength = 5)]
    protected string Sign {
        get => _Sign; init => _Sign = value.ToUpper();
    }
    string _Sign = "";

    /// <summary>
    ///     Generates a <see cref="BDatabase{TDatabases}"/> instance that handles specific database connection
    ///     and configuration properties/methods. 
    /// </summary>
    /// <param name="sign">
    ///     Database implementation signature to identify.
    /// </param>
    /// <remarks> 
    ///     This method gathers the <see cref="Connection"/> options from ./<see cref="Sign"/>(Upper)>/*.json files automatically.
    /// </remarks>
    public BDatabase([StringLength(5, MinimumLength = 5)] string sign)
        : base() {

        Sign = sign;
        Connection = DatabaseUtilities.Retrieve(Sign);
    }

    /// <summary>
    ///     Generates a <see cref="BDatabase{TDatabases}"/> instance that handles specific database connection
    ///     and configuration properties/methods. 
    /// </summary>
    /// <param name="sign">
    ///     Database implementation signature to identify.
    /// </param>
    /// <param name="connection">
    ///     Database connection options.
    /// </param>
    /// <param name="logsOn">
    ///     Whether the logging service is enabled.
    /// </param>
    public BDatabase([StringLength(5, MinimumLength = 5)] string sign, ConnectionOptions connection, bool logsOn = true)
        : base() {

        Sign = sign;
        Connection = connection;
        LogsOn = logsOn;
    }

    /// <summary>
    ///     Generates a <see cref="BDatabase{TDatabases}"/> instance that handles specific database connection
    ///     and configuration properties/methods. 
    /// </summary>
    /// <param name="sign">
    ///     Database implementation signature to identify
    /// </param>
    /// <param name="dbOptions">
    ///     Native EntityFrameworkCore <see cref="DbContext"/> implementation options.
    /// </param>
    /// <remarks> 
    ///     This method gathers the <see cref="Connection"/> options from ./<see cref="Sign"/>(Upper)>/*.json files automatically.
    /// </remarks>
    /// <param name="logsOn">
    ///     Whether the logging service is enabled.
    /// </param>
    public BDatabase([StringLength(5, MinimumLength = 5)] string sign, DbContextOptions<TDatabases> dbOptions, bool logsOn = true)
        : base(dbOptions) {

        Sign = sign;
        Connection = DatabaseUtilities.Retrieve(sign);
        LogsOn = logsOn;
    }

    /// <summary>
    ///     Generates a <see cref="BDatabase{TDatabases}"/> instance that handles specific database connection
    ///     and configuration properties/methods. 
    /// </summary>
    /// <param name="sign">
    ///     Database implementation signature to identify
    /// </param>
    /// <param name="connection">
    ///     Database connection options.
    /// </param>
    /// <param name="dbOptions">
    ///     Native EntityFrameworkCore <see cref="DbContext"/> implementation options.
    /// </param>
    /// <param name="logsOn">
    ///     Whether the logging service is enabled.
    /// </param>
    public BDatabase([StringLength(5, MinimumLength = 5)] string sign, ConnectionOptions connection, DbContextOptions<TDatabases> dbOptions, bool logsOn = true)
        : base(dbOptions) {

        Sign = sign;
        Connection = connection;
        LogsOn = logsOn;
    }

    /// <summary>
    ///     Validates if all the <see cref="Sets"/> <see cref="Type"/>s are <see cref="BEntity"/> assuring contains the correct
    ///     methods needed.
    /// </summary>
    /// <returns>
    ///     The strict validated collection of [<see cref="BBusinessDatabaseEntity"/>]s and [<see cref="BConnector{TSource, TTarget}"/>]s.
    /// </returns>
    BEntity[] ValidateSets() {
        Type databaseType = GetType();

        List<BEntity> sets = [];
        IEnumerable<PropertyInfo> dbSets = databaseType
           .GetProperties()
           .Where(
               (propInfo) => {
                   Type propType = propInfo.PropertyType;

                   return propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(DbSet<>);
               }
           );

        foreach (PropertyInfo dbSet in dbSets) {
            Type generic = dbSet.PropertyType.GetGenericArguments()[0]
                ?? throw new Exception($"DBSet [{dbSet.Name}] generic gathering failure");

            sets.Add((BEntity)Activator.CreateInstance(generic)!);
        }

        return [.. sets];
    }


    public void ValidateHealth() {
        Logger.Announce(
            $"Setting up ORM",
            new() {
                { "Database", GetType()?.Namespace ?? "---" },
                { "Base", nameof(BDatabase<TDatabases>) }
            }
        );

        if (Database.CanConnect()) {
            Logger.Success($"[{GetType().FullName}] ORM Set");

            IEnumerable<string> pendingMigrations = Database.GetPendingMigrations();
            if (pendingMigrations.Any()) {
                throw new Exception($"ORM ({GetType().FullName}) has pending migrations ({pendingMigrations.Count()})");
            }
            Evaluate();
        } else {
            try {
                Database.OpenConnection();
            } catch (Exception ex) {
                throw new Exception($"Invalid connection with Database ({GetType().FullName}) | {ex.InnerException?.Message}");
            }
        }
    }

    /// <summary>
    ///     Evaluates if <see cref="Sets"/> are correctly configured and translated to the internal framework handler.
    /// </summary>
    public void Evaluate() {
        BEntity[] sets = ValidateSets();

        Logger.Announce(
            $"[{GetType().Name}] Validatig Sets...",
            new() {
                { "Count", sets.Length }
            }
        );

        Exception[] evResults = [];
        foreach (BEntity set in sets) {
            Exception[] result = set.EvaluateDefinition();
            if (result.Length > 0) {
                Logger.Warning(
                    "Wrong [Set] definition",
                    new() {
                        { "Set", set.GetType().Name },
                        { "Exceptions", result },
                    }
                );
            }

            evResults = [.. evResults, .. result];
        }

        if (evResults.Length > 0) {
            throw new Exception("Database [Set] definition failures");
        } else {
            Logger.Success($"[{GetType().Name}] Set validation succeeded");
        }
    }

    protected virtual void DefineSet(BEntity Entity, EntityTypeBuilder mBuilder) { }

    protected virtual void DefineSource(ModelBuilder mBuilder) { }

    #region EF Native Methods


    /// <summary>
    ///     This is overriden from <see cref="BDatabase{TDatabases}"/> to Configure an SQL Server Connection using
    ///     <see cref="Connection"/> generated string, this natively has another behavior but using <see cref="BDatabase{TDatabases}"/>
    ///     will automatically configure the SQL Server connection.
    /// </summary>
    /// <param name="optionsBuilder">
    ///     Relations builder proxy object.
    /// </param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        string connectionString = Connection.GenerateConnectionString();
        optionsBuilder.UseSqlServer(connectionString);

        if (AppDomain.CurrentDomain.FriendlyName.Contains("ef")) {
            Logger.Warning(
                    $"Running EF Design Time Execution",
                    new Dictionary<string, object?> {
                        { "Environment", ServerUtils.Environment.ToString() },
                        { "Connection", connectionString },
                    }
                );
        }
    }

    protected override void OnModelCreating(ModelBuilder mBuilder) {

        DefineSource(mBuilder);

        IEnumerable<IMutableEntityType> entityTypes = mBuilder.Model.GetEntityTypes();
        foreach (IMutableEntityType entityType in entityTypes) {

            IEnumerable<IMutableForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];
            foreach (IMutableForeignKey foreignKey in foreignKeys) {

                if (foreignKey.DependentToPrincipal is null) {
                    continue;
                }

                mBuilder.Entity(entityType.ClrType).Ignore(foreignKey.DependentToPrincipal.Name);
            }
        }

        BEntity[] sets = ValidateSets();

        foreach (BEntity set in sets) {
            Type setType = set.GetType();
            mBuilder.Entity(
                setType,
                (etBuilder) => {
                    etBuilder.HasKey(nameof(IEntity.Id));
                    etBuilder.Property<long>(nameof(IEntity.Id)).IsRequired();

                    if (set is BNamedEntity) {
                        PropertyInfo nameProperty = set.GetProperty(nameof(BNamedEntity.Name));
                        PropertyInfo descriptionProperty = set.GetProperty(nameof(BNamedEntity.Description));

                        etBuilder.HasIndex(nameProperty.Name).IsUnique();
                        etBuilder.Property(nameProperty.Name).HasMaxLength(100).IsRequired();

                        etBuilder.Property(descriptionProperty.Name).HasMaxLength(200);
                    }

                    DefineSet(set, etBuilder);

                    etBuilder.Property(nameof(IEntity.Timestamp)).HasColumnType("datetime2(7)").HasDefaultValueSql("GETUTCDATE()");

                    set.DesignEntity(etBuilder);
                }
            );
        }

        base.OnModelCreating(mBuilder);
    }

    #endregion
}

/// <summary>
///     [Abstract] Partial implementation to expose generation/validation methods to <see cref="BDatabase{TDatabases}"/> handler.
/// </summary>
public abstract partial class BEntity
    : BObject<IEntity>, IEntity {

    /// <summary>
    ///     Describe to the Entity Framework manager how to handle the [Entity] object, its proeprties and relations, instructing
    ///     the <see cref="EntityTypeBuilder"/> how to handle them.
    /// </summary>
    /// <param name="etBuilder">
    ///     Proxy object to configure Entity Model to Entity Framework Core.
    /// </param>
    /// <remarks>
    ///     Don't describe <see cref="IEntity"/> properties they are being auto-described by the [CSM] engine, <see cref="IEntity.Id"/>, <see cref="IEntity.Timestamp"/> and <see cref="IEntity.Name"/>.
    /// </remarks>
    protected internal virtual void DesignEntity(EntityTypeBuilder etBuilder) { }
}