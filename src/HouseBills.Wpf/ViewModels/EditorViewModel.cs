using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace HouseBills.Wpf.ViewModels;

/// <summary>Base for edit forms: data-annotation validation plus a server-side error message.</summary>
public abstract partial class EditorViewModel : ObservableValidator
{
    public abstract string Title { get; }

    /// <summary>Error returned by the service (e.g. duplicate name, concurrency conflict).</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>Validates all fields; returns <c>true</c> if there are no errors.</summary>
    public bool Validate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

    /// <summary>Clears errors raised while the constructor populated the fields, so a new form starts clean.</summary>
    protected void ResetValidation()
    {
        ClearErrors();
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
    }
}