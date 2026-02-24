# ADR 0003: Holding Grouping Model

## Status
Accepted

## Decision
Each holding belongs to exactly one editable user-defined group.
Deleting a group reassigns holdings to `Uncategorized`.

## Rationale
- Keeps model simple and understandable
- Avoids accidental data loss

## Consequences
- Must guarantee presence of `Uncategorized` group per user
- Reassignment logic required in group delete flow
