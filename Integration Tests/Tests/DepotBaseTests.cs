using CSM_Database_Core.Depots.Abstractions.Bases;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Depots.Models.Structs;

using CSM_Database_Testing.Abstractions.Bases;

using DatabaseProxy;
using DatabaseProxy.Entities;

using Microsoft.EntityFrameworkCore;

namespace Integration_Tests.Tests;

/// <summary>
///     Integration tests class for <see cref="DepotBase{TDatabase, TEntity}"/>
/// </summary>
public class DepotBaseTests
    : DepotIntegrationTestsBase<EntityProxy, DepotProxy, DatabaseProxy.DatabaseProxy> {

    protected override EntityProxy EntityFactory(string Entropy) {
        return new EntityProxy {
        };
    }

    /// <summary>
    ///     Method: <see cref="DepotBase{TDatabase, TEntity}.Update(QueryInput{TEntity, UpdateInput{TEntity}})"/> 
    ///     Expectation: Success updating.
    /// </summary>
    public override async Task Update_Single_Success() {
        //Expectation
        EntityProxy testEntity = await Store(
               new EntityProxy {

               }
           );

        EntityDependencyProxy dependency = await Store(
                new EntityDependencyProxy()
            );

        EntityDependantProxy dependant = await Store(
                new EntityDependantProxy()
            );

        await Store(
                new EntityDependantProxy {
                    EntityProxy = testEntity
                }
            );

        testEntity.EntityDependencyProxy = dependency;

        UpdateOutput<EntityProxy> updateOutput = await _depot.Update(
                new QueryInput<EntityProxy, UpdateInput<EntityProxy>> {
                    Parameters = new UpdateInput<EntityProxy> {
                        Entity = testEntity,
                        Relations = new Dictionary<string, IDictionary<string, RelationUpdate[]>> {
                            {
                                nameof(EntityProxy.EntityDependantProxies),
                                new Dictionary<string, RelationUpdate[]> {
                                    {
                                        string.Empty,
                                        new RelationUpdate[] {
                                            new() {
                                                Entity = dependant,
                                                Action = RelationUpdateAction.ADD
                                            }
                                        }
                                    }
                                }

                            },
                        },
                    },
                    PostProcessor = (query) => {
                        return query.Include(i => i.EntityDependantProxies);
                    }
                }
            );

        EntityProxy? ogEntity = updateOutput.Original;
        EntityProxy updatedEntity = updateOutput.Updated;


        Assert.NotNull(ogEntity);
        Assert.Single(ogEntity.EntityDependantProxies);

        Assert.NotEmpty(updatedEntity.EntityDependantProxies);

        Assert.Contains(
                updatedEntity.EntityDependantProxies,
                (dependantEntry) => dependantEntry.Id == dependant.Id
            );
        Assert.Equal(
                dependency.Id,
                updatedEntity.EntityDependencyProxy.Id
            );
    }
}
