using Whisper.net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BBQSpeechToText_Whisper
{
    public class WhisperTranscriber : IDisposable
    {
        private readonly WhisperProcessor _processor;

        public WhisperTranscriber(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                // This exception will be caught during service registration in Program.cs
                throw new FileNotFoundException("Whisper model not found", modelPath);
            }

            var factory = WhisperFactory.FromPath(modelPath);
            _processor = factory.CreateBuilder().Build();
            Console.WriteLine($"Whisper transcriber initialized with model: {modelPath}");
        }

        public async Task<string> TranscribeAsync(string wavPath)
        {
            if (!File.Exists(wavPath))
                throw new FileNotFoundException("Audio file not found", wavPath);

            using var audioStream = File.OpenRead(wavPath);
            Console.WriteLine($"Starting transcription for {wavPath}...");
            var segments = _processor.ProcessAsync(audioStream);

            string resultText = "";
            await foreach (var segment in segments)
            {
                resultText += segment.Text;
            }
            Console.WriteLine($"Transcription complete for {wavPath}.");
            return resultText;
        }

        public void Dispose()
        {
            _processor.Dispose();
            Console.WriteLine("Whisper transcriber disposed.");
            GC.SuppressFinalize(this); // Prevent the finalizer from running
        }
    }
}

