using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace BatteryStatus.Desktop;

public partial class MainWindow : Window
{
    private readonly Forms.NotifyIcon trayIcon;
    private readonly Forms.ContextMenuStrip trayMenu;
    private readonly DispatcherTimer refreshTimer;
    private AlertSettings settings = AlertSettings.Load();
    private PowerSource lastSource;
    private bool alertSent;
    private bool exiting;

    public MainWindow()
    {
        InitializeComponent();
        HighEnabled.IsChecked = settings.HighEnabled;
        HighInput.Text = settings.High.ToString();
        LowEnabled.IsChecked = settings.LowEnabled;
        LowInput.Text = settings.Low.ToString();

        trayMenu = new Forms.ContextMenuStrip();
        trayMenu.Items.Add("Open Battery Status", null, (_, _) => ShowStatus());
        trayMenu.Items.Add("Exit", null, (_, _) => Exit());
        trayIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "Battery Status",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        trayIcon.DoubleClick += (_, _) => ShowStatus();

        refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
        refreshTimer.Tick += (_, _) => RefreshStatus();
        Closing += KeepRunningInTray;
        Closed += (_, _) => ReleaseTray();
        RefreshStatus();
        refreshTimer.Start();
    }

    private void ShowStatus()
    {
        RefreshStatus();
        Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
    }

    private void KeepRunningInTray(object? sender, CancelEventArgs e)
    {
        if (exiting)
            return;
        e.Cancel = true;
        Hide();
    }

    private void Exit()
    {
        exiting = true;
        System.Windows.Application.Current.Shutdown();
    }

    private void ReleaseTray()
    {
        refreshTimer.Stop();
        trayIcon.Visible = false;
        trayIcon.Dispose();
        trayMenu.Dispose();
    }

    private void RefreshStatus()
    {
        var reading = BatteryReader.Read();
        var percent = reading.Available && reading.HasBattery ? reading.Percent : null;
        ChargeText.Text = percent is int value ? $"{value}%" : "—";
        ChargeBar.Visibility = percent.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ChargeBar.Value = percent ?? 0;

        string status = !reading.Available ? "Power status unavailable"
            : !reading.HasBattery ? "No battery detected"
            : reading.Charging ? "Charging"
            : reading.Source == PowerSource.PluggedIn ? "Plugged in"
            : reading.Source == PowerSource.Unplugged ? "On battery"
            : "Power source unknown";

        PowerText.Text = status;
        if (reading.Available && reading.HasBattery && !percent.HasValue)
            PowerText.Text += " · Charge level unavailable";
        if (reading.Available && reading.HasBattery && reading.SaverOn)
            PowerText.Text += " · Energy saver on";

        trayIcon.Text = percent is int charge ? $"Battery {charge}% · {status}" : $"Battery · {status}";
        if (!reading.Available || !reading.HasBattery || reading.Source == PowerSource.Unknown)
            return;

        if (reading.Source != lastSource)
        {
            lastSource = reading.Source;
            alertSent = false;
        }

        if (alertSent || !percent.HasValue)
            return;

        string? message = reading.Source switch
        {
            PowerSource.PluggedIn when settings.HighEnabled && percent.Value >= settings.High
                => $"The battery is at {percent.Value}%.",
            PowerSource.Unplugged when settings.LowEnabled && percent.Value <= settings.Low
                => $"The battery is down to {percent.Value}%. Consider plugging in.",
            _ => null
        };

        if (message is null)
            return;

        alertSent = true;
        trayIcon.ShowBalloonTip(4000, "Battery reminder", message, Forms.ToolTipIcon.Info);
    }

    private void SaveAlerts(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(HighInput.Text, out int high) || !int.TryParse(LowInput.Text, out int low))
        {
            SettingsMessage.Text = "Enter whole numbers for both percentages.";
            return;
        }

        var updated = new AlertSettings(HighEnabled.IsChecked == true, high, LowEnabled.IsChecked == true, low);
        if (!updated.IsValid)
        {
            SettingsMessage.Text = "Use percentages from 1 to 100. The low alert must be below the high alert.";
            return;
        }

        try
        {
            updated.Save();
            if (settings != updated)
                alertSent = false;
            settings = updated;
            SettingsMessage.Text = "Alerts saved.";
            RefreshStatus();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SettingsMessage.Text = "Couldn’t save your alerts. Your previous settings are still in use.";
        }
    }
}
