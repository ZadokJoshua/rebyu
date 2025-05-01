using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using Microsoft.Extensions.DependencyInjection;
using Rebyu.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using TextMateSharp.Grammars;

namespace Rebyu.Views;

public partial class MainView : UserControl
{
    private RegistryOptions _registryOptions;
    private TextMate.Installation _textMateInstallation;
    private readonly MainViewModel _viewModel;

    private const double FontSizeStep = 1.0;
    private const double MinFontSize = 8.0;
    private const double MaxFontSize = 48.0;

    private readonly List<string> _sqliteExtensions = [ ".db", ".sqlite", ".sqlite3", ".db3", ".s3db", ".sl3" ];

    public MainView()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        _registryOptions = new RegistryOptions(ThemeName.Monokai);
        _textMateInstallation = sqlEditor.InstallTextMate(_registryOptions);

        Language sqlLanguage = _registryOptions.GetLanguageByExtension(".sql");
        if (sqlLanguage != null)
        {
            string scopeName = _registryOptions.GetScopeByLanguageId(sqlLanguage.Id);
            _textMateInstallation.SetGrammar(scopeName);
        }

        LineNumberText.Text = "1";
        ColumnNumberText.Text = "1";

        sqlEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        sqlEditor.PointerWheelChanged += SqlEditor_OnPointerWheelChanged;
        AttachedToVisualTree += OnAttached;
    }

    private void SqlEditor_OnPointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        if ((e.KeyModifiers & Avalonia.Input.KeyModifiers.Control) != 0)
        {
            if (e.Delta.Y > 0)
            {
                IncreaseFontSize();
            }
            else if (e.Delta.Y < 0)
            {
                DecreaseFontSize();
            }
            e.Handled = true;
        }
    }

    private void IncreaseFontSize()
    {
        if (sqlEditor.FontSize < MaxFontSize)
        {
            sqlEditor.FontSize += FontSizeStep;
        }
    }
    private void DecreaseFontSize()
    {
        if (sqlEditor.FontSize > MinFontSize)
        {
            sqlEditor.FontSize -= FontSizeStep;
        }
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _viewModel.SqlQueries.CollectionChanged += OnSqlQueriesCollectionChanged;
    }

    private async void OnSqlQueriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var originalEditorBorderBrush = EditorBorder.BorderBrush;

            EditorBorder.BorderBrush = new SolidColorBrush(Color.Parse("#6ccb5f"));

            await Task.Delay(1000);

            EditorBorder.BorderBrush = originalEditorBorderBrush;
        }
    }

    private bool IsSqliteFile(string file)
    {
        return _sqliteExtensions.Contains(Path.GetExtension(file));
    }

    private string? GetSingleSqliteDbFile(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            if (IsSqliteFile(file))
            {
                return file;
            }
        }
        return null;
    }

    private async Task ProcessDatabaseFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        Console.WriteLine($"Processing database file: {fileName}");

        // Example: Read the .db file (you can use SQLite libraries here)
        // using var connection = new SqliteConnection($"Data Source={filePath}");
        // await connection.OpenAsync();

        // Example output
        await Task.Delay(500); // Simulate processing
        Console.WriteLine($"Finished processing {fileName}");
    }

    private void Caret_PositionChanged(object? sender, System.EventArgs e)
    {
        var caret = sqlEditor.TextArea.Caret;
        LineNumberText.Text = caret.Line.ToString();
        ColumnNumberText.Text = caret.Column.ToString();
    }

    private void AddSqlKeywords(IList<ICompletionData> completionData)
    {
        // Common SQL keywords
        var keywords = new[] {
                "SELECT", "FROM", "WHERE", "JOIN", "LEFT JOIN", "RIGHT JOIN", "INNER JOIN",
                "GROUP BY", "ORDER BY", "HAVING", "LIMIT", "OFFSET", "INSERT INTO", "UPDATE",
                "DELETE FROM", "CREATE TABLE", "ALTER TABLE", "DROP TABLE", "UNION", "DISTINCT",
                "AS", "ON", "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS NULL", "IS NOT NULL"
            };

        foreach (var keyword in keywords)
        {
           // completionData.Add(new SqlCompletionData(keyword, "Keyword", $"SQL keyword: {keyword}"));
        }
    }

    private void AddSqlFunctions(IList<ICompletionData> completionData)
    {
        // Common SQL functions
        var functions = new[] {
                "COUNT", "SUM", "AVG", "MIN", "MAX", "ROUND", "UPPER", "LOWER",
                "LENGTH", "SUBSTR", "REPLACE", "DATETIME", "DATE", "STRFTIME"
            };

        foreach (var function in functions)
        {
            // completionData.Add(new SqlCompletionData(function, "Function", $"SQL function: {function}"));
        }
    }

    //private void ExecuteSqlButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    //{
    //    var result = DatabaseHelper.ExecuteSql(_viewModel.SelectedDatabase.ConnectionString, sqlEditor.Text);
    //}
}