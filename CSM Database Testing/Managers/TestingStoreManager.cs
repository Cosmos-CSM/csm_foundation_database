using CSM_Database_Core.Core.Utils;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Database_Testing.Abstractions.Bases;
using CSM_Database_Testing.Disposing;
using CSM_Database_Testing.Disposing.Abstractions.Bases;
using CSM_Database_Testing.Disposing.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

using Microsoft.EntityFrameworkCore;

namespace CSM_Database_Testing.Managers;

/// <summary>
///     Represents a testing data storing methods manager.
/// </summary>
public class TestingStoreManager
    : IDisposable {

    /// <summary>
    ///     Testing data disposer.
    /// </summary>
    readonly ITestingDisposer _disposer;

    /// <summary>
    ///     Database factories available for Samples Storing/Disposing.
    /// </summary>
    readonly Dictionary<Type, DatabaseFactory> _dbFactories = [];

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="disposer">
    ///     Testing data disposition manager.
    /// </param>
    /// <param name="dbFactories">
    ///     Storing manager context factories..
    /// </param>
    public TestingStoreManager(ITestingDisposer? disposer, params DatabaseFactory[] dbFactories) {
        foreach (DatabaseFactory factory in dbFactories) {
            using DbContext dbContext = factory();
            Type dbType = dbContext.GetType();

            _dbFactories.Add(dbType, factory);
        }

        _disposer = disposer ?? new TestingDisposer(dbFactories);
    }

    /// <inheritdoc/>
    public void Dispose() {
        _disposer.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Stores the given <paramref name="entity"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the <see cref="IEntity"/> to store.
    /// </typeparam>
    /// <param name="entity">
    ///     Entity to store.
    /// </param>
    /// <returns>
    ///     Stored <paramref name="entity"/>.
    /// </returns>
    public async Task<TEntity2> Store<TEntity2>(TEntity2 entity)
        where TEntity2 : class, IEntity {

        DbContext db = GetDatabase(entity.Database);

        entity = await DatabaseUtils.SanitizeEntity(db, entity);
        db.Set<TEntity2>().Add(entity);
        db.SaveChanges();

        _disposer.Push(entity);

        return entity;
    }

    /// <summary>
    ///     Stores the given <paramref name="entities"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the <see cref="IEntity"/> to store.
    /// </typeparam>
    /// <param name="entities">
    ///     Entities to store.
    /// </param>
    /// <returns>
    ///     Stored <paramref name="entities"/>.
    /// </returns>
    public async Task<TEntity2[]> Store<TEntity2>(TEntity2[] entities)
        where TEntity2 : class, IEntity {

        DbContext db = GetDatabase(entities[0].Database);
        List<TEntity2> refs = [];

        foreach (TEntity2 entity in entities) {
            refs.Add(
                    await DatabaseUtils.SanitizeEntity(db, entity)
                );
        }

        await db.Set<TEntity2>().AddRangeAsync(refs);
        await db.SaveChangesAsync();

        _disposer.Push([.. refs]);

        return [.. refs];
    }

    /// <summary>
    ///     Stores the given <paramref name="entityFactory"/> built <typeparamref name="TEntity2"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the <see cref="IEntity"/> to store.
    /// </typeparam>
    /// <param name="entityFactory">
    ///     Entity factory method.
    /// </param>
    /// <returns>
    ///     Stored <paramref name="entityFactory"/> built <typeparamref name="TEntity2"/>.
    /// </returns>
    public async Task<TEntity2> Store<TEntity2>(EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity {

        TEntity2 toStore = RunEntityFactory(entityFactory);
        toStore = await Store(toStore);

        return toStore;
    }

    /// <summary>
    ///     Iterates based on <paramref name="quantity"/> to generate [Entities] to store based on <paramref name="entityFactory"/>.
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] to store.
    /// </typeparam>
    /// <param name="quantity">
    ///     Quantity of iterations to call <paramref name="entityFactory"/> and store the factory result.
    /// </param>
    /// <param name="entityFactory">
    ///     Factory to build the [Entity] to store.
    /// </param>
    /// <returns>
    ///     The stored and updated [Entities] stored.
    /// </returns>
    public async Task<TEntity2[]> Store<TEntity2>(int quantity, EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity, new() {

        List<TEntity2> entities = [];

        using DbContext database = GetDatabase(new TEntity2().Database);
        for (int i = 0; i < quantity; i++) {

            TEntity2 entity = RunEntityFactory(entityFactory);
            entity = await DatabaseUtils.SanitizeEntity(database, entity);
            entities.Add(entity);
        }

        await database.Set<TEntity2>().AddRangeAsync(entities);
        await database.SaveChangesAsync();
        _disposer.Push([.. entities]);

        return [.. entities];
    }

    /// <summary>
    ///     Internal runner for <see cref="EntityFactory{TEntity}"/> utilizations, automatically sends the [Entropy] parameter. 
    /// </summary>
    /// <typeparam name="TEntity2">
    ///     Type of the [Entity] build by the <paramref name="factory"/>.
    /// </typeparam>
    /// <param name="factory">
    ///     [Entity] factory function.
    /// </param>
    /// <returns>
    ///     The generated [Entity] object.
    /// </returns>
    public static TEntity2 RunEntityFactory<TEntity2>(EntityFactory<TEntity2> factory)
        where TEntity2 : class, IEntity {

        return factory(RandomUtils.String(16));
    }

    /// <summary>
    ///    Retrieves the database instance for the given <paramref name="databaseType"/> based on the subscribed DatabaseFactories.
    /// </summary>
    /// <param name="databaseType">
    ///     <see cref="Type"/> of the database requested.
    /// </param>
    /// <returns>
    ///     The matched <see cref="Type"/> database context instance.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown when the requested database <see cref="Type"/> isn't found in the subcribed database factories.
    /// </exception>
    DbContext GetDatabase(Type databaseType) {
        return !_dbFactories.TryGetValue(databaseType, out DatabaseFactory? factory)
            ? throw new Exception($"No factory subscribed for [({databaseType.Name})]")
            : factory();
    }
}
