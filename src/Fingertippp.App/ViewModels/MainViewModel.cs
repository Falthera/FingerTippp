using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Fingertippp.Core.Benchmarking;
using Fingertippp.Core.Devices;
using Fingertippp.Core.Diagnostics;
using Fingertippp.Core.Input;
using Fingertippp.Core.Optimization;

namespace Fingertippp.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly MouseDeviceStatusProvider deviceStatusProvider;
    private readonly ClickBenchmarkAnalyzer benchmarkAnalyzer;

    private double confidence;
    private double estimatedCps;
    private double jitter;
    private int doubleClickCount;
    private string selectedDeviceSummary = "No device selected yet.";
    private string selectedRecommendationSummary = "Refresh devices to analyze capability and select a safe optimization path.";
    private string selectedMethodDisplay = "unsupported";
    private bool isSafeModeEnabled = true;
    private bool isExpertModeEnabled;

    public MainViewModel(MouseDeviceStatusProvider deviceStatusProvider, ClickBenchmarkAnalyzer benchmarkAnalyzer)
    {
        this.deviceStatusProvider = deviceStatusProvider;
        this.benchmarkAnalyzer = benchmarkAnalyzer;

        Devices = new ObservableCollection<DeviceStatusViewModel>();
        RefreshCommand = new RelayCommand(RefreshDevices);
        LoadDemoState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DeviceStatusViewModel> Devices { get; }

    public ICommand RefreshCommand { get; }

    public string SelectedDeviceSummary
    {
        get => selectedDeviceSummary;
        private set => SetField(ref selectedDeviceSummary, value);
    }

    public string SelectedRecommendationSummary
    {
        get => selectedRecommendationSummary;
        private set => SetField(ref selectedRecommendationSummary, value);
    }

    public string SelectedMethodDisplay
    {
        get => selectedMethodDisplay;
        private set => SetField(ref selectedMethodDisplay, value);
    }

    public string ConfidenceDisplay => $"{confidence:P0}";

    public string EstimatedCpsDisplay => estimatedCps <= 0 ? "—" : estimatedCps.ToString("0.0");

    public string JitterDisplay => jitter <= 0 ? "—" : $"{jitter:0.00} ms";

    public string DoubleClickDisplay => doubleClickCount.ToString();

    public bool IsSafeModeEnabled
    {
        get => isSafeModeEnabled;
        set => SetField(ref isSafeModeEnabled, value);
    }

    public bool IsExpertModeEnabled
    {
        get => isExpertModeEnabled;
        set => SetField(ref isExpertModeEnabled, value);
    }

    private void LoadDemoState()
    {
        var samples = new[]
        {
            new ClickTimingSample(DateTimeOffset.UtcNow.AddMilliseconds(0), true, false),
            new ClickTimingSample(DateTimeOffset.UtcNow.AddMilliseconds(88), true, false),
            new ClickTimingSample(DateTimeOffset.UtcNow.AddMilliseconds(172), true, true),
            new ClickTimingSample(DateTimeOffset.UtcNow.AddMilliseconds(261), true, false)
        };

        UpdateBenchmark(benchmarkAnalyzer.Analyze(samples));

        var fingerprint = new MouseDeviceFingerprint(0x046D, 0xC53D, MouseTransportKind.Usb, @"\\?\HID#VID_046D&PID_C53D&MI_00#8&12ab34cd&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}");
        var assessment = new MouseCapabilityClassifier(new MouseProfileDatabase()).Assess(fingerprint);
        var device = new MouseDeviceInfo(
            "demo-logitech",
            "Logitech demo mouse",
            fingerprint.VendorId,
            fingerprint.ProductId,
            fingerprint.Transport,
            assessment.Level,
            assessment.Confidence,
            assessment.Reason,
            true,
            fingerprint.RawInputPath,
            DateTimeOffset.UtcNow);

        ApplyDevice(device);
    }

    public void RefreshDevices()
    {
        Devices.Clear();

        var devices = deviceStatusProvider.GetDevices();

        foreach (var device in devices)
        {
            Devices.Add(new DeviceStatusViewModel(deviceStatusProvider.BuildStatus(device)));
        }

        ApplyDevice(devices[0]);
    }

    private void ApplyDevice(MouseDeviceInfo device)
    {
        var recommendation = deviceStatusProvider.BuildStatus(device).Recommendation;

        SelectedDeviceSummary = $"{device.FriendlyName} · {device.CapabilityLevel} · confidence {device.Confidence:P0}";
        SelectedRecommendationSummary = $"{recommendation.DisplayLabel}: {recommendation.Explanation}";
        SelectedMethodDisplay = recommendation.DisplayLabel;
        confidence = recommendation.Confidence;

        OnPropertyChanged(nameof(ConfidenceDisplay));
        OnPropertyChanged(nameof(EstimatedCpsDisplay));
        OnPropertyChanged(nameof(JitterDisplay));
        OnPropertyChanged(nameof(DoubleClickDisplay));
        OnPropertyChanged(nameof(SelectedDeviceSummary));
        OnPropertyChanged(nameof(SelectedRecommendationSummary));
        OnPropertyChanged(nameof(SelectedMethodDisplay));
    }

    private void UpdateBenchmark(BenchmarkResult result)
    {
        estimatedCps = result.EstimatedCps;
        jitter = result.JitterMilliseconds;
        doubleClickCount = result.DoubleClickCandidates;

        OnPropertyChanged(nameof(ConfidenceDisplay));
        OnPropertyChanged(nameof(EstimatedCpsDisplay));
        OnPropertyChanged(nameof(JitterDisplay));
        OnPropertyChanged(nameof(DoubleClickDisplay));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}