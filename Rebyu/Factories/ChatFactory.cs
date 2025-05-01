using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using Rebyu.Models;
using Rebyu.StructuredFormats;
using System;
using System.Text.Json;

namespace Rebyu.Factories;

internal class ChatFactory
{
    private ChatCompletionAgent BuildAgent(Kernel kernel, AgentType agentType)
    {
        ChatCompletionAgent agent = new()
        {
            Name = SystemPromptFactory.GetAgentName(agentType),
            Instructions = $"""{SystemPromptFactory.GetAgentPrompts(agentType)}""",
            Kernel = PluginFactory.GetAgentKernel(kernel, agentType),
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };

        return agent;
    }

    private OpenAIPromptExecutionSettings GetExecutionSettings(ChatResponseFormatBuilder.ChatResponseStrategy strategyType)
    {
        ChatResponseFormat infoFormat;
        infoFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        jsonSchemaFormatName: $"agent_result_{strategyType.ToString()}",
        jsonSchema: BinaryData.FromString($"""
                {ChatResponseFormatBuilder.BuildFormat(strategyType)}
                """));
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = infoFormat
        };

        return executionSettings;
    }

    private KernelFunction GetStrategyFunction(ChatResponseFormatBuilder.ChatResponseStrategy strategyType)
    {
        KernelFunction function =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                    {{{SystemPromptFactory.GetStrategyPrompts(strategyType)}}}
                    
                    RESPONSE:
                    {{$lastmessage}}
                    """,
                safeParameterNames: "lastmessage");

        return function;
    }

    public AgentGroupChat BuildAgentGroupChat(Kernel kernel, ILoggerFactory loggerFactory)
    {
        AgentGroupChat agentGroupChat = new();
        var chatModel = kernel.GetRequiredService<IChatCompletionService>();

        foreach (AgentType agentType in Enum.GetValues<AgentType>())
        {
            agentGroupChat.AddAgent(BuildAgent(kernel, agentType));
        }

        agentGroupChat.ExecutionSettings = GetAgentGroupChatSettings(kernel);


        return agentGroupChat;
    }


    private AgentGroupChatSettings GetAgentGroupChatSettings(Kernel kernel)
    {
        ChatHistoryTruncationReducer historyReducer = new(5);

        AgentGroupChatSettings ExecutionSettings = new()
        {
            SelectionStrategy =
                new KernelFunctionSelectionStrategy(GetStrategyFunction(ChatResponseFormatBuilder.ChatResponseStrategy.Continuation), kernel)
                {
                    Arguments = new KernelArguments(GetExecutionSettings(ChatResponseFormatBuilder.ChatResponseStrategy.Continuation)),
                    HistoryReducer = historyReducer,
                    HistoryVariableName = "lastmessage",
                    ResultParser = (result) =>
                    {
                        var resultString = result.GetValue<string>();
                        if (!string.IsNullOrEmpty(resultString))
                        {
                            var ContinuationInfo = JsonSerializer.Deserialize<ContinuationInfo>(resultString);
                            return ContinuationInfo.AgentName;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                },
            TerminationStrategy =
                new KernelFunctionTerminationStrategy(GetStrategyFunction(ChatResponseFormatBuilder.ChatResponseStrategy.Termination), kernel)
                {
                    Arguments = new KernelArguments(GetExecutionSettings(ChatResponseFormatBuilder.ChatResponseStrategy.Termination)),
                    HistoryReducer = historyReducer,
                    HistoryVariableName = "lastmessage",
                    MaximumIterations = 8,
                    ResultParser = (result) =>
                    {
                        var resultString = result.GetValue<string>();
                        if (!string.IsNullOrEmpty(resultString))
                        {
                            var terminationInfo = JsonSerializer.Deserialize<TerminationInfo>(resultString);
                            return !terminationInfo.ShouldContinue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                },
        };

        return ExecutionSettings;
    }
}