using CSM_Foundation.Database;

using CSM_Foundation_Core;

namespace CSM_Foundation_Database_Testing.Disposing;

/// <summary>
///     [Interface] for [Quality] purposes [Disposer] implementations.
/// </summary>
/// <remarks>
///     This Disposer only must be used on [Quality]/[Testing] strictly purposes.
/// </remarks>
public interface IQ_Disposer
    : IDisposer<IEntity> {
}
