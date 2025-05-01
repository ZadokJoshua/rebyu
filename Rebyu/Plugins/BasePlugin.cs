using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Rebyu.ViewModels;
using System;
using System.ComponentModel;
using System.Text;

namespace Rebyu.Plugins;

public class BasePlugin
{

    [KernelFunction]
    [Description("Retrieves the selected SQLite database schema")]
    public string GetDatabaseSchema()
    {
        var mainViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        if (mainViewModel.SelectedDatabase == null)
        {
            throw new InvalidOperationException("No database selected.");
        }

        var database = mainViewModel.SelectedDatabase;
        var schemaContext = new StringBuilder();

        schemaContext.AppendLine($"### {database.Name} DATABASE SCHEMA ###");

        foreach (var table in database.Tables)
        {
            schemaContext.AppendLine($"Table: {table.Name} (row count: {table.RowCount})");
            foreach (var col in table.Columns)
            {
                var info = $"{col.Name}: {col.DataType}";
                if (col.IsPrimaryKey) info += " (PK)";
                if (col.IsForeignKey) info += $" (FK to {col.ForeignKeyTable}.{col.ForeignKeyColumn})";
                schemaContext.AppendLine($"  - {info}");
            }
            schemaContext.AppendLine();
        }

        return schemaContext.ToString();
    }

    [KernelFunction]
    [Description("Retrieves the current time")]
    public string GetCurrentTime() => DateTime.Now.ToString("R");
}
