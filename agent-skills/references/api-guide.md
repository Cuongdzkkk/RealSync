# 🔌 API Guide — RealSync

> Hướng dẫn thiết kế và sử dụng API cho hệ thống RealSync.

---

## 1. API Design Principles

1. **RESTful** — Tuân thủ REST conventions.
2. **Versioned** — Prefix `/api/v1/`.
3. **JSON** — Request/Response body dùng JSON.
4. **Consistent** — Cùng format response cho mọi endpoint.
5. **Secure** — JWT Bearer authentication.
6. **Documented** — Swagger/OpenAPI auto-generated.

---

## 2. Base URL

```
Development:  https://localhost:5001/api/v1
Staging:      https://api-staging.realsync.vn/api/v1
Production:   https://api.realsync.vn/api/v1
```

---

## 3. Authentication

### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@realsync.vn",
  "password": "********"
}
```

### Response
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
    "expiresIn": 900,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@realsync.vn",
      "fullName": "Nguyễn Văn A",
      "role": "Manager"
    }
  }
}
```

### Authorization Header
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 4. Standard Response Format

### Success Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Thao tác thành công",
  "data": { },
  "errors": null,
  "meta": null
}
```

### Success with Pagination
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy danh sách thành công",
  "data": [ ],
  "errors": null,
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Error Response
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Dữ liệu không hợp lệ",
  "data": null,
  "errors": [
    {
      "field": "Title",
      "message": "Tiêu đề không được để trống"
    },
    {
      "field": "Price",
      "message": "Giá phải lớn hơn 0"
    }
  ],
  "meta": null
}
```

### C# Implementation
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<ApiError>? Errors { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, StatusCode = 200, Message = message, Data = data };

    public static ApiResponse<T> Created(T data, string message = "Tạo mới thành công")
        => new() { Success = true, StatusCode = 201, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<ApiError>? errors = null)
        => new() { Success = false, StatusCode = statusCode, Message = message, Errors = errors };
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

## 5. API Endpoints

### 5.1 Properties

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/properties` | Danh sách BĐS (paginated, filtered) | ✅ |
| `GET` | `/properties/{id}` | Chi tiết BĐS | ✅ |
| `POST` | `/properties` | Tạo mới BĐS | ✅ Manager+ |
| `PUT` | `/properties/{id}` | Cập nhật BĐS | ✅ Manager+ |
| `PATCH` | `/properties/{id}/status` | Đổi trạng thái | ✅ Manager+ |
| `DELETE` | `/properties/{id}` | Xóa mềm BĐS | ✅ Admin |
| `POST` | `/properties/{id}/images` | Upload ảnh | ✅ Manager+ |
| `DELETE` | `/properties/{id}/images/{imageId}` | Xóa ảnh | ✅ Manager+ |

#### Query Parameters (GET /properties)
```
?page=1
&pageSize=20
&search=keyword
&propertyTypeId=guid
&status=Active
&listingType=Sale
&minPrice=1000000000
&maxPrice=5000000000
&minArea=50
&maxArea=200
&province=Hồ Chí Minh
&district=Quận 1
&sortBy=createdAt
&sortOrder=desc
```

### 5.2 Leads

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/leads` | Danh sách Lead | ✅ |
| `GET` | `/leads/{id}` | Chi tiết Lead | ✅ |
| `POST` | `/leads` | Tạo mới Lead | ✅ |
| `PUT` | `/leads/{id}` | Cập nhật Lead | ✅ |
| `PATCH` | `/leads/{id}/status` | Đổi trạng thái | ✅ |
| `PATCH` | `/leads/{id}/assign` | Assign cho agent | ✅ Manager+ |
| `DELETE` | `/leads/{id}` | Xóa mềm Lead | ✅ Admin |
| `GET` | `/leads/{id}/activities` | Lịch sử tương tác | ✅ |
| `POST` | `/leads/{id}/activities` | Thêm hoạt động | ✅ |

### 5.3 Crawler

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/crawl-sources` | Danh sách nguồn crawl | ✅ Admin |
| `POST` | `/crawl-sources` | Thêm nguồn crawl | ✅ Admin |
| `POST` | `/crawl-jobs/start` | Bắt đầu crawl job | ✅ Admin |
| `GET` | `/crawl-jobs` | Danh sách jobs | ✅ Admin |
| `GET` | `/crawl-jobs/{id}` | Chi tiết job | ✅ Admin |
| `POST` | `/crawl-jobs/{id}/stop` | Dừng job | ✅ Admin |

### 5.4 Dashboard

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/dashboard/summary` | Tổng quan số liệu | ✅ |
| `GET` | `/dashboard/properties/stats` | Thống kê BĐS | ✅ |
| `GET` | `/dashboard/leads/stats` | Thống kê Lead | ✅ |
| `GET` | `/dashboard/leads/pipeline` | Lead pipeline | ✅ |
| `GET` | `/dashboard/crawl/stats` | Thống kê crawl | ✅ Admin |

### 5.5 Auth & Users

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/auth/login` | Đăng nhập | ❌ |
| `POST` | `/auth/refresh` | Refresh token | ❌ |
| `POST` | `/auth/logout` | Đăng xuất | ✅ |
| `GET` | `/auth/me` | Thông tin user hiện tại | ✅ |
| `PUT` | `/auth/me/password` | Đổi mật khẩu | ✅ |
| `GET` | `/users` | Danh sách users | ✅ Admin |
| `POST` | `/users` | Tạo user | ✅ Admin |

### 5.6 Posting

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/posts` | Danh sách bài đăng (paginated) | ✅ |
| `GET` | `/posts/{id}` | Chi tiết bài đăng | ✅ |
| `POST` | `/posts` | Tạo bài đăng | ✅ Manager+ |
| `PUT` | `/posts/{id}` | Cập nhật bài đăng | ✅ Manager+ |
| `PATCH` | `/posts/{id}/status` | Đổi trạng thái | ✅ Manager+ |
| `DELETE` | `/posts/{id}` | Xóa mềm | ✅ Admin |
| `GET` | `/posts/{id}/channels` | Channels của post | ✅ |
| `POST` | `/posts/{id}/channels` | Thêm channel | ✅ Manager+ |
| `POST` | `/posts/{id}/channels/{channelId}/publish` | Publish lên kênh | ✅ Manager+ |
| `GET` | `/posts/{id}/schedules` | Lịch của post | ✅ |
| `POST` | `/posts/{id}/schedules` | Lên lịch đăng | ✅ Manager+ |
| `DELETE` | `/posts/{id}/schedules/{scheduleId}` | Hủy lịch | ✅ Manager+ |
| `GET` | `/posts/{id}/analytics` | Analytics của post | ✅ |
| `GET` | `/post-analytics/summary` | Tổng hợp analytics | ✅ Manager+ |
| `POST` | `/posts/{id}/ai-content/generate` | Generate AI content | ✅ Manager+ |
| `GET` | `/posts/{id}/ai-content` | Lịch sử AI generation | ✅ |

---

## 6. HTTP Status Codes

| Code | Usage |
|------|-------|
| `200` | Success (GET, PUT, PATCH) |
| `201` | Created (POST) |
| `204` | No Content (DELETE) |
| `400` | Bad Request (validation error) |
| `401` | Unauthorized (missing/invalid token) |
| `403` | Forbidden (insufficient role) |
| `404` | Not Found |
| `409` | Conflict (duplicate) |
| `422` | Unprocessable Entity (business rule violation) |
| `429` | Too Many Requests (rate limit) |
| `500` | Internal Server Error |

---

## 7. Controller Template

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PropertyFilterRequest filter)
    {
        var result = await _propertyService.GetAllAsync(filter);
        return Ok(ApiResponse<PagedResult<PropertyResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _propertyService.GetByIdAsync(id);
        return Ok(ApiResponse<PropertyResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] PropertyCreateRequest request)
    {
        var result = await _propertyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PropertyResponse>.Created(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PropertyUpdateRequest request)
    {
        var result = await _propertyService.UpdateAsync(id, request);
        return Ok(ApiResponse<PropertyResponse>.Ok(result, "Cập nhật thành công"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _propertyService.DeleteAsync(id);
        return NoContent();
    }
}
```

---

## 8. Frontend API Service Template

```typescript
// services/propertyService.ts
import api from './api';
import type { Property, PropertyCreateRequest, PropertyFilter, PagedResult, ApiResponse } from '@/types';

const BASE = '/properties';

export const propertyService = {
  getAll(filter: PropertyFilter): Promise<ApiResponse<PagedResult<Property>>> {
    return api.get(BASE, { params: filter });
  },

  getById(id: string): Promise<ApiResponse<Property>> {
    return api.get(`${BASE}/${id}`);
  },

  create(data: PropertyCreateRequest): Promise<ApiResponse<Property>> {
    return api.post(BASE, data);
  },

  update(id: string, data: Partial<Property>): Promise<ApiResponse<Property>> {
    return api.put(`${BASE}/${id}`, data);
  },

  delete(id: string): Promise<void> {
    return api.delete(`${BASE}/${id}`);
  },

  updateStatus(id: string, status: string): Promise<ApiResponse<Property>> {
    return api.patch(`${BASE}/${id}/status`, { status });
  },
};
```

---

> Xem thêm: `architecture-guide.md`, `database-guide.md`
