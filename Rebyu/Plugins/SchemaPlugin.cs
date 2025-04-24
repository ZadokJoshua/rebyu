using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Rebyu.ViewModels;
using System;
using System.ComponentModel;
using System.Text;

namespace Rebyu.Plugins;

public sealed class SchemaPlugin
{

    [KernelFunction]
    [Description("Retrieves a JSON-formatted representation of the SQLite database schema, including each table's name, columns (name, data type, primary key and foreign key flags)")]
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
}
