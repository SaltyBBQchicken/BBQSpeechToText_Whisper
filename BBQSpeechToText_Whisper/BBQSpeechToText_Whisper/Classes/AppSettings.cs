using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBQSpeechToText_Whisper
{
    public class AppSettings
    {
        // SettingsPath is now a static read-only field, not tied to the singleton instance anymore
        private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        public WhisperSettings Whisper { get; set; } = new();
        public HotkeySettings Hotkeys { get; set; } = new();
        public TranscriptionOutputSettings TranscriptionOutput { get; set; } = new();
        public FileCleanupSettings FileCleanup { get; set; } = new();

        // The 'Instance' property is removed as settings are now managed by IOptions in Program.cs

        public static AppSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                // Return a default AppSettings if the file doesn't exist
                return new AppSettings();
            }

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AppSettings();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
