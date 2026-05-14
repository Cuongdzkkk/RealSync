# Verification-Driven Development — Reference Guide

> A methodology where verification criteria are defined BEFORE implementation.

---

## 1. The Core Idea

Traditional: Code → Hope it works → Test (maybe)
Verification-Driven: Define "works" → Code → Prove it works

```
┌─────────────────────────────────────────────┐
│  Traditional Development                    │
│                                             │
│  Task → Code → "Looks right" → Ship        │
│                     ❌ No proof              │
├─────────────────────────────────────────────┤
│  Verification-Driven Development            │
│                                             │
│  Task → Define "Done" → Code → Verify       │
│                                   ✅ Proof  │
└─────────────────────────────────────────────┘
```

---

## 2. Verification Levels

### Level 1: Compilation / Syntax
The most basic level. The code compiles and the linter passes.

```
Verification: `dotnet build` exits with code 0
              `npm run build` exits with code 0
```

### Level 2: Unit Verification
Individual functions produce expected outputs for given inputs.

```
Verification: `dotnet test` — PropertyServiceTests pass
              Specific test: CreateProperty_ValidInput_ReturnsCreatedProperty
```

### Level 3: Integration Verification
Components work together correctly.

```
Verification: POST /api/v1/properties returns 201
              GET /api/v1/properties/{id} returns the created property
              Created property appears in GET /api/v1/properties list
```

### Level 4: Behavioral Verification
The system behaves correctly from the user's perspective.

```
Verification: User can create a property via the form
              Property appears in the list after creation
              Validation errors show when submitting invalid data
```

### Level 5: Non-Regression Verification
Existing functionality is not broken.

```
Verification: All existing tests still pass
              Previously working endpoints still return expected responses
              No new console errors in frontend
```

---

## 3. Verification Criteria Template

Use this template for every task:

```markdown
## Verification Criteria

### Must Pass (blocking)
1. [ ] [Specific, measurable criterion]
2. [ ] [Specific, measurable criterion]

### Should Pass (important but non-blocking)
3. [ ] [Specific, measurable criterion]

### Must NOT Break (regression check)
4. [ ] [Existing functionality that must continue to work]
5. [ ] [Existing tests that must continue to pass]
```

---

## 4. Examples for RealSync

### Example A: Adding a Property Endpoint

```markdown
## Verification Criteria

### Must Pass
1. [ ] `POST /api/v1/properties` with valid body returns 201
2. [ ] Response body contains the created property with generated Id
3. [ ] `GET /api/v1/properties/{id}` returns the property created in step 1
4. [ ] `POST /api/v1/properties` with missing Title returns 400 with validation error
5. [ ] `POST /api/v1/properties` without auth token returns 401

### Should Pass
6. [ ] `dotnet test` — all PropertyService unit tests pass
7. [ ] Swagger page shows the new endpoint with correct schema

### Must NOT Break
8. [ ] All existing API endpoints still respond correctly
9. [ ] All existing unit tests pass
10. [ ] Frontend still loads without console errors
```

### Example B: Fixing Soft Delete Bug

```markdown
## Verification Criteria

### Must Pass
1. [ ] `DELETE /api/v1/properties/{id}` sets IsDeleted=true (not hard delete)
2. [ ] `GET /api/v1/properties` does NOT return soft-deleted items
3. [ ] `GET /api/v1/properties/{id}` for a soft-deleted item returns 404
4. [ ] Database record still exists with IsDeleted=true and DeletedAt set

### Must NOT Break
5. [ ] Non-deleted properties still appear in list endpoint
6. [ ] All existing tests pass
```

### Example C: Frontend Filter Component

```markdown
## Verification Criteria

### Must Pass
1. [ ] Filter bar renders with all fields (type, status, area, price)
2. [ ] Selecting a filter triggers API call with correct query params
3. [ ] Reset button clears all filters and reloads unfiltered data
4. [ ] Loading indicator shows while API call is in progress

### Should Pass
5. [ ] Search input has 300ms debounce
6. [ ] Filter state persists across page navigation

### Must NOT Break
7. [ ] Property table still renders correctly
8. [ ] Pagination still works
9. [ ] No TypeScript compilation errors
```

---

## 5. When Verification Is Not Possible

Sometimes automated verification isn't feasible. In those cases:

```markdown
## Verification (Manual Required)

### Automated
1. [ ] `dotnet build` succeeds
2. [ ] No TypeScript errors

### Manual Verification Needed
3. [ ] Visual check: Modal renders correctly on 1920x1080 screen
4. [ ] Visual check: Chart data matches expected values
5. [ ] Note: Unable to automatically verify SignalR real-time updates
        → Recommend manual test: open two browser tabs and verify
          property creation in tab 1 appears in tab 2's list
```

Being explicit about what you CAN'T verify is just as important as what you can.

---

## 6. Verification Anti-Patterns

| Anti-Pattern | Problem | Better |
|-------------|---------|--------|
| "It should work" | No proof | Specific test command or curl |
| "I tested it mentally" | Not reproducible | Provide steps others can follow |
| "Works on my machine" | Environment-dependent | Docker test or CI verification |
| "The code looks correct" | Visual inspection only | Run it. Measure it. Prove it. |
| No regression mention | Ignores side effects | Always check existing tests |

---

> Part of the Karpathy Guidelines behavioral skill.
