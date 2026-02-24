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

## Plan 10 - Frontend Navigation Visibility (Auth-Only Dashboard Link)
Status: Implemented.

Goal:
- Make `Dashboard` navigation visible only after login.

Implemented behavior:
- Logged out users see `Login` and `Register`.
- Logged in users see `Dashboard`, `Account`, and `Logout`.
- Admin users additionally see `Admin`.
- Root route (`/`) and wildcard route now redirect to `/login`.
- Route guards continue to protect direct access to private routes.

Files impacted:
- `frontend/src/app/app.html`
- `frontend/src/app/app.routes.ts`

## Plan 11 - Frontend Tab Navigation and Logic Split
Status: Implemented.

Goal:
- Add frontend navigation/menu points:
  - Dashboard
  - Holdings
  - Allocations
  - History
  - Snapshots
  - Settings
- Move current dashboard HTML/logic into corresponding tabs.

Implemented behavior:
- Authenticated users now navigate via the six requested tabs.
- Existing dashboard was decomposed into page-specific routes/components.
- Shared cross-tab portfolio state and refresh flow implemented via a workspace service.

Files impacted:
- `frontend/src/app/app.html`
- `frontend/src/app/app.routes.ts`
- `frontend/src/app/portfolio-workspace.service.ts`
- `frontend/src/app/pages/dashboard.page.ts`
- `frontend/src/app/pages/holdings.page.ts`
- `frontend/src/app/pages/allocations.page.ts`
- `frontend/src/app/pages/history.page.ts`
- `frontend/src/app/pages/snapshots.page.ts`

## Plan 12 - Frontend UI Upgrade (Bootstrap + ng-bootstrap)
Status: Implemented.

Decision:
- Adopt `bootstrap` + `@ng-bootstrap/ng-bootstrap` for frontend UI consistency and Angular-native interactions.

Implemented:
- Bootstrap globally imported and design tokens added in `frontend/src/styles.css`.
- Top navigation moved to responsive Bootstrap navbar with active route styling.
- Page layouts refactored to Bootstrap cards/grids/forms/tables.
- ng-bootstrap components integrated where useful:
  - `ngbDropdown` for compact actions
  - `ngbModal` for destructive confirmations
  - `ngbToast` for feedback messages
  - `ngbNav` for Settings sub-sections

Follow-up:
- Frontend bundle size increased and now exceeds configured warning budget; optimization/budget tuning needed.

## Plan 13 - Holdings Full Edit Flow + Icon Actions
Status: Implemented in code (build verification pending local environment fix).

Goal:
- Allow editing of all holding properties from frontend through backend.
- Add compact icon actions in holdings table.

Implemented:
- Frontend holdings table now provides icon buttons for edit and delete.
- Holdings page form supports create + edit mode with cancel/reset behavior.
- Editable property coverage:
  - `symbol`
  - `quantity`
  - `averagePurchasePrice`
  - `currency`
  - `groupId`
  - `purchaseDate`
- Workspace/service layer now calls holding update API and refreshes shared state.
- Backend already supported full update via:
  - `PUT /api/v1/portfolios/{portfolioId}/holdings/{id}`
  - `UpdateHoldingRequest : CreateHoldingRequest` (same editable property set).

Validation note:
- Frontend compile check (`npm run build`) currently fails in this machine/session with `spawn EPERM` before compilation completes.

## Plan 14 - Move Allocation/Exposure Charts to Dashboard
Status: Implemented.

Goal:
- Move "Allocation by Group" and "Exposure by Currency" visuals from `Allocations` to `Dashboard`.

Implemented:
- `Dashboard` now renders both charts below portfolio summary.
- `Allocations` no longer renders these charts and remains dedicated to portfolio/group management.
- Chart helper methods were moved from allocations page component into dashboard page component.
