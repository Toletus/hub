# Toletus.Hub

Solução para descoberta, conexão e operação de dispositivos Toletus (LiteNet1, LiteNet2, LiteNet3 e leitor SM25) na rede local. Inclui uma API ASP.NET Core e um aplicativo gerenciador desktop.

> Versão em inglês: [README_EN.md](README_EN.md)

## Projetos
- **Toletus.Hub** — biblioteca central: descoberta de dispositivos, conexão/desconexão, comandos e notificações.
- **Toletus.Hub.API** — API ASP.NET Core (net9.0) que expõe a biblioteca central via HTTP.
- **Toletus.Hub.Manager.UI** — interface em Blazor (Razor Class Library) compartilhada pelo gerenciador.
- **Toletus.Hub.Manager.Maui** — aplicativo gerenciador que hospeda a interface e se comunica diretamente com a biblioteca central.

## Dispositivos suportados
LiteNet1, LiteNet2, LiteNet3 e leitor SM25.

## Requisitos
- .NET SDK 9
- Para compilar o gerenciador MAUI: workload MAUI (`dotnet workload install maui`)

## Tecnologias
- .NET 9 / C# 13
- ASP.NET Core (Web API)
- .NET MAUI + Blazor Hybrid

---

## API (Toletus.Hub.API)

### Executando
```
dotnet run --project src/Toletus.Hub.API
```
A documentação interativa (Scalar/OpenAPI) fica disponível na raiz da aplicação.

### Endpoints principais

- DeviceConnectionController
    - GET `/DeviceConnection/GetNetworks` — Lista os nomes de redes disponíveis.
    - GET `/DeviceConnection/GetDefaultNetworkName` — Retorna a rede padrão.
    - GET `/DeviceConnection/DiscoverDevices?network={opcional}` — Descobre dispositivos na rede.
    - GET `/DeviceConnection/GetDevices?network={opcional}` — Lista dispositivos conhecidos.
    - POST `/DeviceConnection/Connect?ip={ip}&type={DeviceType}&network={opcional}` — Conecta a um dispositivo.
    - POST `/DeviceConnection/Disconnect?ip={ip}&type={DeviceType}` — Desconecta de um dispositivo.

- BasicCommonCommandsController
    - POST `/BasicCommonCommands/ReleaseEntry` — Libera a entrada.
        - Body/query: `Device device`, `string message`
    - POST `/BasicCommonCommands/ReleaseEntryAndExit` — Libera a entrada e a saída.
        - Body/query: `Device device`, `string message`
    - POST `/BasicCommonCommands/ReleaseExit` — Libera a saída.
        - Body/query: `Device device`, `string message`

- WebhookController
    - POST `/Webhook/SetEndpoint?endpoint={url}` — Define o endpoint de webhook para os callbacks.

Observações:
- Parâmetros complexos como `Device` podem ser enviados via JSON (body) conforme o modelo da aplicação.
- `DeviceType` é um enum esperado pelos endpoints de conexão.

### Coleção Postman
Para facilitar os testes, utilize a coleção:
https://documenter.getpostman.com/view/45933287/2sB34bLPRv#intro

---

## Gerenciador MAUI (Toletus.Hub.Manager.Maui)

Aplicativo desktop para operar os dispositivos por uma interface gráfica. Ele se comunica diretamente com a biblioteca `Toletus.Hub`, portanto **não depende da API em execução**.

### Recursos
- Descoberta de dispositivos na rede e seleção da rede de trabalho.
- Conexão e desconexão via rede ou porta serial.
- Leitura e envio de configurações por módulo.
- Execução de comandos comuns e específicos de cada família (LiteNet1, LiteNet2, LiteNet3 e SM25).
- Linha do tempo de atividades e histórico de notificações.
- Localização em Português (pt-BR) e Inglês (en-US), com alternância de tema.

### Executando em desenvolvimento (Windows)
```
dotnet run --project src/Toletus.Hub.Manager.Maui -f net9.0-windows10.0.19041.0
```

### Gerando o instalador (MSIX)
Empacota um instalável assinado para distribuição:
```
dotnet publish src/Toletus.Hub.Manager.Maui -f net9.0-windows10.0.19041.0 -c Release -r win-x64 -p:UseMonoRuntime=false -p:WindowsPackageType=MSIX -p:GenerateAppxPackageOnBuild=true -p:PackageCertificateThumbprint={thumbprint}
```
`{thumbprint}` é o identificador de um certificado de assinatura instalado na máquina. O pacote é gerado em `.../win-x64/AppPackages/`. Para instalar em outra máquina, confie no certificado (`.cer`) e execute o `.msix`.

---

## Suporte ao integrador

- Telefone/WhatsApp: +55 62 99342-7398
- E-mail: assistencia@toletus.com
