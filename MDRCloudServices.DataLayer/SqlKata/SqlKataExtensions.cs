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
using System.Linq;
using Microsoft.Data.SqlClient;
using NPoco;
using SqlKata;
using SqlKata.Compilers;

namespace MDRCloudServices.DataLayer.SqlKata;

public enum CompilerType
{ SqlServer, MySql, Postgres, Firebird, SQLite, Oracle, Custom };

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
public static class SqlKataExtensions
{
    private static CompilerType _defaultCompilerType = CompilerType.SqlServer;
    private static Compiler? _customCompiler;
    private static readonly object _compilerLock = new();

    /// <summary>
    /// Indicates the <seealso cref="Compiler"/> that gets used when one is not specified.
    /// Defaults to SqlServer.
    /// </summary>
    public static CompilerType DefaultCompilerType
    {
        get => _defaultCompilerType;
        set
        {
            lock (_compilerLock)
            {
                if (value != _defaultCompilerType && value != CompilerType.Custom)
                {
                    _customCompiler = null;
                }

                _defaultCompilerType = value;
            }
        }
    }

    /// <summary>
    /// A custom <seealso cref="Compiler"/> instance to use when one is not specified.
    /// </summary>
    public static Compiler? CustomCompiler
    {
        get => _customCompiler;
        set
        {
            lock (_compilerLock)
            {
                if (value != null)
                {
                    _defaultCompilerType = CompilerType.Custom;
                }

                _customCompiler = value;
            }
        }
    }

    /// <summary>The NPoco data factory used to map table and column names.</summary>
    public static IPocoDataFactory DataFactory { get; set; } = new Database(new SqlConnection()).PocoDataFactory;

    /// <summary>
    /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object,
    /// using a <seealso cref="SqlServerCompiler"/>.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static Sql ToSql(this Query query) => query.ToSql(DefaultCompilerType);

    /// <summary>
    /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="compilerType"></param>
    /// <returns></returns>
    public static Sql ToSql(this Query query, CompilerType compilerType)
    {
        Compiler compiler;
        if (compilerType == CompilerType.Custom)
        {
            compiler = CustomCompiler
                ?? throw new InvalidOperationException($"'{nameof(compilerType)}' is 'Custom' but no CustomCompiler was provided.");
        }
        else
        {
            compiler = DefaultCompilers.Get(compilerType);
        }

        return query.ToSql(compiler);
    }

    /// <summary>
    /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
    /// </summary>
    /// <typeparam name="T">Type of <seealso cref="Compiler"/> to use.</typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static Sql ToSql<T>(this Query query) where T : Compiler, new()
    {
        var compiler = new T();
        return query.ToSql(compiler);
    }

    /// <summary>
    /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="compiler"></param>
    /// <returns></returns>
    public static Sql ToSql(this Query query, Compiler compiler)
    {
        query = query ?? throw new ArgumentNullException(nameof(query));
        compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));

        var compiled = compiler.Compile(query);
        var ppSql = Helper.ReplaceAll(compiled.RawSql, "?", x => "@" + x);

        return new Sql(";" + ppSql, [.. compiled.Bindings]);
    }

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static Query ForType<T>(this Query query) => query.ForType<T>(DataFactory);

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type and mapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query ForType<T>(this Query query, IPocoDataFactory factory) => query.ForType(typeof(T), factory);

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Query ForType(this Query query, Type type) => query.ForType(type, DataFactory);

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type and mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query ForType(this Query query, Type type, IPocoDataFactory factory)
    {
        query = query ?? throw new ArgumentNullException(nameof(query));
        factory ??= DataFactory ?? throw new ArgumentNullException(nameof(factory));

        var tableInfo = factory.ForType(type).TableInfo;
        return query.FromRaw(tableInfo.TableName);
    }

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given object, using a default mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="poco"></param>
    /// <returns></returns>
    public static Query ForObject(this Query query, object poco) => query.ForObject(poco, DataFactory);

    /// <summary>
    /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given object and mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="poco"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query ForObject(this Query query, object poco, IPocoDataFactory factory) => query.ForType(poco.GetType(), factory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static Query GenerateSelect<T>(this Query query) => query.GenerateSelect<T>(DataFactory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type and mapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query GenerateSelect<T>(this Query query, IPocoDataFactory factory) => query.GenerateSelect(typeof(T), factory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given object, using a default mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="poco"></param>
    /// <returns></returns>
    public static Query GenerateSelect(this Query query, object poco) => query.GenerateSelect(poco, DataFactory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given object and mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="poco"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query GenerateSelect(this Query query, object poco, IPocoDataFactory factory)
        => query.GenerateSelect(poco.GetType(), factory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Query GenerateSelect(this Query query, Type type) => query.GenerateSelect(type, DataFactory);

    /// <summary>
    /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type and mapper.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Query GenerateSelect(this Query query, Type type, IPocoDataFactory factory)
    {
        query = query ?? throw new ArgumentNullException(nameof(query));

        if (!query.HasComponent("select"))
        {
            factory ??= DataFactory ?? throw new ArgumentNullException(nameof(factory));
            var pd = factory.ForType(type);

            if (!query.HasComponent("from"))
            {
                query.FromRaw($"{pd.TableInfo.TableName} as \"a\"");
                query = pd.Columns.Count != 0 ? query.Select(pd.QueryColumns.Select(x => $"a.{x.Value.ColumnName}").ToArray()) : query.SelectRaw("NULL");
            }
            else
            {
                query = pd.Columns.Count != 0 ? query.Select(pd.QueryColumns.Select(x => x.Value.ColumnName).ToArray()) : query.SelectRaw("NULL");
            }
        }

        return query;
    }
}
