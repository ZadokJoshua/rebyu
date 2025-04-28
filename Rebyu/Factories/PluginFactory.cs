using Microsoft.SemanticKernel;
using Rebyu.Models;
using Rebyu.Plugins;
using System;

namespace Rebyu.Factories;

public static class PluginFactory
{
    public static Kernel GetAgentKernel(Kernel kernel, AgentType agentType)
    {
        Kernel agentKernel = kernel.Clone();
        switch (agentType)
        {
            case AgentType.NL2SQL:
                var nL2SQLPlugin = new NL2SQLPlugin();
                agentKernel.Plugins.AddFromObject(nL2SQLPlugin);
                break;
            case AgentType.ChartMaker:
                var chartMakerPlugin = new ChartMakerPlugin();
                agentKernel.Plugins.AddFromObject(chartMakerPlugin);
                break;
            case AgentType.Coordinator:
                var CoordinatorPlugin = new CoordinatorPlugin();
                agentKernel.Plugins.AddFromObject(CoordinatorPlugin);
                break;
            default:
                throw new ArgumentException("Invalid plugin name");
        }

        return agentKernel;
    }
}
