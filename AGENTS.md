# AGENTS.md — RealSync AI Agent Configuration

> This file defines how AI coding agents operate within the RealSync repository.
> All agents MUST read this file as their entry point.

---

## 1. Agent Architecture

```
AGENTS.md (this file)
    │
    ├── Required: Behavioral Skills (load FIRST)
    │   └── agent-skills/karpathy-guidelines/SKILL.md
    │
    └── Required: Technical Skills (load SECOND)
        └── agent-skills/SKILL.md
            ├── references/architecture-guide.md
            ├── references/database-guide.md
            ├── references/api-guide.md
            ├── references/ai-guide.md
            ├── references/crawler-guide.md
            └── references/deployment-guide.md
```

---

## 2. Required Behavioral Skills

### ⚠️ MANDATORY — Load Before Any Coding

All coding agents operating in this repository MUST load and internalize the following behavioral skill **before** performing any implementation work:

| Skill | Path | Purpose |
|-------|------|---------|
| **Karpathy Guidelines** | `agent-skills/karpathy-guidelines/SKILL.md` | Behavioral control layer |

### What This Means

The Karpathy Guidelines **override** the agent's default behavior. Specifically:

1. **No eager generation** — Agents must analyze before they code.
2. **No speculative abstraction** — Build for the stated requirement, not hypothetical futures.
3. **No drive-by refactoring** — Refactors are separate tasks, never smuggled into fixes.
4. **No scope creep** — Do what was asked. Suggest extras in the summary.
5. **No confident hallucinations** — Flag uncertainty instead of shipping bad code.

### Enforcement

Agents must:
- ✅ State assumptions before coding
- ✅ Define verification criteria for every change
- ✅ Minimize changed lines (smallest possible diff)
- ✅ Follow existing patterns in the codebase
- ✅ Separate observations from implementation

Agents must NOT:
- ❌ Rewrite working code without explicit request
- ❌ Add abstractions for features that don't exist yet
- ❌ Rename, restructure, or "clean up" files during unrelated tasks
- ❌ Generate code for APIs or libraries without verifying correctness
- ❌ Expand task scope unilaterally

---

## 3. Required Technical Skills

After loading behavioral skills, agents must load the project's technical skill:

| Skill | Path | Purpose |
|-------|------|---------|
| **RealSync Project Skill** | `agent-skills/SKILL.md` | Architecture, conventions, rules |

This skill provides:
- Project overview and tech stack
- Backend/Frontend/Crawler/AI/DevOps rules
- Database conventions
- Security rules
- Coding conventions
- Git workflow

---

## 4. Agent Workflow

The canonical workflow for all agents in this repository:

```
Task Received
     │
     ▼
┌────────────────────────────────────────────┐
│  PHASE 1: BEHAVIORAL LOADING               │
│  Load: karpathy-guidelines/SKILL.md        │
│  Activate: anti-pattern filters             │
│  Activate: scope discipline                 │
│  Activate: verification-driven mindset      │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 2: TECHNICAL LOADING                │
│  Load: agent-skills/SKILL.md               │
│  Load: relevant reference guides            │
│  Understand: architecture, conventions      │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 3: ANALYSIS                         │
│  Read existing code in scope               │
│  Identify what already exists              │
│  Check if the problem is already solved    │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 4: ASSUMPTIONS                      │
│  State all assumptions explicitly          │
│  Identify unknowns                         │
│  Flag anything uncertain                   │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 5: SUCCESS CRITERIA                 │
│  Define what "done" looks like             │
│  Define how to verify                      │
│  Define what should NOT change             │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 6: MINIMAL IMPLEMENTATION           │
│  Smallest possible diff                    │
│  Follow existing patterns                  │
│  No speculative abstractions               │
│  No drive-by refactoring                   │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 7: VERIFICATION                     │
│  Run tests if available                    │
│  Confirm expected behavior                 │
│  Check for regressions                     │
└────────────────┬───────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────┐
│  PHASE 8: SUBMISSION                       │
│  Summary of changes (what + why)           │
│  Verification results                      │
│  Observations (clearly separated)          │
│  Suggested follow-up tasks (if any)        │
└────────────────────────────────────────────┘
```

---

## 5. Skill Precedence

When skills conflict, apply this precedence order:

| Priority | Skill | Rationale |
|----------|-------|-----------|
| 1 (highest) | Karpathy Guidelines | Behavioral safety overrides everything |
| 2 | User's explicit instruction | User intent takes priority over conventions |
| 3 | Project SKILL.md | Technical architecture and conventions |
| 4 | Reference guides | Detailed implementation guidance |
| 5 (lowest) | Agent's general knowledge | Only when no skill provides guidance |

### Conflict Resolution Examples

- **User says "rewrite this module"** → Karpathy Rule 8 (Preserve What Works) suggests caution, but User's explicit instruction overrides. Proceed with rewrite, but verify thoroughly.
- **SKILL.md says use FluentValidation, but existing code uses manual validation** → Karpathy Rule 4 (Respect Existing Patterns) says follow existing code. Note the inconsistency in the summary.
- **Agent wants to add a caching layer "for performance"** → Karpathy Rule 3 (No Speculative Abstraction). Don't add it unless the user asked for it.

---

## 6. Module Ownership

| Module | Path | Primary Skill |
|--------|------|---------------|
| Backend | `backend/` | `agent-skills/SKILL.md` §4 |
| Frontend | `frontend/` | `agent-skills/SKILL.md` §5 |
| Crawler | `crawler/` | `agent-skills/references/crawler-guide.md` |
| AI | `ai/` | `agent-skills/references/ai-guide.md` |
| DevOps | `devops/` | `agent-skills/references/deployment-guide.md` |
| Database | `backend/` | `agent-skills/references/database-guide.md` |
| Agent Skills | `agent-skills/` | This file |

---

## 7. Quality Gates

Before submitting any change, the agent must pass these gates:

### Gate 1: Behavioral Compliance
- [ ] No scope creep
- [ ] No drive-by refactoring
- [ ] No speculative abstractions
- [ ] Assumptions stated explicitly
- [ ] Verification criteria defined

### Gate 2: Technical Compliance
- [ ] Follows project SKILL.md conventions
- [ ] Follows existing code patterns
- [ ] Error handling present
- [ ] No hardcoded secrets or credentials
- [ ] Naming conventions respected

### Gate 3: Delivery Quality
- [ ] Changes are minimal and focused
- [ ] Verification steps provided
- [ ] Summary clearly separates changes from observations
- [ ] File list of changes included
- [ ] Follow-up suggestions listed (not implemented)

---

> **Last Updated**: 2026-05-14
> **Version**: 1.0.0
