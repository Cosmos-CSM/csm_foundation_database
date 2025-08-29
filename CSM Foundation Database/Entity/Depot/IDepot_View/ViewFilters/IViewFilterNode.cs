using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

using CSM_Foundation.Database;

using CSM_Foundation_Core.Convertion;

namespace CSM_Foundation_Database.Entity.Depot.IDepot_View.ViewFilters;

/// <summary>
///     Represents a View system filter node, building and evaluating by itself (implementations) the way the View operation will be filtered.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> object being handled.
/// </typeparam>
public interface IViewFilterNode<TEntity>
    : IConverterVariation
    where TEntity : IEntity {
    /// <summary>
    ///     Filtering application order when a collection of <see cref="IViewFilterNode{T}"/> was given.
    /// </summary>
    int Order { get; set; }

    /// <summary>
    ///     Composes the current <see cref="IViewFilterNode{T}"/> instance into and translatable <see cref="Expression"/>
    ///     object understanable for the native { EF } engine.
    /// </summary>
    /// <returns>
    ///     The translated { EF } filtering expression.
    /// </returns>
    Expression<Func<TEntity, bool>> Compose();
}

/// <summary>
///     Represents a <see cref="JsonConverterFactory"/> for a <see cref="IViewFilterNode{T}"/> transaction data convertion factory.
/// </summary>
public class IViewFilterNodeConverterFactory
    : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) {
        if (!typeToConvert.IsGenericType) {
            return false;
        }

        Type genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(IViewFilterNode<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        Type itemType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(IViewFilterNodeConverter<>).MakeGenericType(itemType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>
///     Represents a <see cref="JsonConverter"/> handler for <see cref="IViewFilterNode{T}"/> system objects through complex data transactions.
/// </summary>
/// <typeparam name="TEntity">
///     <see cref="IEntity"/> type the <see cref="IViewFilterNode{T}"/> uses.
/// </typeparam>
public class IViewFilterNodeConverter<TEntity>
    : BConverter<IViewFilterNode<TEntity>>
    where TEntity : IEntity {


    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public IViewFilterNodeConverter()
        : base(
                [
                    typeof(ViewFilterLogical<>),
                    typeof(ViewFilterProperty<>),
                    typeof(ViewFilterDate<>)
                ]
            ) {
    }
}