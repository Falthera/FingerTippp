using System.Windows;
using Fingertippp.App.Infrastructure;
using Fingertippp.App.ViewModels;

namespace Fingertippp.App;

public partial class MainWindow : Window
{
    private MouseHotplugMonitor? hotplugMonitor;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        SourceInitialized += (_, _) =>
        {
            hotplugMonitor = MouseHotplugMonitor.Attach(this, viewModel.RefreshDevices);
        };

        Closed += (_, _) => hotplugMonitor?.Dispose();
    }
}