using System;
using System.Threading;
using System.Windows.Forms;

namespace BBQSpeechToText_Whisper
{
    public class WindowsTextTyper
    {
        /// <summary>
        /// Sends the specified text to the currently focused window.
        /// </summary>
        /// <param name="text">The text to type.</param>
        public void TypeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // SendKeys requires an STA thread. Creating a new one for each call is generally acceptable
            // for intermittent operations in a tray application.
            Thread thread = new Thread(() =>
            {
                try
                {
                    SendKeys.SendWait(text + " ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while sending keys: {ex.Message}");
                    // Log the error more robustly in a production app
                }
            });

            thread.SetApartmentState(ApartmentState.STA); // required for SendKeys
            thread.Start();
            thread.Join(); // Wait for the typing operation to complete
        }

        /// <summary>
        /// Copies the specified text to the clipboard.
        /// </summary>
        /// <param name="text">The text to copy.</param>
        public void CopyToClipboard(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Clipboard operations also require an STA thread.
            Thread thread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetText(text + " ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while copying to clipboard: {ex.Message}");
                    // Log the error more robustly in a production app
                }
            });

            thread.SetApartmentState(ApartmentState.STA); // required for Clipboard
            thread.Start();
            thread.Join(); // Wait for the clipboard operation to complete
        }
    }
}
