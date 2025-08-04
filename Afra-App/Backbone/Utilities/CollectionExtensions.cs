namespace Afra_App.Backbone.Utilities;

/// <summary>
///     Contains extension methods on the <see cref="ICollection{T}" /> interface to help working with primitive
///     collections that cannot be easily mapped to a more complete data type.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    ///     Adds an item to a collection if it is not already part of the collection
    /// </summary>
    /// <param name="collection">The collection to add to</param>
    /// <param name="item">The item to add</param>
    /// <typeparam name="T">The collections entries type</typeparam>
    /// <returns>True, iff the item was added; Otherwise, false.</returns>
    /// <remarks>
    ///     This has the same behaviour as calling TryAdd on a <see cref="Dictionary{TKey,TValue}" /> or Add on a
    ///     <see cref="ISet{T}" />.
    /// </remarks>
    public static bool AddOnce<T>(this ICollection<T> collection, T item)
    {
        if (collection.Contains(item)) return false;
        collection.Add(item);
        return true;
    }
}
