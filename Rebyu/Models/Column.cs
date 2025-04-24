namespace Rebyu.Models;

public class Column
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string ForeignKeyTable { get; set; }
    public string ForeignKeyColumn { get; set; }
    public bool IsNullable { get; set; }
    public string DefaultValue { get; set; }
}