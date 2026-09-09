namespace ProjectFlowAI.Application.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

/// <summary>Common paging/sorting/search parameters shared by every List*Query.</summary>
public abstract record PagedQueryBase
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } = "asc";
}

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortBy, string? sortDir, string defaultSortBy)
    {
        var property = string.IsNullOrWhiteSpace(sortBy) ? defaultSortBy : sortBy;
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var prop = typeof(T).GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop == null) return query;

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, prop);
        var orderByExp = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);
        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExp = System.Linq.Expressions.Expression.Call(
            typeof(Queryable), methodName, new[] { typeof(T), prop.PropertyType },
            query.Expression, System.Linq.Expressions.Expression.Quote(orderByExp));
        return query.Provider.CreateQuery<T>(resultExp);
    }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key) : base($"{entity} '{key}' was not found.") { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string message) : base(message) { }
}
