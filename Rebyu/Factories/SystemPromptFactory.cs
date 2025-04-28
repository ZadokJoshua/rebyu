using Rebyu.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rebyu.StructuredFormats.ChatResponseFormatBuilder;

namespace Rebyu.Factories;

public static class SystemPromptFactory
{
    public static string GetAgentName(AgentType agentType)
    {
        string name = agentType switch
        {
            AgentType.NL2SQL => "NL2SQL",
            AgentType.ChartMaker => "Transactions",
            AgentType.Coordinator => "Coordinator",
            _ => throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null),
        };
        return name;
    }

    public static string GetAgentPrompts(AgentType agentType)
    {
        string promptFile = agentType switch
        {
            AgentType.NL2SQL => "NL2SQL.prompty",
            AgentType.ChartMaker => "ChartMaker.prompty",
            AgentType.Coordinator => "Coordinator.prompty",
            _ => throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null),
        };
        string prompt = $"{File.ReadAllText("Prompts/" + promptFile)}{File.ReadAllText("Prompts/CommonAgentRules.prompty")}";

        return prompt;
    }

    public static string GetStrategyPrompts(ChatResponseStrategy strategyType)
    {
        string prompt = string.Empty;
        switch (strategyType)
        {
            case ChatResponseStrategy.Continuation:
                prompt = File.ReadAllText("Prompts/SelectionStrategy.prompty");
                break;
            case ChatResponseStrategy.Termination:
                prompt = File.ReadAllText("Prompts/TerminationStrategy.prompty");
                break;

        }
        return prompt;
    }
}
