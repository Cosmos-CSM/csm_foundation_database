namespace CSM_Database_Core.Entities.Abstractions.Interfaces;
/// <summary>
///     Represents an <see cref="IEntity"/> with enablement control.
/// </summary>
public interface IActivableEntity
    : IEntity {

    /// <summary>
    ///     Whether is enabled.
    /// </summary>
    bool IsEnabled { get; set; }
}