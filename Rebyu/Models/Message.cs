using System;
using static System.Net.Mime.MediaTypeNames;

namespace Rebyu.Models;

public class Message
{
    public string Id { get; set; }
    public string? Text { get; set; }
    public string Type { get; set; }
    public bool IsUser { get; set; }
    public string? Sender { get; set; }
    public string? SenderRole { get; set; }
    public string? DebugLogId { get; set; }
    public DateTime TimeStamp { get; set; }

    public Message(string author, string authorRole, string text, string? id = null, string? debugLogId = null)
    {
        Id = id ?? Guid.NewGuid().ToString();
        if (debugLogId != null)
            DebugLogId = debugLogId;
        Type = nameof(Message);
        Sender = author;
        SenderRole = authorRole;
        Text = text;
        TimeStamp = DateTime.UtcNow;
    }
}