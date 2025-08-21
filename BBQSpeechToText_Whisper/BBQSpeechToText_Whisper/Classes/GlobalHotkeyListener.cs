using Microsoft.Extensions.Options;
using SharpHook;
using SharpHook.Data;
using SharpHook.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BBQSpeechToText_Whisper
{
    public class GlobalHotkeyListener
    {
        private HotkeyCombination _startKeys;
        private HotkeyCombination _stopKeys;
        private readonly MicrophoneCapture _micCapture;
        private SimpleGlobalHook? _hook;

        private bool _isCtrlPressed;
        private bool _isAltPressed;

        public GlobalHotkeyListener(IOptionsMonitor<HotkeySettings> hotkeySettingsMonitor, MicrophoneCapture micCapture)
        {
            _startKeys = hotkeySettingsMonitor.CurrentValue.StartKeys;
            _stopKeys = hotkeySettingsMonitor.CurrentValue.StopKeys;
            _micCapture = micCapture;

            hotkeySettingsMonitor.OnChange(newSettings =>
            {
                _startKeys = newSettings.StartKeys;
                _stopKeys = newSettings.StopKeys;
                Console.WriteLine("Hotkey settings updated.");
            });
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            _hook = new SimpleGlobalHook();
            _hook.KeyPressed += OnKeyPressed;
            _hook.KeyReleased += OnKeyReleased;

            _ = Task.Run(() => _hook.Run(), cancellationToken);
            await Task.CompletedTask;
        }

        public void StopListening()
        {
            if (_hook != null)
            {
                _hook.KeyPressed -= OnKeyPressed;
                _hook.KeyReleased -= OnKeyReleased;
                _hook.Dispose();
                _hook = null;
            }
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            var keyCode = e.Data.KeyCode;

            if (keyCode == KeyCode.VcLeftControl || keyCode == KeyCode.VcRightControl)
                _isCtrlPressed = true;

            if (keyCode == KeyCode.VcLeftAlt || keyCode == KeyCode.VcRightAlt)
                _isAltPressed = true;

            if (IsHotkeyPressed(_startKeys, keyCode))
            {
                Console.WriteLine("Start hotkey detected.");
                _micCapture.StartRecording();
            }

            if (IsHotkeyPressed(_stopKeys, keyCode))
            {
                Console.WriteLine("Stop hotkey detected.");
                _micCapture.StopAndSaveRecording();
            }
        }

        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            var keyCode = e.Data.KeyCode;

            if (keyCode == KeyCode.VcLeftControl || keyCode == KeyCode.VcRightControl)
                _isCtrlPressed = false;

            if (keyCode == KeyCode.VcLeftAlt || keyCode == KeyCode.VcRightAlt)
                _isAltPressed = false;
        }

        private bool IsHotkeyPressed(HotkeyCombination combo, KeyCode key)
        {
            // Corrected logic: if a modifier is NOT in the combo, its state doesn't matter (always true).
            // If it IS in the combo, then its pressed state must match.
            bool ctrlMatches = !combo.Modifiers.Contains("Control") || _isCtrlPressed;
            bool altMatches = !combo.Modifiers.Contains("Alt") || _isAltPressed;

            return ctrlMatches && altMatches && key == GetKeyCode(combo.Key);
        }

        private KeyCode GetKeyCode(string key)
        {
            if (Enum.TryParse($"Vc{key.ToUpper()}", out KeyCode result))
                return result;

            throw new ArgumentException($"Invalid key: {key}");
        }

        public void CancelIfRecording()
        {
            _micCapture.CancelRecording();
        }
    }
}
