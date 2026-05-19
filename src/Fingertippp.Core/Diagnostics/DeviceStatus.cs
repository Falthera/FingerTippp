using Fingertippp.Core.Devices;
using Fingertippp.Core.Optimization;

namespace Fingertippp.Core.Diagnostics;

public sealed record DeviceStatus(MouseDeviceInfo Device, OptimizationRecommendation Recommendation);