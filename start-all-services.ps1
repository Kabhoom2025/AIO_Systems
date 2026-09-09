<#
.SYNOPSIS
    Starts the ApiGateway and every backend service it proxies to, then verifies each is
    actually listening on its expected port before reporting ready.

.DESCRIPTION
    The ApiGateway's cluster destinations in ApiGateway/appsettings.json are hardcoded to
    fixed localhost ports. If any one backend isn't running yet, requests routed through
    that cluster return 502 Bad Gateway even though the gateway itself is healthy. Starting
    each service by hand and guessing whether it's ready is the actual root cause of the
    recurring 502s - this script starts them all and waits for confirmation instead.

    Logs for each service are written to the script's own "logs" subfolder so failures can
    be diagnosed without re-running anything.

.PARAMETER SkipGateway
    Start only the backend services, not the ApiGateway itself.

.EXAMPLE
    .\start-all-services.ps1
#>
param(
    [switch]$SkipGateway
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$logDir = Join-Path $root 'logs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# name, project directory (relative to repo root), port
$services = @(
    @{ Name = 'AIO_Systems (SuperAdmin)'; Dir = 'AIO_Systems';                                         Port = 5284 },
    @{ Name = 'FoodOrder.API';            Dir = 'FoodOrderingSystemManagement\src\FoodOrder.API';       Port = 5275 },
    @{ Name = 'Pharmacy.API';             Dir = 'PharmacyManagement\Pharmacy.API';                      Port = 5002 },
    @{ Name = 'HRMS.API';                 Dir = 'HRMSManagement\HRMS.API';                              Port = 5003 },
    @{ Name = 'NovaERP.API';              Dir = 'NovaERP\NovaERP.API';                                  Port = 5004 },
    @{ Name = 'Workflow.API';             Dir = 'WorkflowBuilder\Workflow.API';                         Port = 5005 }
)

if (-not $SkipGateway) {
    $services += @{ Name = 'ApiGateway'; Dir = 'ApiGateway\ApiGateway'; Port = 5000 }
}

function Test-PortOpen {
    param([int]$Port)
    try {
        $conn = New-Object System.Net.Sockets.TcpClient
        $result = $conn.BeginConnect('127.0.0.1', $Port, $null, $null)
        $success = $result.AsyncWaitHandle.WaitOne(300)
        if ($success -and $conn.Connected) { $conn.Close(); return $true }
        $conn.Close()
        return $false
    } catch { return $false }
}

Write-Host "Starting $($services.Count) service(s)...`n" -ForegroundColor Cyan

foreach ($svc in $services) {
    if (Test-PortOpen -Port $svc.Port) {
        Write-Host "  [skip]    $($svc.Name) already listening on port $($svc.Port)" -ForegroundColor Yellow
        continue
    }

    $projectPath = Join-Path $root $svc.Dir
    $logFile = Join-Path $logDir ("{0}.log" -f ($svc.Name -replace '[^\w.-]', '_'))

    Write-Host "  [start]   $($svc.Name) -> http://localhost:$($svc.Port)"

    Start-Process -FilePath 'dotnet' `
        -ArgumentList @('run', '--urls', "http://localhost:$($svc.Port)") `
        -WorkingDirectory $projectPath `
        -RedirectStandardOutput $logFile `
        -RedirectStandardError "$logFile.err" `
        -WindowStyle Hidden
}

Write-Host "`nWaiting for all services to come up (up to 60s each)...`n" -ForegroundColor Cyan

$allUp = $true
foreach ($svc in $services) {
    $waited = 0
    $timeoutSeconds = 60
    while (-not (Test-PortOpen -Port $svc.Port) -and $waited -lt $timeoutSeconds) {
        Start-Sleep -Seconds 2
        $waited += 2
    }

    if (Test-PortOpen -Port $svc.Port) {
        Write-Host "  [ready]   $($svc.Name) (port $($svc.Port)) - up after ${waited}s" -ForegroundColor Green
    } else {
        $allUp = $false
        $logFile = Join-Path $logDir ("{0}.log" -f ($svc.Name -replace '[^\w.-]', '_'))
        Write-Host "  [FAILED]  $($svc.Name) (port $($svc.Port)) did not come up within ${timeoutSeconds}s - check $logFile" -ForegroundColor Red
    }
}

Write-Host ""
if ($allUp) {
    Write-Host "All services are up. Gateway: http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "One or more services failed to start - see logs above before using the gateway." -ForegroundColor Red
    exit 1
}
