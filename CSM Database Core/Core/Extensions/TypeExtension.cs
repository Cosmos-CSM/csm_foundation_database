using System.Collections;
using System.Reflection;

namespace CSM_Database_Core.Core.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="Type" /> class.
/// </summary>
public static class TypeExtension {


    /// <summary>
    ///     Gets the properties from given <paramref name="type"/> that matches the given <paramref name="propType"/>.
    /// </summary>
    /// <param name="type">
    ///     Type to track properties.
    /// </param>
    /// <param name="propType">
    ///     Type of the properties to be tracked.
    /// </param>
    /// <param name="seeCollections">
    ///     Whether the logic should see the <paramref name="propType"/> from collections generics declarations. Only checks collection with one generic declarations.
    /// </param>
    /// <returns>
    ///     Properties that match <paramref name="propType"/>.
    /// </returns>
    public static PropertyInfo[] GetProperties(this Type type, Type propType, bool seeCollections = true) {

        PropertyInfo[] props = type.GetProperties();

        IEnumerable<PropertyInfo> matchedProps = props.Where(
            (prop) => {

                if (prop.PropertyType == propType)
                    return true;

                if (!seeCollections || !prop.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                    return false;

                Type[] innerTypes = prop.PropertyType
                    .GetInterfaces()
                    .FirstOrDefault(
                            tarRelCollTypeInterface =>
                                tarRelCollTypeInterface.IsGenericType
                                && tarRelCollTypeInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        )
                        !.GetGenericArguments();

                if (innerTypes.Length != 1)
                    return false;

                Type toCheckGeneric = innerTypes[0];
                return toCheckGeneric == propType;
            }
        );

        return [.. matchedProps];
    }
}
