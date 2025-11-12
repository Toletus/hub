# Toletus.Hub

API ASP.NET Core (net9.0) para gerenciamento de dispositivos e comandos básicos, incluindo descoberta de dispositivos na rede, conexão/desconexão e execução de comandos comuns.

## Tecnologias
- .NET 9
- ASP.NET Core (Web API)
- C# 13
- MVC / Controllers

## Executando o projeto
1. Requisitos:
    - .NET SDK 9 instalado
2. Restaurar e executar:
    - `dotnet restore`
    - `dotnet run`

## Endpoints principais

- DeviceConnectionController
    - GET `/DeviceConnection/GetNetworks` — Lista nomes de redes disponíveis.
    - GET `/DeviceConnection/GetDefaultNetworkName` — Retorna a rede padrão.
    - GET `/DeviceConnection/DiscoverDevices?network={opcional}` — Descobre dispositivos na rede.
    - GET `/DeviceConnection/GetDevices?network={opcional}` — Lista dispositivos conhecidos.
    - POST `/DeviceConnection/Connect?ip={ip}&type={DeviceType}&network={opcional}` — Conecta a um dispositivo.
    - POST `/DeviceConnection/Disconnect?ip={ip}&type={DeviceType}` — Desconecta de um dispositivo.

- BasicCommonCommandsController
    - POST `/BasicCommonCommands/ReleaseEntry` — Libera entrada.
        - Body/query: `Device device`, `string message`
    - POST `/BasicCommonCommands/ReleaseEntryAndExit` — Libera entrada e saída.
        - Body/query: `Device device`, `string message`
    - POST `/BasicCommonCommands/ReleaseExit` — Libera saída.
        - Body/query: `Device device`, `string message`

- WebhookController
    - POST `/Webhook/SetEndpoint?endpoint={url}` — Define o endpoint de webhook para callbacks.

Observações:
- Parâmetros complexos como `Device` podem ser enviados via JSON (body) conforme o modelo da aplicação.
- `DeviceType` é um enum esperado pelos endpoints de conexão.

## Coleção Postman
Para facilitar os testes, utilize a coleção:
https://documenter.getpostman.com/view/45933287/2sB34bLPRv#intro

## Estrutura (alto nível)
- Controllers: endpoints da API.
- Services: regras de negócio e integração com rede/dispositivos.
- Models: contratos de dados (por exemplo, `Device`, `DeviceType`, respostas de dispositivo).

## Licença
Defina aqui a licença do projeto MIT.
