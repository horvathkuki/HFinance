# ADR 0002: Snapshot Policy

## Status
Accepted

## Decision
Snapshots are manual and per-portfolio.
If live market/FX data is partially unavailable, use cached values and mark snapshot partial.

## Rationale
- User controls capture timing
- Keeps history available during temporary upstream outages

## Consequences
- Need `IsPartial` and metadata fields on snapshots
- UI must clearly indicate partial snapshots
