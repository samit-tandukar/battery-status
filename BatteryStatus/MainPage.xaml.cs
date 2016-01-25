using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BatteryStatus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string task_name = "batterystatus_task";

        public MainPage()
        {
            this.InitializeComponent();
            RegisterBackgroundTask();
        }

        private async void RegisterBackgroundTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == task_name)
                {
                    return; //IF THE TASK IS ALREADY REGISTERED, THE CODE EXITS
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = task_name;
            builder.TaskEntryPoint = "Tasks.updater";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.CancelOnConditionLoss = true;
            BackgroundAccessStatus access_status = await BackgroundExecutionManager.RequestAccessAsync();
            if (access_status != BackgroundAccessStatus.Denied)
            {
                BackgroundTaskRegistration mytask = builder.Register();
            }
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
            ProgressInPer.Value = getPercentage * 100;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            BatteryStatus.Text = loader.GetString("status") + loader.GetString(status.ToString()) + " - " + loader.GetString("percentage") + per;
        }
    }
}
