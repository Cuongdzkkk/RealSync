# LLM Coding Pitfalls — Reference Guide

> Common failure modes of AI coding agents and how to recognize them.

---

## 1. The Hallucination Spectrum

LLM coding errors exist on a spectrum from subtle to catastrophic:

### Level 1: Plausible but Wrong API Usage
The agent generates code that *looks* correct but uses an API incorrectly.

```csharp
// ❌ Agent hallucination — DbContext.Update() doesn't work this way
dbContext.Update(entity); // Agent assumes this marks specific fields
await dbContext.SaveChangesAsync();

// ✅ Correct — EF Core tracks changes automatically
var existing = await dbContext.Properties.FindAsync(id);
existing.Title = newTitle; // EF tracks this change
await dbContext.SaveChangesAsync();
```

**Why it happens**: The agent pattern-matches from training data. It has seen `Update()` used in various ORMs and conflates behaviors.

**Defense**: Always verify ORM-specific behavior against documentation.

### Level 2: Confident Nonsense
The agent generates code using methods or properties that don't exist.

```typescript
// ❌ Agent hallucination — Element Plus doesn't have this method
ElMessage.showGlobal({ message: 'Success', type: 'success' });

// ✅ Correct
ElMessage.success('Success');
```

**Why it happens**: The agent interpolates from similar APIs it has seen.

**Defense**: Check the actual library documentation. When uncertain, flag it.

### Level 3: Architectural Fantasy
The agent proposes an architecture that sounds sophisticated but adds no value.

```
❌ Agent suggests:
"Let's implement a CQRS pattern with event sourcing, a message bus,
and separate read/write databases for the property listing endpoint."

✅ Reality:
This is a 5-person team building an internal tool. A simple service layer
with EF Core is more than sufficient.
```

**Why it happens**: The agent has been trained on articles about enterprise patterns and applies them regardless of context.

**Defense**: Always ask: "Does the complexity match the problem?"

---

## 2. The Overgeneration Bias

LLMs are generative models. Their default mode is to produce output. This creates several pitfalls:

### 2.1 The Completeness Trap
The agent generates every possible endpoint, every possible validation, every possible edge case — when only one specific thing was asked for.

**Cost**: Huge review burden. Untested code. Hidden bugs in the "extras."

### 2.2 The Elegance Trap
The agent rewrites working-but-ugly code into elegant-but-untested code.

**Cost**: Broken functionality. Introduced regressions. Lost institutional knowledge embedded in the "ugly" code.

### 2.3 The Helper Trap
The agent creates utility functions, helper classes, and abstractions "just in case."

**Cost**: Dead code. Increased surface area. Maintenance burden for things nobody uses.

---

## 3. Context Window Limitations

### What the Agent Forgets
- Code it read 10,000 tokens ago
- Decisions made earlier in the conversation
- Files it hasn't explicitly been shown

### What This Causes
- Inconsistent solutions across a conversation
- Contradicting its own earlier recommendations
- Missing integration points between files

### Defense
- Keep tasks focused and small
- Re-state critical context when starting new tasks
- Verify cross-file consistency manually

---

## 4. The Copy-Paste Mutation

When an agent generates code based on an existing pattern, it often mutates the pattern slightly and incorrectly.

```csharp
// Original (working) code
public async Task<Property> GetByIdAsync(Guid id)
{
    return await _context.Properties
        .Include(p => p.PropertyType)
        .FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException("Property", id);
}

// ❌ Agent's mutation (broken)
public async Task<Lead> GetByIdAsync(Guid id)
{
    return await _context.Leads
        .Include(p => p.LeadType)     // LeadType doesn't exist!
        .FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException("Property", id);  // Wrong entity name!
}
```

**Defense**: Review agent-generated code character by character, especially when it's based on existing patterns.

---

## 5. Mitigation Strategies Summary

| Pitfall | Mitigation |
|---------|-----------|
| API hallucination | Verify against actual docs; flag uncertainty |
| Overgeneration | Explicitly constrain scope in the prompt |
| Elegance trap | "Don't refactor" as an explicit instruction |
| Context loss | Re-state critical context; keep tasks small |
| Copy-paste mutation | Review all names, types, and references |
| Architectural fantasy | "Use the simplest approach that works" |

---

> Part of the Karpathy Guidelines behavioral skill.
