using System.Linq.Expressions;
using System.Reflection;

using CSM_Database_Core;
using CSM_Database_Core.Core.Errors;
using CSM_Database_Core.Core.Models;
using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Depots.ViewFilters;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Database_Testing.Disposing.Abstractions.Bases;
using CSM_Database_Testing.Managers;

using Xunit;
using Xunit.Sdk;

namespace CSM_Database_Testing.Abstractions.Bases;

/// <summary>
///     Represents a testing base class for a data source depot.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> being tested.
/// </typeparam>
/// <typeparam name="TDepot">
///     Type of the <see cref="IDepot{TEntity}"/> being tested.
/// </typeparam>
/// <typeparam name="TDatabase">
///     Type of the <see cref="CSM_Database_Core.Abstractions.Interfaces.IDatabase"/> handling the <typeparamref name="TDepot"/>.
/// </typeparam>
public abstract class DepotIntegrationTestsBase<TEntity, TDepot, TDatabase>
    : TestingDataHandlerBase
    where TEntity : class, IEntity, new()
    where TDepot : IDepot<TEntity>
    where TDatabase : DatabaseBase<TDatabase>, new() {

    /// <summary>
    ///     <see cref="IDepot{TEntity}"/> instance to test.
    /// </summary>
    protected readonly TDepot _depot;

    /// <summary>
    ///     <see cref="_depot"/>'s database context.
    /// </summary>
    protected readonly TDatabase _database;

    /// <summary>
    ///     Stores the most valid evaluable property from the current  <typeparamref name="TEntity"/>. used for ordering and filtering at View operations and evaluate their quality.
    /// </summary>
    protected readonly PropertyInfo _evaluableProperty;

    /// <summary>
    ///     Generates a new behavior base for <see cref="DepotIntegrationTestsBase{TMigrationSet, TMigrationDepot, TMigrationDatabases}"/>.
    /// </summary>
    /// <param name="factories">
    ///     Database factories for relations sampleEntity at external databases needed for <typeparamref name="TEntity"/>.
    /// </param>
    /// <param name="database">
    ///     Main Entity <typeparamref name="TEntity"/> database handler instance. If isn't given will use a default built instance.
    /// </param>
    public DepotIntegrationTestsBase(DatabaseFactory? database = null, params DatabaseFactory[] factories)
        : base(
            [
                ..factories,
                () => database?.Invoke()
                    ?? (TDatabase)Activator.CreateInstance(
                        typeof(TDatabase),
                        new DatabaseOptions<TDatabase> {
                            ForTesting = true,
                        }
                    )!,
            ]
        ) {

        _database = (TDatabase)database?.Invoke()!
                    ?? (TDatabase)Activator.CreateInstance(
                        typeof(TDatabase),
                        new DatabaseOptions<TDatabase> {
                            ForTesting = true,
                        }
                    )!;
        _depot = (TDepot)Activator.CreateInstance(typeof(TDepot), _database, null)!;

        PropertyInfo[] entityProperties = typeof(TEntity).GetProperties();

        PropertyInfo? orderableTmp = null;
        foreach (PropertyInfo propertyInfo in entityProperties) {

            Type propertyType = propertyInfo.PropertyType;

            if ((propertyType != typeof(string) && propertyType != typeof(int)) || propertyInfo.Name == nameof(IEntity.Discriminator)) {
                continue;
            }

            orderableTmp = propertyInfo;
            break;
        }

        _evaluableProperty = orderableTmp ?? typeof(TEntity).GetProperty(nameof(IEntity.Id))!; // By default if the [Entity] doesn't have a valid evaluable property will use the Id. 
    }

    #region Abtraction

    /// <summary>
    ///     Creates a context [Entity] for testing data creation and assertion.
    /// </summary>
    /// <param name="Entropy">
    ///     Random 16 length value for unique properties.
    /// </param>
    /// <returns>
    ///     A correctly built <typeparamref name="TEntity"/>.
    /// </returns>
    protected abstract TEntity EntityFactory(string Entropy);

    #endregion

    #region Private / Protected Functions

    /// <summary>
    ///     
    /// </summary>
    /// <param name="SampleEntities"></param>
    protected async Task CommitSampleEntities(ICollection<IEntity> SampleEntities) {
        await _database.SaveChangesAsync();
        _disposer.Push([.. SampleEntities]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    protected void AssertEvaluable(TEntity expected, TEntity actual) {
        object? sampleEvaluableValue = _evaluableProperty.GetValue(expected);
        object? overwrittenEvaluableValue = _evaluableProperty.GetValue(actual);

        Assert.Equal(sampleEvaluableValue, overwrittenEvaluableValue);
    }

    /// <summary>
    ///     Looks into the database for a random <see cref="IEntity.Id"/> that hasn't been used yet.
    /// </summary>
    /// <param name="noStored">
    ///     Wheter the generator should verify the generated <see cref="IEntity.Id"/> isn't used
    ///     for a stored <typeparamref name="TEntity"/> yet.
    /// </param>
    /// <returns>
    ///     A valid random value for <see cref="IEntity.Id"/>
    /// </returns>
    protected async Task<long> GeneratePointer(bool noStored = false) {
        Random random = new();

        // Generate two 32-bit random integers
        int high = random.Next(int.MinValue, int.MaxValue);
        int low = random.Next(int.MinValue, int.MaxValue);

        // Combine them into a long value
        long randomLong = (((long)high << 32) | (uint)low) & long.MaxValue;
        if (!noStored) {
            return randomLong;
        }

        bool iteratorLock;
        do {
            iteratorLock = await _database.Set<TEntity>().FindAsync(randomLong) is not null;
        } while (iteratorLock);

        return randomLong;
    }

    #endregion

    #region Sampling

    /// <summary>
    ///     Creates a new <typeparamref name="TEntity"/> instance based on the <see cref="EntityFactory(string)"/> implementation.
    /// </summary>
    /// <returns> A new <typeparamref name="TEntity"/> instance </returns>
    /// <remarks>
    ///     This <see cref="IEntity"/> instance is created but not stored in the database.
    /// </remarks>
    protected TEntity Sampling() {
        return TestingStoreManager.RunEntityFactory(EntityFactory);
    }

    /// <summary>
    ///    Creates a new collection of <typeparamref name="TEntity"/> instances based on the <see cref="EntityFactory(string)"/> implementation.
    /// </summary>
    /// <param name="Count">
    ///     Number of instances to create.
    /// </param>
    /// <returns>
    ///     A new <typeparamref name="TEntity"/> instance collection.
    /// </returns>
    /// <remarks>
    ///     This <see cref="IEntity"/> instance collection is created but not stored in the database.
    /// </remarks>
    protected TEntity[] Sampling(int Count) {
        return [.. Enumerable.Range(0, Count).Select(_ => TestingStoreManager.RunEntityFactory(EntityFactory))];
    }

    #endregion

    #region Create Base Tests

    /// <summary>
    ///     Method: <see cref="IDepotCreate{TEntity}.Create(TEntity)"/> 
    ///     Expectation: Success creation.
    /// </summary>
    [Fact(DisplayName = "[Create Single]: Entity created")]
    public virtual async Task Create_Single_Success() {
        TEntity sample = Sampling();

        TEntity storedEntity = await _depot.Create(sample);
        await CommitSampleEntities([storedEntity]);

        Assert.Multiple(
            [
                () => Assert.True(storedEntity.Id > 0),
                async () => {
                    await Assert.ThrowsAnyAsync<Exception>(
                        async () => {
                            await _depot.Create(sample);
                            await CommitSampleEntities([sample]);
                        }
                    );
                },
            ]
        );
    }

    /// <summary>
    ///     Method: <see cref="IDepotCreate{TEntity}.Create(ICollection{TEntity}, bool)"/> 
    ///     Expectation: Success creation.
    /// </summary>
    [Fact(DisplayName = "[Create Batch]: Entities created")]
    public virtual async Task Create_Batch_Success() {
        TEntity[] samples = Sampling(3);

        BatchOperationOutput<TEntity> qOut = await _depot.Create(samples);
        await CommitSampleEntities(samples);

        Assert.Multiple(
            [
                () => Assert.Equal(qOut.OperationsCount, samples.Length),
                () => Assert.True(qOut.SuccessesCount.Equals(samples.Length), qOut.FailuresCount > 0 ? qOut.Failures[0].Message : ""),
                () => Assert.All(qOut.Successes, i => { Assert.True(i.Id > 0); })
            ]
        );
    }

    #endregion

    #region Read Base Tests

    /// <summary>
    ///     Method: <see cref="IDepotRead{TEntity}.Read(long)"/> 
    ///     Expectation: Success reading.
    /// </summary>
    [Fact(DisplayName = "[Read Single]: Entity read by (Id)")]
    public virtual async Task Read_Single_ById_Success() {
        TEntity sample = await Store(EntityFactory);

        TEntity readEntity = await _depot.Read(sample.Id);
        Assert.Multiple(
                [
                    () => Assert.Equal(sample.Id, readEntity.Id),
                    () => Assert.Equal(sample.Timestamp, readEntity.Timestamp),
                    () => {
                            object? sampleEvaluableValue = _evaluableProperty.GetValue(sample);
                            object? readEvaluableValue = _evaluableProperty.GetValue(readEntity);
                            Assert.Equal(sampleEvaluableValue, readEvaluableValue);
                        }
                ]
            );
    }

    /// <summary>
    ///     Method: <see cref="IDepotRead{TEntity}.Read(long[])"/> 
    ///     Expectation: Success reading.
    /// </summary>
    [Fact(DisplayName = "[Read Batch]: Entities read by (Id)")]
    public virtual async Task Read_Batch_ById_Sucess() {
        TEntity[] samples = await Store(20, EntityFactory);
        long[] sampleIds = [.. samples.Select(i => i.Id)];

        BatchOperationOutput<TEntity> readEntities = await _depot.Read(sampleIds);
        Assert.Multiple(
                [
                    () => Assert.Empty(readEntities.Failures),
                    () => Assert.Equal(samples.Length, readEntities.SuccessesCount),
                    () => Assert.All(
                        readEntities.Successes,
                        (entity) => {
                            TEntity sample = samples.First(j => j.Id == entity.Id);

                            Assert.Equal(sample.Id, entity.Id);
                            Assert.Equal(sample.Timestamp, entity.Timestamp);

                            object? evaluableSampleValue = _evaluableProperty.GetValue(sample);
                            object? evaluableEntityValue = _evaluableProperty.GetValue(entity);
                            Assert.Equal(evaluableSampleValue, evaluableEntityValue);
                        }
                    )
                ]
            );
    }

    /// <summary>
    ///     Method: <see cref="IDepotRead{TEntity}.Read(QueryInput{TEntity, FilterQueryInput{TEntity}})"/> 
    ///     Expectation: Success reading.
    /// </summary>
    [Fact(DisplayName = "[Read Batch]: Entities read by (Query [First matching])")]
    public virtual async Task Read_Batch_ByQueryFirstMatch_Success() {
        TEntity[] samples = await Store(2, EntityFactory);
        TEntity samplePivot = samples[0];

        BatchOperationOutput<TEntity> readEntites = await _depot.Read(
                new QueryInput<TEntity, FilterQueryInput<TEntity>> {
                    Parameters = new FilterQueryInput<TEntity> {
                        Behavior = FilteringBehaviors.First,
                        Filter = (entity) => entity.Id == samplePivot.Id || entity.Id == samples[1].Id
                    }
                }
            );

        Assert.Multiple(
                [
                    () => Assert.Empty(readEntites.Failures),
                    () => Assert.Equal(1, readEntites.SuccessesCount),
                    () => {
                        TEntity readEntity = readEntites.Successes[0];

                        Assert.Equal(samplePivot.Id, readEntity.Id);
                        Assert.Equal(samplePivot.Timestamp, readEntity.Timestamp);

                        object? evaluableSampleValue = _evaluableProperty.GetValue(samplePivot);
                        object? evaluableEntityValue = _evaluableProperty.GetValue(readEntity);
                        Assert.Equal(evaluableSampleValue, evaluableEntityValue);
                    },
                ]
            );
    }

    /// <summary>
    ///     Method: <see cref="IDepotRead{TEntity}.Read(QueryInput{TEntity, FilterQueryInput{TEntity}})"/> 
    ///     Expectation: Success reading.
    /// </summary>
    [Fact(DisplayName = "[Read Batch]: Entities read by (Query [Last matching])")]
    public virtual async Task Read_Batch_ByQueryLastMatch_Success() {
        TEntity[] samples = await Store(2, EntityFactory);
        TEntity samplePivot = samples[1];

        BatchOperationOutput<TEntity> readEntites = await _depot.Read(
                new QueryInput<TEntity, FilterQueryInput<TEntity>> {
                    Parameters = new FilterQueryInput<TEntity> {
                        Behavior = FilteringBehaviors.Last,
                        Filter = (entity) => entity.Id == samplePivot.Id || entity.Id == samples[0].Id
                    }
                }
            );

        Assert.Multiple(
                [
                    () => Assert.Empty(readEntites.Failures),
                    () => Assert.Equal(1, readEntites.SuccessesCount),
                    () => {
                        TEntity readEntity = readEntites.Successes[0];

                        Assert.Equal(samplePivot.Id, readEntity.Id);
                        Assert.Equal(samplePivot.Timestamp, readEntity.Timestamp);

                        object? evaluableSampleValue = _evaluableProperty.GetValue(samplePivot);
                        object? evaluableEntityValue = _evaluableProperty.GetValue(readEntity);
                        Assert.Equal(evaluableSampleValue, evaluableEntityValue);
                    },
                ]
            );
    }

    /// <summary>
    ///     Method: <see cref="IDepotRead{TEntity}.Read(QueryInput{TEntity, FilterQueryInput{TEntity}})"/> 
    ///     Expectation: Success reading.
    /// </summary>
    [Fact(DisplayName = "[Read Batch]: Entities read by (Query [All matching])")]
    public virtual async Task Read_Batch_ByQueryAllMatches_Success() {
        TEntity[] samples = await Store(2, EntityFactory);

        BatchOperationOutput<TEntity> readEntites = await _depot.Read(
                new QueryInput<TEntity, FilterQueryInput<TEntity>> {
                    Parameters = new FilterQueryInput<TEntity> {
                        Behavior = FilteringBehaviors.All,
                        Filter = (entity) => entity.Id == samples[0].Id || entity.Id == samples[1].Id
                    }
                }
            );

        Assert.Multiple(
                [
                    () => Assert.Empty(readEntites.Failures),
                    () => Assert.Equal(2, readEntites.SuccessesCount),
                    () => Assert.All(
                            samples,
                            (sample) => {
                                TEntity entity = readEntites.Successes.First(i => i.Id == sample.Id);

                                Assert.Equal(sample.Id, entity.Id);
                                Assert.Equal(sample.Timestamp, entity.Timestamp);

                                object? evaluableSampleValue = _evaluableProperty.GetValue(sample);
                                object? evaluableEntityValue = _evaluableProperty.GetValue(entity);
                                Assert.Equal(evaluableSampleValue, evaluableEntityValue);
                            }
                        ),
                ]
            );
    }

    #endregion

    #region Update Base Tests

    /// <summary>
    ///     Method: <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> 
    ///     Expectation: Succeess created.
    /// </summary>
    [Fact(DisplayName = $"[Update Single]: Created when (Create) property enabled")]
    public virtual async Task Update_Single_OnCreateEnabled_Success() {
        TEntity sample = TestingStoreManager.RunEntityFactory(EntityFactory);

        UpdateOutput<TEntity> updateOutput = await _depot.Update(
                new QueryInput<TEntity, UpdateInput<TEntity>> {
                    Parameters = new UpdateInput<TEntity> {
                        Entity = sample,
                        Create = true,
                    },
                }
            );

        await CommitSampleEntities([updateOutput.Updated]);

        Assert.Multiple(
                [
                    () => Assert.Null(updateOutput.Original),
                    () => {
                        TEntity overwritten = updateOutput.Updated;

                        Assert.True(overwritten.Id > 0);
                        AssertEvaluable(sample, overwritten);
                    },
                ]
            );
    }

    /// <summary>
    ///     Method: <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> 
    ///     Expectation: Throws (<see cref="DepotError{TEntity}"/>) with event (<see cref="DepotErrorEvents.CREATE_DISABLED"/>)
    /// </summary>
    [Fact(DisplayName = $"[Update Single]: Throws exception (CREATE_DISABLED).")]
    public virtual async Task Update_Single_OnCreateDisabled_ErrorCreateDisabled() {
        TEntity sample = TestingStoreManager.RunEntityFactory(EntityFactory);

        DepotError<TEntity> depotException = await Assert.ThrowsAsync<DepotError<TEntity>>(
                async () => {
                    UpdateOutput<TEntity> updateOutput = await _depot.Update(
                new QueryInput<TEntity, UpdateInput<TEntity>> {
                    Parameters = new UpdateInput<TEntity> {
                        Entity = sample,
                    },
                }
                    );
                }
            );

        Assert.Equal(DepotErrorEvents.CREATE_DISABLED, depotException.Event);
    }

    /// <summary>
    ///     Method: <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> 
    ///     Expectation: Throws (<see cref="DepotError{TEntity}"/>) with event (<see cref="DepotErrorEvents.UNFOUND"/>)
    /// </summary>
    [Fact(DisplayName = $"[Update Single]: Throws exception (UNFOUND)")]
    public virtual async Task Update_Single_ErrorUnfound() {
        TEntity sample = TestingStoreManager.RunEntityFactory(EntityFactory);
        sample.Id = await GeneratePointer();

        DepotError<TEntity> depotException = await Assert.ThrowsAsync<DepotError<TEntity>>(
                async () => {
                    UpdateOutput<TEntity> updateOutput = await _depot.Update(
                        new QueryInput<TEntity, UpdateInput<TEntity>> {
                            Parameters = new UpdateInput<TEntity> {
                                Entity = sample,
                            },
                        }
                    );
                }
            );
        Assert.Equal(DepotErrorEvents.UNFOUND, depotException.Event);
    }

    /// <summary>
    ///     Method: <see cref="IDepotUpdate{TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/>
    ///     Expectation: Success update.
    /// </summary>
    [Fact(DisplayName = $"[Update Single]: Entity gets updated correctly")]
    public abstract Task Update_Single_Success();

    #endregion

    #region Delete Base Tests

    /// <summary>
    ///     Method: <see cref="IDepotDelete{TEntity}.Delete(long)"/> 
    ///     Expectation: Throws error (<see cref="DepotError{TEntity}"/>) with event (<see cref="DepotErrorEvents.UNFOUND"/>).
    /// </summary>
    [Fact(DisplayName = $"[Delete Single]: Throws error (UNFOUND)")]
    public virtual async Task Delete_Single_ById_ErrorUnfound() {
        long unexistPointer = await GeneratePointer(true);

        DepotError<TEntity> depotException = await Assert.ThrowsAsync<DepotError<TEntity>>(
                async () => {
                    await _depot.Delete(unexistPointer);
                }
            );

        Assert.Equal(DepotErrorEvents.UNFOUND, depotException.Event);
    }

    /// <summary>
    ///     Method: <see cref="IDepotDelete{TEntity}.Delete(long)"/> 
    ///     Expectation: Success deleted.
    /// </summary>
    [Fact(DisplayName = $"[Delete Single]: Entity deleted by (Id)")]
    public virtual async Task Delete_Single_ById_Success() {
        TEntity entity = await Store(EntityFactory);

        await _depot.Delete(entity.Id);
        await CommitSampleEntities([]);

        TEntity? searchedEntity = _database.Set<TEntity>().Find(entity.Id);
        Assert.Null(searchedEntity);
    }

    /// <summary>
    ///     Method: <see cref="IDepotDelete{TEntity}.Delete(QueryInput{TEntity, FilterQueryInput{TEntity}})"/> 
    ///     Expectation: Success deleted.
    /// </summary>
    [Fact(DisplayName = $"[Delete Batch]: Entities deleted by (Query)")]
    public virtual async Task Delete_Batch_ByQuery_Success() {
        TEntity entity = (await Store(10, EntityFactory))[0];

        BatchOperationOutput<TEntity> deleteOutput = await _depot.Delete(
                new QueryInput<TEntity, FilterQueryInput<TEntity>>() {
                    Parameters = new FilterQueryInput<TEntity> {
                        Filter = (entityB) => entityB.Id == entity.Id,
                    }
                }
            );
        await CommitSampleEntities([]);

        Assert.Multiple(
                [
                    () => Assert.False(deleteOutput.Failed),
                    () => Assert.Empty(deleteOutput.Failures),
                    () => Assert.NotEmpty(deleteOutput.Successes),
                    () => {
                        TEntity deletedEntity = deleteOutput.Successes[0];

                        Assert.Equal(entity.Id, deletedEntity.Id);
                    },
                    () => {
                        Assert.Null(
                                _database.Set<TEntity>().Find(entity.Id)
                            );
                    }
                ]
            );
    }

    #endregion

    #region View Base Tests

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = "[View]: Simple view calculation")]
    public virtual async Task View_Scucess() {
        const int viewPage = 1;
        await Store(30, EntityFactory);

        ViewOutput<TEntity> viewOutput = await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = new() {
                        Retroactive = false,
                        Range = 20,
                        Page = viewPage,
                    }
                }
            );

        Assert.Multiple(
            () => Assert.True(viewOutput.Pages > 1),
            () => Assert.True(viewOutput.Length > 0),
            () => Assert.Equal(viewPage, viewOutput.Page),
            () => Assert.Equal(viewOutput.Length, viewOutput.Entities.Length)
        );
    }

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = "[View]: Specific page")]
    public virtual async Task View_OnSpecificPage_Sucess() {
        const int viewPage = 2;
        await Store(30, EntityFactory);

        ViewOutput<TEntity> viewOutput = await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = new ViewInput<TEntity> {
                        Retroactive = false,
                        Range = 20,
                        Page = viewPage,
                    }
                }
            );

        Assert.Multiple(
            () => Assert.True(viewOutput.Pages > 1),
            () => Assert.True(viewOutput.Length > 0),
            () => Assert.Equal(viewPage, viewOutput.Page),
            () => Assert.Equal(viewOutput.Length, viewOutput.Entities.Length)
        );
    }

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = $"[View]: Ordering by property")]
    public virtual async Task View_OnOrdering_Success() {

        ViewOutput<TEntity> orderedViewOutput = await _depot.View(
                        new QueryInput<TEntity, ViewInput<TEntity>> {
                            Parameters = new() {
                                Page = 1,
                                Range = 20,
                                Retroactive = false,
                                Orderings = [
                                    new ViewOrdering {
                                        Property = _evaluableProperty.Name,
                                        Ordering = ViewOrderings.Descending,
                                    },
                                ],
                            },
                        }
                   );

        // --> Manual ordering undordered result for reference.
        IEnumerable<TEntity> orderedReferenceRecords = orderedViewOutput.Entities.Cast<TEntity>();
        {
            Type setType = typeof(TEntity);
            ParameterExpression parameterExpression = Expression.Parameter(setType, $"X0");

            MemberExpression memberExpression = Expression.MakeMemberAccess(parameterExpression, _evaluableProperty);
            UnaryExpression translationExpression = Expression.Convert(memberExpression, typeof(object));
            Expression<Func<TEntity, object>> orderingExpression = Expression.Lambda<Func<TEntity, object>>(translationExpression, parameterExpression);

            IQueryable<TEntity> sorted = orderedReferenceRecords.AsQueryable();
            sorted = sorted.OrderByDescending(orderingExpression);
            orderedReferenceRecords = [.. sorted];
        }

        for (int i = 0; i < orderedReferenceRecords.Count(); i++) {
            TEntity expected = orderedReferenceRecords.ElementAt(i);
            TEntity actual = (TEntity)orderedViewOutput.Entities[i];

            Assert.Equal(_evaluableProperty.GetValue(expected), _evaluableProperty.GetValue(actual));
        }
    }

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = "[View]: Using date filter")]
    public virtual async Task View_DateFiltered_Success() {
        ViewOutput<TEntity> viewOutput = await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = new() {
                        Page = 1,
                        Range = 20,
                        Retroactive = false,
                        Filters = [
                            new ViewFilterDate<TEntity> {
                                From = DateTime.UtcNow.Date,
                            },
                        ],
                    },
                }
            );


        Assert.All(
            viewOutput.Entities,
            (i) => {
                Assert.True(DateTime.Compare(i.Timestamp, DateTime.UtcNow.Date) > 0);
            }
        );
    }

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = "[View]: Using property filter (CONTAINS)")]
    public virtual async Task View_ProeprtyFilteredByContains_Success() {
        if (_evaluableProperty.PropertyType != typeof(string)) {
            throw SkipException.ForSkip("This assertion is only available for entities that have an evaluable string property since CONTAINS method is currently only supported to filter string type properties.");
        }

        TEntity sampleEntity = await Store(EntityFactory);
        object? sampleValue = _evaluableProperty.GetValue(sampleEntity);

        ViewOutput<TEntity> qOut = await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = new() {
                        Retroactive = false,
                        Range = 20,
                        Page = 1,
                        Filters = [
                            new ViewFilterProperty<TEntity> {
                                Operator = ViewFilterOperators.CONTAINS,
                                Property = _evaluableProperty.Name,
                                Value = sampleValue,
                            }
                        ],
                    }
                }
            );
        Assert.All(
            qOut.Entities,
            (i) => {
                object? value = _evaluableProperty.GetValue(i);

                Assert.Equal(sampleValue, value);
            }
        );
    }

    /// <summary>
    ///     Method: <see cref="IDepotView{TEntity}.View(QueryInput{TEntity, ViewInput{TEntity}})"/>.
    ///     Expectation: Success view.
    /// </summary>
    [Fact(DisplayName = "[View]: Using filter logical (OR)")]
    public virtual async Task View_LogicalFilteredByOr_Success() {
        if (_evaluableProperty.PropertyType != typeof(string)) {
            throw SkipException.ForSkip("This assertion is only available for entities that have an evaluable string property since CONTAINS method is currently only supported to filter string type properties.");
        }

        TEntity[] entities = await Store(2, EntityFactory);

        List<object?> possibleValues = [];
        List<IViewFilter<TEntity>> filters = [];

        foreach (TEntity entity in entities) {
            object? sampleValue = _evaluableProperty.GetValue(entity);
            filters.Add(
                    new ViewFilterProperty<TEntity> {
                        Operator = ViewFilterOperators.CONTAINS,
                        Property = _evaluableProperty.Name,
                        Value = sampleValue,
                    }
                );
            possibleValues.Add(sampleValue);
        }
        ViewOutput<TEntity> viewOutput = await _depot.View(
                new QueryInput<TEntity, ViewInput<TEntity>> {
                    Parameters = new() {
                        Retroactive = false,
                        Range = 20,
                        Page = 1,
                        Filters = [
                            new ViewFilterLogical<TEntity>{
                                Operator = ViewFilterLogicalOperators.OR,
                                Filters = [..filters],
                            },
                        ],
                    }
                }
            );
        Assert.All(
            viewOutput.Entities,
            (i) => {
                object? actualValue = _evaluableProperty.GetValue(i);
                Assert.Contains(actualValue, possibleValues);
            }
        );
    }

    #endregion
}