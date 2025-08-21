using System.Collections.Generic;

namespace BBQSpeechToText_Whisper
{
    public class HotkeyCombination
    {
        public List<string> Modifiers { get; set; } = new();
        public string Key { get; set; } = string.Empty;
    }
}
