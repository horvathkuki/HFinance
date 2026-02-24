# Running HFinance Locally

This guide covers database creation, backend startup, frontend startup, and common local issues.

## Prerequisites
- .NET SDK 10
- Node.js + npm
- SQL Server Express instance `.\SQLEXPRESS01`
- `dotnet-ef` tool

Install `dotnet-ef` if needed:

```powershell
dotnet tool install --global dotnet-ef
```

## 1) Start and prepare the backend
From repository root:

```powershell
cd d:\Work\HFinance\backend
```

Set development environment and apply migrations (creates/updates DB):

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update
```

Trust HTTPS dev certificate (first time only):

```powershell
dotnet dev-certs https --trust
```

Run backend with HTTPS profile:

```powershell
dotnet run --launch-profile https
```

Backend URLs:
- `https://localhost:7292`
- `http://localhost:5047`

## 2) Start frontend
In a new terminal:

```powershell
cd d:\Work\HFinance\frontend
npm install
npm start
```

Frontend URL:
- `http://localhost:4200`

## 3) API base URL used by frontend
Configured in:
- `frontend/src/app/api.config.ts`

Current value:
- `https://localhost:7292/api/v1`

## 4) Common issue: CORS request did not succeed (status null)
This usually means backend is not reachable (not a true CORS policy mismatch).

Checklist:
1. Confirm backend is running with `ASPNETCORE_ENVIRONMENT=Development`.
2. Confirm DB connection exists in `backend/appsettings.Development.json`.
3. Confirm backend endpoint opens in browser:
   - `https://localhost:7292/openapi/v1.json`
4. Trust HTTPS cert:
   - `dotnet dev-certs https --trust`

If HTTPS still fails locally, temporary fallback:
- Change `frontend/src/app/api.config.ts` to `http://localhost:5047/api/v1`
- Restart frontend.

## 5) Environment-specific connection strings
Configuration files:
- `backend/appsettings.json` (safe baseline)
- `backend/appsettings.Development.json` (local `.\SQLEXPRESS01`)
- `backend/appsettings.Staging.json` (placeholder)
- `backend/appsettings.Production.json` (placeholder)

In staging/production, set real secret via environment variable:

```text
ConnectionStrings__DefaultConnection
```

And set environment:

```text
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_ENVIRONMENT=Production
```
