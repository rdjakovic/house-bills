namespace HouseBills.Wpf.Services;

/// <summary>User-facing message boxes, abstracted so ViewModels stay testable.</summary>
public interface IDialogService
{
    /// <summary>Asks a yes/no question; returns <c>true</c> for yes.</summary>
    bool Confirm(string title, string message);

    void ShowInfo(string message);

    void ShowError(string message);
}