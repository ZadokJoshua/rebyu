using System;
using System.Collections.Generic;

namespace Rebyu.Models;

public class Session
{
    public string Id { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public List<Message> Messages { get; set; }

    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        Name = "New Chat";
        Messages = [];
    }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }
}
