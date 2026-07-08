# Toletus.Hub

Solution for discovering, connecting to, and operating Toletus devices (LiteNet1, LiteNet2, LiteNet3, and the SM25 reader) on the local network. It includes an ASP.NET Core API and a desktop manager application.

> Portuguese version: [README.md](README.md)

## Projects
- **Toletus.Hub** — core library: device discovery, connection/disconnection, commands, and notifications.
- **Toletus.Hub.API** — ASP.NET Core API (net9.0) that exposes the core library over HTTP.
- **Toletus.Hub.Manager.UI** — Blazor user interface (Razor Class Library) shared by the manager.
- **Toletus.Hub.Manager.Maui** — manager application that hosts the interface and communicates directly with the core library.

## Supported devices
LiteNet1, LiteNet2, LiteNet3, and the SM25 reader.

## Requirements
- .NET SDK 9
- To build the MAUI manager: the MAUI workload (`dotnet workload install maui`)

## Technologies
- .NET 9 / C# 13
- ASP.NET Core (Web API)
- .NET MAUI + Blazor Hybrid

---

## API (Toletus.Hub.API)

### Running
```
dotnet run --project src/Toletus.Hub.API
```
The interactive documentation (Scalar/OpenAPI) is available at the application root.

### Main endpoints

- **DeviceConnectionController**
    - **GET** `/DeviceConnection/GetNetworks` — Lists available network names.
    - **GET** `/DeviceConnection/GetDefaultNetworkName` — Returns the default network.
    - **GET** `/DeviceConnection/DiscoverDevices?network={optional}` — Discovers devices on the network.
    - **GET** `/DeviceConnection/GetDevices?network={optional}` — Lists known devices.
    - **POST** `/DeviceConnection/Connect?ip={ip}&type={DeviceType}&network={optional}` — Connects to a device.
    - **POST** `/DeviceConnection/Disconnect?ip={ip}&type={DeviceType}` — Disconnects from a device.

- **BasicCommonCommandsController**
    - **POST** `/BasicCommonCommands/ReleaseEntry` — Releases entry access.
        - Body/query: `Device device`, `string message`
    - **POST** `/BasicCommonCommands/ReleaseEntryAndExit` — Releases both entry and exit access.
        - Body/query: `Device device`, `string message`
    - **POST** `/BasicCommonCommands/ReleaseExit` — Releases exit access.
        - Body/query: `Device device`, `string message`

- **WebhookController**
    - **POST** `/Webhook/SetEndpoint?endpoint={url}` — Defines the webhook endpoint for callbacks.

Notes:
- Complex parameters like `Device` can be sent via JSON (body) according to the application's model.
- `DeviceType` is an enum expected by the connection endpoints.

### Postman collection
For easier testing, use the following collection (documentation in Brazilian Portuguese):
https://documenter.getpostman.com/view/45933287/2sB34bLPRv#intro

---

## MAUI Manager (Toletus.Hub.Manager.Maui)

Desktop application for operating the devices through a graphical interface. It communicates directly with the `Toletus.Hub` library, so it **does not require the API to be running**.

### Features
- Device discovery on the network and selection of the working network.
- Connection and disconnection over the network or a serial port.
- Reading and sending configurations per module.
- Execution of common and family-specific commands (LiteNet1, LiteNet2, LiteNet3, and SM25).
- Activity timeline and notification history.
- Localization in Portuguese (pt-BR) and English (en-US), with theme switching.

### Running in development (Windows)
```
dotnet run --project src/Toletus.Hub.Manager.Maui -f net9.0-windows10.0.19041.0
```

### Building the installer (MSIX)
Packages a signed installer for distribution:
```
dotnet publish src/Toletus.Hub.Manager.Maui -f net9.0-windows10.0.19041.0 -c Release -r win-x64 -p:UseMonoRuntime=false -p:WindowsPackageType=MSIX -p:GenerateAppxPackageOnBuild=true -p:PackageCertificateThumbprint={thumbprint}
```
`{thumbprint}` is the identifier of a signing certificate installed on the machine. The package is generated under `.../win-x64/AppPackages/`. To install it on another machine, trust the certificate (`.cer`) and run the `.msix`.
