# HFinance Context Summary

This file is a compact source of truth for fast context reload in future sessions.

## Product and Architecture
- App: `HFinance` portfolio tracker.
- Frontend: Angular (standalone components).
- Backend: ASP.NET Core Web API + EF Core.
- Database: SQL Server Express locally (`.\\SQLEXPRESS01`), staging/production via environment variables.
- Market data provider: Yahoo Finance.
- FX provider: ECB daily XML.

## Core Functional Scope
- Authentication and user management:
  - ASP.NET Identity + JWT.
  - Roles: `User`, `Admin`.
  - Account self-service + admin user controls.
- Portfolio domain:
  - Portfolios and holdings CRUD.
  - Ownership enforced by authenticated user.
- Grouping:
  - Each holding belongs to one group.
  - Deleting a group moves holdings to `Uncategorized`.
- Currency rules:
  - Allowed currencies: `EUR`, `USD`, `RON`.
  - Base currency configured per user.
- Analytics:
  - Portfolio totals, unrealized P/L, allocations, currency exposure.
- Snapshots:
  - Manual, per-portfolio.
  - Partial snapshots allowed with cached fallback.
  - Historical timeline based on snapshots.

## Frontend UX Structure
- Authenticated navigation tabs:
  - `Dashboard`, `Holdings`, `Allocations`, `History`, `Snapshots`, `Settings`.
- Public navigation:
  - `Login`, `Register`.
- `Dashboard` link is visible only after login.
- Routing:
  - Root and wildcard routes redirect to `/login`.
- Shared frontend state:
  - `PortfolioWorkspaceService` keeps tab data consistent.
- UI system:
  - Bootstrap + ng-bootstrap.
  - Charts: `ng2-charts` + `chart.js`.

## Operations and Configuration
- Config files:
  - `backend/appsettings.json` (baseline)
  - `backend/appsettings.Development.json` (local)
  - `backend/appsettings.Staging.json` (placeholder)
  - `backend/appsettings.Production.json` (placeholder)
- Staging/production secret connection string key:
  - `ConnectionStrings__DefaultConnection`
- Environment switch key:
  - `ASPNETCORE_ENVIRONMENT`
- Runbook:
  - `docs/RUNNING.md`

## Engineering Standards
- Readability for mid-level developers:
  - Clear naming.
  - Explicit DTOs/types.
  - Short focused methods.
  - Comments only for non-obvious business logic.
- Predictable architecture:
  - Controller -> Service -> Provider/Repository.
- Planning and decisions tracked in:
  - `docs/IMPLEMENTATION_PLAN.md`
  - `docs/PLAN_HISTORY_2026-02-24.md`
  - `docs/ADR/`

## Known Follow-ups
- Expand backend/frontend/e2e automated tests.
- Improve production hardening and runbook depth.
- Optimize frontend bundle size (current warning threshold exceeded after Bootstrap rollout).
