namespace Altafraner.Backbone.Abstractions;

/// <inheritdoc />
/// <typeparam name="T">The module that is depended upon</typeparam>
public class DependsOnAttribute<T>() : DependsOnAttribute(typeof(T));

/// <summary>
///     Specifies that a module depends on another module
/// </summary>
/// <param name="t">The module that is depended upon</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute(Type t) : Attribute
{
    /// <summary>
    ///     The type of the module this module depends on.
    /// </summary>
    public Type ModuleType { get; } = t;
}
