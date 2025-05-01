using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Rebyu.Interfaces;
using Rebyu.Models;
using Rebyu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private FlatTreeDataGridSource<Dictionary<string, object>> _dataGridSource;

    [ObservableProperty]
    private string _userPrompt;

    [ObservableProperty]
    private Message? _lastMessage;

    [ObservableProperty]
    private bool _chatIsBusy;

    [ObservableProperty]
    private SessionSqlQuery _currentSessionQuery;

    public ObservableCollection<Database> TreeItems { get; set; } = [];
    public ObservableCollection<Message> Chat { get; set; } = [];
    public ObservableCollection<SessionSqlQuery> SqlQueries { get; set; } = [];

    public ChatHistory ChatHistory { get; set; } = [];

    [ObservableProperty]
    private int _currentQueryIndex;

    [ObservableProperty]
    private int _totalQueryCount;

    private readonly SqliteDataService _sqliteDataService;
    private readonly ISemanticKernelService _skService;

    public MainViewModel()
    {
        CurrentSessionQuery = new("-- Enter your SQL query here\nSELECT * FROM ", 1);

        _sqliteDataService = App.ServiceProvider.GetRequiredService<SqliteDataService>();
        _skService = App.ServiceProvider.GetRequiredService<ISemanticKernelService>();


        SqlQueries.CollectionChanged += OnSqlQueriesCollectionChanged;
    }

    private void OnSqlQueriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TotalQueryCount = SqlQueries.Count;

        if (SqlQueries.Count == 0)
        {
            CurrentSessionQuery = new SessionSqlQuery("-- Enter your SQL query here\nSELECT * FROM ", AssignSqlQueryId());
            CurrentQueryIndex = 0;
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    // Set the last added item as the current session query
                    if (e.NewItems[^1] is SessionSqlQuery newItem)
                    {
                        CurrentSessionQuery = newItem;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    if (e.OldItems[0] is SessionSqlQuery removedItem)
                    {
                        HandleRemovedQuery(removedItem);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                CurrentSessionQuery = new SessionSqlQuery("-- Enter your SQL query here\nSELECT * FROM ", AssignSqlQueryId());
                CurrentQueryIndex = 0;
                break;

            case NotifyCollectionChangedAction.Replace:
                // Handle replacing items
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    if (e.NewItems[0] is SessionSqlQuery replacedItem)
                    {
                        CurrentSessionQuery = replacedItem;
                    }
                }
                break;
        }

        CurrentQueryIndex = SqlQueries.IndexOf(CurrentSessionQuery) + 1;
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

        var userMessage = new Message("User", "User", text: UserPrompt)
        { 
            IsUser = true,  
        };

        AddMessageToChat(userMessage);

        UserPrompt = string.Empty;

        var response = await _skService.GetResponse(userMessage, [.. Chat]);

        foreach (var message in response.Item1)
        {
            AddMessageToChat(message);
        }
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
    private void ResetChat() => Chat.Clear();

    partial void OnCurrentSessionQueryChanged(SessionSqlQuery value)
    {
        CurrentQueryIndex = SqlQueries.IndexOf(value) + 1;
        TotalQueryCount = SqlQueries.Count;

        NavigateBackwardCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBackward))]
    private void NavigateBackward()
    {
        if (SqlQueries.Count == 0) return;

        var currentIndex = SqlQueries.IndexOf(CurrentSessionQuery);
        if (currentIndex > 0)
        {
            CurrentSessionQuery = SqlQueries[currentIndex - 1];
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward()
    {
        if (SqlQueries.Count == 0) return;

        var currentIndex = SqlQueries.IndexOf(CurrentSessionQuery);
        if (currentIndex < SqlQueries.Count - 1)
        {
            CurrentSessionQuery = SqlQueries[currentIndex + 1];
        }
    }

    private bool CanNavigateBackward()
    {
        return CurrentSessionQuery != null && SqlQueries.IndexOf(CurrentSessionQuery) > 0;
    }

    private bool CanNavigateForward()
    {
        return CurrentSessionQuery != null && SqlQueries.IndexOf(CurrentSessionQuery) < SqlQueries.Count - 1;
    }

    [RelayCommand]
    private void FetchRecommendatedPromptsInit()
    {
        throw new NotImplementedException();
    }
}   

public class SqlPlugin()
{

}
