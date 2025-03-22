# Run this script as administrator to install the Windows Service

# Temporarily set the execution policy to Bypass for this session
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Your script commands go here
Write-Host "Execution policy set to Bypass for this session."

$serviceName = "MinimalApiWindowsService"
$displayName = "Minimal API Windows Service"
$description = "Windows Service hosting a .NET Minimal API"
$binPath = "`"$PSScriptRoot\src\bin\Release\net8.0\MinimalApiWindowsService.exe`""

# Check if service exists
$serviceExists = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($serviceExists) {
    Write-Host "Service already exists. Stopping and removing it first..."
    Stop-Service -Name $serviceName -Force
    sc.exe delete $serviceName
    Start-Sleep -Seconds 2
}

Write-Host "Installing service: $serviceName..."
sc.exe create $serviceName binPath= $binPath start= auto displayname= $displayName

if ($?) {
    Write-Host "Setting service description..."
    sc.exe description $serviceName $description
    
    Write-Host "Starting service..."
    Start-Service -Name $serviceName
    
    Write-Host "Service installed and started successfully!"
    Write-Host "The API is now accessible at: http://localhost:5000"
} else {
    Write-Host "Failed to install service. Make sure you're running this script as Administrator."
}