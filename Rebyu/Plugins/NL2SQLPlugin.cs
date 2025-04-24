using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Rebyu.Plugins;

public class NL2SQLPlugin
{
    [KernelFunction]
    [Description("Converts a natural language query into an SQL query.")]
    public string ConvertNLToSQL(string naturalLanguageQuery)
    {
        return $"SELECT * FROM table WHERE condition = '{naturalLanguageQuery}'";
    }
}