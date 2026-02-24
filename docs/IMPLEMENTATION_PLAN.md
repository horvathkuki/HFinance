# HFinance Implementation Plan

## Purpose
This document is the source of truth for implementing HFinance MVP.
It is written to be easy to follow for a mid-level developer.

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
