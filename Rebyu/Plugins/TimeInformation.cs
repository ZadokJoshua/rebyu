using Microsoft.SemanticKernel;
using System.ComponentModel;
using System;

namespace Rebyu.Plugins;


public sealed class TimeInformation
{
    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}
