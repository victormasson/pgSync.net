// See https://aka.ms/new-console-template for more information
using CommandLine;
using PgSync;
using PgSync.Model;
using Tomlyn;

Console.WriteLine("Hello, World!");


var toml = @"

[databases_connection]
from = ""postgres://postgres:password@127.0.0.1:5433/Histo-Source""
to = ""postgres://postgres:password@127.0.0.1:5433/Histo-UAT""

# group Histo full
[[groups]]
name = ""histo_group""

[[groups.tables]]
name = ""fiche""
where_condition = ""where tenant_id is null""
delete_condition_target = ""where tenant_id is null""

[[groups.tables]]
name = ""modification""
where_condition = ""where tenant_id is null""
delete_condition_target = ""where tenant_id is null""

[[groups.tables]]
name = ""valorisation""
where_condition = ""where tenant_id is null""
delete_condition_target = ""where tenant_id is null""
";

var model = Toml.ToModel<ConfigurationFile>(toml);

System.Console.WriteLine(model);
model.Groups.ForEach(g =>
{
    System.Console.WriteLine(g);
    g.Tables?.ForEach(t => System.Console.WriteLine(t));
});


Parser.Default.ParseArguments<PgSync.Model.Options>(args)
    .WithParsed<PgSync.Model.Options>(o =>
    {
        var a = new Client(o);

        a.Start();
    });

public class Error : Exception
{
    public Error(string? message) : base(message)
    {
    }
}