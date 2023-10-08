// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Threading.Tasks;
// using System.Runtime.Serialization;

// namespace PgSync;

// public class Utils
// {
//     private static readonly Dictionary<string, int> ColorCodes = new Dictionary<string, int>
//         {
//             { "red", 31 },
//             { "green", 32 },
//             { "yellow", 33 },
//             { "cyan", 36 }
//         };

//     public static void Log(string message = null)
//     {
//         Console.WriteLine(message);
//     }

//     public static string Colorize(string message, string color)
//     {
//         if (Console.IsOutputRedirected || !ColorCodes.ContainsKey(color))
//             return message;

//         return $"\u001b[{ColorCodes[color]}m{message}\u001b[0m";
//     }

//     public static void Warning(string message)
//     {
//         Log(Colorize(message, "yellow"));
//     }

//     public static void Deprecated(string message)
//     {
//         Warning("[DEPRECATED] " + message);
//     }

//     public static TextWriter Output => Console.Error;

//     public static string DbConfigFile(string db)
//     {
//         return $".pgsync-{db}.yml";
//     }

//     public static void ConfirmTablesExist(IDataSource dataSource, List<Task> tasks, string description)
//     {
//         foreach (var task in tasks)
//         {
//             if (!dataSource.TableExists(task.Table))
//             {
//                 throw new Error($"Table not found in {description}: {task.Table}");
//             }
//         }
//     }

//     public static string FirstSchema(ISource source)
//     {
//         return source.SearchPath.FirstOrDefault(sp => sp != "pg_catalog");
//     }

//     public static string TaskName(Task task)
//     {
//         return FriendlyName(task.Table);
//     }

//     public static string FriendlyName(Table table)
//     {
//         if (table.Schema == FirstSchema(table.Source))
//         {
//             return table.Name;
//         }
//         else
//         {
//             return table.FullName;
//         }
//     }

//     public static string QuoteIdentFull(object ident)
//     {
//         if (ident is Table table || ident is Sequence sequence)
//         {
//             return $"{QuoteIdent(((Table)ident).Schema)}.{QuoteIdent(((Table)ident).Name)}";
//         }
//         else // temp table names are strings
//         {
//             return QuoteIdent(ident.ToString());
//         }
//     }

//     public static string QuoteIdent(string value)
//     {
//         // Implement quoting logic here for C#
//         return value;
//     }

//     public static object Escape(object value)
//     {
//         if (value is string strValue)
//         {
//             return $"'{QuoteString(strValue)}'";
//         }
//         else
//         {
//             return value;
//         }
//     }

//     public static string QuoteString(string s)
//     {
//         // Implement string quoting logic here for C#
//         return s;
//     }

//     public static double MonotonicTime()
//     {
//         return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
//     }
// }
