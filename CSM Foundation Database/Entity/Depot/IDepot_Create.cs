﻿using CSM_Foundation.Database;

using CSM_Foundation_Database.Entity.Models.Output;

namespace CSM_Foundation_Database.Entity.Depot;


/// <summary>
///     Represents a creating logic interface for a product service. A product service usually works as
///     an operation scoped to the product business needs.
/// </summary>
/// <typeparam name="TEntity">
///     [Entity] type of the [Depot] implementation.
/// </typeparam>
public interface IDepot_Create<TEntity>
    where TEntity : class, IEntity {
    /// <summary>
    ///     Creates a single <paramref name="Record"/> record into the live migration.
    ///     <br>
    ///         <list type="bullet">
    ///         <listheader> NOTES: </listheader>
    ///         <item> Validates if the <paramref name="Record"/> has unique properties and validates if they already exists. </item>
    ///         <item> The <see cref="TEntity.Id"/> property is always auto-generated. </item>
    ///         <item> Can auto-generate properties dependengin on the object behavior. </item>
    ///         </list>
    ///     </br>
    /// </summary>
    /// <param name="Record">
    ///     <paramref name="Record"/> to store.
    /// </param>
    /// <returns> 
    ///     The successfully stored object
    /// </returns>
    Task<TEntity> Create(TEntity Record);

    /// <summary>
    ///     Creates a collection of <paramref name="Records"/> records into the live migration.
    /// </summary>
    /// <param name="Records">
    ///     <paramref name="Records"/> to store.
    /// </param>
    /// <param name="Sync">
    ///     If the transaction should finish at the first failure found.
    ///     throwing instantly an exception not returning the result.
    /// </param>
    /// <returns>
    ///     The operation result information.
    /// </returns>
    Task<BatchOperationOutput<TEntity>> Create(ICollection<TEntity> Records, bool Sync = false);
}
