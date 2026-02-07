@echo off
set "MP_EXE=C:\Program Files\Multipass\bin\multipass.exe"
set "VM_NAME=moltbolt-env"
set "INSTALLER=install-moltbolt-env.sh"

echo 🚀 MoltBolt Windows Launcher (Batch Mode)

if not exist "%MP_EXE%" (
    echo ❌ Multipass not found at default location.
    echo Please install it first: winget install Canonical.Multipass
    pause
    exit /b
)

echo 🏗️  Checking VM status...
"%MP_EXE%" info %VM_NAME% >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo 📦 Creating new VM...
    "%MP_EXE%" launch --name %VM_NAME% --cpus 2 --memory 2G --disk 10G 22.04
    if %ERRORLEVEL% NEQ 0 ( echo ❌ Launch failed. && pause && exit /b )
    
    echo 📂 Transferring installer...
    "%MP_EXE%" transfer %INSTALLER% %VM_NAME%:/tmp/install.sh
    
    echo ⚙️  Running installation script...
    "%MP_EXE%" exec %VM_NAME% -- sudo bash /tmp/install.sh
) else (
    echo ✅ VM exists. Starting...
    "%MP_EXE%" start %VM_NAME%
)

echo 🛰️  Getting IP address...
for /f "tokens=*" %%a in ('"%MP_EXE%" info %VM_NAME% --format csv ^| findstr "ipv4"') do set VM_IP=%%a
:: Extraction of IP from CSV format can be tricky in batch, let's use a simpler way if info -ipv4 fails
for /f "usebackq tokens=2 delims=," %%a in (`"%MP_EXE%" info %VM_NAME% --format csv ^| findstr /r "[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*"`) do set VM_IP=%%a

echo ✅ MoltBolt is READY at http://%VM_IP%:6080
start http://%VM_IP%:6080
pause
