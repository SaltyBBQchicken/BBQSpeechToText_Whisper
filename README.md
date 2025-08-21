# BBQSpeechToText_Whisper
A Standalone Windows 11 C# Implementation of OpenAI's Whisper models. A Whisper AI Voice Transcription Interface for Windows 
No internet or connection required. It's purely standalone and runs on your system only.  No voice data is sent  to any cloud service. 

Whisper model downloads.  Other models may work but have not been tested. 
All 4 models can be changed during runtime. 

https://huggingface.co/ggerganov/whisper.cpp

ggml-large-v3.bin
ggml-large-v3-turbo.bin
ggml-medium.bin
ggml-tiny.en.bin


.NET 8. Free from Microsoft Visual Studio Community 2022 Edition. https://visualstudio.microsoft.com/vs/community/ 

All Required External libraries are also available on NuGet. Can be easily installed with Tools/NuGet Package Manager/Manage NuGet packages for solution. 

Required Microsoft Net packages that may need to be installed from NuGet. 
Microsoft.Extensions.Options (v 9.0.8)  
Microsoft.Extensions.Configuration (v 9.0.8)  
Microsoft.Extensions.Hosting  (v 9.0.8) 

NAudio (v 2.2.1)  https://github.com/naudio/NAudio
Used For Audio Input Mic

SharpHook (v 7.0.1)  https://sharphook.tolik.io/
Global keys to activate the audio listener for transcription. 
Can be Modified in appsettings.json.  Control+Alt+N (Start Recording) Control+Alt+M (Stop Recording)

Whisper.net.AllRuntimes (v 1.8.1) https://github.com/sandrohanea/whisper.net
Whisper.net is the wrapper for OpenAI's Whisper model. 


If not already installed, the required  Microsoft runtime can be found at: 
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.19-windows-x64-installer?cid=getdotnetcore

Microphone must be enabled in privacy settings within Windows. 

Right-click the taskbar tray icon to toggle settings. 
These settings can also be found  and manually changed within the appsettings.json

Auto type: will type the Transcription in to the focused window text input. 

Copy the clipboard: Will copy the Transcription onto the clipboard. 
 
Save Wave Toggle: will save the voice recording to a WAV file within the Out directory.  This is good to check  volume levels and can be disabled so it does not save any files. 

Save Transcript: can be used to toggle saving transcripts to a text file within the out directory. This can also be disabled. 

Select Model: will populate with the models within the models directory and can be changed at runtime.  This is useful to help with selecting lower models for slower PCs.  Changing models were prompt to restart the application to load the new model. 

Restart: is to restart the application. Sometimes it does get hung up and this is a useful quick fix. 

Operational suggestions: 

This is AI, so it's resource intensive.  Experiment with smaller models if you have performance issues.  If any issues just restart it through the menu.  There are other models that may be compatible from the Hugging Face Link to Whisper. 
For the Auto Type option, the output text window should be selected (in focus) before recording.
Give it a second after talking before you stop the recording.  Depending on PC performance, there may be a small delay causing the recording to stop prematurely, so give it a second or two.  Enable "Save WAV file" to test microphone volume if you're having issues.  The longer the sentence or phrase, the longer the process time. I recommend staying under two minutes long. 

---   Author's note. 
I wrote this simple program because I do not want my voice recording spread across the internet. Simply sharing something that I made for myself that others may find use full.  If any kind of data collection is discovered from these third-party libraries, please notify me right away so I may find an alternative. For peace of mind,  included is a firewall bat script "..FirewallRules/AddFirewallRules.bat"  which contains a bat file that will add a block for inbound and outbound traffic to Windows Defender Firewall for this program. 
Like many others, I'm simply tired of data collection ,sleazy practices, and buried Terms of Service trickery.

Ideas for possible additions full file transcriptions,  Add voice commands for computer processes.  Encrypted voice password management and input.  and whatever else comes to mind that I might find useful. 


Enjoy. 

Primary credit goes to authors of 

Microsoft and .NET
Open AI. Whisper
Whisper.net
NAudio
SharpHook





