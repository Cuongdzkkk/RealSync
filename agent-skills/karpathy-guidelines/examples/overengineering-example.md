# Overengineering Example

> How AI agents overengineer solutions, and what the right-sized solution looks like.

---

## Scenario

**Task**: Create an endpoint to return the total count of active properties.

---

## ❌ OVERENGINEERED

Agent creates 8 new files (~200 lines): generic `IMetric` interface, `IDashboardMetricProvider<T>`, `MetricContext`, `DateRange`, `CountMetric`, `ActivePropertyCountProvider`, caching layer, DI registration — all for a single COUNT query.

**What went wrong**: Generic interfaces for one implementation. Multi-tenant support for one tenant. Caching for a fast query. 8 files for one number.

---

## ✅ RIGHT-SIZED

```csharp
// DashboardController.cs — add one endpoint (7 lines)
[HttpGet("properties/count")]
public async Task<IActionResult> GetActivePropertyCount()
{
    var count = await _context.Properties
        .CountAsync(p => !p.IsDeleted && p.Status == "Active");
    return Ok(ApiResponse<object>.Ok(new { count }));
}
```

**1 file, 7 lines, same result.**

---

## The Rule

> 1 abstraction : 1 implementation = overengineering.
> 1 abstraction : 3+ implementations = justified abstraction.

---

> Part of the Karpathy Guidelines behavioral skill.
