using CSM_Foundation.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.Reflection;

namespace CSM_Foundation_Database.Extensions;

/// <summary>
/// 
/// </summary>
public static class EntityTypeBuilderExtension
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Builder"></param>
    /// <param name="Relation"></param>
    /// <param name="SourceReference"></param>
    /// <param name="Required"></param>
    /// <param name="Auto"></param>
    /// <param name="Deletion"></param>
    public static void Link(this EntityTypeBuilder Builder, (Type Source, Type Target) Relation, string SourceReference, string? TargetReference = null, bool Required = false, bool Auto = false, bool Index = false, DeleteBehavior Deletion = DeleteBehavior.Restrict)
    {
        Type entityIType = typeof(IEntity);
        Type source = Relation.Source;
        Type target = Relation.Target;

        if (!(source.IsAssignableTo(entityIType) && target.IsAssignableTo(entityIType)))
        {
            throw new Exception($"[SourceT ({source.Name})] or [Target ({target.Name})] Relation configuration is not an [IEntity]");
        }
        PropertyInfo sourceNavigation = source.GetProperty(SourceReference)
            ?? throw new Exception($"[Source {source.Name}] doesn't contain navigation reference ({SourceReference})");
        if (sourceNavigation.PropertyType.IsGenericType && sourceNavigation.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            throw new Exception($"This method only supports one to on/many relationship for many-to-many use ({1})");
        }

        string targetReference = TargetReference ?? $"{source.Name}";
        PropertyInfo? targetNavigation = target.GetProperty(targetReference);
        if (TargetReference == null && targetNavigation == null)
        {
            targetReference = $"{source.Name}s";
            targetNavigation = target.GetProperty(targetReference);
        }
        string shadowProperty = $"{SourceReference}Shadow";

        Type propType = Required ? typeof(long) : typeof(long?);
        Builder.Property(propType, shadowProperty).HasColumnName(SourceReference).HasColumnType("bigint").IsRequired(Required);

        ReferenceNavigationBuilder relationBuilder = Builder.HasOne(target, SourceReference);
        if (targetNavigation != null && targetNavigation.PropertyType.IsGenericType && targetNavigation.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            relationBuilder.WithMany(targetReference).HasForeignKey(shadowProperty).OnDelete(Deletion).IsRequired(Required);
        }
        else
        {

            relationBuilder.WithOne(targetNavigation is null ? null : targetReference).HasForeignKey(source, shadowProperty).OnDelete(Deletion).IsRequired(Required);
        }

        if (Auto)
        {
            Builder.Navigation(SourceReference).AutoInclude();
        }

        if (Index && Required)
        {
            Builder.HasIndex(shadowProperty).IsUnique();
        }
    }
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="SourceT"></typeparam>
    /// <typeparam name="TargetT"></typeparam>
    /// <param name="etBuilder"></param>
    /// <param name="SourceReference"></param>
    /// <param name="Required"></param>
    /// <param name="Auto"></param>
    /// <param name="Deletion"></param>
    public static void Link<SourceT, TargetT>(this EntityTypeBuilder etBuilder, string SourceReference, string? TargetReference = null, bool Required = false, bool Auto = false, bool Index = false, DeleteBehavior Deletion = DeleteBehavior.Restrict)
        where SourceT : class, IEntity
        where TargetT : class, IEntity
    {

        Link(
                etBuilder,
                (typeof(SourceT), typeof(TargetT)),
                SourceReference: SourceReference,
                TargetReference: TargetReference,
                Required: Required,
                Auto: Auto,
                Index: Index,
                Deletion: Deletion
            );
    }
}
