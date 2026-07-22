using CSM_Database_Core;
using CSM_Database_Core.Core.Attributes;
using CSM_Database_Core.Core.Extensions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseProxy.Entities;

/// <summary>
///     Reepresents an entity proxy.
/// </summary>
public class EntityProxy
    : EntityBase {

    public override Type Database { get; init; } = typeof(DatabaseProxy);

    [EntityRelation]
    public EntityDependencyProxy EntityDependencyProxy { get; set; } = default!;


    [EntityRelation]
    public ICollection<EntityDependantProxy> EntityDependantProxies { get; set; } = [];


    protected override void DesignEntity(EntityTypeBuilder etBuilder) {

        etBuilder.Link<EntityProxy, EntityDependencyProxy>(
                nameof(EntityDependencyProxy),
                targetRef: nameof(EntityDependencyProxy.EntityProxies),
                isAutoLoaded: true,
                deleteBehavior: Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade
            );
    }
}
