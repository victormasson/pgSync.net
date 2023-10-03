using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CommandLine;
using PgSync.Model;
using Spectre.Console;

namespace PgSync;


public class Client
{
    private readonly Options _options;
    public int VERSION { get; private set; } = 1;

    public Client(Options options)
    {
        Console.Out.Flush();
        _options = options;
    }

    public void Start()
    {
        if (_options.Version)
        {
            AnsiConsole.Console.WriteLine($"{VERSION}", StyleApp.BasicStype);
            return;
        }

        if (_options.Debug)
        {
            AnsiConsole.Console.WriteLine($"Debug output enabled. Current Arguments: --debug {_options.Debug}", StyleApp.BasicStype);
        }
        else
        {
            AnsiConsole.Console.WriteLine("Quick Start!", StyleApp.BasicStype);
        }

        if (_options.GroupName != null)
        {
            AnsiConsole.Console.WriteLine($"Group: {_options.GroupName}", StyleApp.BasicStype);
        }

        if (_options.PathSchema != null)
        {
            AnsiConsole.Console.WriteLine($"PathSchema: {_options.PathSchema}", StyleApp.BasicStype);
        }
    }



    // public void Perform()
    // {
    //     var result = ParseSlopOptions(_args);
    //     var arguments = result.Item1;
    //     var options = result.Item2;

    //     options["defer_constraints_v2"] = options.GetValueOrDefault("defer_constraints");

    //     if (options.ContainsKey("db") && options.ContainsKey("config"))
    //     {

    //         throw new Error("Specify either --db or --config, not both");
    //     }
    //     if (options.ContainsKey("overwrite") && options.ContainsKey("in_batches"))
    //     {
    //         throw new Error("Cannot use --overwrite with --in-batches");
    //     }

    //     if (options.ContainsKey("version") && options["version"] == "true")
    //     {
    //         Console.WriteLine(VERSION);
    //     }
    //     else if (options.ContainsKey("help") && options["help"] == "true")
    //     {
    //         PrintSlopOptions(slopOptions());
    //     }
    //     else if (options.ContainsKey("init") && options["init"] == "true")
    //     {
    //         new Init(arguments.ToArray(), options).Perform();
    //     }
    //     else
    //     {
    //         new Sync(arguments.ToArray(), options).Perform();
    //     }
    // }

    // private Tuple<string[], Dictionary<string, string>> ParseSlopOptions(string[] args)
    // {
    //     var slopOptions = SlopOptions();
    //     var parser = new Slop.Parser(slopOptions);
    //     var result = parser.Parse(args);
    //     var arguments = result.Arguments;
    //     var options = result.ToDictionary();

    //     options["defer_constraints_v2"] = options.GetValueOrDefault("defer_constraints");

    //     return new Tuple<string[], Dictionary<string, string>>(arguments, options);
    // }

    // private void PrintSlopOptions(Slop.Options slopOptions)
    // {
    //     Console.WriteLine($"Usage:\n    pgsync [tables,groups] [sql] [options]\n");
    //     Console.WriteLine("Table options:");
    //     Console.WriteLine("  -t, --tables\ttables to sync");
    //     Console.WriteLine("  -g, --groups\tgroups to sync");
    //     Console.WriteLine("  --exclude\ttables to exclude");
    //     Console.WriteLine("  --schemas\tschemas to sync");
    //     Console.WriteLine("  --all-schemas\tsync all schemas\n");
    //     Console.WriteLine("Row options:");
    //     Console.WriteLine("  --overwrite\toverwrite existing rows");
    //     Console.WriteLine("  --preserve\tpreserve existing rows");
    //     Console.WriteLine("  --truncate\ttruncate existing rows\n");
    //     Console.WriteLine("Foreign key options:");
    //     Console.WriteLine("  --defer-constraints\tdefer constraints");
    //     Console.WriteLine("  --disable-integrity\tdisable foreign key triggers");
    //     Console.WriteLine("  -j, --jobs\tnumber of tables to sync at a time");
    //     Console.WriteLine("  --defer-constraints-v1\tdefer constraints");
    //     Console.WriteLine("  --defer-constraints-v2\tdefer constraints");
    //     Console.WriteLine("  --disable-integrity-v2\tdisable foreign key triggers\n");
    //     Console.WriteLine("Schema options:");
    //     Console.WriteLine("  --schema-first\tsync schema first");
    //     Console.WriteLine("  --schema-only\tsync schema only\n");
    //     Console.WriteLine("Config options:");
    //     Console.WriteLine("  --config\tconfig file (defaults to .pgsync.yml)");
    //     Console.WriteLine("  -d, --db\tdatabase-specific config file\n");
    //     Console.WriteLine("Connection options:");
    //     Console.WriteLine("  --from\tsource database URL");
    //     Console.WriteLine("  --to\tdestination database URL");
    //     Console.WriteLine("  --to-safe\tconfirms destination is safe (when not localhost)\n");
    //     Console.WriteLine("Other options:");
    //     Console.WriteLine("  --debug\tshow SQL statements");
    //     Console.WriteLine("  --disable-user-triggers\tdisable non-system triggers");
    //     Console.WriteLine("  --fail-fast\tstop on the first failed table");
    //     Console.WriteLine("  --no-rules\tdon't apply data rules");
    //     Console.WriteLine("  --no-sequences\tdon't sync sequences");
    //     Console.WriteLine("  --in-batches\tsync in batches");
    //     Console.WriteLine("  --batch-size\tbatch size");
    //     Console.WriteLine("  --sleep\ttime to sleep between batches\n");
    //     Console.WriteLine("Other commands:");
    //     Console.WriteLine("  --init\tcreate config file");
    //     Console.WriteLine("  --list\tlist tables");
    //     Console.WriteLine("  -h, --help\tprint help");
    //     Console.WriteLine("  -v, --version\tprint version");
    // }

    // private Slop.Options SlopOptions()
    // {
    //     var o = new Slop.Options();
    //     o.Banner = "Usage:\n    pgsync [tables,groups] [sql] [options]";

    //     o.String("-t", "--tables", "tables to sync");
    //     o.String("-g", "--groups", "groups to sync");
    //     o.String("--exclude", "tables to exclude");
    //     o.String("--schemas", "schemas to sync");
    //     o.Boolean("--all-schemas", "sync all schemas", false);

    //     o.Boolean("--overwrite", "overwrite existing rows", false);
    //     o.Boolean("--preserve", "preserve existing rows", false);
    //     o.Boolean("--truncate", "truncate existing rows", false);

    //     o.Boolean("--defer-constraints", "defer constraints", false);
    //     o.Boolean("--disable-integrity", "disable foreign key triggers", false);
    //     o.Integer("-j", "--jobs", "number of tables to sync at a time");
    //     o.Boolean("--defer-constraints-v1", "defer constraints", false);
    //     o.Boolean("--defer-constraints-v2", "defer constraints", false);
    //     o.Boolean("--disable-integrity-v2", "disable foreign key triggers", false);

    //     o.Boolean("--schema-first", "sync schema first", false);
    //     o.Boolean("--schema-only", "sync schema only", false);

    //     o.String("--config", "config file (defaults to .pgsync.yml)");
    //     o.String("-d", "--db", "database-specific config file");

    //     o.String("--from", "source database URL");
    //     o.String("--to", "destination database URL");
    //     o.Boolean("--to-safe", "confirms destination is safe (when not localhost)", false);

    //     o.Boolean("--debug", "show SQL statements", false);
    //     o.Boolean("--disable-user-triggers", "disable non-system triggers", false);
    //     o.Boolean("--fail-fast", "stop on the first failed table", false);
    //     o.Boolean("--no-rules", "don't apply data rules", false);
    //     o.Boolean("--no-sequences", "don't sync sequences", false);

    //     o.Boolean("--in-batches", "sync in batches", false);
    //     o.Integer("--batch-size", "batch size", 10000);
    //     o.Float("--sleep", "time to sleep between batches", 0);

    //     o.Boolean("--init", "create config file", false);
    //     o.Boolean("--list", "list tables", false);
    //     o.Boolean("-h", "--help", "print help", false);
    //     o.Boolean("-v", "--version", "print version", false);

    //     return o;
    // }
}