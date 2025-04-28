using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rebyu.Logs;

public sealed class AutoFunctionInvocationLoggingFilter(ILogger<AutoFunctionInvocationLoggingFilter> logger) : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {

        var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last()).ToList();

        if (logger.IsEnabled(LogLevel.Trace))
        {
            functionCalls.ForEach(functionCall
                => logger.LogTrace(
                    "Function call requests: {PluginName}-{FunctionName}({Arguments})",
                    functionCall.PluginName,
                    functionCall.FunctionName,
                    JsonSerializer.Serialize(functionCall.Arguments)));
        }

        await next(context);
    }
}
