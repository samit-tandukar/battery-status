using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Tasks
{
    public sealed class updater : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            var details = Windows.Devices.Power.Battery.AggregateBattery.GetReport();
            var getPercentage = (details.RemainingCapacityInMilliwattHours.Value / (double)details.FullChargeCapacityInMilliwattHours.Value);
            var status = details.Status;
            string per = getPercentage.ToString("##%");
            double batteryPercentage = getPercentage * 100;
            _deferral.Complete();
        }
    }
}
