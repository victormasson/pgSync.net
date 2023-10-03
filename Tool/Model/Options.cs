using CommandLine;

namespace PgSync.Model;

public class Options
{
    [Option('v', "version", Required = false, HelpText = "print version.")]
    public bool Version { get; set; }

    [Option("debug", Required = false, HelpText = "Debug mode.")]
    public bool Debug { get; set; }

    [Option("init", Required = false, HelpText = "Write configuration file.")]
    public bool Init { get; set; }

    [Option('s', "schema", Default = ".pgsync.toml", Required = false, HelpText = "Schema location.")]
    public string PathSchema { get; set; } = string.Empty;

    [Value(0, Required = false, HelpText = "Group name")]
    public string? GroupName { get; set; }

    [Option('j', "jobs", Required = false, HelpText = "number of tables to sync at a time")]
    public int? Jobs { get; set; } = null;

    public static string HelpText() => @"
        Usage:\n    pgsync [group name] [options]\n
        -g, --groups\tgroups to sync
    ";
}

