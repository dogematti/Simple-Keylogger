@echo off
REM SecurMe Keylogger Simulator — FOR EDR DETECTION TESTING ONLY
REM
REM Usage:
REM   run.bat build      Build the .exe
REM   run.bat sim        Run the keylogger simulator GUI
REM   run.bat c2         Start the C2 listener (Python)
REM   run.bat demo       Start C2 + simulator together
REM   run.bat clean      Remove build artifacts

setlocal

set PROJ_DIR=%~dp0

if "%1"=="" goto usage
if "%1"=="build" goto build
if "%1"=="sim" goto sim
if "%1"=="c2" goto c2
if "%1"=="demo" goto demo
if "%1"=="clean" goto clean
goto usage

:build
echo [*] Building KeyloggerSim...
dotnet build "%PROJ_DIR%KeyloggerSim.csproj" -c Release
if errorlevel 1 (
    echo [!] Build failed. Ensure .NET 8 SDK is installed.
    echo     Download: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)
echo [+] Build complete. Binary: bin\Release\net8.0-windows\KeyloggerSim.exe
goto :eof

:sim
echo [*] Starting Keylogger Simulator...
if not exist "%PROJ_DIR%bin\Release\net8.0-windows\KeyloggerSim.exe" (
    echo [!] Not built yet. Running build first...
    call :build
    if errorlevel 1 exit /b 1
)
start "" "%PROJ_DIR%bin\Release\net8.0-windows\KeyloggerSim.exe"
goto :eof

:c2
echo [*] Starting C2 Listener...
python "%PROJ_DIR%c2_listener.py" %2 %3 %4 %5
goto :eof

:demo
echo [*] Starting full demo (C2 + Simulator)...
echo [*] Launching C2 listener in background...
start "C2 Listener" cmd /k python "%PROJ_DIR%c2_listener.py"
timeout /t 2 /nobreak >nul
echo [*] Launching Keylogger Simulator...
call :sim
echo.
echo [+] Both running. Type in the simulator window — watch C2 console.
echo [+] Close both windows when done.
goto :eof

:clean
echo [*] Cleaning build artifacts...
if exist "%PROJ_DIR%bin" rmdir /s /q "%PROJ_DIR%bin"
if exist "%PROJ_DIR%obj" rmdir /s /q "%PROJ_DIR%obj"
echo [+] Clean complete.
goto :eof

:usage
echo.
echo  SecurMe Keylogger Simulator — EDR Detection Testing
echo  ====================================================
echo.
echo  Usage: run.bat ^<command^>
echo.
echo  Commands:
echo    build    Build the keylogger .exe (requires .NET 8 SDK)
echo    sim      Run the keylogger simulator GUI
echo    c2       Start the C2 listener (requires Python 3)
echo    demo     Launch both C2 + simulator together
echo    clean    Remove build artifacts
echo.
echo  Quick start:
echo    1. run.bat demo        (launches everything)
echo    2. Click "Connect" in the simulator window
echo    3. Type — watch keys appear in the C2 console
echo    4. Check SecurMe sensor for detections
echo.
goto :eof
