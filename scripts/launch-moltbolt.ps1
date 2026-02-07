# ==============================================================================
# MoltBolt Windows Launcher
# Automates the creation of the MoltBolt Linux Environment via Multipass.
# ==============================================================================

$VM_NAME = "moltbolt-env"
$CPU = 2
$MEM = "2G"
$DISK = "10G"

Write-Host "🚀 MoltBolt Windows Launcher Initialized" -ForegroundColor Cyan

# 1. Check for Multipass
$MP_EXE = "multipass"
if (!(Get-Command $MP_EXE -ErrorAction SilentlyContinue)) {
    $FALLBACK = "C:\Program Files\Multipass\bin\multipass.exe"
    if (Test-Path $FALLBACK) {
        $MP_EXE = $FALLBACK
        Write-Host "ℹ️  Using Multipass from fallback path." -ForegroundColor Cyan
    }
    else {
        Write-Host "❌ Multipass is NOT installed." -ForegroundColor Red
        Write-Host "Please install Multipass from: https://multipass.run/install" -ForegroundColor Yellow
        exit
    }
}

# Define a function to call multipass easily
function mp { & $MP_EXE @args }

# 2. Check if VM already exists
$VM_STATUS = mp info $VM_NAME --format json | ConvertFrom-Json -ErrorAction SilentlyContinue
if ($VM_STATUS) {
    Write-Host "✅ VM '$VM_NAME' already exists. Starting it..." -ForegroundColor Green
    mp start $VM_NAME
}
else {
    Write-Host "🏗️  Creating new MoltBolt Environment VM..." -ForegroundColor Cyan
    mp launch --name $VM_NAME --cpus $CPU --memory $MEM --disk $DISK 22.04
    
    Write-Host "📦 Injecting and running installer..." -ForegroundColor Gray
    mp transfer ./install-moltbolt-env.sh ($VM_NAME + ":/tmp/install.sh")
    mp exec $VM_NAME -- sudo bash /tmp/install.sh
}

# 3. Get IP and Open Browser
$VM_IP = (mp info $VM_NAME --format json | ConvertFrom-Json).info.$VM_NAME.ipv4[0]
$URL = "http://$($VM_IP):6080"

Write-Host "-------------------------------------------------------" -ForegroundColor White
Write-Host "✅ MoltBolt Environment is RUNNING at: $URL" -ForegroundColor Green
Write-Host "-------------------------------------------------------" -ForegroundColor White

# Open Browser
Start-Process $URL
