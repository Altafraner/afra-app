#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// Currenty not needed. Therefore, documentation has no priority. I know i'm gonna regret this later.
namespace Afra_App.Data.DTO;

public record Pagination<T>
{
    public Pagination(int Total, IEnumerable<T> Items)
    {
        this.Total = Total;
        this.Items = Items;
    }

    public Pagination(IQueryable<T> queryItems, int page, int pageSize)
    {
        Total = queryItems.Count();
        Items = queryItems.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public int Total { get; init; }
    public IEnumerable<T> Items { get; init; }
}