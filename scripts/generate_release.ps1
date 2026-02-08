
# 🛠️ PiBot Pro - Automated Release & Packaging Script
# Version: 0.1 Alpha
# URL: https://pibot.club/

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path "$ScriptDir\.."
Set-Location $ProjectRoot

$ReleaseDir = "$ProjectRoot\Release\PiBot_v0.1_Alpha"
$FinalZip = "$ProjectRoot\PiBot_v0.1_Alpha_Public.zip"
$CSCPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

Write-Host "🚀 Starting PiBot Pro Build Process..." -ForegroundColor Cyan

# 1. Clean previous builds
if (Test-Path $ReleaseDir) { Remove-Item -Recurse -Force $ReleaseDir }
if (Test-Path $FinalZip) { Remove-Item -Force $FinalZip }
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null
New-Item -ItemType Directory -Path "$ReleaseDir\assets" -Force | Out-Null
New-Item -ItemType Directory -Path "$ReleaseDir\Data" -Force | Out-Null
New-Item -ItemType Directory -Path "$ReleaseDir\web" -Force | Out-Null
New-Item -ItemType Directory -Path "$ReleaseDir\docs" -Force | Out-Null

# 2. Compile Binaries
Write-Host "📦 Compiling Native Windows Core..." -ForegroundColor Yellow

# WPF Assemblies explicitly located in GAC to avoid Ref Assembly missing on some machines
$Refs = @(
    "System.Windows.Forms.dll",
    "System.Drawing.dll",
    "System.Web.Extensions.dll",
    "C:\Windows\Microsoft.NET\assembly\GAC_64\PresentationCore\v4.0_4.0.0.0__31bf3856ad364e35\PresentationCore.dll",
    "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\PresentationFramework\v4.0_4.0.0.0__31bf3856ad364e35\PresentationFramework.dll",
    "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\WindowsBase\v4.0_4.0.0.0__31bf3856ad364e35\WindowsBase.dll",
    "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Xaml\v4.0_4.0.0.0__b77a5c561934e089\System.Xaml.dll"
)
$RefArgs = $Refs -join ","

# Main Control Center
& $CSCPath /target:winexe /out:"$ReleaseDir\PiBotControlCenter.exe" /r:$RefArgs .\src\PiBotControlCenter.cs
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to compile Control Center"; exit }

# Tray App
& $CSCPath /target:winexe /out:"$ReleaseDir\PiBotTray.exe" /r:System.Windows.Forms.dll, System.Drawing.dll .\src\PiBotTray.cs
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to compile Tray App"; exit }

# Installer
& $CSCPath /target:winexe /out:"$ReleaseDir\PiBotInstaller.exe" /r:System.Windows.Forms.dll, System.Drawing.dll .\src\PiBotInstaller.cs
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to compile Installer"; exit }

# Verification
$Exes = Get-ChildItem -Path $ReleaseDir -Filter *.exe
if ($Exes.Count -lt 3) { Write-Error "Missing expected binaries in release folder!"; exit }

# 3. Mirroring Assets & Documentation
Write-Host "🚚 Mirroring AI DNA and UI Assets..." -ForegroundColor Yellow
Copy-Item ".\assets\*" "$ReleaseDir\assets\" -Recurse -ErrorAction SilentlyContinue
Copy-Item ".\Data\cloud-init.yaml" "$ReleaseDir\Data\"
Copy-Item ".\Data\pibot_names_expanded.csv" "$ReleaseDir\Data\"
Copy-Item ".\web\*" "$ReleaseDir\web\" -Recurse
Copy-Item ".\docs\*" "$ReleaseDir\docs\" -Recurse
Copy-Item ".\README.md" "$ReleaseDir\"

# 4. Final Cleanup (Remove root clutter)
Write-Host "🧹 Cleaning up workspace..." -ForegroundColor Yellow
$Clutter = @("PiBotControlCenter.exe", "PiBotInstaller.exe", "PiBotTray.exe", "PiBotWebBackend.exe")
foreach ($f in $Clutter) { if (Test-Path ".\$f") { Remove-Item ".\$f" -Force } }

# 5. Compress for Distribution
Write-Host "📦 Creating final ZIP archive..." -ForegroundColor Green
Compress-Archive -Path "$ReleaseDir\*" -DestinationPath $FinalZip -Force

Write-Host "`n✅ RELEASE READY!" -ForegroundColor Green
Write-Host "Final Package: $FinalZip" -ForegroundColor White
Write-Host "All binaries organized in $ReleaseDir`n" -ForegroundColor Cyan
