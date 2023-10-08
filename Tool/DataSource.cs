// using System;
// using System.Collections.Generic;
// using CommandLine;
// using Npgsql;

// namespace PgSync;


// public class DataSource
// {
//     private readonly string _url = string.Empty;
//     private readonly string _name = string.Empty;
//     private readonly bool _debug;

//     private NpgsqlConnection _conn = new NpgsqlConnection();
//     private string _host = string.Empty;
//     private int? _port;
//     private string _dbname = string.Empty;
//     private List<Table> _tableSet = new List<Table>();

//     public DataSource(string url = "", string name = "", bool debug = false)
//     {
//         _url = url;
//         _name = name;
//         _debug = debug;
//     }

//     public bool Exists => !string.IsNullOrEmpty(_url);

//     public bool Local => string.IsNullOrEmpty(Host) || new[] { "localhost", "127.0.0.1" }.Contains(Host);

//     public string Host => _host ??= DedupLocalHost(Conninfo["host"]);

//     public int Port => _port ??= DedupLocalHost(Conninfo["port"]);

//     public string DbName => _dbname ??= Conninfo["dbname"];

//     public List<Table> Tables
//     {
//         get
//         {
//             if (_tableSet == null)
//             {
//                 string query = @"
//                         SELECT
//                             table_schema AS schema,
//                             table_name AS table
//                         FROM
//                             information_schema.tables
//                         WHERE
//                             table_type = 'BASE TABLE' AND
//                             table_schema NOT IN ('information_schema', 'pg_catalog')
//                         ORDER BY 1, 2";
//                 _tableSet = new List<Table>();

//                 using (var connection = GetConnection())
//                 using (var command = new NpgsqlCommand(query, connection))
//                 using (var reader = command.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         string schema = reader.GetString(0);
//                         string table = reader.GetString(1);
//                         _tableSet.Add(new Table(schema, table));
//                     }
//                 }
//             }
//             return _tableSet;
//         }
//     }

//     public bool TableExists(Table table)
//     {
//         return Tables.Contains(table);
//     }

//     public int MaxId(Table table, string primaryKey, string sqlClause = null)
//     {
//         string query = $"SELECT MAX({QuoteIdent(primaryKey)}) FROM {QuoteIdentFull(table)}{sqlClause}";
//         return ExecuteScalar<int>(query);
//     }

//     public int MinId(Table table, string primaryKey, string sqlClause = null)
//     {
//         string query = $"SELECT MIN({QuoteIdent(primaryKey)}) FROM {QuoteIdentFull(table)}{sqlClause}";
//         return ExecuteScalar<int>(query);
//     }

//     public string LastValue(string seq)
//     {
//         string query = $"SELECT last_value FROM {QuoteIdentFull(seq)}";
//         return ExecuteScalar<string>(query);
//     }

//     public void Truncate(Table table)
//     {
//         string query = $"TRUNCATE {QuoteIdentFull(table)} CASCADE";
//         Execute(query);
//     }

//     public List<string> Schemas
//     {
//         get
//         {
//             string query = "SELECT schema_name FROM information_schema.schemata ORDER BY 1";
//             return ExecuteList<string>(query, reader => reader.GetString(0));
//         }
//     }

//     public void CreateSchema(string schema)
//     {
//         string query = $"CREATE SCHEMA {QuoteIdent(schema)}";
//         Execute(query);
//     }



//     public NpgsqlConnection GetConnection()
//     {
//         if (_conn == null)
//         {
//             string connString = _url.StartsWith("postgres://") || _url.StartsWith("postgresql://")
//                 ? _url
//                 : $"dbname={_url}";
//             _conn = new NpgsqlConnection(connString);
//         }
//         return _conn;
//     }

//     public void Close()
//     {
//         if (_conn != null)
//         {
//             _conn.Close();
//             _conn = null;
//         }
//     }

//     private Dictionary<string, string> Conninfo
//     {
//         get
//         {
//             var connBuilder = new NpgsqlConnectionStringBuilder(_url);
//             return connBuilder
//                 .ConnectionString
//                 .Split(";")
//                 .Select(part => part.Split("="))
//                 .ToDictionary(split => split[0].Trim(), split => split[1].Trim());
//         }
//     }

//     private T ExecuteScalar<T>(string query)
//     {
//         using (var connection = GetConnection())
//         using (var command = new NpgsqlCommand(query, connection))
//         {
//             return (T)command.ExecuteScalar();
//         }
//     }

//     private List<T> ExecuteList<T>(string query, NpgsqlParameter parameter, Func<NpgsqlDataReader, T> selector)
//     {
//         using (var connection = GetConnection())
//         using (var command = new NpgsqlCommand(query, connection))
//         {
//             command.Parameters.Add(parameter);
//             List<T> result = new List<T>();
//             using (var reader = command.ExecuteReader())
//             {
//                 while (reader.Read())
//                 {
//                     result.Add(selector(reader));
//                 }
//             }
//             return result;
//         }
//     }

//     private void Execute(string query)
//     {
//         using (var connection = GetConnection())
//         using (var command = new NpgsqlCommand(query, connection))
//         {
//             command.ExecuteNonQuery();
//         }
//     }


//     private int DedupLocalHost(string value)
//     {
//         if (Conninfo["host"] == "localhost,localhost" && Conninfo["port"].Split(",").Distinct().Count() == 1)
//         {
//             return int.Parse(value.Split(",")[0]);
//         }
//         return int.Parse(value);
//     }
// }
