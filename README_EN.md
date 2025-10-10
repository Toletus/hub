# Toletus.Hub

ASP.NET Core API (net9.0) for managing devices and basic commands, including network device discovery, connection/disconnection, and execution of common commands.

## Technologies
- .NET 9
- ASP.NET Core (Web API)
- C# 13
- MVC / Controllers

## Running the project
1. Requirements:
    - .NET SDK 9 installed
2. Restore and run:
    - `dotnet restore`
    - `dotnet run`

## Main Endpoints

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

## Postman Collection
For easier testing, use the following collection Language(Brazilian Portuguese):  
https://documenter.getpostman.com/view/45933287/2sB34bLPRv#intro

## Structure (high-level)
- **Controllers:** API endpoints.
- **Services:** Business logic and network/device integration.
- **Models:** Data contracts (e.g., `Device`, `DeviceType`, device responses).

## License
Define the project license here MIT.
