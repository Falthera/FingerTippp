namespace Fingertippp.Core.Profiles;

public sealed class ProfileStore
{
    private readonly List<DeviceProfile> profiles = new();

    public IReadOnlyList<DeviceProfile> All => profiles;

    public void Add(DeviceProfile profile)
    {
        profiles.Add(profile);
    }

    public DeviceProfile? FindByVendorProduct(int? vendorId, int? productId)
    {
        return profiles.FirstOrDefault(profile => profile.VendorId == vendorId && profile.ProductId == productId);
    }
}