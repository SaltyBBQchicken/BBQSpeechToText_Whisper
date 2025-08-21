using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms; // Required for Console.Beep and Clipboard operations if not moved

namespace BBQSpeechToText_Whisper
{
    public class MicrophoneCapture
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private string? _outputPath;

        private bool _isRecording;
        private readonly WhisperTranscriber _transcriber;

        private TranscriptionOutputSettings _outputSettings;
        private FileCleanupSettings _cleanupSettings;

        private readonly object _recordingLock = new object(); // Object for locking access to recording state

        public MicrophoneCapture(WhisperTranscriber transcriber,
                                 IOptionsMonitor<TranscriptionOutputSettings> outputSettingsMonitor,
                                 IOptionsMonitor<FileCleanupSettings> cleanupSettingsMonitor)
        {
            _transcriber = transcriber;

            _outputSettings = outputSettingsMonitor.CurrentValue;
            _cleanupSettings = cleanupSettingsMonitor.CurrentValue;

            outputSettingsMonitor.OnChange(settings => _outputSettings = settings);
            cleanupSettingsMonitor.OnChange(settings => _cleanupSettings = settings);
        }

        public void StartRecording()
        {
            lock (_recordingLock) // Ensure only one recording can start at a time
            {
                if (_isRecording)
                    return;

                _isRecording = true;

                string directory = Path.Combine(AppContext.BaseDirectory, "Out");
                Directory.CreateDirectory(directory);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                _outputPath = Path.Combine(directory, $"{timestamp}.wav");

                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1) // 16kHz, Mono
                };

                _writer = new WaveFileWriter(_outputPath, _waveIn.WaveFormat);

                _waveIn.DataAvailable += WaveIn_DataAvailable;
                _waveIn.RecordingStopped += WaveIn_RecordingStopped;

                _waveIn.StartRecording();
                Console.Beep(1000, 500); // Beep to indicate start
                Console.WriteLine("Recording started...");
            }
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs a)
        {
            lock (_recordingLock) // Protect writer access
            {
                _writer?.Write(a.Buffer, 0, a.BytesRecorded);
            }
        }

        private void WaveIn_RecordingStopped(object? sender, StoppedEventArgs a)
        {
            lock (_recordingLock) // Protect shared resources during stop and cleanup
            {
                // Dispose resources when recording genuinely stops
                _writer?.Dispose();
                _waveIn?.Dispose();

                _writer = null;
                _waveIn = null;
                _isRecording = false;

                if (a.Exception != null)
                {
                    Console.WriteLine($"Recording stopped with error: {a.Exception.Message}");
                }
            }
        }

        public void StopAndSaveRecording()
        {
            string? pathForTranscription = null;

            lock (_recordingLock) // Ensure consistent state during stop
            {
                if (!_isRecording)
                    return;

                // Stop recording. The WaveIn_RecordingStopped event will handle cleanup.
                _waveIn?.StopRecording();
                Console.Beep(500, 250); // Beep to indicate stop

                pathForTranscription = _outputPath; // Capture path before potential nulling
            }

            Console.WriteLine($"Recording stopped. Processing file: {pathForTranscription}");

            if (!string.IsNullOrEmpty(pathForTranscription))
            {
                // Run transcription and file cleanup in a separate task
                Task.Run(async () =>
                {
                    try
                    {
                        string text = await _transcriber.TranscribeAsync(pathForTranscription);

                        Console.WriteLine("Transcription:");
                        Console.WriteLine(text);

                        if (_outputSettings.EnableAutoType || _outputSettings.EnableClipboard)
                        {
                            var typer = new WindowsTextTyper();

                            if (_outputSettings.EnableAutoType)
                                typer.TypeText(text);

                            if (_outputSettings.EnableClipboard)
                                typer.CopyToClipboard(text);
                        }

                        if (_cleanupSettings.SaveTranscript)
                        {
                            string transcriptDir = Path.Combine(AppContext.BaseDirectory, "Transcripts");
                            Directory.CreateDirectory(transcriptDir);
                            // Use async file write for efficiency
                            await File.AppendAllTextAsync(Path.Combine(transcriptDir, "transcript.txt"), text + Environment.NewLine);
                            Console.WriteLine("Transcript saved.");
                        }

                        // Clean up WAV file if not set to save
                        if (!_cleanupSettings.SaveWav && File.Exists(pathForTranscription))
                        {
                            File.Delete(pathForTranscription);
                            Console.WriteLine("WAV file deleted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Transcription or file operation error: {ex.Message}");
                    }
                });
            }
        }

        public void CancelRecording()
        {
            lock (_recordingLock)
            {
                if (_isRecording)
                {
                    _waveIn?.StopRecording(); // This will trigger WaveIn_RecordingStopped for cleanup
                    Console.Beep(250, 250); // Beep to indicate cancellation
                    Console.WriteLine("Recording canceled.");

                    // Immediately delete the partially recorded file if canceled
                    if (!string.IsNullOrEmpty(_outputPath) && File.Exists(_outputPath))
                    {
                        try
                        {
                            File.Delete(_outputPath);
                            Console.WriteLine("Canceled WAV file deleted.");
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"Error deleting canceled WAV file: {ex.Message}");
                        }
                    }
                    _outputPath = null; // Clear the path as the file is gone or unwanted
                }
            }
        }
    }
}
