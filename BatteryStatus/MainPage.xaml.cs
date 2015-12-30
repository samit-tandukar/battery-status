﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BatteryStatus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AggregateBattery_ReportUpdated(Windows.Devices.Power.Battery sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var details = sender.GetReport();
                CalculatePercentage(details);
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
            var details = Windows.Devices.Power.Battery.AggregateBattery.GetReport();
            CalculatePercentage(details);
        }

        private void CalculatePercentage(Windows.Devices.Power.BatteryReport details)
        {
            var getPercentage = (details.RemainingCapacityInMilliwattHours.Value / (double)details.FullChargeCapacityInMilliwattHours.Value);
            var status = details.Status;
            string per = getPercentage.ToString("##%");
            progressInPer.Value = getPercentage * 100;
            batteryStatus.Text = "Status: " + status + " - " + "Percentage: " + per;
        }
    }
}
