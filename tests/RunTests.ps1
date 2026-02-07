C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /out:"c:\Users\MASTER\Desktop\PhoenixBOT\tests\PiBotTestSuite.exe" "c:\Users\MASTER\Desktop\PhoenixBOT\tests\PiBotTestSuite.cs"
if ($LASTEXITCODE -eq 0) {
    c:\Users\MASTER\Desktop\PhoenixBOT\tests\PiBotTestSuite.exe
} else {
    Write-Host "Test compilation failed." -ForegroundColor Red
    exit 1
}
