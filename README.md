# Battery Status

A Windows 11 tray utility that shows battery charge and sends configurable charge reminders.

- Double-click the system tray icon to open the status window, or hover over it to see charge and power status.
- Set separate reminders for when the device is plugged in or running on battery. Each reminder fires at most once per detected plug-in or unplugged session; changing the saved alert settings resets it.
- The window can be closed while the utility continues in the tray. Use **Exit** in the tray menu to quit.
- The app handles unavailable readings and PCs without a battery. Alerts do not control charging, and Windows notification settings may suppress them.

## Run on Windows 11

Install the .NET 10 SDK, then run:

```powershell
dotnet run --project BatteryStatus.Desktop/BatteryStatus.Desktop.csproj
```

The desktop app is written in C# with WPF. It reads the Windows power status locally and polls every 20 seconds while running. Alert preferences are stored in `%LOCALAPPDATA%\BatteryStatus.Desktop\alerts.json`. It does not send data over the network or start automatically with Windows.

## Original Windows 10 project

`BatteryStatus.sln`, `BatteryStatus/` and `Tasks/` contain the original Windows 10 PC and Mobile sample. They are preserved separately from the Windows 11 app.

## License

[MIT](LICENSE)
