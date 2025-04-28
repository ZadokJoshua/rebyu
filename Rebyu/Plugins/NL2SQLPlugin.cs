using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Rebyu.ViewModels;
using System;
using System.ComponentModel;

namespace Rebyu.Plugins;

public sealed class NL2SQLPlugin
{
    [KernelFunction]
    [Description("Updates the preview editor with the given string.")]
    public void UpdatePreviewEditor([Description("The exact sql statement to be added")] string sqlStatement)
    {
        var mainViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        if (mainViewModel.SelectedDatabase == null)
        {
            throw new InvalidOperationException("No database selected.");
        }

        Dispatcher.UIThread.Post(() =>
        {
            mainViewModel.AddNewSqlQuery(sqlStatement);
        }, DispatcherPriority.Background);
    }
}