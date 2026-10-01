using Microsoft.Phone.Shell;
using System;
using System.Windows.Threading;

namespace HaruApp.Helpers
{
    public static class ProgressHelper
    {
        // Pass the page's timer every time, so a pending error timeout can't hide newer progress.
        public static void ShowProgress(ProgressIndicator indicator, string text, bool isError = false, DispatcherTimer timer = null)
        {
            if (timer != null) timer.Stop();
            indicator.IsIndeterminate = !isError;
            indicator.Text = text;
            indicator.IsVisible = true;
            if (isError && timer != null) timer.Start();
        }

        public static void HideProgress(ProgressIndicator indicator, DispatcherTimer timer = null)
        {
            if (timer != null) timer.Stop();
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
