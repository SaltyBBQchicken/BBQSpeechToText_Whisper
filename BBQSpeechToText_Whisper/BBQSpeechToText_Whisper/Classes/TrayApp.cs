using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Extensions.Options;

namespace BBQSpeechToText_Whisper
{
    public class TrayApp : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;

        private ToolStripMenuItem _autoTypeItem;
        private ToolStripMenuItem _clipboardItem;
        private ToolStripMenuItem _saveWavItem;
        private ToolStripMenuItem _saveTranscriptItem;
        private ToolStripMenuItem _modelSelectMenu;

        private const string SettingsFilePath = "appsettings.json";

        private readonly IOptionsMonitor<TranscriptionOutputSettings> _outputSettingsMonitor;
        private readonly IOptionsMonitor<FileCleanupSettings> _cleanupSettingsMonitor;

        public TrayApp(IOptionsMonitor<TranscriptionOutputSettings> outputSettingsMonitor,
                       IOptionsMonitor<FileCleanupSettings> cleanupSettingsMonitor)
        {
            _outputSettingsMonitor = outputSettingsMonitor;
            _cleanupSettingsMonitor = cleanupSettingsMonitor;

            // Get current values to initialize menu items
            var currentOutputSettings = _outputSettingsMonitor.CurrentValue;
            var currentCleanupSettings = _cleanupSettingsMonitor.CurrentValue;

            _autoTypeItem = new ToolStripMenuItem("Auto Type", null, ToggleAutoType)
            {
                Checked = currentOutputSettings.EnableAutoType
            };

            _clipboardItem = new ToolStripMenuItem("Clipboard Copy", null, ToggleClipboard)
            {
                Checked = currentOutputSettings.EnableClipboard
            };

            _saveWavItem = new ToolStripMenuItem("Save WAV", null, ToggleSaveWav)
            {
                Checked = currentCleanupSettings.SaveWav
            };

            _saveTranscriptItem = new ToolStripMenuItem("Save Transcript", null, ToggleSaveTranscript)
            {
                Checked = currentCleanupSettings.SaveTranscript
            };



            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(_autoTypeItem);
            _contextMenu.Items.Add(_clipboardItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_saveWavItem);
            _contextMenu.Items.Add(_saveTranscriptItem);

            _modelSelectMenu = new ToolStripMenuItem("Select Model");
            LoadModelSelectionMenu();
            if (_modelSelectMenu != null)
            {
                _contextMenu.Items.Add(new ToolStripSeparator());
                _contextMenu.Items.Add(_modelSelectMenu);
            }

            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Restart", null, OnRestart);
            _contextMenu.Items.Add("Exit", null, OnExit);




            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                ContextMenuStrip = _contextMenu,
                Text = "BBQ Speech To Text",
                Visible = true
            };

            // **CRITICAL FIX HERE:** Marshal UI updates to the UI thread
            _outputSettingsMonitor.OnChange(settings =>
                    {
                        // _trayIcon is a Control, so we can use its Invoke method
                        if (_trayIcon.ContextMenuStrip.InvokeRequired) // Use a control's InvokeRequired
                        {
                            _trayIcon.ContextMenuStrip.Invoke((MethodInvoker)delegate
                            {
                                _autoTypeItem.Checked = settings.EnableAutoType;
                                _clipboardItem.Checked = settings.EnableClipboard;
                            });
                        }
                        else
                        {
                            _autoTypeItem.Checked = settings.EnableAutoType;
                            _clipboardItem.Checked = settings.EnableClipboard;
                        }
                    });

            _cleanupSettingsMonitor.OnChange(settings =>
            {
                if (_trayIcon.ContextMenuStrip.InvokeRequired) // Use a control's InvokeRequired
                {
                    _trayIcon.ContextMenuStrip.Invoke((MethodInvoker)delegate
                    {
                        _saveWavItem.Checked = settings.SaveWav;
                        _saveTranscriptItem.Checked = settings.SaveTranscript;
                    });
                }
                else
                {
                    _saveWavItem.Checked = settings.SaveWav;
                    _saveTranscriptItem.Checked = settings.SaveTranscript;
                }
            });
        }

        private void ToggleAutoType(object? sender, EventArgs e)
        {
            _autoTypeItem.Checked = !_autoTypeItem.Checked;
            UpdateSettingAndSave(s => s.TranscriptionOutput.EnableAutoType = _autoTypeItem.Checked);
        }

        private void ToggleClipboard(object? sender, EventArgs e)
        {
            _clipboardItem.Checked = !_clipboardItem.Checked;
            UpdateSettingAndSave(s => s.TranscriptionOutput.EnableClipboard = _clipboardItem.Checked);
        }

        private void ToggleSaveWav(object? sender, EventArgs e)
        {
            _saveWavItem.Checked = !_saveWavItem.Checked;
            UpdateSettingAndSave(s => s.FileCleanup.SaveWav = _saveWavItem.Checked);
        }

        private void ToggleSaveTranscript(object? sender, EventArgs e)
        {
            _saveTranscriptItem.Checked = !_saveTranscriptItem.Checked;
            UpdateSettingAndSave(s => s.FileCleanup.SaveTranscript = _saveTranscriptItem.Checked);
        }

        private void LoadModelSelectionMenu()
        {
            _modelSelectMenu.DropDownItems.Clear();

            string modelsDir = Path.Combine(AppContext.BaseDirectory, "Models");
            if (!Directory.Exists(modelsDir))
                Directory.CreateDirectory(modelsDir);

            string[] modelFiles = Directory.GetFiles(modelsDir, "*.bin");
            foreach (string modelPath in modelFiles)
            {
                string fileName = Path.GetFileName(modelPath);
                var item = new ToolStripMenuItem(fileName)
                {
                    Checked = IsCurrentModel(fileName)
                };
                item.Click += (s, e) => SelectModel(fileName);
                _modelSelectMenu.DropDownItems.Add(item);
            }
        }



        private void SelectModel(string selectedModel)
        {
            var settings = AppSettings.Load();
            settings.Whisper.ModelPath = $"Models//{selectedModel}";
            settings.Save();

            MessageBox.Show($"Model set to {selectedModel}. Please restart the app to apply changes.",
                            "Model Changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            OnRestart(null, null);
        }

        private bool IsCurrentModel(string fileName)
        {
            var settings = AppSettings.Load();
            string currentModel = Path.GetFileName(settings.Whisper.ModelPath);
            return string.Equals(currentModel, fileName, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateSettingAndSave(Action<AppSettings> updateAction)
        {
            try
            {
                AppSettings currentSettings = AppSettings.Load();
                updateAction(currentSettings);
                currentSettings.Save();
                Console.WriteLine("Settings updated and saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnRestart(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            // Release the mutex BEFORE restarting
            var mutexField = typeof(Program).GetField("_mutex", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (mutexField?.GetValue(null) is Mutex mutex)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }

            // Start a new process after a short delay
            string exePath = Application.ExecutablePath;
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C timeout 1 && start \"\" \"{exePath}\"",
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            System.Diagnostics.Process.Start(startInfo);

            Environment.Exit(0); // Exit current instance
        }


        private void OnExit(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}