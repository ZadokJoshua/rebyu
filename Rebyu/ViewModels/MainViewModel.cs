using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Rebyu.Models;
using Rebyu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static Rebyu.Helper.StorageProviderHelper;

namespace Rebyu.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private IStorageFile? _selectedFile;

    [ObservableProperty]
    private Database? _selectedDatabase;

    [ObservableProperty]
    private TextDocument _editorContent = new("-- Enter your SQL query here\nSELECT * FROM ");

    [ObservableProperty]
    private FlatTreeDataGridSource<Dictionary<string, object>> _dataGridSource;

    [ObservableProperty]
    private string _userPrompt;

    [ObservableProperty]
    private Message? _lastMessage;

    public ObservableCollection<Database> TreeItems { get; set; } = [];
    public ObservableCollection<Message> Chat { get; set; } = [];
    private ChatHistory _chatHistory { get; set; } = [];

    private readonly SqliteDataService _sqliteDataService;
    private readonly Kernel _kernel;

    public MainViewModel()
    {
        _sqliteDataService = App.ServiceProvider.GetRequiredService<SqliteDataService>();
        _kernel = App.ServiceProvider.GetRequiredService<Kernel>();
    }

    private void AddMessageToChat(Message message)
    {
        Chat.Add(message);

        Dispatcher.UIThread.Post(() =>
        {
            LastMessage = message;
        }, DispatcherPriority.Background);
    }


    [RelayCommand]
    private async Task SelectDbFile()
    {
        var dbFile = await OpenFilePickerAsync();
        if (dbFile == null) return;

        SelectedFile = (IStorageFile)dbFile;
        var filePath = SelectedFile.Path.LocalPath;

        SelectedDatabase = _sqliteDataService.GetDatabaseDetails(filePath);
        IsConnected = true;
        TreeItems.Add(SelectedDatabase);
    }

    [RelayCommand]
    private async Task ExecuteCommand()
    {
        // TODO: Check if the sql statement contains a non-idempotent operation
        var queryResult = _sqliteDataService.ExecuteSql(SelectedDatabase.ConnectionString, EditorContent.Text);

        var newSource = new FlatTreeDataGridSource<Dictionary<string, object>>(queryResult.Rows);

        foreach (var columnName in queryResult.Columns)
        {
            newSource.Columns.Add(new TextColumn<Dictionary<string, object>, string>(
                columnName,
                row => row.ContainsKey(columnName)
                    ? (row[columnName] != null ? row[columnName].ToString() : string.Empty)
                    : string.Empty
            ));
        }

        DataGridSource = newSource;
    }

    [RelayCommand]
    private async Task ProcessUserPrompt()
    {
        if (string.IsNullOrEmpty(UserPrompt)) return;

        List<Message> existingChatMessages;

        #region TESTING AI

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        KernelArguments kernelArguments = new(openAIPromptExecutionSettings);

        var userMessage = new Message
        {
            Text = UserPrompt,
            IsUser = true
        };
        AddMessageToChat(userMessage);

        var aiResponse = await _kernel.InvokePromptAsync(UserPrompt, kernelArguments);

        var aiMessage = new Message
        {
            Text = aiResponse.ToString(),
            IsUser = false
        };
        AddMessageToChat(aiMessage);

        UserPrompt = string.Empty;

        #endregion
    }

    [RelayCommand]
    private void RefreshItems()
    {
        TreeItems.Clear();
        IsConnected = false;
        SelectedDatabase = null;
        SelectedFile = null;

        // Populate the Items again
    }

    [RelayCommand]
    private void FetchRecommendatedPromptsInit()
    {
        throw new NotImplementedException();
    }

    private void LoadChat()
    {
        throw new NotImplementedException();
    }
}   

public class SqlPlugin()
{

}
