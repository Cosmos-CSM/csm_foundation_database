using CSM_Database_Core;
using CSM_Database_Core.Core.Attributes;

namespace DatabaseProxy.Entities;


/// <summary>
///     Represents a dependency entity proxy.
/// </summary>
public class EntityDependencyProxy
    : EntityBase {

    public override Type Database { get; init; } = typeof(DatabaseProxy);

    [EntityRelation]
    public ICollection<EntityProxy> EntityProxies { get; set; } = [];
}
