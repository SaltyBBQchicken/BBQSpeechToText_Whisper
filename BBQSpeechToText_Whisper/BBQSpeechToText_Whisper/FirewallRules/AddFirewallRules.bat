@echo off
setlocal

:: Check if the script is run as administrator
openfiles >nul 2>&1
if errorlevel 1 (
    echo This script must be run as Administrator.
    echo Please right-click the script and select "Run as Administrator."
    pause
    exit /b
)

:: Set rule names and parameters
set "RULE_NAME_IN=Block_IN_BBQSpeechToText_Whisper"
set "RULE_NAME_OUT=Block_OUT_BBQSpeechToText_Whisper"
set "RULE_PROGRAM=%~dp0..\BBQSpeechToText_Whisper.exe"
set "RULE_DESCRIPTION=Block BBQSpeechToText_Whisper.exe"

:: Check and add Inbound Rule
netsh advfirewall firewall show rule name="%RULE_NAME_IN%" >nul 2>&1
if errorlevel 1 (
    echo Adding Inbound Rule...
    netsh advfirewall firewall add rule name="%RULE_NAME_IN%" ^
        dir=in action=block program="%RULE_PROGRAM%" ^
        enable=yes profile=any description="%RULE_DESCRIPTION%" protocol=any
) else (
    echo Inbound Rule already exists.
)

:: Check and add Outbound Rule
netsh advfirewall firewall show rule name="%RULE_NAME_OUT%" >nul 2>&1
if errorlevel 1 (
    echo Adding Outbound Rule...
    netsh advfirewall firewall add rule name="%RULE_NAME_OUT%" ^
        dir=out action=block program="%RULE_PROGRAM%" ^
        enable=yes profile=any description="%RULE_DESCRIPTION%" protocol=any
) else (
    echo Outbound Rule already exists.
)

endlocal
pause
