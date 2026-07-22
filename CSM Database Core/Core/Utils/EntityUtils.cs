using System.Collections;
using System.Reflection;

using CSM_Database_Core.Core.Attributes.Abstractions.Interfaces;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Database_Core.Core.Utils;

/// <summary>
///     Provide utility methods for <see cref="IEntity"/> objects.
/// </summary>
public static class EntityUtils {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="relationName"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetEntityRelations(this IEntity entity, string relationName = "", Type? targetType = null) {
        Type entityType = entity.GetType();

        IEnumerable<PropertyInfo> relProps = entityType
            .GetProperties()
            .Where(
                    entityProp => {
                        IEnumerable<IRelationAttribute> attrs = entityProp
                            .GetCustomAttributes(inherit: true)
                            .OfType<IRelationAttribute>();


                        IRelationAttribute? attr = attrs.FirstOrDefault();
                        if (attr is null)
                            return false;

                        Type relType = entityProp.PropertyType;

                        if (
                            !string.IsNullOrWhiteSpace(relationName)
                            && entityProp.Name != relationName
                        ) {
                            return false;
                        }

                        if (targetType != null) {
                            if(!relType.IsAssignableTo(typeof(IEnumerable)) && relType != targetType) 
                                return false;


                            // --> Here we know is a collection.
                            Type tarRelEntityType = relType
                                .GetInterfaces()
                                .FirstOrDefault(
                                    tarRelCollTypeInterface =>
                                        tarRelCollTypeInterface.IsGenericType
                                        && tarRelCollTypeInterface.GetGenericTypeDefinition() == typeof(IEnumerable)
                                )
                                !.GetGenericArguments()[0];

                            if(tarRelEntityType != targetType)
                                return false;
                        }

                        return true;
                    }
                );

        return [.. relProps];
    }
}
