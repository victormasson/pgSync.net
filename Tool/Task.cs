// using System;
// using System.Collections.Generic;

// namespace PgSync;

// public class Task
// {
//     private readonly ISource source;
//     private readonly IDestination destination;
//     private readonly Dictionary<string, string> config;
//     private readonly Table table;
//     private readonly Dictionary<string, string> opts;

//     public List<string> FromSequences { get; set; }
//     public List<string> ToSequences { get; set; }
//     public List<string> ToPrimaryKeys { get; set; }

//     public Task(ISource source, IDestination destination, Dictionary<string, string> config, Table table, Dictionary<string, string> opts)
//     {
//         this.source = source;
//         this.destination = destination;
//         this.config = config;
//         this.table = table;
//         this.opts = opts;
//         FromSequences = new List<string>();
//         ToSequences = new List<string>();
//     }

//     public string QuotedTable => QuoteIdentFull(table);

//     public void Perform()
//     {
//         PerformWithNotices(() =>
//         {
//             HandleErrors(() =>
//             {
//                 MaybeDisableTriggers(() =>
//                 {
//                     SyncData();
//                 });
//             });
//         });
//     }

//     private void PerformWithNotices(Action action)
//     {
//         var notices = new List<string>();
//         var dataSources = new List<IDataSource> { source, destination };
//         foreach (var dataSource in dataSources)
//         {
//             dataSource.Conn.SetNoticeProcessor(message =>
//             {
//                 notices.Add(message.Trim());
//             });
//         }

//         action.Invoke();

//         // Clear the notice processor
//         foreach (var dataSource in dataSources)
//         {
//             dataSource.Conn.SetNoticeProcessor(null);
//         }
//     }

//     private void HandleErrors(Action action)
//     {
//         try
//         {
//             action.Invoke();
//         }
//         catch (Exception e)
//         {
//             if (opts.ContainsKey("debug"))
//             {
//                 throw;
//             }

//             var message = e switch
//             {
//                 PGException pgException when pgException is PGConnectionBadException => "Connection failed",
//                 PGException pgException => pgException.Message.Substring("ERROR:  "),
//                 Error customError => customError.Message,
//                 _ => $"{e.GetType().Name}: {e.Message}"
//             };

//             // Handle the error...
//         }
//     }

//     private void SyncData()
//     {
//         if (SharedFields.Count == 0)
//         {
//             throw new Error("This should never happen. Please file a bug.");
//         }

//         var sqlClause = string.Empty;
//         if (opts.ContainsKey("sql"))
//         {
//             sqlClause = " " + opts["sql"];
//         }

//         var badFields = opts.ContainsKey("no_rules") ? new List<string>() : config["data_rules"];
//         var primaryKeys = ToPrimaryKeys;

//         string primaryKeyName = null;
//         if (primaryKeys.Count == 1)
//         {
//             primaryKeyName = primaryKeys[0];
//         }
//         else if (primaryKeys.Count > 1)
//         {
//             throw new Error("Composite primary keys are not supported.");
//         }
//         else
//         {
//             throw new Error("Primary key required to sync specific rows.");
//         }

//         var copyFields = SharedFields.Select(field =>
//         {
//             var badField = badFields.ContainsKey(field) ? badFields[field] : null;
//             if (badField != null)
//             {
//                 // Apply data rule strategy here...
//                 return ApplyStrategy(badField, table, field, primaryKeyName);
//             }
//             else
//             {
//                 return $"{QuotedTable}.{QuoteIdent(field)}";
//             }
//         }).ToList();

//         var fields = string.Join(", ", SharedFields.Select(field => QuoteIdent(field)));

//         var copyToCommand = $"COPY (SELECT {string.Join(", ", copyFields)} FROM {QuotedTable}{sqlClause}) TO STDOUT";

//         // Continue with the rest of the logic...
//     }

//     private string ApplyStrategy(string rule, Table table, string column, string primaryKey)
//     {
//         if (rule is Dictionary<string, string> ruleDict)
//         {
//             // Apply rule strategy based on the rule dictionary...
//             // You'll need to implement this based on your rules
//         }
//         else
//         {
//             switch (rule)
//             {
//                 case "untouched":
//                     return QuoteIdent(column);
//                 case "unique_email":
//                     // Apply unique email strategy...
//                     break;
//                 case "unique_phone":
//                     // Apply unique phone strategy...
//                     break;
//                 // Add more rule cases...
//                 default:
//                     throw new Error($"Unknown rule {rule} for column {column}");
//             }
//         }

//         // Return the result of applying the strategy...
//         return null;
//     }

//     private List<string> SharedFields
//     {
//         get
//         {
//             var sharedFields = new List<string>();
//             // Calculate shared fields...
//             return sharedFields;
//         }
//     }

//     // Other properties and methods...

//     private void MaybeDisableTriggers(Action action)
//     {
//         if (opts.ContainsKey("disable_integrity") || opts.ContainsKey("disable_integrity_v2") || opts.ContainsKey("disable_user_triggers"))
//         {
//             // Implement trigger disabling logic...
//         }
//         else
//         {
//             action.Invoke();
//         }
//     }
// }
