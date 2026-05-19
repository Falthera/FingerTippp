using Fingertippp.Core.Devices;
using Fingertippp.Core.Optimization;

namespace Fingertippp.Core.Profiles;

public sealed record DeviceProfile(
    string Name,
    int? VendorId,
    int? ProductId,
    MouseCapabilityLevel CapabilityLevel,
    OptimizationStrategy Strategy,
    bool RequiresConfirmation,
    string Notes);