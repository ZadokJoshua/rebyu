using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Rebyu.Models;
using Rebyu.Debug;

namespace Rebyu.Interfaces;

public interface ISemanticKernelService
{
    Task<Tuple<List<Message>, List<DebugLog>>> GetResponse(Message userMessage, List<Message> messageHistory);

    Task<string> Summarize(string sessionId, string userPrompt);
}
