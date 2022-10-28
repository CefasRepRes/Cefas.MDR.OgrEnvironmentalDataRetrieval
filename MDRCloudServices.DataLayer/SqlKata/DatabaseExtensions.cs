/*
 Ported to NPoco from PetaPoco by David Lees 
 
 Copyright 2018-21 Aaron Sherber

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NPoco;
using SqlKata;
using SqlKata.Compilers;

namespace MDRCloudServices.DataLayer.SqlKata;


/// <summary>
/// Ported to NPoco from PetaPoco by David Lees 
/// 
/// Copyright 2018-21 Aaron Sherber
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
/// http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// </summary>
public static class DatabaseExtensions
{
    public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>(this IDatabase db, Query query) where TKey : notnull => db.Dictionary<TKey, TValue>(query, db.GetCompiler());

    public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>(this IDatabase db, Query query, Compiler compiler) where TKey : notnull
    {
        return db.Dictionary<TKey, TValue>(query.ToSql(compiler));
    }

    public static int Execute(this IDatabase db, Query query) => db.Execute(query, db.GetCompiler());

    public static int Execute(this IDatabase db, Query query, Compiler compiler) => db.Execute(query.ToSql(compiler));

    public static Task<int> ExecuteAsync(this IDatabase db, Query query) => db.ExecuteAsync(query, db.GetCompiler());

    public static Task<int> ExecuteAsync(this IDatabase db, Query query, Compiler compiler) => db.ExecuteAsync(query.ToSql(compiler));

    public static T ExecuteScalar<T>(this IDatabase db, Query query) => db.ExecuteScalar<T>(query, db.GetCompiler());

    public static T ExecuteScalar<T>(this IDatabase db, Query query, Compiler compiler) => db.ExecuteScalar<T>(query.ToSql(compiler));

    public static Task<T> ExecuteScalarAsync<T>(this IDatabase db, Query query) => db.ExecuteScalarAsync<T>(query, db.GetCompiler());

    public static Task<T> ExecuteScalarAsync<T>(this IDatabase db, Query query, Compiler compiler) => db.ExecuteScalarAsync<T>(query.ToSql(compiler));

    public static List<T> Fetch<T>(this IDatabase db, Query query) => db.Fetch<T>(query, db.GetCompiler());

    public static List<T> Fetch<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.Fetch<T>(query.ToSql(compiler));
    }

    public static List<T> Fetch<T>(this IDatabase db, long page, long itemsPerPage, Query query) => db.Fetch<T>(page, itemsPerPage, query, db.GetCompiler());

    public static List<T> Fetch<T>(this IDatabase db, long page, long itemsPerPage, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.Fetch<T>(page, itemsPerPage, query.ToSql(compiler));
    }

    public static Task<List<T>> FetchAsync<T>(this IDatabase db, Query query) => db.FetchAsync<T>(query, db.GetCompiler());

    public static Task<List<T>> FetchAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FetchAsync<T>(query.ToSql(compiler));
    }

    public static Task<List<T>> FetchAsync<T>(this IDatabase db, long page, long itemsPerPage, Query query) => db.FetchAsync<T>(page, itemsPerPage, query, db.GetCompiler());

    public static Task<List<T>> FetchAsync<T>(this IDatabase db, long page, long itemsPerPage, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FetchAsync<T>(page, itemsPerPage, query.ToSql(compiler));
    }

    public static TRet FetchMultiple<T1, T2, TRet>(this IDatabase db, Func<List<T1>, List<T2>, TRet> cb, Query query) => db.FetchMultiple<T1, T2, TRet>(cb, query, db.GetCompiler());

    public static TRet FetchMultiple<T1, T2, TRet>(this IDatabase db, Func<List<T1>, List<T2>, TRet> cb, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultiple<T1, T2, TRet>(cb, query.ToSql(compiler));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static TRet FetchMultiple<T1, T2, T3, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, TRet> cb, Query query) => db.FetchMultiple<T1, T2, T3, TRet>(cb, query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static TRet FetchMultiple<T1, T2, T3, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, TRet> cb, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultiple<T1, T2, T3, TRet>(cb, query.ToSql(compiler));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static TRet FetchMultiple<T1, T2, T3, T4, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Query query) => db.FetchMultiple<T1, T2, T3, T4, TRet>(cb, query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static TRet FetchMultiple<T1, T2, T3, T4, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultiple<T1, T2, T3, T4, TRet>(cb, query.ToSql(compiler));
    }

    public static (List<T1>, List<T2>) FetchMultiple<T1, T2>(this IDatabase db, Query query) => db.FetchMultiple<T1, T2>(query, db.GetCompiler());

    public static (List<T1>, List<T2>) FetchMultiple<T1, T2>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultiple<T1, T2>(query.ToSql(compiler));

    public static (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(this IDatabase db, Query query) => db.FetchMultiple<T1, T2, T3>(query, db.GetCompiler());

    public static (List<T1>, List<T2>, List<T3>) FetchMultiple<T1, T2, T3>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultiple<T1, T2, T3>(query.ToSql(compiler));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(this IDatabase db, Query query) => db.FetchMultiple<T1, T2, T3, T4>(query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static (List<T1>, List<T2>, List<T3>, List<T4>) FetchMultiple<T1, T2, T3, T4>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultiple<T1, T2, T3, T4>(query.ToSql(compiler));

    public static Task<TRet> FetchMultipleAsync<T1, T2, TRet>(this IDatabase db, Func<List<T1>, List<T2>, TRet> cb, Query query) => db.FetchMultipleAsync(cb, query, db.GetCompiler());

    public static Task<TRet> FetchMultipleAsync<T1, T2, TRet>(this IDatabase db, Func<List<T1>, List<T2>, TRet> cb, Query query, Compiler compiler)
    {

        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultipleAsync(cb, query.ToSql(compiler));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, TRet> cb, Query query) => db.FetchMultipleAsync(cb, query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<TRet> FetchMultipleAsync<T1, T2, T3, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, TRet> cb, Query query, Compiler compiler)
    {

        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultipleAsync(cb, query.ToSql(compiler));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Query query) => db.FetchMultipleAsync(cb, query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<TRet> FetchMultipleAsync<T1, T2, T3, T4, TRet>(this IDatabase db, Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Query query, Compiler compiler)
    {

        query = query.GenerateSelect<TRet>(db.PocoDataFactory);
        return db.FetchMultipleAsync(cb, query.ToSql(compiler));
    }

    public static Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(this IDatabase db, Query query) => db.FetchMultipleAsync<T1, T2>(query, db.GetCompiler());

    public static Task<(List<T1>, List<T2>)> FetchMultipleAsync<T1, T2>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultipleAsync<T1, T2>(query.ToSql(compiler));

    public static Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(this IDatabase db, Query query) => db.FetchMultipleAsync<T1, T2, T3>(query, db.GetCompiler());

    public static Task<(List<T1>, List<T2>, List<T3>)> FetchMultipleAsync<T1, T2, T3>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultipleAsync<T1, T2, T3>(query.ToSql(compiler));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(this IDatabase db, Query query) => db.FetchMultipleAsync<T1, T2, T3, T4>(query, db.GetCompiler());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Overriding original library")]
    public static Task<(List<T1>, List<T2>, List<T3>, List<T4>)> FetchMultipleAsync<T1, T2, T3, T4>(this IDatabase db, Query query, Compiler compiler) => db.FetchMultipleAsync<T1, T2, T3, T4>(query.ToSql(compiler));

    public static List<T> FetchOneToMany<T>(this IDatabase db, Expression<Func<T, IList>> many, Query query) => db.FetchOneToMany<T>(many, query, db.GetCompiler());

    public static List<T> FetchOneToMany<T>(this IDatabase db, Expression<Func<T, IList>> many, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FetchOneToMany<T>(many, query.ToSql(compiler));
    }

    public static List<T> FetchOneToMany<T>(this IDatabase db, Expression<Func<T, IList>> many, Func<T, object> idFunc, Query query) => db.FetchOneToMany<T>(many, idFunc, query, db.GetCompiler());

    public static List<T> FetchOneToMany<T>(this IDatabase db, Expression<Func<T, IList>> many, Func<T, object> idFunc, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FetchOneToMany<T>(many, idFunc, query.ToSql(compiler));
    }

    public static T First<T>(this IDatabase db, Query query) => db.First<T>(query, db.GetCompiler());

    public static T First<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.First<T>(query.ToSql(compiler));
    }

    public static Task<T> FirstAsync<T>(this IDatabase db, Query query) => db.FirstAsync<T>(query, db.GetCompiler());

    public static Task<T> FirstAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FirstAsync<T>(query.ToSql(compiler));
    }

    public static T? FirstOrDefault<T>(this IDatabase db, Query query) => db.FirstOrDefault<T>(query, db.GetCompiler());

    public static T? FirstOrDefault<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FirstOrDefault<T>(query.ToSql(compiler));
    }

    public static Task<T> FirstOrDefaultAsync<T>(this IDatabase db, Query query) => db.FirstOrDefaultAsync<T>(query, db.GetCompiler());

    public static Task<T> FirstOrDefaultAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.FirstOrDefaultAsync<T>(query.ToSql(compiler));
    }

    public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, Query query) => db.Page<T>(page, itemsPerPage, query, db.GetCompiler());

    public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.Page<T>(page, itemsPerPage, query.ToSql(compiler));
    }

    public static Task<Page<T>> PageAsync<T>(this IDatabase db, long page, long itemsPerPage, Query query) => db.PageAsync<T>(page, itemsPerPage, query, db.GetCompiler());

    public static Task<Page<T>> PageAsync<T>(this IDatabase db, long page, long itemsPerPage, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.PageAsync<T>(page, itemsPerPage, query.ToSql(compiler));
    }

    public static IEnumerable<T> Query<T>(this IDatabase db, Query query) => db.Query<T>(query, db.GetCompiler());

    public static IEnumerable<T> Query<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.Query<T>(query.ToSql(compiler));
    }

    public static IAsyncEnumerable<T> QueryAsync<T>(this IDatabase db, Query query) => db.QueryAsync<T>(query, db.GetCompiler());

    public static IAsyncEnumerable<T> QueryAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.QueryAsync<T>(query.ToSql(compiler));
    }

    public static T Single<T>(this IDatabase db, Query query) => db.Single<T>(query, db.GetCompiler());

    public static T Single<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.Single<T>(query.ToSql(compiler));
    }

    public static Task<T> SingleAsync<T>(this IDatabase db, Query query) => db.SingleAsync<T>(query, db.GetCompiler());

    public static Task<T> SingleAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.SingleAsync<T>(query.ToSql(compiler));
    }

    public static T SingleOrDefault<T>(this IDatabase db, Query query) => db.SingleOrDefault<T>(query, db.GetCompiler());

    public static T SingleOrDefault<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.SingleOrDefault<T>(query.ToSql(compiler));
    }

    public static Task<T> SingleOrDefaultAsync<T>(this IDatabase db, Query query) => db.SingleOrDefaultAsync<T>(query, db.GetCompiler());

    public static Task<T> SingleOrDefaultAsync<T>(this IDatabase db, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.SingleOrDefaultAsync<T>(query.ToSql(compiler));
    }

    public static List<T> SkipTake<T>(this IDatabase db, long skip, long take, Query query) => db.SkipTake<T>(skip, take, query, db.GetCompiler());

    public static List<T> SkipTake<T>(this IDatabase db, long skip, long take, Query query, Compiler compiler)
    {
        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.SkipTake<T>(skip, take, query.ToSql(compiler));
    }

    public static Task<List<T>> SkipTakeAsync<T>(this IDatabase db, long skip, long take, Query query) => db.SkipTakeAsync<T>(skip, take, query, db.GetCompiler());

    public static Task<List<T>> SkipTakeAsync<T>(this IDatabase db, long skip, long take, Query query, Compiler compiler)
    {

        query = query.GenerateSelect<T>(db.PocoDataFactory);
        return db.SkipTakeAsync<T>(skip, take, query.ToSql(compiler));
    }

    private static Compiler GetCompiler(this IDatabase db)
    {
        if (DefaultCompilers.TryGetCustom(db.DatabaseType, out var compiler))
            return compiler ?? db.DatabaseType.ToCompiler();
        else
            return db.DatabaseType.ToCompiler();
    }

    private static Compiler ToCompiler(this DatabaseType type)
    {
        var compilerType = type.GetProviderName() switch
        {
            "FirebirdSql.Data.FirebirdClient" => CompilerType.Firebird,
            "Oracle.ManagedDataAccess.Client" => CompilerType.Oracle,
            "Oracle.DataAccess.Client" => CompilerType.Oracle,
            "MySql.Data.MySQLClient" => CompilerType.MySql,
            "System.Data.SQLite" => CompilerType.SQLite,
            "Npgsql2" => CompilerType.Postgres,
            _ => CompilerType.SqlServer,
        };        

        return DefaultCompilers.Get(compilerType);
    }
}
