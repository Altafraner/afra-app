namespace Altafraner.Backbone.Abstractions;

public class DependsOnAttribute<T>() : DependsOnAttribute(typeof(T));

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute(Type t) : Attribute
{
    public Type ModuleType { get; } = t;
}
