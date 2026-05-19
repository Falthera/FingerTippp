# Fingertippp

Windows desktop application for mouse latency diagnostics and safe optimization.

## Current stage

This first implementation establishes the core architecture:

- WPF desktop shell
- Core device capability classification
- Optimization strategy selection
- Click timing benchmark analysis
- Initial test project

## Build

Build the app project directly:

```powershell
dotnet build .\src\Fingertippp.App\Fingertippp.App.csproj
```

## Notes

- The app is intentionally limited to safe, user-mode behavior.
- Firmware-level debounce control is only labeled when a verified profile exists.
- Raw Input and HID plumbing will be added next.