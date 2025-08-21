//MainForm.cs
// This file is largely vestigial in a pure tray application setup.
// It's kept for completeness but not actively used for UI by the tray icon.
namespace BBQSpeechToText_Whisper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // This form is not shown for a tray application.
            // If you wanted a settings window, you might use this.
            this.ShowInTaskbar = false; // Hide from taskbar
            this.WindowState = FormWindowState.Minimized; // Start minimized
            this.Visible = false; // Make sure it's not visible
        }
    }
}