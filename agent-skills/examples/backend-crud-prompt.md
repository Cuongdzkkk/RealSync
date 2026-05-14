# 🔧 Backend CRUD Prompt Template

> Dùng prompt này khi cần agent tạo CRUD endpoints cho một entity mới.

---

## Prompt Template

```
[BACKEND] Tạo CRUD đầy đủ cho entity "{EntityName}"

Yêu cầu:
1. Entity class trong RealSync.Core/Entities/
   - Kế thừa BaseEntity
   - Properties: {list properties và kiểu dữ liệu}
   - Relationships: {FK nếu có}

2. EF Configuration trong RealSync.Data/Configurations/
   - Fluent API configuration
   - Indexes
   - Query filter (soft delete)

3. DTOs trong RealSync.Shared/DTOs/
   - {EntityName}CreateRequest
   - {EntityName}UpdateRequest
   - {EntityName}Response
   - {EntityName}FilterRequest (pagination + filter)

4. Repository trong RealSync.Data/Repositories/
   - I{EntityName}Repository (interface trong Core)
   - {EntityName}Repository (implementation)

5. Service trong RealSync.Services/
   - I{EntityName}Service (interface trong Core)
   - {EntityName}Service (implementation)
   - Validation (FluentValidation)

6. Controller trong RealSync.Api/Controllers/
   - GET /api/v1/{entities} (list, paginated)
   - GET /api/v1/{entities}/{id}
   - POST /api/v1/{entities}
   - PUT /api/v1/{entities}/{id}
   - DELETE /api/v1/{entities}/{id}

7. Migration
   - Tạo migration cho entity mới

Tuân thủ:
- Async/Await cho mọi I/O
- ApiResponse<T> wrapper
- Soft delete
- Error handling
- Naming convention trong SKILL.md
```

---

## Ví dụ cụ thể: Property CRUD

```
[BACKEND] Tạo CRUD đầy đủ cho entity "Property"

Properties:
- PropertyCode: string (required, unique, max 50)
- Title: string (required, max 500)
- Description: string (optional)
- PropertyTypeId: Guid (FK → PropertyTypes)
- ProjectId: Guid? (FK → Projects, nullable)
- AreaId: Guid (FK → Areas)
- Address: string (max 500)
- District: string (max 100)
- Province: string (max 100)
- Latitude: decimal?
- Longitude: decimal?
- Area: decimal (m²)
- Price: decimal (VND)
- Bedrooms: int
- Bathrooms: int
- Floors: int
- Direction: string
- LegalStatus: string
- Status: enum (Draft, Active, Sold, Rented, Expired)
- ListingType: enum (Sale, Rent)
- Slug: string (unique)

Relationships:
- PropertyType (1-N)
- Project (1-N, optional)
- Area (1-N)
- PropertyImages (1-N, cascade)

Filters:
- search (title, address)
- propertyTypeId
- status
- listingType
- province, district
- minPrice, maxPrice
- minArea, maxArea
- sortBy, sortOrder
```

---

## Expected Output Structure

```
Files to create/modify:

✅ RealSync.Core/Entities/Property.cs
✅ RealSync.Core/Interfaces/IPropertyRepository.cs
✅ RealSync.Core/Interfaces/IPropertyService.cs
✅ RealSync.Data/Configurations/PropertyConfiguration.cs
✅ RealSync.Data/Repositories/PropertyRepository.cs
✅ RealSync.Services/Implementations/PropertyService.cs
✅ RealSync.Services/Validators/PropertyValidator.cs
✅ RealSync.Services/Mappings/PropertyMappingProfile.cs
✅ RealSync.Shared/DTOs/Requests/PropertyCreateRequest.cs
✅ RealSync.Shared/DTOs/Requests/PropertyUpdateRequest.cs
✅ RealSync.Shared/DTOs/Requests/PropertyFilterRequest.cs
✅ RealSync.Shared/DTOs/Responses/PropertyResponse.cs
✅ RealSync.Api/Controllers/PropertiesController.cs
✅ RealSync.Api/Extensions/ServiceExtensions.cs (DI registration)
```
