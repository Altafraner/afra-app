namespace Afra_App.Data.Json;

public record Pagination<T>
{
    public int Total { get; init; }
    public IEnumerable<T> Items { get; init; }
    
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
}