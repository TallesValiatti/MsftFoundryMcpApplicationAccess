# Books MCP Server with Microsoft Entra ID Protection for Microsoft Foundry
A **.NET 10** **Model Context Protocol (MCP)** server exposing book CRUD tools, protected by **Microsoft Entra ID** so that only a specific **Microsoft Foundry** project identity can call it.

The project registers a JWT Bearer authentication scheme via `Microsoft.Identity.Web`, validates that incoming tokens are app only tokens issued to the Foundry project (matching tenant and object id), and maps an MCP endpoint that requires that custom authorization policy. Books are persisted with EF Core using the in memory provider, and the app can be deployed to an existing Azure App Service with a bundled shell script.

## What's inside

* `Books.Mcp/Program.cs` builds the web application, wires configuration through `AddConfigurations()`, and starts the host.
* `Books.Mcp/Extensions/ConfigurationExtensions.cs` registers authentication, the `FoundryProjectOnly` authorization policy, EF Core, and the MCP server, then maps `/health` and `/mcp`.
* `Books.Mcp/Tools/BookTools.cs` exposes the MCP tools `create_book`, `get_books`, `get_book_by_id`, `update_book`, and `delete_book`.
* `Books.Mcp/Services/BookService.cs` implements the CRUD operations against the `BooksDbContext`.
* `Books.Mcp/Entities/Book.cs` is the book entity (`Id`, `Name`, `Description`).
* `Books.Mcp/Data/BooksDbContext.cs` configures the EF Core in memory model for `Book`.
* `Books.Mcp/appsettings.json` and `appsettings.Development.json` hold the `AzureAd` and `FoundryCaller` configuration sections.
* `Atlas-Mcp-Auth-Guide.md` documents the authentication pattern this project follows.
* `deploy.sh` publishes and deploys the project to an existing Azure App Service via ZIP deploy.
* `Articles/` contains the technical article written about this project.

## Prerequisites

* .NET SDK 10.0
* An Azure subscription with a Microsoft Entra ID tenant
* An App Registration for the MCP Server, with the Application ID URI exposed as `api://<client-id>`
* A Microsoft Foundry project whose managed identity will be the only allowed caller
* Azure CLI (`az`) and `zip`, required only for `deploy.sh`

## Configure credentials

The server requires the following configuration values, set in `Books.Mcp/appsettings.json`, `Books.Mcp/appsettings.Development.json`, or as environment variables.

`AzureAd__TenantId`: Microsoft Entra ID tenant (directory) id.
`AzureAd__ClientId`: Application (client) id of the MCP Server's App Registration.
`FoundryCaller__ObjectId`: Object id of the Microsoft Foundry project identity allowed to call the MCP endpoint.

On macOS/Linux:

```bash
export AzureAd__TenantId="<tenant-guid>"
export AzureAd__ClientId="<mcp-api-client-id>"
export FoundryCaller__ObjectId="<foundry-project-managed-identity-object-id>"
```

On Windows PowerShell:

```powershell
$env:AzureAd__TenantId = "<tenant-guid>"
$env:AzureAd__ClientId = "<mcp-api-client-id>"
$env:FoundryCaller__ObjectId = "<foundry-project-managed-identity-object-id>"
```

## Run

```bash
cd Books.Mcp
dotnet run
```

The server starts on `http://localhost:5055` (and `https://localhost:7148` for the `https` profile), exposes an anonymous `/health` endpoint, and requires an authorized Microsoft Foundry token for `/mcp`.

## Deploy

```bash
chmod +x deploy.sh
./deploy.sh
```

Edit `SUBSCRIPTION_ID`, `RESOURCE_GROUP`, and `WEB_APP_NAME` in `deploy.sh` before running it. The script logs in via `az login` if needed, publishes `Books.Mcp` in `Release` configuration, packages it into a ZIP, and deploys it to the target App Service with `az webapp deploy`.

## Customizing

Add new MCP tools by creating methods in `Books.Mcp/Tools/BookTools.cs` decorated with `[McpServerTool]`, or register additional tool classes via `.WithTools<T>()` in `ConfigurationExtensions.cs`. To allow more than one caller, replace the single `FoundryCaller:ObjectId` check in `IsFoundryProjectCaller()` with a list of allowed object ids or switch to app role based authorization.

## References

Model Context Protocol tools in Microsoft Foundry: https://learn.microsoft.com/en-us/azure/ai-foundry/agents/how-to/tools/model-context-protocol  
MCP authentication in Microsoft Foundry: https://learn.microsoft.com/en-us/azure/ai-foundry/agents/how-to/mcp-authentication  
Agent identity concepts: https://learn.microsoft.com/en-us/azure/ai-foundry/agents/concepts/agent-identity  
Protect a web API with Microsoft.Identity.Web: https://learn.microsoft.com/en-us/entra/msidweb/getting-started/quickstart-webapi  
Access token claims reference: https://learn.microsoft.com/en-us/entra/identity-platform/access-token-claims-reference  
