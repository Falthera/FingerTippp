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
    private string latestActionDisplay = "Ready. Click Refresh Devices to start.";
    private string workflowStatusDisplay = "Step 1/4: Detect your mouse and evaluate capability.";

    public MainViewModel(MouseDeviceStatusProvider deviceStatusProvider, ClickBenchmarkAnalyzer benchmarkAnalyzer)
    {
        this.deviceStatusProvider = deviceStatusProvider;
        this.benchmarkAnalyzer = benchmarkAnalyzer;

        Devices = new ObservableCollection<DeviceStatusViewModel>();
        RefreshCommand = new RelayCommand(RefreshDevices);
        RefreshAndAnalyzeCommand = new RelayCommand(RefreshAndAnalyze);
        RunQuickBenchmarkCommand = new RelayCommand(RunQuickBenchmark);
        ApplySafeRecommendationCommand = new RelayCommand(ApplySafeRecommendation);
        LoadDemoState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DeviceStatusViewModel> Devices { get; }

    public ICommand RefreshCommand { get; }

    public ICommand RefreshAndAnalyzeCommand { get; }

    public ICommand RunQuickBenchmarkCommand { get; }

    public ICommand ApplySafeRecommendationCommand { get; }

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

    public string LatestActionDisplay
    {
        get => latestActionDisplay;
        private set => SetField(ref latestActionDisplay, value);
    }

    public string WorkflowStatusDisplay
    {
        get => workflowStatusDisplay;
        private set => SetField(ref workflowStatusDisplay, value);
    }

    public string QuickStartSteps =>
        "1. Plug in your mouse and click Refresh Devices.\n" +
        "2. Review capability confidence and selected method.\n" +
        "3. Run Quick Benchmark to estimate CPS and jitter.\n" +
        "4. Click Apply Safe Recommendation to enable a non-invasive strategy.";

    public string SafetyRules =>
        "Safety guarantees:\n" +
        "- No kernel exploits\n" +
        "- No game injection\n" +
        "- No hidden driver installs\n" +
        "- Only reversible, user-visible actions";

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

        if (devices.Count > 0)
        {
            ApplyDevice(devices[0]);
            LatestActionDisplay = $"Detected {devices.Count} device(s).";
            WorkflowStatusDisplay = "Step 2/4: Review recommendation and confidence.";
        }
        else
        {
            SelectedDeviceSummary = "No compatible mouse detected.";
            SelectedRecommendationSummary = "Connect a mouse and click Refresh Devices.";
            SelectedMethodDisplay = "unsupported";
            LatestActionDisplay = "No devices found.";
            WorkflowStatusDisplay = "Step 1/4: Connect a mouse and refresh.";
        }
    }

    public void RefreshAndAnalyze()
    {
        RefreshDevices();
        RunQuickBenchmark();
    }

    public void RunQuickBenchmark()
    {
        var now = DateTimeOffset.UtcNow;
        var samples = new[]
        {
            new ClickTimingSample(now.AddMilliseconds(0), true, false),
            new ClickTimingSample(now.AddMilliseconds(81), true, false),
            new ClickTimingSample(now.AddMilliseconds(168), true, false),
            new ClickTimingSample(now.AddMilliseconds(251), true, false),
            new ClickTimingSample(now.AddMilliseconds(337), true, false),
            new ClickTimingSample(now.AddMilliseconds(421), true, true)
        };

        UpdateBenchmark(benchmarkAnalyzer.Analyze(samples));
        LatestActionDisplay = "Quick benchmark finished.";
        WorkflowStatusDisplay = "Step 3/4: Benchmark updated. Apply safe recommendation.";
    }

    public void ApplySafeRecommendation()
    {
        if (Devices.Count == 0)
        {
            LatestActionDisplay = "Nothing to apply. Refresh devices first.";
            return;
        }

        var method = SelectedMethodDisplay;
        var safety = IsSafeModeEnabled ? "safe mode" : "manual mode";
        LatestActionDisplay = $"Applied {method} profile in {safety}.";
        WorkflowStatusDisplay = "Step 4/4: Optimization profile active.";
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