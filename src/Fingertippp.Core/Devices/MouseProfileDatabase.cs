namespace Fingertippp.Core.Devices;

public sealed class MouseProfileDatabase
{
    private static readonly IReadOnlyDictionary<int, MouseProfile> Profiles = new Dictionary<int, MouseProfile>
    {
        [0x046D] = new MouseProfile(0x046D, "Logitech", "Logitech", true, true, false),
        [0x1532] = new MouseProfile(0x1532, "Razer", "Razer", true, true, false),
        [0x258A] = new MouseProfile(0x258A, "Glorious", "Glorious", true, true, false),
        [0x1E7D] = new MouseProfile(0x1E7D, "Finalmouse", "Finalmouse", true, true, false),
        [0x1038] = new MouseProfile(0x1038, "SteelSeries", "SteelSeries", true, true, false),
        [0x04F3] = new MouseProfile(0x04F3, "Zowie", "BenQ Zowie", true, true, false),
        [0x0951] = new MouseProfile(0x0951, "HyperX", "HyperX", true, true, false),
        [0x1B1C] = new MouseProfile(0x1B1C, "Corsair", "Corsair", true, true, false)
    };

    public bool TryGetProfile(int vendorId, out MouseProfile profile) => Profiles.TryGetValue(vendorId, out profile!);
}