// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;

// namespace PgSync;

// public class SchemaSync
// {
//     private readonly DataSource _source;
//     private readonly DataSource _destination;
//     private readonly List<Task> _tasks;
//     private readonly string[] _args;
//     private readonly Options _opts;

//     public SchemaSync(DataSource source, DataSource destination, List<Task> tasks, string[] args, Options opts)
//     {
//         _source = source;
//         _destination = destination;
//         _tasks = tasks;
//         _args = args;
//         _opts = opts;
//     }

//     public void Perform()
//     {
//         if (_opts.Preserve)
//         {
//             throw new Error("Cannot use --preserve with --schema-first or --schema-only");
//         }

//         string[] dumpCommand = GenerateDumpCommand();
//         string[] restoreCommand = GenerateRestoreCommand();

//         bool showSpinner = Console.IsOutputRedirected && !_opts.Debug;

//         if (showSpinner)
//         {
//             ConsoleSpinner spinner = new ConsoleSpinner("Syncing schema");
//             spinner.Start();
//         }

//         if (SpecifyTables())
//         {
//             CreateSchemas();
//         }

//         bool success = RunCommand(dumpCommand, restoreCommand);

//         if (showSpinner)
//         {
//             if (success)
//             {
//                 ConsoleSpinner.Stop("Done");
//             }
//             else
//             {
//                 ConsoleSpinner.Stop("Error");
//             }
//         }

//         if (!success)
//         {
//             throw new Error("Schema sync returned non-zero exit code");
//         }
//     }

//     private bool RunCommand(string[] dumpCommand, string[] restoreCommand)
//     {
//         ProcessStartInfo dumpStartInfo = new ProcessStartInfo(dumpCommand[0], string.Join(" ", dumpCommand.Skip(1)))
//         {
//             RedirectStandardError = true,
//             UseShellExecute = false
//         };

//         ProcessStartInfo restoreStartInfo = new ProcessStartInfo(restoreCommand[0], string.Join(" ", restoreCommand.Skip(1)))
//         {
//             RedirectStandardError = true,
//             UseShellExecute = false
//         };

//         using (Process dumpProcess = Process.Start(dumpStartInfo))
//         using (Process restoreProcess = Process.Start(restoreStartInfo))
//         {
//             List<string> lines = new List<string>();

//             dumpProcess.ErrorDataReceived += (sender, e) => lines.Add(e.Data);
//             restoreProcess.ErrorDataReceived += (sender, e) =>
//             {
//                 lines.Add(e.Data);
//                 Console.Error.WriteLine(e.Data);
//             };

//             dumpProcess.BeginErrorReadLine();
//             restoreProcess.BeginErrorReadLine();

//             dumpProcess.WaitForExit();
//             restoreProcess.WaitForExit();

//             return dumpProcess.ExitCode == 0 && restoreProcess.ExitCode == 0;
//         }
//     }

//     private string[] GenerateDumpCommand()
//     {
//         List<string> cmd = new List<string>
//             {
//                 "pg_dump", "-Fc", "--verbose", "--schema-only", "--no-owner", "--no-acl"
//             };

//         if (SpecifyTables())
//         {
//             cmd.AddRange(_tasks.Select(task => $"-t {task.QuotedTable}"));
//         }

//         cmd.AddRange(new[]
//         {
//                 "-d", _source.Url
//             });

//         return cmd.ToArray();
//     }

//     private string[] GenerateRestoreCommand()
//     {
//         List<string> cmd = new List<string>
//             {
//                 "pg_restore", "--verbose", "--no-owner", "--no-acl", "--clean"
//             };

//         if (SupportsIfExists())
//         {
//             cmd.Add("--if-exists");
//         }

//         cmd.AddRange(new[]
//         {
//                 "-d", _destination.Url
//             });

//         return cmd.ToArray();
//     }

//     private bool SupportsIfExists()
//     {
//         try
//         {
//             ProcessStartInfo startInfo = new ProcessStartInfo("pg_restore", "--help")
//             {
//                 RedirectStandardOutput = true,
//                 UseShellExecute = false
//             };

//             using (Process process = Process.Start(startInfo))
//             {
//                 string output = process.StandardOutput.ReadToEnd();
//                 return output.Contains("--if-exists");
//             }
//         }
//         catch (FileNotFoundException)
//         {
//             throw new Error("pg_restore not found");
//         }
//     }

//     private void CreateSchemas()
//     {
//         IEnumerable<string> schemasToCreate = _tasks
//             .Select(task => task.Table.Schema)
//             .Where(schema => !_destination.Schemas.Contains(schema))
//             .Distinct()
//             .OrderBy(schema => schema);

//         foreach (string schema in schemasToCreate)
//         {
//             _destination.CreateSchema(schema);
//         }
//     }

//     private bool SpecifyTables()
//     {
//         return !_opts.AllSchemas || _opts.Tables != null || _opts.Groups != null ||
//             _args.Length > 0 || _opts.Exclude != null || _opts.Schemas != null;
//     }
// }
