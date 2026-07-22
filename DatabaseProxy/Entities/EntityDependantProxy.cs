using CSM_Database_Core;
using CSM_Database_Core.Core.Attributes;
using CSM_Database_Core.Core.Extensions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseProxy.Entities;

/// <summary>
///     Represents a dependant entity proxy.
/// </summary>
public class EntityDependantProxy
    : EntityBase {

    public override Type Database { get; init; } = typeof(DatabaseProxy);


    [EntityRelation]
    public EntityProxy EntityProxy { get; init; } = default!;

    protected override void DesignEntity(EntityTypeBuilder etBuilder) {

        etBuilder.Link<EntityDependantProxy, EntityProxy>(
                nameof(EntityProxy),
                targetRef: nameof(EntityProxy.EntityDependantProxies),
                isAutoLoaded: true
            );
    }
}
