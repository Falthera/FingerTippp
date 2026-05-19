namespace Fingertippp.Core.Devices;

public sealed record MouseDeviceInfo(
    string DeviceId,
    string FriendlyName,
    int? VendorId,
    int? ProductId,
    MouseTransportKind Transport,
    MouseCapabilityLevel CapabilityLevel,
    double Confidence,
    string ClassificationReason,
    bool IsHotSwappable,
    string RawInputPath,
    DateTimeOffset DiscoveredAt)
{
    public bool HasVendorIdentity => VendorId.HasValue && ProductId.HasValue;
}