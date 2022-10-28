using System.Linq.Expressions;
using NPoco;

namespace MDRCloudServices.DataLayer.Models;

public static class DatabaseExtensions
{
    public static async Task<T?> SingleOrDefaultAsync<T>(this IDatabase db, int? id)
    {
        if (!id.HasValue) return default;
        return await db.SingleOrDefaultByIdAsync<T>(id);
    }

    public static Task<T> SingleAsync<T>(this IDatabase db, int? id)
    {
        if (!id.HasValue) throw new ArgumentNullException(nameof(id));
        return SingleInnerAsync<T>(db, id.Value);
    }

    private static async Task<T> SingleInnerAsync<T>(IDatabase db, int id)
    {
        return await db.SingleByIdAsync<T>(id);
    }

    public static async Task<bool> AnyAsync<T>(this IDatabase db, string query, params object[] args)
    {
        return await db.QueryAsync<T>(query, args).AnyAsync();
    }

    public static async Task<bool> AnyAsync<T>(this IDatabase db, Expression<Func<T, bool>> expression)
    {
        return await db.Query<T>().Where(expression).AnyAsync();
    }

    public static async Task<object> InsertAsync(this IDatabase db, string tableName, object poco)
    {
        return await db.InsertAsync(tableName, string.Empty, poco);
    }
}
