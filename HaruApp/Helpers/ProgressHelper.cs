using System;
using System.Windows.Threading;
using Microsoft.Phone.Shell;

namespace HaruApp.Helpers
{
    public static class ProgressHelper
    {
        public static void ShowProgress(ProgressIndicator indicator, string text, bool isError = false, DispatcherTimer timer = null)
        {
            indicator.IsIndeterminate = !isError;
            indicator.Text = text;
            indicator.IsVisible = true;
            if (isError && timer != null) timer.Start();
        }

        public static void HideProgress(ProgressIndicator indicator)
        {
            indicator.IsVisible = false;
        }

        public static DispatcherTimer CreateProgressTimer(ProgressIndicator indicator, int seconds = 3)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            timer.Tick += (s, e) => { timer.Stop(); indicator.IsVisible = false; };
            return timer;
        }
    }
}
