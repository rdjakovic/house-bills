using System.Windows;

using HouseBills.Wpf.ViewModels;

namespace HouseBills.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Start();
    }
}