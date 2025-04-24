using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Rebyu.Models;

public class Database
{
    public string Name { get; init; }
    public string Directory => new DirectoryInfo(Path.GetDirectoryName(FilePath)).Name;
    public string FilePath { get; init; }
    public string ConnectionString => $"Data Source={FilePath};Version=3;FailIfMissing=True";
    public ObservableCollection<Table> Tables { get; set; } = [];
    public int TableCount => Tables.Count;

    public string Header => "Tables";
}
