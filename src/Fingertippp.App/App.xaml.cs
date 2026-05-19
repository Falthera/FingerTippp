using System.Windows;
using Fingertippp.App.ViewModels;
using Fingertippp.Core.Benchmarking;
using Fingertippp.Core.Devices;
using Fingertippp.Core.Input;
using Fingertippp.Core.Optimization;

namespace Fingertippp.App;

public partial class App : Application
{
    private readonly OptimizationEngine optimizationEngine = new();
    private readonly ClickBenchmarkAnalyzer benchmarkAnalyzer = new();
    private readonly MouseDeviceStatusProvider deviceStatusProvider;

    public App()
    {
        var profileDatabase = new MouseProfileDatabase();
        var capabilityClassifier = new MouseCapabilityClassifier(profileDatabase);
        var catalog = new MouseDeviceCatalog(capabilityClassifier, optimizationEngine);
        deviceStatusProvider = new MouseDeviceStatusProvider(catalog, optimizationEngine);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var viewModel = new MainViewModel(deviceStatusProvider, benchmarkAnalyzer);
        var window = new MainWindow(viewModel);
        MainWindow = window;
        window.Show();
    }
}