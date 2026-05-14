# Surgical Change Example

> How to make the smallest possible change that solves the problem.

---

## Scenario

**Task**: Lead status should not allow transition from "Won" back to "New".

**Existing code** in `LeadService.cs`:

```csharp
public async Task UpdateStatusAsync(Guid id, string newStatus)
{
    var lead = await _context.Leads.FindAsync(id)
        ?? throw new NotFoundException("Lead", id);

    lead.Status = newStatus;
    lead.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
}
```

---

## ❌ BAD: The Nuclear Option

Agent rewrites the entire method, introduces a state machine class, an enum, a transition validator, a strategy pattern for different lead types, and adds 4 new files.

**180 lines changed. Original bug: fixed somewhere in there.**

---

## ✅ GOOD: The Surgical Fix

```csharp
public async Task UpdateStatusAsync(Guid id, string newStatus)
{
    var lead = await _context.Leads.FindAsync(id)
        ?? throw new NotFoundException("Lead", id);

    // Prevent backward transitions from terminal states
    var terminalStatuses = new[] { "Won", "Lost" };
    if (terminalStatuses.Contains(lead.Status) && lead.Status != newStatus)
        throw new BusinessException($"Cannot change status from '{lead.Status}'");

    lead.Status = newStatus;
    lead.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
}
```

### What changed:
- **3 lines added** to existing method
- No new files
- No new abstractions
- Business rule is clear and readable
- Easy to extend later (add more rules to the check)

### Verification:
```
1. PATCH /api/v1/leads/{id}/status with status="New" when current="Won"
   → Should return 422 with error message
2. PATCH /api/v1/leads/{id}/status with status="Contacted" when current="New"
   → Should return 200 (allowed)
3. All existing lead tests still pass
```

### Observations (NOT implemented):
- A full state machine may be warranted if transition rules become complex
- Consider making valid transitions configurable — recommend as Phase 2 task

---

## Metrics

| Metric | Nuclear | Surgical |
|--------|---------|----------|
| Files changed | 5 | 1 |
| Lines changed | 180 | 3 |
| New abstractions | 3 | 0 |
| Risk of regression | High | Low |
| Review time | 20 min | 2 min |

---

> Part of the Karpathy Guidelines behavioral skill.
