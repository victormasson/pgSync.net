// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;

// namespace PgSync;

// public class TableSync
// {
//     private readonly DataSource source;
//     private readonly DataSource destination;
//     private readonly List<Task> tasks;
//     private readonly Options opts;
//     private readonly TaskResolver resolver;

//     public TableSync(DataSource source, DataSource destination, List<Task> tasks, Options opts, TaskResolver resolver)
//     {
//         this.source = source;
//         this.destination = destination;
//         this.tasks = tasks;
//         this.opts = opts;
//         this.resolver = resolver;
//     }

//     public void Perform()
//     {
//         ConfirmTablesExist(destination, tasks, "destination");

//         AddColumns();

//         AddPrimaryKeys();

//         if (!opts.NoSequences)
//         {
//             AddSequences();
//         }

//         ShowNotes();

//         // Don't sync tables with no shared fields
//         // We show a warning message above
//         RunTasks(tasks.Where(task => task.SharedFields.Any()).ToList());
//     }

//     private void AddColumns()
//     {
//         var sourceColumns = Columns(source);
//         var destinationColumns = Columns(destination);

//         foreach (var task in tasks)
//         {
//             task.FromColumns = sourceColumns.GetValueOrDefault(task.Table) ?? new List<Column>();
//             task.ToColumns = destinationColumns.GetValueOrDefault(task.Table) ?? new List<Column>();
//         }
//     }

//     private void AddPrimaryKeys()
//     {
//         var destinationPrimaryKeys = PrimaryKeys(destination);

//         foreach (var task in tasks)
//         {
//             task.ToPrimaryKey = destinationPrimaryKeys.GetValueOrDefault(task.Table) ?? new List<string>();
//         }
//     }

//     private void AddSequences()
//     {
//         var sourceSequences = Sequences(source);
//         var destinationSequences = Sequences(destination);

//         foreach (var task in tasks)
//         {
//             var sharedColumns = new HashSet<string>(task.SharedFields);

//             task.FromSequences = (sourceSequences.GetValueOrDefault(task.Table) ?? new List<Sequence>())
//                 .Where(s => sharedColumns.Contains(s.Column)).ToList();
//             task.ToSequences = (destinationSequences.GetValueOrDefault(task.Table) ?? new List<Sequence>())
//                 .Where(s => sharedColumns.Contains(s.Column)).ToList();
//         }
//     }

//     private void ShowNotes()
//     {
//         foreach (var note in resolver.Notes)
//         {
//             Warning(note);
//         }

//         foreach (var task in tasks)
//         {
//             foreach (var note in task.Notes)
//             {
//                 Warning($"{TaskName(task)}: {note}");
//             }
//         }

//         if (opts.DeferConstraintsV1)
//         {
//             var constraints = NonDeferrableConstraints(destination);
//             constraints = tasks.SelectMany(t => constraints.GetValueOrDefault(t.Table) ?? new List<string>()).ToList();
//             Warning($"Non-deferrable constraints: {string.Join(", ", constraints)}");
//         }
//     }

//     private void RunTasks(List<Task> tasks)
//     {
//         var notices = new List<string>();
//         var failedTables = new List<string>();
//         var startedAt = new Dictionary<Task, double>();

//         var showSpinners = Console.IsOutputRedirected && !opts.InBatches && !opts.Debug;
//         var spinners = new Dictionary<Task, Spinner>();
//         var taskSpinners = new Dictionary<Task, Spinner>();

//         var start = new Action<Task, int>((task, i) =>
//         {
//             var message = $"{TaskName(task)}";

//             if (showSpinners)
//             {
//                 var spinner = new Spinner(message);
//                 spinner.Start();
//                 spinners.Add(task, spinner);
//                 taskSpinners.Add(task, spinner);
//             }
//             else if (opts.InBatches)
//             {
//                 Log($"{message}...");
//             }

//             startedAt[task] = MonotonicTime();
//         });

//         var finish = new Action<Task, int, Dictionary<string, object>>((task, i, result) =>
//         {
//             var time = Math.Round(MonotonicTime() - startedAt[task], 1);

//             var success = (string)result["status"] == "success";

//             var message = result.ContainsKey("message") ?
//                 $"({result["message"].ToString().Split(Environment.NewLine).FirstOrDefault()?.Trim()})" :
//                 $"- {time}s";

//             notices.AddRange(((List<string>)result["notices"]));

//             if (showSpinners)
//             {
//                 var spinner = taskSpinners[task];
//                 if (success)
//                 {
//                     spinner.Success(message);
//                 }
//                 else
//                 {
//                     spinner.Error(message);
//                 }
//             }
//             else
//             {
//                 var status = success ? "✔" : "✖";
//                 Log($"{status} {TaskName(task)} {message}");
//             }

//             if (!success)
//             {
//                 failedTables.Add(TaskName(task));
//                 FailSync(failedTables);
//             }
//         });

//         var options = new ParallelOptions
//         {
//             StartAction = start,
//             FinishAction = finish,
//             CancellationToken = null // Optional cancellation token if needed
//         };

//         var jobs = opts.Jobs;

//         if (opts.Debug || opts.InBatches || opts.DeferConstraintsV1 || opts.DeferConstraintsV2 ||
//             opts.DisableIntegrity || opts.DisableIntegrityV2)
//         {
//             Warning("--jobs ignored");
//             jobs = 0;
//         }

//         if (IsWindows())
//         {
//             options.MaxDegreeOfParallelism = jobs ?? 4;
//         }
//         else
//         {
//             options.MaxDegreeOfParallelism = jobs ?? Environment.ProcessorCount;
//         }

//         MaybeDeferConstraints(() =>
//         {
//             Parallel.ForEach(tasks, options, task =>
//             {
//                 source.ReconnectIfNeeded();
//                 destination.ReconnectIfNeeded();

//                 task.Perform();
//             });
//         });

//         foreach (var notice in notices)
//         {
//             Warning(notice);
//         }

//         if (failedTables.Any())
//         {
//             FailSync(failedTables);
//         }
//     }

//     private void MaybeDeferConstraints(Action action)
//     {
//         if (opts.DisableIntegrity || opts.DisableIntegrityV2)
//         {
//             using (source.Transaction())
//             {
//                 action.Invoke();
//             }
//         }
//         else if (opts.DeferConstraintsV1 || opts.DeferConstraintsV2)
//         {
//             using (destination.Transaction())
//             {
//                 if (opts.DeferConstraintsV2)
//                 {
//                     var tableConstraints = NonDeferrableConstraints(destination);
//                     foreach (var (table, constraints) in tableConstraints)
//                     {
//                         foreach (var constraint in constraints)
//                         {
//                             destination.Execute($"ALTER TABLE {QuoteIdentFull(table)} ALTER CONSTRAINT {QuoteIdent(constraint)} DEFERRABLE");
//                         }
//                     }
//                 }

//                 destination.Execute("SET CONSTRAINTS ALL DEFERRED");

//                 using (source.Transaction())
//                 {
//                     action.Invoke();
//                 }

//                 if (opts.DeferConstraintsV2)
//                 {
//                     destination.Execute("SET CONSTRAINTS ALL IMMEDIATE");

//                     var tableConstraints = NonDeferrableConstraints(destination);
//                     foreach (var (table, constraints) in tableConstraints)
//                     {
//                         foreach (var constraint in constraints)
//                         {
//                             destination.Execute($"ALTER TABLE {QuoteIdentFull(table)} ALTER CONSTRAINT {QuoteIdent(constraint)} NOT DEFERRABLE");
//                         }
//                     }
//                 }
//             }
//         }
//         else
//         {
//             action.Invoke();
//         }
//     }

//     private void FailSync(List<string> failedTables)
//     {
//         throw new Error($"Sync failed for {failedTables.Count} table{(failedTables.Count == 1 ? "" : "s")}: {string.Join(", ", failedTables)}");
//     }

//     private string TaskName(Task task)
//     {
//         return $"{task.Table.Schema}.{task.Table.Name}";
//     }

//     private bool IsWindows()
//     {
//         return Environment.OSVersion.Platform == PlatformID.Win32NT;
//     }

//     private double MonotonicTime()
//     {
//         var stopwatch = Stopwatch.StartNew();
//         return stopwatch.Elapsed.TotalSeconds;
//     }

//     private void Log(string message)
//     {
//         Console.WriteLine(message);
//     }

//     private void Warning(string message)
//     {
//         Console.WriteLine($"WARNING: {message}");
//     }

//     // Rest of the methods translation
//     // ...
// }