# Verification Example

> How to define and execute verification criteria for a real task.

---

## Scenario

**Task**: Add a `PATCH /api/v1/properties/{id}/status` endpoint that changes a property's status.

---

## Step 1: Define Verification BEFORE Coding

```markdown
## Verification Criteria

### Must Pass
1. [ ] PATCH /api/v1/properties/{id}/status with {"status":"Active"}
       → Returns 200, property status is "Active"
2. [ ] PATCH with invalid status "Banana"
       → Returns 400 with validation error
3. [ ] PATCH with non-existent id
       → Returns 404
4. [ ] PATCH without auth token
       → Returns 401
5. [ ] PATCH with Viewer role
       → Returns 403 (requires Manager+)

### Must NOT Break
6. [ ] GET /api/v1/properties still works
7. [ ] PUT /api/v1/properties/{id} still works
8. [ ] All existing unit tests pass: `dotnet test`
```

---

## Step 2: Implement (Minimal)

```csharp
// PropertiesController.cs — add method
[HttpPatch("{id:guid}/status")]
[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> UpdateStatus(
    Guid id, [FromBody] UpdateStatusRequest request)
{
    var result = await _propertyService.UpdateStatusAsync(id, request.Status);
    return Ok(ApiResponse<PropertyResponse>.Ok(result, "Cập nhật trạng thái thành công"));
}

// PropertyService.cs — add method
public async Task<PropertyResponse> UpdateStatusAsync(Guid id, string status)
{
    var validStatuses = new[] { "Draft", "Active", "Sold", "Rented", "Expired" };
    if (!validStatuses.Contains(status))
        throw new BusinessException($"Invalid status: {status}");

    var property = await _context.Properties.FindAsync(id)
        ?? throw new NotFoundException("Property", id);

    property.Status = status;
    property.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return MapToResponse(property);
}

// UpdateStatusRequest.cs
public record UpdateStatusRequest(string Status);
```

---

## Step 3: Execute Verification

```bash
# Test 1: Valid status change
curl -X PATCH https://localhost:5001/api/v1/properties/{id}/status \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"status":"Active"}'
# Expected: 200

# Test 2: Invalid status
curl -X PATCH https://localhost:5001/api/v1/properties/{id}/status \
  -H "Authorization: Bearer {token}" \
  -d '{"status":"Banana"}'
# Expected: 400

# Test 3: Non-existent ID
curl -X PATCH https://localhost:5001/api/v1/properties/00000000-.../status \
  -H "Authorization: Bearer {token}" \
  -d '{"status":"Active"}'
# Expected: 404

# Test 4: No auth
curl -X PATCH https://localhost:5001/api/v1/properties/{id}/status \
  -d '{"status":"Active"}'
# Expected: 401

# Test 5: Existing tests
dotnet test
# Expected: All pass
```

---

## Step 4: Report Results

```markdown
## Results

### Must Pass
1. [x] PATCH with valid status → 200 ✅
2. [x] PATCH with invalid status → 400 ✅
3. [x] PATCH with bad id → 404 ✅
4. [x] PATCH without auth → 401 ✅
5. [x] PATCH with Viewer role → 403 ✅

### Must NOT Break
6. [x] GET /properties works ✅
7. [x] PUT /properties/{id} works ✅
8. [x] All existing tests pass ✅

All criteria met. Change is ready for review.
```

---

> Part of the Karpathy Guidelines behavioral skill.
