# TrustRecruitment Starter Template

This starter template matches your diagram with a **3-layer architecture**:

- **TrustRecruitment.Web** → Presentation layer (MVC + API)
- **TrustRecruitment.Business** → Business layer (services + rules)
- **TrustRecruitment.Data** → Data layer (entities + repositories + storage)

## What is included

- ASP.NET Core MVC web app
- API endpoints for jobs, applications, health, and version
- In-memory repositories for local development
- Local file storage service for CV uploads
- Starter admin and candidate flows
- Dockerfile and sample GitHub Actions workflow
- Step-by-step delivery plan that maps to your architecture diagram

## Solution structure

```text
TrustRecruitment.sln
src/
  TrustRecruitment.Web
  TrustRecruitment.Business
  TrustRecruitment.Data
```

## Step-by-step plan that matches your exact diagram

### Phase 1 — Local starter app
Goal: get a working MVC + API app running fast.

Build:
- Home page
- Job listing page
- Job details page
- Apply form
- Admin dashboard
- Public API:
  - `GET /api/jobs`
  - `GET /api/jobs/{id}`
  - `GET /api/health`
  - `GET /api/version`

Use:
- In-memory repositories
- Local file upload folder
- Console logging

### Phase 2 — Identity and roles
Goal: match the sign-in boxes in your diagram.

Add:
- ASP.NET Core Identity
- SQLite for identity data
- Roles: `Admin`, `Candidate`
- Seed a default admin user
- Google OAuth login

### Phase 3 — Business rules
Goal: match the candidate/admin flows.

Add:
- One application per job limit
- Candidate can only see their own applications
- Admin can create, edit, delete jobs
- Admin can list all applications
- Admin can download CV files

### Phase 4 — External integrations
Goal: match the service boxes in your diagram.

Add:
- REST Countries API service
- Blob Storage for CV uploads
- Key Vault for secrets
- Application Insights for telemetry

### Phase 5 — Data platform
Goal: match the database boxes in your diagram.

Replace the in-memory repositories with:
- Cosmos DB repositories for jobs
- Cosmos DB repositories for applications

Keep:
- SQLite only for Identity

### Phase 6 — Containers and deployment
Goal: match your CI/CD and Azure runtime flow.

Add:
- Dockerfile
- docker-compose for local multi-service testing
- GitHub Actions build and publish
- Azure Container Registry push
- Azure Web App or Container Apps deployment

## How the layers should depend on each other

```text
TrustRecruitment.Web -> TrustRecruitment.Business -> TrustRecruitment.Data
```

Do not reverse those dependencies.

## Commands to create this structure on your machine

```bash
dotnet new sln -n TrustRecruitment

mkdir src
cd src

dotnet new mvc -n TrustRecruitment.Web
dotnet new classlib -n TrustRecruitment.Business
dotnet new classlib -n TrustRecruitment.Data

cd ..

dotnet sln add src/TrustRecruitment.Web/TrustRecruitment.Web.csproj
dotnet sln add src/TrustRecruitment.Business/TrustRecruitment.Business.csproj
dotnet sln add src/TrustRecruitment.Data/TrustRecruitment.Data.csproj

dotnet add src/TrustRecruitment.Web/TrustRecruitment.Web.csproj reference src/TrustRecruitment.Business/TrustRecruitment.Business.csproj
dotnet add src/TrustRecruitment.Business/TrustRecruitment.Business.csproj reference src/TrustRecruitment.Data/TrustRecruitment.Data.csproj
```

## Package suggestions for next steps

```bash
dotnet add src/TrustRecruitment.Web package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/TrustRecruitment.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/TrustRecruitment.Web package Microsoft.EntityFrameworkCore.Tools
dotnet add src/TrustRecruitment.Web package Microsoft.AspNetCore.Authentication.Google
dotnet add src/TrustRecruitment.Web package Azure.Storage.Blobs
dotnet add src/TrustRecruitment.Web package Azure.Identity
dotnet add src/TrustRecruitment.Web package Azure.Security.KeyVault.Secrets
dotnet add src/TrustRecruitment.Web package Microsoft.ApplicationInsights.AspNetCore
dotnet add src/TrustRecruitment.Data package MongoDB.Driver
```

## Notes

This scaffold is intentionally lightweight so you can open it in VS Code and extend it in stages. I could not compile it in this environment because the .NET SDK is not available here, so treat it as a starter template and run `dotnet restore` and `dotnet run` on your Windows machine.


## Navbar update

This package hides the **Admin Portal** link from public users. The admin page route still exists at `/Admin`, but it is no longer shown in the top navigation until real authentication and role checks are added.
