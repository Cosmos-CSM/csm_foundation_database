﻿using CSM_Foundation.Database;

using CSM_Foundation_Database.Entity.Bases;
using CSM_Foundation_Database.Entity.Models.Input;

namespace CSM_Foundation_Database.Entity.Depot.IDepot_Update;

/// <summary>
///     [Interface] describing [Update] actions for [Depot] implementations.
/// </summary>
/// <typeparam name="TEntity">
///     [Entity] type for the [Depot] implementation.
/// </typeparam>
public interface IDepot_Update<TEntity>
    where TEntity : class, IEntity {

    /// <summary>
    ///     Updates the given record calculating the current stored values with the given <paramref name="entity"/> to update and store the new values.
    /// </summary>
    /// <param name="Input">
    ///     Operation input parameters.
    /// </param>
    /// <returns></returns>
    /// <remarks>
    ///     Always the record to be overriden will be defined by the <see cref="IEntity.Id"/> property, if isn't given, will try with <see cref="BNamedEntity.Name"/> property in case the
    ///     [Entity] implementation does have it, otherwise will finally create a new record with the given values.
    /// </remarks>
    Task<UpdateOutput<TEntity>> Update(QueryInput<TEntity, UpdateInput<TEntity>> Input);
}
