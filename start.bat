@echo off
title Kindergarten System - Connection Diagnostics
color 0B

echo.
echo =========================================================
echo               KINDERGARTEN SYSTEM DIAGNOSTICS
echo =========================================================
echo.

REM Build the project first
echo [1/4] Building project...
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" KindergartenSystem.sln /p:Configuration=Debug /p:Platform="Any CPU" /t:Build /verbosity:quiet

if %errorlevel% neq 0 (
    echo ❌ Build failed! Check Visual Studio for errors.
    pause
    exit /b 1
)
echo ✅ Build successful!
echo.

REM Test available ports
echo [2/4] Finding available port...
set FOUND_PORT=0

for %%p in (3000 3001 3002 8080 8081 5000 5001) do (
    if !FOUND_PORT! equ 0 (
        netstat -an | findstr ":%%p " >nul 2>&1
        if errorlevel 1 (
            set FOUND_PORT=%%p
            echo ✅ Port %%p is available
        ) else (
            echo ⚠️  Port %%p is busy
        )
    )
)

if %FOUND_PORT% equ 0 (
    echo ❌ No available ports found! Try closing other applications.
    pause
    exit /b 1
)
echo.

REM Start IIS Express
echo [3/4] Starting IIS Express on port %FOUND_PORT%...
echo.
echo =========================================================
echo                    🚀 APPLICATION READY!
echo =========================================================
echo.
echo 🌐 TEST THESE URLs IN YOUR BROWSER:
echo.
echo   Connection Test:  http://localhost:%FOUND_PORT%/Test
echo   Debug Info:       http://localhost:%FOUND_PORT%/Debug  
echo   Main Application: http://localhost:%FOUND_PORT%/?subdomain=ornek
echo.
echo 🔑 Default login credentials:
echo   Email: admin@ornek.com
echo   Password: admin123
echo.
echo 🛠️  TROUBLESHOOTING:
echo   - If still connection refused: Run as Administrator
echo   - Check Windows Firewall/Antivirus settings
echo   - Ensure port %FOUND_PORT% is not blocked
echo.
echo ⏹️  Press Ctrl+C to stop the server
echo =========================================================
echo.

REM Open browser and start server
echo [4/4] Opening browser and starting server...
timeout /t 2 /nobreak >nul
start http://localhost:%FOUND_PORT%/Test

"C:\Program Files\IIS Express\iisexpress.exe" /path:"%~dp0" /port:%FOUND_PORT% /systray:false

echo.
echo Server stopped.
pause