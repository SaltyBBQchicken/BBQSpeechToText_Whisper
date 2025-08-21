using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BBQSpeechToText_Whisper
{
    public class Worker : BackgroundService
    {
        private readonly GlobalHotkeyListener _hotkeyListener;
        private readonly ILogger<Worker> _logger;

        public Worker(GlobalHotkeyListener hotkeyListener, ILogger<Worker> logger)
        {
            _hotkeyListener = hotkeyListener;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service started: GlobalHotkeyListener is active.");
            await _hotkeyListener.StartListeningAsync(stoppingToken);
            // The Task.Run(() => _hook.Run(), cancellationToken); within StartListeningAsync
            // will keep the background service alive until the token is cancelled.
            // So, this ExecuteAsync method will effectively complete when the cancellationToken is requested.
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service stopping: Cancelling recording and stopping hotkey listener.");
            _hotkeyListener.CancelIfRecording(); // Request microphone capture to cancel any active recording
            _hotkeyListener.StopListening(); // Stop the global hook listener
            _logger.LogInformation("Service stopped.");
            await base.StopAsync(cancellationToken);
        }
    }
}
