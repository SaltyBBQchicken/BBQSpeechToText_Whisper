namespace BBQSpeechToText_Whisper
{
    public class HotkeySettings
    {
        public HotkeyCombination StartKeys { get; set; } = new();
        public HotkeyCombination StopKeys { get; set; } = new();
    }
}