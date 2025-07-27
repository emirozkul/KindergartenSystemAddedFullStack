@echo off
color 0A
echo ============================================
echo    Kindergarten Management System
echo ============================================
echo.
echo Building project...
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" KindergartenSystem.sln /p:Configuration=Debug /p:Platform="Any CPU" /t:Clean,Build

if %errorlevel% neq 0 (
    echo.
    echo *** BUILD FAILED! ***
    echo Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo *** BUILD SUCCESSFUL! ***
echo.

REM Try different ports
set PORT=3000
echo Testing port %PORT%...

echo.
echo Starting IIS Express on port %PORT%...
echo.
echo ============================================
echo    APPLICATION READY!
echo ============================================
echo.
echo Test URLs:
echo   ^> http://localhost:%PORT%/Test (Connection Test)
echo   ^> http://localhost:%PORT%/Debug (Debug Info)
echo   ^> http://localhost:%PORT%/?subdomain=ornek (Main App)
echo.
echo Default admin login:
echo   Email: admin@ornek.com
echo   Password: admin123
echo.
echo TROUBLESHOOTING:
echo   - If connection refused: try running as Administrator
echo   - If port busy: we'll try port 3001, 3002, etc.
echo   - Check Windows Firewall settings
echo.
echo Press Ctrl+C to stop the server
echo ============================================
echo.

start http://localhost:%PORT%/Test
"C:\Program Files\IIS Express\iisexpress.exe" /path:"%~dp0" /port:%PORT% /systray:false

if %errorlevel% neq 0 (
    echo.
    echo Port %PORT% failed, trying 3001...
    set PORT=3001
    start http://localhost:%PORT%/Test
    "C:\Program Files\IIS Express\iisexpress.exe" /path:"%~dp0" /port:%PORT% /systray:false
)

pause