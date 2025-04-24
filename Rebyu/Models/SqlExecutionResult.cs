using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rebyu.Models;

public class SqlExecutionResult
{
    public string CommandText { get; set; }
    public List<string> Columns { get; set; } = [];
    public ObservableCollection<Dictionary<string, object>> Rows { get; set; } = [];
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
}
