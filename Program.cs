// See https://aka.ms/new-console-template for more information
using CommandLine;
using PgSync;
using PgSync.Model;
using Tomlyn;

Console.WriteLine("Hello, World!");


var toml = @"

[databases_connection]
from = ""Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase""
to = ""Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase""

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
        var a = new Client(o, model);

        a.Start();
    });

public class Error : Exception
{
    public Error(string? message) : base(message)
    {
    }
}