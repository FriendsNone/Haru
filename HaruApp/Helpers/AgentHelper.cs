using HaruApp.Resources;
using HaruCore;
using Microsoft.Phone.Scheduler;
using System;
using System.Windows;

namespace HaruApp.Helpers
{
    public static class AgentHelper
    {
        private const string TaskName = "HaruAgent";

        // Removes and re-registers the periodic task, which also renews its 14-day expiration.
        // Call it when the app launches and after settings are saved.
        public static void StartPeriodicAgent()
        {
            if (ScheduledActionService.Find(TaskName) != null)
                ScheduledActionService.Remove(TaskName);

            if (!HaruSettings.BackgroundUpdateEnabled
                || (!HaruSettings.LiveTileEnabled && !HaruSettings.NotificationEnabled))
                return;

            var task = new PeriodicTask(TaskName)
            {
                Description = AppResources.BackgroundAgentDescription,
                ExpirationTime = DateTime.Now.AddDays(14)
            };

            try
            {
                ScheduledActionService.Add(task);

                if (HaruSettings.BackgroundAgentDisabledShown)
                {
                    HaruSettings.BackgroundAgentDisabledShown = false;
                    HaruSettings.Save();
                }
#if DEBUG
                ScheduledActionService.LaunchForTest(TaskName, TimeSpan.FromSeconds(60));
                System.Diagnostics.Debug.WriteLine("Periodic task is started: " + TaskName);
#endif
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("BNS Error: The action is disabled") && !HaruSettings.BackgroundAgentDisabledShown)
                {
                    HaruSettings.BackgroundAgentDisabledShown = true;
                    HaruSettings.Save();
                    MessageBox.Show(AppResources.BackgroundAgentDisabled);
                }
            }
            catch (SchedulerServiceException) { }
        }
    }
}
