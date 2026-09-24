using System.Windows;

namespace HouseBills.Wpf.Views;

/// <summary>Shown while startup work (database preparation) takes noticeably long.</summary>
public partial class StartupWindow : Window
{
    public StartupWindow()
    {
        InitializeComponent();
    }
}