# HFinance Implementation Plan

## Purpose
This document is the source of truth for implementing HFinance MVP.
It is written to be easy to follow for a mid-level developer.

## Related Plan Files
- `docs/PLAN_HISTORY_2026-02-24.md` (complete planning history from 2026-02-24)
- `docs/ADR/0001-fx-source-ecb.md`
- `docs/ADR/0002-snapshot-policy.md`
- `docs/ADR/0003-grouping-model.md`

## Current Implementation Status (Updated 2026-02-24)
### Implemented
- Security foundation: ASP.NET Identity + JWT + role policy + global exception handler.
- User management APIs: auth/register/login, account profile/password, admin user list/status/roles/reset-password.
- Ownership baseline: portfolio and holdings access filtered by authenticated user.
- Environment-specific DB config files:
  - `appsettings.Development.json` (local `.\SQLEXPRESS01`)
  - `appsettings.Staging.json` (placeholder)
  - `appsettings.Production.json` (placeholder)
- Domain expansion:
  - Holding groups (`HoldingGroup`) and groups API
  - Holding currency support (`EUR|USD|RON`)
  - Portfolio snapshots (`PortfolioSnapshot`)
  - Analytics and timeseries endpoints
  - Market quote/history service layer with cache
  - ECB FX service with cache + fallback
- Frontend MVP shell:
  - Auth service + interceptor + guards
  - Login/Register/Account/Admin Users/Dashboard pages
  - Portfolio/holding/group/snapshot flows
  - Charts via `ng2-charts + Chart.js`
  - Navigation hardening:
    - `Dashboard` link visible only for authenticated users
    - Public nav shows `Login` and `Register` when logged out
    - Root and wildcard routes redirect to `login` for public-first entry
  - Frontend navigation/tab structure for authenticated users:
    - `Dashboard`
    - `Holdings`
    - `Allocations`
    - `History`
    - `Snapshots`
    - `Settings`
  - Dashboard logic split into tab-specific pages with shared workspace state service.
  - Dashboard now contains the portfolio summary plus:
    - Allocation by Group chart
    - Exposure by Currency chart
  - Allocations tab now focuses on portfolio/group management actions.
  - Bootstrap + ng-bootstrap UI system rollout:
    - Bootstrap global styles and design tokens added
    - Navbar migrated to responsive Bootstrap navbar with active-link highlighting
    - Forms/tables/cards migrated to Bootstrap structure across pages
    - ng-bootstrap interactions added (`ngbDropdown`, `ngbModal`, `ngbToast`, `ngbNav`)
  - Holdings management UX upgrade:
    - Holdings table now includes icon actions for edit/delete
    - Holding edit mode supports all editable properties end-to-end:
      `symbol`, `quantity`, `averagePurchasePrice`, `currency`, `groupId`, `purchaseDate`
    - Frontend uses the existing `PUT /api/v1/portfolios/{portfolioId}/holdings/{id}` backend flow for updates

### Partially Implemented / Needs Hardening
- Backend APIs exist but still need stronger production validation/error standardization across all endpoints.
- Frontend UX is functional and tab-structured, but design polish and deeper state/error handling remain.
- API coverage tests are not fully implemented yet (unit + integration + e2e pending).
- Deployment/runbook docs need a complete end-to-end operational checklist.
- Bundle optimization is needed after Bootstrap adoption (current frontend build exceeds budget).
- Frontend local build verification is currently blocked by environment error `spawn EPERM` on `ng build`.

### Immediate Next Steps
1. Add backend automated tests for auth, ownership, groups, snapshots, analytics, and FX fallback.
2. Add frontend tests for auth/session flows, dashboard behaviors, and chart data rendering.
3. Run and verify all EF migrations in target environments (`Development`, `Staging`, `Production`).
4. Normalize any remaining legacy route usage so all endpoints are consistently under `/api/v1`.
5. Add explicit frontend tests for nav visibility by auth state (logged out vs logged in vs admin).
6. Add frontend tests for cross-tab state consistency (portfolio selection and data refresh across Dashboard/Holdings/Allocations/History/Snapshots).
7. Reduce frontend bundle size and adjust/optimize Angular budgets for the new UI stack.

## Engineering Readability Standards
1. Use clear, explicit names (`portfolioSnapshotService`, not abbreviations).
2. Keep methods focused and short; move complex logic into dedicated services/helpers.
3. Add concise comments only where business logic is non-obvious:
   - FX fallback behavior
   - snapshot partial behavior
   - ownership/security enforcement
4. Prefer explicit DTOs and typed responses over dynamic/anonymous responses.
5. Avoid clever one-liners; prefer straightforward control flow.
6. Keep structure predictable (controller -> service -> provider/repository).
7. Include tests that document expected behavior for critical logic.

## Definition of Done
A feature is complete only if:
1. Request flow is easy to trace without guesswork.
2. Public methods/DTOs are self-explanatory from names and types.
3. Edge-case behavior is documented in comments and/or tests.

## MVP Scope
- Auth + user management (ASP.NET Identity + JWT)
- Roles: User, Admin
- Portfolio and holdings CRUD
- Grouping: one editable group per holding
- Group delete behavior: move holdings to `Uncategorized`
- Currencies: EUR, USD, RON only
- FX conversion: base currency per user
- Graphs: portfolio value, allocation by group, P/L trend, currency exposure
- Snapshots: manual, per portfolio
- Snapshot fallback: last cached market/FX values with partial flag

## System Decisions
- Frontend state: Angular services + RxJS
- Charts: ng2-charts + Chart.js
- Quote strategy: server-side short cache
- FX provider: ECB daily rates XML

## API Surface (High Level)
### Auth/Account
- POST `/api/v1/auth/register`
- POST `/api/v1/auth/login`
- GET `/api/v1/account/me`
- PUT `/api/v1/account/me`
- PUT `/api/v1/account/password`

### Admin (Admin only)
- GET `/api/v1/admin/users`
- GET `/api/v1/admin/users/{id}`
- PUT `/api/v1/admin/users/{id}`
- PUT `/api/v1/admin/users/{id}/status`
- PUT `/api/v1/admin/users/{id}/roles`
- POST `/api/v1/admin/users/{id}/reset-password`

### Portfolios/Holdings
- GET `/api/v1/portfolios`
- POST `/api/v1/portfolios`
- GET `/api/v1/portfolios/{id}`
- PUT `/api/v1/portfolios/{id}`
- DELETE `/api/v1/portfolios/{id}`
- GET `/api/v1/portfolios/{portfolioId}/holdings`
- POST `/api/v1/portfolios/{portfolioId}/holdings`
- PUT `/api/v1/portfolios/{portfolioId}/holdings/{id}`
- DELETE `/api/v1/portfolios/{portfolioId}/holdings/{id}`

### Groups
- GET `/api/v1/groups`
- POST `/api/v1/groups`
- PUT `/api/v1/groups/{id}`
- DELETE `/api/v1/groups/{id}`

### Market/Analytics/Snapshots
- GET `/api/v1/market/quote/{symbol}`
- GET `/api/v1/market/history/{symbol}?startDate=&endDate=`
- GET `/api/v1/analytics/portfolio/{portfolioId}`
- GET `/api/v1/analytics/portfolio/{portfolioId}/timeseries?from=&to=`
- POST `/api/v1/portfolios/{portfolioId}/snapshots`
- GET `/api/v1/portfolios/{portfolioId}/snapshots?from=&to=`

## Data Model Additions
- `AppUser`: `BaseCurrency` (EUR|USD|RON), `IsActive`
- `HoldingGroup`: user-scoped groups (unique by UserId + Name)
- `Holding`: required `GroupId`, required `Currency` (EUR|USD|RON)
- `Portfolio`: required `UserId`
- `PortfolioSnapshot`: totals, `IsPartial`, missing data metadata, FX timestamp/rates used

## FX Conversion Rules
Primary provider: ECB daily XML
- https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml

Rates:
- EUR->USD from ECB USD
- EUR->RON from ECB RON
- USD<->RON via EUR cross-rate

Policy:
- Cache rates for short TTL (15-60 min)
- Use last-known-good fallback if provider unavailable
- Persist exact FX values and timestamp on snapshot for historical reproducibility

## Implementation Order
1. Security foundation (Identity + JWT + policies)
2. Ownership enforcement and admin user management
3. Groups + currency constraints
4. Market + FX services with caching/fallback
5. Analytics + snapshots
6. Angular pages/services/guards/interceptor
7. Charts and UX polish
8. Tests (unit/integration/component/e2e smoke)

## Minimum Acceptance Criteria
1. Users can manage own data; admins can manage users.
2. Every holding belongs to exactly one group.
3. Only EUR/USD/RON are accepted.
4. Snapshot history and graphs show portfolio evolution over time.
5. Partial snapshots are clearly marked when fallback data is used.
