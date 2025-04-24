using System.Data.SQLite;
using System;
using Rebyu.Models;
using System.Linq;
using System.Collections.Generic;

namespace Rebyu.Services;

public class SqliteDataService
{
    public Database GetDatabaseDetails(string databasePath)
    {
        var database = new Database
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(databasePath),
            FilePath = databasePath
        };

        try
        {
            using var connection = new SQLiteConnection(database.ConnectionString);
            connection.Open();

            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

            using var tableReader = tableCommand.ExecuteReader();
            while (tableReader.Read())
            {
                var tableName = tableReader.GetString(0);
                var table = new Table { Name = tableName };

                using var columnCommand = connection.CreateCommand();
                columnCommand.CommandText = $"PRAGMA table_info({tableName});";

                using var columnReader = columnCommand.ExecuteReader();
                while (columnReader.Read())
                {
                    var column = new Column
                    {
                        Name = columnReader["name"].ToString(),
                        DataType = columnReader["type"].ToString(),
                        IsPrimaryKey = columnReader["pk"].ToString() == "1",
                        IsNullable = columnReader["notnull"].ToString() == "0",
                        DefaultValue = columnReader["dflt_value"]?.ToString()
                    };

                    table.Columns.Add(column);
                }

                using var foreignKeyCommand = connection.CreateCommand();
                foreignKeyCommand.CommandText = $"PRAGMA foreign_key_list({tableName});";

                using var foreignKeyReader = foreignKeyCommand.ExecuteReader();
                while (foreignKeyReader.Read())
                {
                    var fkColumnName = foreignKeyReader["from"].ToString();
                    var fkReferencedTable = foreignKeyReader["table"].ToString();
                    var fkReferencedColumn = foreignKeyReader["to"].ToString();

                    var column = table.Columns.FirstOrDefault(c => c.Name == fkColumnName);
                    if (column != null)
                    {
                        column.IsForeignKey = true;
                        column.ForeignKeyTable = fkReferencedTable;
                        column.ForeignKeyColumn = fkReferencedColumn;
                    }
                }

                using var rowCountCommand = connection.CreateCommand();
                rowCountCommand.CommandText = $"SELECT COUNT(*) FROM {tableName};";

                table.RowCount = Convert.ToInt32(rowCountCommand.ExecuteScalar());

                database.Tables.Add(table);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching database details: {ex.Message}");
        }

        return database;
    }

    public SqlExecutionResult ExecuteSql(string connectionString, string sqlText)
    {
        var result = new SqlExecutionResult { CommandText = sqlText };

        try
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sqlText;

            using var reader = command.ExecuteReader();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value;
                }

                result.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            result.IsError = true;
            result.ErrorMessage = ex.Message;
            result.Columns.Add("Error");
            result.Rows.Add(new Dictionary<string, object> { { "Error", ex.Message } });
        }

        return result;
    }
}
