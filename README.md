# MinimalApiWindowsService

This project demonstrates how to host a .NET Minimal API within a Windows Service. When the Windows Service starts, it makes the API accessible; when the service stops, the API becomes inaccessible.

## Features

- Windows Service acting as an HTTP server
- Minimal API hosted within the Windows Service
- API automatically starts/stops with the Windows Service
- Built with .NET 8
- Includes Swagger UI for API documentation

## Prerequisites

- Windows OS
- .NET 8 SDK
- Administrator privileges (for service installation)

## Building the Project

```bash
dotnet build --configuration Release
```

## Installing the Windows Service

Run PowerShell as Administrator and execute:

```powershell
cd <project-directory>
.\InstallService.ps1
```

This will:

1. Create a Windows Service named "MinimalApiWindowsService"
2. Start the service
3. Make the API accessible at https://localhost:7145

## Uninstalling the Windows Service

Run PowerShell as Administrator and execute:

```powershell
cd <project-directory>
.\UninstallService.ps1
```

## API Endpoints

Once the service is running, the following endpoints are available:

- `GET /` - Basic greeting message
- `GET /weatherforecast` - Sample weather forecast data
- `GET /health` - Health check endpoint
- `/swagger` - Swagger UI for API documentation

## Manual Service Management

You can also manage the service using standard Windows commands:

```
# Start the service
net start MinimalApiWindowsService

# Stop the service
net stop MinimalApiWindowsService
```

## How It Works

The project uses `IHostedService` to start and stop a web application when the Windows Service starts or stops. The web application hosts the Minimal API, making it accessible while the service is running.

## Troubleshooting

- Ensure you're running installation scripts as Administrator
- Check Windows Event Logs for any service-related errors
- Verify that port 7145 is not in use by another application
