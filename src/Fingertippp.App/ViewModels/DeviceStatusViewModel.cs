using Fingertippp.Core.Diagnostics;

namespace Fingertippp.App.ViewModels;

public sealed class DeviceStatusViewModel
{
    public DeviceStatusViewModel(DeviceStatus status)
    {
        FriendlyName = status.Device.FriendlyName;
        StatusSummary = $"{status.Recommendation.DisplayLabel} · {status.Recommendation.Explanation}";
    }

    public string FriendlyName { get; }

    public string StatusSummary { get; }
}