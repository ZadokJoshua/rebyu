using System;
using System.Collections.Generic;

namespace Rebyu.Debug;

public record DebugLog
{

    public string Id { get; set; }
    public string MessageId { get; set; }

    public string Type { get; set; }

    public DateTime TimeStamp { get; set; }

    public List<LogProperty> PropertyBag { get; set; }

    public DebugLog(string messageId, string id)
    {
        MessageId = messageId;
        Id = id;
        Type = nameof(DebugLog);
        TimeStamp = DateTime.UtcNow;
        PropertyBag = [];
    }
}
