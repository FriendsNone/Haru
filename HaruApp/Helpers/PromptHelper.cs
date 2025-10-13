using System;
using Microsoft.Phone.Controls;

namespace HaruApp.Helpers
{
    public static class PromptHelper
    {
        public static void ShowPrompt(string caption, string message, string leftButton, string rightButton, Action onLeft = null, Action onRight = null, Action onDismiss = null)
        {
            var messageBox = new CustomMessageBox
            {
                Caption = caption,
                Message = message,
                LeftButtonContent = leftButton,
                RightButtonContent = rightButton
            };

            messageBox.Dismissed += (s, e) =>
            {
                switch (e.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        if (onLeft != null) onLeft();
                        break;
                    case CustomMessageBoxResult.RightButton:
                        if (onRight != null) onRight();
                        break;
                    default:
                        if (onDismiss != null) onDismiss();
                        break;
                }
            };

            messageBox.Show();
        }

        public static void ShowAlert(string caption, string message, string buttonText = "okay")
        {
            new CustomMessageBox
            {
                Caption = caption,
                Message = message,
                LeftButtonContent = buttonText
            }.Show();
        }
    }
}
