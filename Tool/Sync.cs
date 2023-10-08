// using System;
// using System.IO;
// using System.Text;
// using System.Diagnostics;
// using System.Collections.Generic;
// using System.Linq;
// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.NamingConventions;

// namespace PgSync;
// public class Sync
// {
//     private readonly string[] arguments;
//     private readonly Dictionary<string, string> options;

//     public Sync(string[] arguments, Dictionary<string, string> options)
//     {
//         this.arguments = arguments;
//         this.options = options;
//     }

//     public void Perform()
//     {
//         var startedAt = Utils.MonotonicTime();

//         var args = arguments;
//         var opts = options;

//         // Only resolve commands from config, not CLI arguments
//         var dataSourceResolver = new Dictionary<string, DataSource>
//             {
//                 { "to", ResolveSource(config.GetValueOrDefault("to")) },
//                 { "from", ResolveSource(config.GetValueOrDefault("from")) }
//             };

//         // Merge other config
//         var otherConfigOptions = new List<string> { "to_safe", "exclude", "schemas" };
//         foreach (var opt in otherConfigOptions)
//         {
//             if (!opts.ContainsKey(opt))
//             {
//                 opts[opt] = config.GetValueOrDefault(opt);
//             }
//         }

//         if (args.Length > 2)
//         {
//             throw new Error("Usage:\n    pgsync [options]");
//         }

//         if (!source.Exists())
//         {
//             throw new Error("No source");
//         }

//         if (!destination.Exists())
//         {
//             throw new Error("No destination");
//         }

//         if (!(opts.ContainsKey("to_safe") || destination.IsLocal()))
//         {
//             throw new Error("Danger! Add `to_safe: true` to `.pgsync.yml` if the destination is not localhost or 127.0.0.1");
//         }

//         PrintDescription("From", source);
//         PrintDescription("To", destination);

//         if ((opts.ContainsKey("preserve") || opts.ContainsKey("overwrite")) && destination.ServerVersionNum < 90500)
//         {
//             throw new Error("Postgres 9.5+ is required for --preserve and --overwrite");
//         }

//         var resolver = new TaskResolver(args, opts, source, destination, config, FirstSchema);
//         var tasks = resolver.Tasks.Select(task => new Task(source, destination, config, task.Table, opts.Merge(new Dictionary<string, string> { { "sql", task.Sql } }))).ToList();

//         if (opts.ContainsKey("in_batches") && tasks.Count > 1)
//         {
//             throw new Error("Cannot use --in-batches with multiple tables");
//         }

//         ConfirmTablesExist(source, tasks, "source");

//         if (opts.ContainsKey("list"))
//         {
//             ConfirmTablesExist(destination, tasks, "destination");
//             tasks.ForEach(task => Log(TaskName(task)));
//         }
//         else
//         {
//             if (opts.ContainsKey("schema_first") || opts.ContainsKey("schema_only"))
//             {
//                 new SchemaSync(source, destination, tasks, args, opts).Perform();
//             }

//             if (!opts.ContainsKey("schema_only"))
//             {
//                 new TableSync(source, destination, tasks, opts, resolver).Perform();
//             }

//             LogCompleted(startedAt);
//         }
//     }

//     private Dictionary<string, string> config;
//     private Dictionary<string, string> Config
//     {
//         get
//         {
//             if (config == null)
//             {
//                 var file = ConfigFile;
//                 if (file != null)
//                 {
//                     using (var reader = new StreamReader(file, Encoding.UTF8))
//                     {
//                         var deserializer = new DeserializerBuilder()
//                             .WithNamingConvention(UnderscoredNamingConvention.Instance)
//                             .Build();

//                         config = deserializer.Deserialize<Dictionary<string, string>>(reader) ?? new Dictionary<string, string>();
//                     }
//                 }
//                 else
//                 {
//                     config = new Dictionary<string, string>();
//                 }
//             }
//             return config;
//         }
//     }

//     private string ConfigFile
//     {
//         get
//         {
//             if (opts.ContainsKey("config"))
//             {
//                 return opts["config"];
//             }
//             else if (opts.ContainsKey("db"))
//             {
//                 var file = DbConfigFile(opts["db"]);
//                 return SearchTree(file) ?? file;
//             }
//             else
//             {
//                 return SearchTree(".pgsync.yml");
//             }
//         }
//     }

//     private string SearchTree(string file)
//     {
//         if (File.Exists(file))
//         {
//             return file;
//         }

//         var path = Directory.GetCurrentDirectory();
//         for (var i = 0; i < 20; i++)
//         {
//             var absoluteFile = Path.Combine(path, file);
//             if (File.Exists(absoluteFile))
//             {
//                 return absoluteFile;
//             }
//             path = Path.GetDirectoryName(path);
//             if (path == "/")
//             {
//                 break;
//             }
//         }
//         return null;
//     }

//     private void PrintDescription(string prefix, DataSource source)
//     {
//         var location = source.Host != null ? $" on {source.Host}:{source.Port}" : "";
//         Log($"{prefix}: {source.DbName}{location}");
//     }

//     private void LogCompleted(double startedAt)
//     {
//         var time = MonotonicTime() - startedAt;
//         var message = $"Completed in {time.Round(1)}s";
//         Log(Colorize(message, "green"));
//     }

//     private DataSource source;
//     private DataSource Source
//     {
//         get
//         {
//             if (source == null)
//             {
//                 source = new DataSource(opts.GetValueOrDefault("from"), "from", opts.GetValueOrDefault("debug") == "true");
//                 GC.ReRegisterForFinalize(this, this.Finalize);
//             }
//             return source;
//         }
//     }

//     private DataSource destination;
//     private DataSource Destination
//     {
//         get
//         {
//             if (destination == null)
//             {
//                 destination = new DataSource(opts.GetValueOrDefault("to"), "to", opts.GetValueOrDefault("debug") == "true");
//                 GC.ReRegisterForFinalize(this, this.Finalize);
//             }
//             return destination;
//         }
//     }

//     private DataSource ResolveSource(string source)
//     {
//         if (source != null)
//         {
//             var resolvedSource = source;
//             resolvedSource = System.Text.RegularExpressions.Regex.Replace(resolvedSource, @"\$\([^)]+\)", m =>
//             {
//                 var command = m.Value.Substring(2, m.Value.Length - 3);
//                 var processInfo = new ProcessStartInfo
//                 {
//                     FileName = "cmd.exe",
//                     RedirectStandardOutput = true,
//                     UseShellExecute = false,
//                     CreateNoWindow = true,
//                     Arguments = $"/C {command}"
//                 };
//                 var process = new Process { StartInfo = processInfo };
//                 process.Start();
//                 var output = process.StandardOutput.ReadToEnd();
//                 process.WaitForExit();
//                 if (process.ExitCode != 0)
//                 {
//                     throw new Error($"Command exited with non-zero status:\n{command}");
//                 }
//                 return output.Trim();
//             });
//             return new DataSource(resolvedSource, debug: opts.GetValueOrDefault("debug") == "true");
//         }
//         return null;
//     }

//     private static Action<object> Finalize(DataSource ds)
//     {
//         return state => ds.Close();
//     }
// }
