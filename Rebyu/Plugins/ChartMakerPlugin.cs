using Microsoft.SemanticKernel;
using System.ComponentModel;
using static ExCSS.AttributeSelectorFactory;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Rebyu.Plugins;

public class ChartMakerPlugin
{
    [KernelFunction]
    [Description("Creates a chart from the current result view if data is available.")]
    public void CreateChartFromCurrentResultView()
    {

    }
}
