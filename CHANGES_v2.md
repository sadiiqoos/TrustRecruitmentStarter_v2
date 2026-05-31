# Ändringar Inlämningsuppgift 2

## Delmoment 1 — Strukturerad loggning
- `AccountController.cs`: Bytt `Console.WriteLine` → `ILogger<AccountController>`. Loggar login/signup/logout på Information/Warning-nivå. Loggar INTE lösenord eller fullständig PII.
- `JobService.cs`: Lagt till `ILogger<JobService>` med loggning i CreateAsync, UpdateAsync, DeleteAsync.
- `ApplicationService.cs`: Lagt till `ILogger<ApplicationService>` med loggning i båda CreateAsync-metoderna.
- `Web.csproj`: Lagt till `Microsoft.ApplicationInsights.AspNetCore`.
- `Program.cs`: Registrerar `AddApplicationInsightsTelemetry()`.

## Delmoment 2 — REST API med Swagger och API-nyckel
- `Middleware/ApiKeyMiddleware.cs`: Ny fil. Skyddar alla /api-routes (utom /api/health) med X-Api-Key header.
- `Web.csproj`: Lagt till `Swashbuckle.AspNetCore`.
- `Program.cs`: Registrerar Swagger med API-nyckel som säkerhetsdefinition. Swagger UI tillgängligt på /swagger.
- `JobsApiController.cs`: Uppdaterad med XML-docs, ILogger, och ProducesResponseType-attribut.
- `ApplicationsApiController.cs`: Uppdaterad med ny endpoint /api/applications/by-job/{jobId}.
- `HealthApiController.cs`: Uppdaterad med info om /healthz.

## Delmoment 3 — Blob Storage och Health Probe
- `Storage/Blob/BlobFileStorageRepository.cs`: Ny fil. Implementerar IFileStorageRepository mot Azure Blob Storage med DefaultAzureCredential (Managed Identity).
- `Data.csproj`: Lagt till `Azure.Storage.Blobs` och `Azure.Identity`.
- `Program.cs`: Registrerar BlobServiceClient med DefaultAzureCredential när BlobStorage:AccountName är satt. Faller tillbaka på LocalFileStorageRepository lokalt.
- `Program.cs`: Registrerar AddHealthChecks() med MongoDB och Blob Storage. Exponerar /healthz med UIResponseWriter.
- `infra/setup.sh`: Utökat med Storage Account, blob-container, Managed Identity-rolltilldelning och readiness probe-konfiguration.

## API-nyckel i produktion
Nyckeln ska INTE ligga i appsettings.json. Sätt den som Container App secret:
```bash
az containerapp secret set --name trustrecruitment-app --resource-group trustrecruitment-rg \
  --secrets "api-key=<DIN_HEMLIGA_NYCKEL>"
az containerapp update --name trustrecruitment-app --resource-group trustrecruitment-rg \
  --set-env-vars "ApiKey=secretref:api-key"
```
