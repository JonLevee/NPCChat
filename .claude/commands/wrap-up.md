Stop your current train of thought and execute the following three steps in order. Do not ask for confirmation â€” just do all three.

---

## Step 1: Status Summary

Print a concise bullet-point summary covering:
- What we accomplished this session
- Current state: what is working, what is not yet done
- Concrete next steps in priority order (what to do first when we resume)
- Any open questions or blockers that need a decision

---

## Step 2: Save to Memory

Save one or more memory files to the auto-memory directory for this project (check the system prompt for the memory path â€” it is machine-specific) capturing anything that would be non-obvious to a future version of me starting a new conversation. Prioritize:
- Key architectural decisions made this session (with the "why")
- Changes to the phase plan or direction
- Any user preferences or feedback revealed this session

Use type `project` for decisions/plans, type `feedback` for preferences/corrections. Update existing memory files rather than creating duplicates. Always update `MEMORY.md` if you write or change any file.

---

## Step 3: Update PLANNING.md

Write (overwrite if it exists) `PLANNING.md` in the project root directory with the following structure:

```
# NPCChat â€” Planning

## Current Phase
[Phase name and one-line goal]

## Completed (this session)
- ...

## Completed (prior sessions)
- ...

## Next Steps (ordered)
1. ...
2. ...
3. ...

## Open Questions / Decisions Needed
- ...

## Key Architecture Decisions
- ...
```

Keep it factual and terse â€” this file is a reference, not a narrative.
