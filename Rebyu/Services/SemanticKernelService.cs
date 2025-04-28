using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Rebyu.Debug;
using Rebyu.Factories;
using Rebyu.Helper;
using Rebyu.Interfaces;
using Rebyu.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rebyu.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SemanticKernelService> _logger;
    private readonly Kernel _semanticKernel;

    private bool _serviceInitialized;

    private List<LogProperty> _promptDebugProperties;

    public SemanticKernelService(ILoggerFactory loggerFactory, string openAiApiKey)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<SemanticKernelService>();
        _promptDebugProperties = [];

        _logger.LogInformation("Initializing the Semantic Kernel service...");

        var builder = Kernel.CreateBuilder();

        builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

        builder.AddOpenAIChatCompletion(
            "gpt-4.1",
            openAiApiKey);

        _semanticKernel = builder.Build();

        Task.Run(Initialize).ConfigureAwait(false);
    }

    private Task Initialize()
    {
        try
        {
            _serviceInitialized = true;
            _logger.LogInformation("Semantic Kernel service initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Semantic Kernel service was not initialized. The following error occurred: {ErrorMessage}.", ex.ToString());
        }
        return Task.CompletedTask;
    }

    public async Task<Tuple<List<Message>, List<DebugLog>>> GetResponse(Message userMessage, List<Message> messageHistory)
    {
        try
        {
            ChatFactory multiAgentChatGeneratorService = new();

            var agentGroupChat = multiAgentChatGeneratorService.BuildAgentGroupChat(_semanticKernel, _loggerFactory);

            foreach (var chatMessage in messageHistory)
            {
                AuthorRole? role = AuthorRoleHelper.FromString(chatMessage.SenderRole);
                var chatMessageContent = new ChatMessageContent
                {
                    Role = role ?? AuthorRole.User,
                    Content = chatMessage.Text
                };
                agentGroupChat.AddChatMessage(chatMessageContent);
            }

            _promptDebugProperties = [];

            List<Message> completionMessages = [];
            List<DebugLog> completionMessagesLogs = [];
            do
            {
                var userResponse = new ChatMessageContent(AuthorRole.User, userMessage.Text);
                agentGroupChat.AddChatMessage(userResponse);

                agentGroupChat.IsComplete = false;

                await foreach (ChatMessageContent response in agentGroupChat.InvokeAsync())
                {
                    string messageId = Guid.NewGuid().ToString();
                    string debugLogId = Guid.NewGuid().ToString();
                    completionMessages.Add(new Message(response.AuthorName ?? string.Empty, response.Role.ToString(), response.Content ?? string.Empty, messageId, debugLogId));

                    if (_promptDebugProperties.Count > 0)
                    {
                        var completionMessagesLog = new DebugLog(messageId, debugLogId)
                        {
                            PropertyBag = _promptDebugProperties
                        };
                        completionMessagesLogs.Add(completionMessagesLog);
                    }

                }
            }
            while (!agentGroupChat.IsComplete);

            return new Tuple<List<Message>, List<DebugLog>>(completionMessages, completionMessagesLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when getting response: {ErrorMessage}", ex.ToString());
            return new Tuple<List<Message>, List<DebugLog>>([], []);
        }
    }

    public async Task<string> Summarize(string sessionId, string userPrompt) 
    {
        try
        {
            var summarizeFunction = _semanticKernel.CreateFunctionFromPrompt(
                "Summarize the following text into exactly two words:\n\n{{$input}}",
                executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 10 }
            );

            // Invoke the function
            var summary = await _semanticKernel.InvokeAsync(summarizeFunction, new() { ["input"] = userPrompt });

            return summary.GetValue<string>() ?? "No summary generated";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when getting response: {ErrorMessage}", ex.ToString());
            return string.Empty;
        }
    }
}
