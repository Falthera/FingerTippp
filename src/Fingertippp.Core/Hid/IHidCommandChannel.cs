namespace Fingertippp.Core.Hid;

public interface IHidCommandChannel
{
    HidCommandResult SendVendorCommand(string devicePath, ReadOnlySpan<byte> payload);
}