# Plan History - 2026-02-24

This file records every planning milestone agreed today.
Superseded plans are kept for traceability.

## Plan 1 - Full-Stack Portfolio Tracker (Initial)
Status: Superseded by later combined plans.

Scope:
- Angular frontend + ASP.NET Core backend + SQL Express + Yahoo Finance.
- Auth included (ASP.NET Identity + JWT).
- Server-side quote cache.
- MVP: portfolios/holdings CRUD, live valuation, P/L, allocation, 30-day chart.

Key decisions:
- Auth: include now.
- Quote strategy: short server cache.
- Frontend state: services + RxJS.

## Plan 2 - Add User Management
Status: Superseded by combined plan.

Added scope:
- Self-service account management.
- Admin basics for user administration.
- Role model: `User`, `Admin`.

Admin features:
- List users, update user profile, enable/disable, role assignment, password reset flow.

## Plan 3 - Unified Plan (Portfolio + User Management)
Status: Superseded by feature-expanded plans.

Combined scope:
- Auth/account/admin user management.
- Portfolio/holdings domain.
- Yahoo market data and analytics endpoints.
- Testing and runbook structure.

## Plan 4 - Feature Expansion (Groups, Currencies, Graphs, Snapshots)
Status: Superseded by final consolidated plan.

New requirements added:
- Every holding belongs to one editable group.
- Allowed currencies: `EUR`, `USD`, `RON`.
- Portfolio graphs in UI.
- Manual snapshots for history over time.

Decisions locked:
- Group model: one group per holding.
- Snapshot trigger: manual button.
- Snapshot scope: per portfolio.
- FX strategy: single base currency per user with conversion.

## Plan 5 - Final Product Decisions
Status: Incorporated into final implementation plan.

Additional decisions:
- Chart library: `ng2-charts + Chart.js`.
- Group deletion behavior: move holdings to `Uncategorized`.
- Snapshot outage behavior: use last cached values and mark snapshot as partial.

## Plan 6 - FX Source Finalization
Status: Incorporated into final implementation plan and ADR.

Decision:
- Use ECB daily XML as primary FX source.

Rules:
- EUR->USD and EUR->RON direct from ECB.
- USD<->RON via EUR cross-rate.
- Short cache + last-known-good fallback.
- Persist rates and timestamp in snapshots for reproducibility.

## Plan 7 - Readability and Plan-as-File Addendum
Status: Active and enforced.

Engineering standards:
- Code must be understandable by a mid-level developer.
- Explicit DTOs/types, small focused methods, clear naming.
- Comments for non-obvious logic only.
- Tests should document critical behavior.

Definition of done additions:
- Traceable flow (`controller -> service -> provider/repository`).
- Self-explanatory public interfaces.
- Edge-case behavior documented in code comments/tests.

Plan storage requirement:
- Main source of truth in `docs/IMPLEMENTATION_PLAN.md`.
- Key decisions tracked in ADR files.

## Plan 8 - Environment-Specific SQL Connection Configuration
Status: Active and implemented.

Environment model:
- `Development`: local `.\SQLEXPRESS01`.
- `Staging`: separate staging connection.
- `Production`: separate production connection.

Configuration contract:
- Continue using `ConnectionStrings:DefaultConnection`.

Deployment approach:
- Keep staging/production secrets out of repo.
- Supply real staging/prod connection strings via `ConnectionStrings__DefaultConnection` environment variable.
- Environment selection via `ASPNETCORE_ENVIRONMENT`.

Expected config files:
- `backend/appsettings.json` (safe baseline/defaults)
- `backend/appsettings.Development.json` (local dev)
- `backend/appsettings.Staging.json` (placeholder only)
- `backend/appsettings.Production.json` (placeholder only)

## Final Consolidated Plan of Record
Status: Active.

Use these as authoritative implementation documents:
- `docs/IMPLEMENTATION_PLAN.md`
- `docs/ADR/0001-fx-source-ecb.md`
- `docs/ADR/0002-snapshot-policy.md`
- `docs/ADR/0003-grouping-model.md`

## Plan 9 - Execution Progress Summary (Implemented Scope)
Status: Active.

What was implemented in code:
- Identity/JWT security baseline and user-management controllers.
- Environment-specific SQL configuration structure (Development/Staging/Production).
- Domain additions: groups, holding currencies, snapshots, analytics, market and FX services.
- Frontend functional shell with auth, dashboard, admin users, account settings, and charts.

Outstanding work after implementation pass:
- Expand automated test coverage (backend + frontend + e2e).
- Improve production hardening (validation consistency, failure-path handling, monitoring/runbooks).
- Continue UI polish and advanced UX behaviors.
