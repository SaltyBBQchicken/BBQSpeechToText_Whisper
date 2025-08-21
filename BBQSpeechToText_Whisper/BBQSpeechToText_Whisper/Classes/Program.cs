// === Program.cs ===
using System;
using System.IO;
using System.Threading; // Added for Mutex
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BBQSpeechToText_Whisper
{
    static class Program
    {
        public static IHost? AppHost;
        private static Mutex? _mutex; // Declare a static Mutex

        // A unique name for your application's Mutex
        // It's good practice to use a GUID or a unique name based on your application.
        private const string MutexId = "BBQSpeechToText_Windows_SingleInstanceMutex";

        [STAThread]
        static async Task Main(string[] args)
        {
            // Check if another instance is already running
            if (!IsSingleInstance())
            {
                MessageBox.Show("BBQ Speech To Text is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit(); // Exit the current instance
                return;
            }

            ApplicationConfiguration.Initialize();

            AppHost = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.Configure<AppSettings>(configuration);
                    services.Configure<HotkeySettings>(configuration.GetSection("Hotkeys"));
                    services.Configure<TranscriptionOutputSettings>(configuration.GetSection("TranscriptionOutput"));
                    services.Configure<FileCleanupSettings>(configuration.GetSection("FileCleanup"));

                    services.AddSingleton<WhisperTranscriber>(provider =>
                    {
                        string? relativeModelPath = configuration["Whisper:ModelPath"];
                        if (string.IsNullOrWhiteSpace(relativeModelPath))
                            throw new InvalidOperationException("Whisper:ModelPath not set in appsettings.json");

                        string modelPath = Path.Combine(AppContext.BaseDirectory, relativeModelPath);
                        if (!File.Exists(modelPath))
                        {
                            Console.WriteLine($"Error: Whisper model not found at {modelPath}");
                            MessageBox.Show($"Whisper model not found at: {modelPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(1);
                        }
                        return new WhisperTranscriber(modelPath);
                    });

                    services.AddSingleton<MicrophoneCapture>();
                    services.AddSingleton<GlobalHotkeyListener>();
                    services.AddHostedService<Worker>();
                    services.AddSingleton<TrayApp>();
                })
                .Build();

            await AppHost.StartAsync();

            Application.Run(AppHost.Services.GetRequiredService<TrayApp>());

            await AppHost.StopAsync();

            // Release the Mutex when the application exits
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }

        /// <summary>
        /// Checks if another instance of the application is already running using a Mutex.
        /// </summary>
        /// <returns>True if this is the first instance, false otherwise.</returns>
        private static bool IsSingleInstance()
        {
            bool createdNew;
            _mutex = new Mutex(true, MutexId, out createdNew);
            return createdNew;
        }
    }
}