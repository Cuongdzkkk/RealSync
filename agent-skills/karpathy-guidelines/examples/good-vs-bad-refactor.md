# Good vs Bad Refactor — Example

> Illustrates the difference between a disciplined refactor and a drive-by refactor.

---

## Scenario

**Task**: Fix a bug where soft-deleted properties still appear in the API response.

**Existing code** in `PropertyService.cs`:

```csharp
public async Task<List<PropertyResponse>> GetAllAsync(PropertyFilterRequest filter)
{
    var query = _context.Properties
        .Include(p => p.PropertyType)
        .AsQueryable();

    if (!string.IsNullOrEmpty(filter.Search))
        query = query.Where(p => p.Title.Contains(filter.Search));

    if (filter.Status != null)
        query = query.Where(p => p.Status == filter.Status);

    var properties = await query.ToListAsync();

    var result = properties.Select(p => new PropertyResponse
    {
        Id = p.Id,
        Title = p.Title,
        Price = p.Price,
        Status = p.Status,
        PropertyTypeName = p.PropertyType?.Name
    }).ToList();

    return result;
}
```

**Bug**: The query doesn't filter out `IsDeleted == true` records.

---

## ❌ BAD: The Drive-By Refactor

The agent "fixes" the bug but also restructures everything:

```csharp
// CHANGED: PropertyService.cs — 47 lines changed, method signature changed
public async Task<PagedResult<PropertyResponse>> GetAllAsync(
    PropertyFilterRequest filter, CancellationToken ct = default)
{
    var specification = new PropertySearchSpecification(filter);
    var query = _repository.ApplySpecification(specification);

    var totalCount = await query.CountAsync(ct);

    var properties = await query
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ProjectTo<PropertyResponse>(_mapper.ConfigurationProvider)
        .ToListAsync(ct);

    return new PagedResult<PropertyResponse>(properties, totalCount, filter.Page, filter.PageSize);
}

// NEW: PropertySearchSpecification.cs — 35 lines
// NEW: ISpecification.cs — 12 lines
// NEW: SpecificationEvaluator.cs — 28 lines
// MODIFIED: IPropertyRepository.cs — added ApplySpecification method
// MODIFIED: PropertyRepository.cs — implemented ApplySpecification
// MODIFIED: PropertyResponse.cs — added AutoMapper profile
// MODIFIED: PropertyFilterRequest.cs — added Page, PageSize properties
```

### What went wrong:
- **7 files changed** for a 1-line bug fix
- Introduced Specification pattern (not in project conventions)
- Changed method signature (breaks callers)
- Added AutoMapper projection (project doesn't use AutoMapper this way)
- Added pagination (not requested)
- Added CancellationToken (nice, but not related to the bug)
- The actual bug fix is buried in 120+ lines of refactoring

---

## ✅ GOOD: The Surgical Fix

The agent fixes the bug and nothing else:

```csharp
// CHANGED: PropertyService.cs — 1 line added
public async Task<List<PropertyResponse>> GetAllAsync(PropertyFilterRequest filter)
{
    var query = _context.Properties
        .Include(p => p.PropertyType)
        .Where(p => !p.IsDeleted)           // ← FIX: filter soft-deleted
        .AsQueryable();

    if (!string.IsNullOrEmpty(filter.Search))
        query = query.Where(p => p.Title.Contains(filter.Search));

    if (filter.Status != null)
        query = query.Where(p => p.Status == filter.Status);

    var properties = await query.ToListAsync();

    var result = properties.Select(p => new PropertyResponse
    {
        Id = p.Id,
        Title = p.Title,
        Price = p.Price,
        Status = p.Status,
        PropertyTypeName = p.PropertyType?.Name
    }).ToList();

    return result;
}
```

### Summary:
```
## Changes
- Added `.Where(p => !p.IsDeleted)` to PropertyService.GetAllAsync

## Verification
1. GET /api/v1/properties should NOT include soft-deleted items
2. Soft-delete a property, call GET, verify it's excluded
3. All existing tests should still pass

## Observations (NOT implemented)
- Consider adding a global query filter in DbContext.OnModelCreating
  to avoid repeating IsDeleted check in every query
- Pagination is not implemented for this endpoint — recommend as separate task
- AutoMapper could simplify the projection — recommend as separate task
```

### What went right:
- **1 file changed, 1 line added**
- Bug is fixed
- No scope creep
- Existing behavior preserved
- Improvements noted but not smuggled in

---

## The Lesson

> A good refactor is planned, isolated, and reviewed on its own merits.
> A bad refactor is smuggled into a bug fix and reviewed as "part of the fix."

| Metric | Bad Refactor | Good Fix |
|--------|-------------|----------|
| Files changed | 7 | 1 |
| Lines changed | 120+ | 1 |
| Risk of regression | High | Near zero |
| Review difficulty | Hard | Trivial |
| Time to understand | 15+ minutes | 10 seconds |
| Bug actually fixed? | Probably, somewhere in there | Clearly, obviously |

---

> Part of the Karpathy Guidelines behavioral skill.
