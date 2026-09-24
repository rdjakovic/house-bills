using System.Windows;

namespace HouseBills.Wpf.Services;

internal sealed class DialogService : IDialogService
{
    private const string Caption = "HouseBills";

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(Owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    public void ShowInfo(string message)
    {
        MessageBox.Show(Owner, message, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(Owner, message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static Window Owner => System.Windows.Application.Current.MainWindow;
}