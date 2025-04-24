using System.Collections.ObjectModel;

namespace Rebyu.Models;

public class Table
{
    public string Name { get; init; }
    public ObservableCollection<Column> Columns { get; set; } = [];
    public int RowCount { get; set; }
}