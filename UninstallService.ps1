# Run this script as administrator to uninstall the Windows Service

# Temporarily set the execution policy to Bypass for this session
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Your script commands go here
Write-Host "Execution policy set to Bypass for this session."

$serviceName = "MinimalApiWindowsService"

# Check if service exists
$serviceExists = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($serviceExists) {
    Write-Host "Stopping $serviceName service..."
    Stop-Service -Name $serviceName -Force
    Start-Sleep -Seconds 2
    
    Write-Host "Removing $serviceName service..."
    sc.exe delete $serviceName
    
    if ($?) {
        Write-Host "Service removed successfully!"
    } else {
        Write-Host "Failed to remove service. Make sure you're running this script as Administrator."
    }
} else {
    Write-Host "Service $serviceName does not exist."
}