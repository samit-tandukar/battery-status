using System.Runtime.InteropServices;

namespace BatteryStatus.Desktop;

internal enum PowerSource { Unknown, PluggedIn, Unplugged }

internal readonly record struct BatteryReading(
    bool Available, bool HasBattery, int? Percent, PowerSource Source, bool Charging, bool SaverOn);

internal static class BatteryReader
{
    internal static BatteryReading Read()
    {
        if (!GetSystemPowerStatus(out var status))
            return default;

        const byte Unknown = 255;
        const byte NoBattery = 128;
        const byte Charging = 8;

        return new BatteryReading(
            Available: true,
            HasBattery: status.BatteryFlag == Unknown || (status.BatteryFlag & NoBattery) == 0,
            Percent: status.BatteryLifePercent <= 100 ? status.BatteryLifePercent : null,
            Source: status.ACLineStatus switch
            {
                0 => PowerSource.Unplugged,
                1 => PowerSource.PluggedIn,
                _ => PowerSource.Unknown
            },
            Charging: status.BatteryFlag != Unknown && (status.BatteryFlag & Charging) != 0,
            SaverOn: status.SystemStatusFlag == 1);
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemPowerStatus(out SystemPowerStatus status);

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }
}
