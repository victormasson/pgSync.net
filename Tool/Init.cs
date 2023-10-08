// using System;
// using System.Diagnostics;
// using System.IO;
// using CommandLine;

// namespace PgSync;

// public class Init
// {
//     private readonly string[] _arguments;
//     private readonly Options _options;

//     public Init(string[] arguments, Options options)
//     {
//         _arguments = arguments;
//         _options = options;
//     }

//     public void Perform()
//     {
//         if (_arguments.Length > 1)
//         {
//             throw new Error("Usage:\n    pgsync --init [db]");
//         }

//         string file;

//         if (!string.IsNullOrEmpty(_options.Config))
//         {
//             file = _options.Config;
//         }
//         else if (_arguments.Length > 0)
//         {
//             file = DbConfigFile(_arguments[0]);
//         }
//         else if (!string.IsNullOrEmpty(_options.Db))
//         {
//             file = DbConfigFile(_options.Db);
//         }
//         else
//         {
//             file = ".pgsync.yml";
//         }

//         if (File.Exists(file))
//         {
//             throw new Error($"{file} exists.");
//         }
//         else
//         {
//             string exclude = GetDefaultExclude();

//             // create file
//             string contents = File.ReadAllText(Path.Combine(__dir__, "../../config.yml"));
//             if (Heroku())
//             {
//                 contents = contents.Replace("$(some_command)", "$(heroku config:get DATABASE_URL)");
//             }
//             File.WriteAllText(file, string.Format(contents, new { exclude }));

//             Console.WriteLine($"{file} created. Add your database credentials.");
//         }
//     }

//     private bool FileExists(string path, string contents = null, RegexOptions options = RegexOptions.None)
//     {
//         try
//         {
//             if (contents != null)
//             {
//                 return File.ReadAllText(path).Contains(contents);
//             }
//             else
//             {
//                 return File.Exists(path);
//             }
//         }
//         catch
//         {
//             return false;
//         }
//     }

//     private string GetDefaultExclude()
//     {
//         var str = new[]{
//             "ar_internal_metadata", "schema_migrations"
//         }
//             return;
//     }
//         else if (Django())
//         {
//             return "exclude:\n  - django_migrations\n";
//         }
//         else if (Laravel())
//         {
//             return "exclude:\n  - migrations\n";
//         }
//         else
//         {
//             return "# exclude:\n#   - table1\n#   - table2\n";
//         }
//     }

// private string DbConfigFile(string db)
// {
//     return $"config.{db}.yml";
// }

// private string ExecuteShellCommand(string command)
// {
//     var processStartInfo = new ProcessStartInfo("bash", "-c")
//     {
//         RedirectStandardOutput = true,
//         RedirectStandardError = true,
//         UseShellExecute = false,
//         CreateNoWindow = true,
//         Arguments = command
//     };

//     using (Process process = Process.Start(processStartInfo))
//     using (StreamReader reader = process.StandardOutput)
//     {
//         return reader.ReadToEnd();
//     }
// }
// }
