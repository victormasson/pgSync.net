using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CommandLine;
using Npgsql;
using PgSync.Model;
using Spectre.Console;

namespace PgSync;


public class Client
{
    private readonly Options _options;
    private readonly ConfigurationFile _configurationFile;
    public int VERSION { get; private set; } = 1;

    public Client(Options options, ConfigurationFile configurationFile)
    {
        Console.Out.Flush();
        _options = options;
        _configurationFile = configurationFile;
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

        Exec();
    }

    public async void Exec()
    {
        // connection


        var dataSourceBuilderFrom = new NpgsqlDataSourceBuilder(_configurationFile.DatabasesConnection.From);
        var dataSourceFrom = dataSourceBuilderFrom.Build();

        var connFrom = await dataSourceFrom.OpenConnectionAsync();

        await using (var cmd = new NpgsqlCommand(SqlHelper.GetTable(), connFrom))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                Console.WriteLine(reader.GetString(0));
        }

        var dataSourceBuilderTo = new NpgsqlDataSourceBuilder(_configurationFile.DatabasesConnection.To);
        var dataSourceTo = dataSourceBuilderTo.Build();

        var connTo = await dataSourceTo.OpenConnectionAsync();

        await using (var cmd = new NpgsqlCommand(SqlHelper.GetTable(), connTo))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                Console.WriteLine(reader.GetString(0));
        }

    }

}