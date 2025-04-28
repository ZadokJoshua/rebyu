using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Rebyu.Interfaces;
using Rebyu.Models;
using Rebyu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

    //[ObservableProperty]
    //private TextDocument _editorContent = new("");

    [ObservableProperty]
    private FlatTreeDataGridSource<Dictionary<string, object>> _dataGridSource;

    [ObservableProperty]
    private string _userPrompt;

    [ObservableProperty]
    private Message? _lastMessage;

    [ObservableProperty]
    private SessionSqlQuery _currentSessionQuery;

    public ObservableCollection<Database> TreeItems { get; set; } = [];
    public ObservableCollection<Message> Chat { get; set; } = [];
    public ObservableCollection<SessionSqlQuery> SqlQueries { get; set; } = [];

    private ChatHistory _chatHistory { get; set; } = [];

    private readonly SqliteDataService _sqliteDataService;
    private readonly ISemanticKernelService _skService;

    public MainViewModel()
    {
        CurrentSessionQuery = new("-- Enter your SQL query here\nSELECT * FROM ", 1);

        _sqliteDataService = App.ServiceProvider.GetRequiredService<SqliteDataService>();
        _skService = App.ServiceProvider.GetRequiredService<ISemanticKernelService>();

        SqlQueries.CollectionChanged += OnSqlQueriesCollectionChanged;
    }

    private Task InitializeConversation()
    {
        _skService.GetResponse()
    }

    private void OnSqlQueriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    var newItem = e.NewItems[0] as SessionSqlQuery;
                    if (newItem != null)
                    {
                        CurrentSessionQuery = newItem;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    var removedItem = e.OldItems[0] as SessionSqlQuery;
                    if (removedItem != null)
                    {
                        HandleRemovedQuery(removedItem);
                    }
                }
                break;
        }
    }

    private void HandleRemovedQuery(SessionSqlQuery removedItem)
    {
        var removedIndex = SqlQueries.IndexOf(removedItem);

        if (removedIndex == -1)
        {
            if (SqlQueries.Count > 0)
            {
                CurrentSessionQuery = SqlQueries[0];
            }
            else
            {
                CurrentSessionQuery = new SessionSqlQuery("-- Enter your SQL query here\nSELECT * FROM ", AssignSqlQueryId());
            }
            return;
        }

        if (removedIndex > 0)
        {
            CurrentSessionQuery = SqlQueries[removedIndex - 1];
        }
        else if (SqlQueries.Count > 0)
        {
            CurrentSessionQuery = SqlQueries[0];
        }
        else
        {
            CurrentSessionQuery = new SessionSqlQuery("-- Enter your SQL query here\nSELECT * FROM ", AssignSqlQueryId());
        }
    }

    private void AddMessageToChat(Message message)
    {
        Chat.Add(message);

        Dispatcher.UIThread.Post(() =>
        {
            LastMessage = message;
        }, DispatcherPriority.Background);
    }

    public void AddNewSqlQuery(string sqlQuery)
    {
        var newSqlQuery = new SessionSqlQuery(sqlQuery, AssignSqlQueryId());
        SqlQueries.Add(newSqlQuery);
    }

    private int AssignSqlQueryId()
    {
        if (SqlQueries.Count == 0) return 1;
        var lastSqlQuery = SqlQueries[^1];
        return lastSqlQuery.Id + 1;
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
        var queryResult = _sqliteDataService.ExecuteSql(SelectedDatabase.ConnectionString, CurrentSessionQuery.EditorContent.Text);

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

        UserPrompt = string.Empty;

        var userMessage = new Message("User", "User", text: UserPrompt)
        { 
            IsUser = true,  
        };

        try
        {
            var response = await _skService.GetResponse(userMessage, Chat.ToList());
        }
        catch (Exception)
        {

            throw;
        }

        AddMessageToChat(userMessage);

        //var aiResponse = await _kernel.InvokePromptAsync(UserPrompt, kernelArguments);

        //var aiMessage = new Message
        //{
        //    Text = aiResponse.ToString(),
        //    IsUser = false
        //};
        //AddMessageToChat(aiMessage);
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
