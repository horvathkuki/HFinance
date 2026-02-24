# ADR 0001: FX Rate Source

## Status
Accepted

## Decision
Use ECB daily reference rates XML as the primary FX source.

## Rationale
- Public and stable source
- Includes EUR base with required USD and RON
- Easy to parse and cache

## Consequences
- Need fallback to last-known-good values
- Snapshot persistence must store rates used at capture time
