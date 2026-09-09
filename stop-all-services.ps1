<#
.SYNOPSIS
    Stops every service started by start-all-services.ps1, by finding and killing whatever
    process is listening on each of its known ports.

.EXAMPLE
    .\stop-all-services.ps1
#>

$ports = @(5284, 5275, 5002, 5003, 5004, 5005, 5000)

foreach ($port in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $conns) {
        Write-Host "  [skip]  nothing listening on port $port"
        continue
    }
    foreach ($conn in $conns) {
        $procId = $conn.OwningProcess
        try {
            $proc = Get-Process -Id $procId -ErrorAction Stop
            Write-Host "  [stop]  port $port -> $($proc.ProcessName) (PID $procId)" -ForegroundColor Yellow
            Stop-Process -Id $procId -Force -Confirm:$false
        } catch {
            Write-Host "  [warn]  could not stop PID $procId on port $port : $_" -ForegroundColor Red
        }
    }
}

Write-Host "`nDone." -ForegroundColor Green
