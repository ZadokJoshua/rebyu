using AvaloniaEdit.Document;
using System;

namespace Rebyu.Models;

public class SessionSqlQuery
{
    public string SqlQuery { get; }
    public int Id { get; }
    public TextDocument EditorContent { get; }
    public DateTime CreatedAt { get; }

    public SessionSqlQuery(string sqlQuery, int id)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            throw new ArgumentException("SQL query cannot be null or empty.", nameof(sqlQuery));
        }

        SqlQuery = sqlQuery;
        Id = id;
        EditorContent = new TextDocument(sqlQuery);
        CreatedAt = DateTime.Now;
    }
}
