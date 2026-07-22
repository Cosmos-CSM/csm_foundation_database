using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Database_Testing.Disposing;
using CSM_Database_Testing.Disposing.Abstractions.Bases;
using CSM_Database_Testing.Managers;

namespace CSM_Database_Testing.Abstractions.Bases;

/// <summary>
///     Public Delegate for [Entity] factory [Quality] purposes.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the [Entity] to build.
/// </typeparam>
/// <param name="Entropy">
///     Random 16 length <see cref="string"/> to generate unique properties records.
/// </param>
/// <returns>
///     The Entity stored in the database.
/// </returns>
public delegate TEntity EntityFactory<TEntity>(string Entropy)
    where TEntity : class, IEntity;

/// <summary>
///     [Abstract] for Quality Suits implementations that uses database data direct handling to store data for testing purposes.
/// </summary>
/// <remarks>
///     All stored data is being removed from a <see cref="TestingDisposer"/>. Testing data purposes can't be hold in the datasources.
/// </remarks>
public class TestingDataHandlerBase
    : IDisposable {

    /// <summary>
    ///     Testing data disposition manager, used to store to-remove entries after tests finished.
    /// </summary>
    protected readonly TestingDisposer _disposer;

    /// <summary>
    ///     Testing data store manager.
    /// </summary>
    protected readonly TestingStoreManager _storeManager;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="dbFactories">
    ///     Collection of databases factories available for the handler to operate data.
    /// </param>
    public TestingDataHandlerBase(params DatabaseFactory[] dbFactories) {
        _disposer = new TestingDisposer(dbFactories);
        _storeManager = new TestingStoreManager(_disposer, dbFactories);
    }

    /// <inheritdoc/>
    public void Dispose() {
        _storeManager.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="TestingStoreManager.Store{TEntity2}(TEntity2)"/>
    public Task<TEntity2> Store<TEntity2>(TEntity2 entity)
        where TEntity2 : class, IEntity {

        return _storeManager.Store(entity);
    }

    /// <inheritdoc cref="TestingStoreManager.Store{TEntity2}(EntityFactory{TEntity2})"/>
    protected Task<TEntity2> Store<TEntity2>(EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity {

        return _storeManager.Store(entityFactory);
    }

    /// <inheritdoc cref="TestingStoreManager.Store{TEntity2}(int, EntityFactory{TEntity2})"/>
    protected async Task<TEntity2[]> Store<TEntity2>(int quantity, EntityFactory<TEntity2> entityFactory)
        where TEntity2 : class, IEntity, new() {

        return await _storeManager.Store(quantity, entityFactory);
    }
}
