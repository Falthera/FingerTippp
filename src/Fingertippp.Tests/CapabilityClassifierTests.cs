using Fingertippp.Core.Devices;
using Fingertippp.Core.Optimization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fingertippp.Tests;

[TestClass]
public sealed class CapabilityClassifierTests
{
    [TestMethod]
    public void GenericUsbMouse_UsesSoftwareAssistedOrPollingPathWithoutFalseFirmwareClaims()
    {
        var classifier = new MouseCapabilityClassifier(new MouseProfileDatabase());
        var assessment = classifier.Assess(new MouseDeviceFingerprint(null, null, MouseTransportKind.Usb, "generic"));

        Assert.AreNotEqual(MouseCapabilityLevel.TrueDebounceControl, assessment.Level);
        Assert.AreNotEqual(MouseCapabilityLevel.Unsupported, assessment.Level);
    }

    [TestMethod]
    public void OptimizationEngine_MapsCapabilityLevelToTransparentLabel()
    {
        var engine = new OptimizationEngine();
        var device = new MouseDeviceInfo("id", "device", 0x046D, 0x1234, MouseTransportKind.Usb, MouseCapabilityLevel.SoftwareAssistedOptimization, 0.81, "reason", true, "path", DateTimeOffset.UtcNow);

        var recommendation = engine.Recommend(device);

        Assert.AreEqual("software-assisted optimization", recommendation.DisplayLabel);
        Assert.IsTrue(recommendation.RequiresConfirmation);
    }
}