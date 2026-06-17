# CRM API Documentation

## Overview

The CRM module covers Lead CRUD, lead workflows, Customers, Notifications, and CRM Analytics. All APIs return the shared `ApiResponse<T>` envelope and require a Bearer token unless explicitly stated otherwise.

## Auth

Base URL:

```text
http://localhost:5000
```

Login:

```http
POST /api/v1/auth/login
```

```json
{
  "email": "admin@realsync.vn",
  "password": "Admin@123"
}
```

Use `data.accessToken` as:

```http
Authorization: Bearer {{token}}
```

## Response Format

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Thanh cong",
  "data": {},
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

Validation and not-found errors are handled by the existing API exception middleware.

## Permissions

- Leads: `leads.read`, `leads.create`, `leads.update`, `leads.delete`, `leads.assign`
- Customers: `customers.read`, `customers.create`, `customers.update`, `customers.delete`
- Notifications: `notifications.read`, `notifications.update`, `notifications.delete`
- CRM Analytics: `dashboard.analytics`

## Leads

| Method | Endpoint | Permission | Notes |
| --- | --- | --- | --- |
| GET | `/api/v1/leads` | `leads.read` | Paginated list, supports filters such as status, source, follow-up. |
| GET | `/api/v1/leads/{id}` | `leads.read` | Lead detail with activities. |
| POST | `/api/v1/leads` | `leads.create` | Create lead. |
| PUT | `/api/v1/leads/{id}` | `leads.update` | Update lead fields. |
| DELETE | `/api/v1/leads/{id}` | `leads.delete` | Soft delete. |

Create example:

```json
{
  "fullName": "Nguyen Van A",
  "phone": "0901234567",
  "email": "lead@example.com",
  "score": 80,
  "sourceChannel": "Website"
}
```

Common errors:

- Missing both phone and email: `422`
- Invalid status or priority: `422`
- Missing lead id: `404`

## Lead Workflow

| Method | Endpoint | Permission |
| --- | --- | --- |
| PATCH | `/api/v1/leads/{id}/status` | `leads.update` |
| PATCH | `/api/v1/leads/{id}/assign` | `leads.assign` |
| POST | `/api/v1/leads/{id}/activities` | `leads.update` |
| GET | `/api/v1/leads/{id}/activities` | `leads.read` |
| PATCH | `/api/v1/leads/{id}/follow-up` | `leads.update` |
| POST | `/api/v1/leads/{id}/convert-to-customer` | `customers.create` |

Convert lead example:

```json
{
  "company": "RealSync",
  "address": "District 1",
  "notes": "Ready to become customer"
}
```

Convert lead notes:

- Creates a Customer linked by `convertedFromLeadId`.
- Sets Lead status to `Won`.
- Sets `ConvertedAt` if missing.
- Prevents duplicate conversion with validation error.
- Adds a LeadActivity with type `Converted`.

## Customers

| Method | Endpoint | Permission | Notes |
| --- | --- | --- | --- |
| GET | `/api/v1/customers` | `customers.read` | Paginated list. |
| GET | `/api/v1/customers/{id}` | `customers.read` | Customer detail. |
| POST | `/api/v1/customers` | `customers.create` | Create direct customer. |
| PUT | `/api/v1/customers/{id}` | `customers.update` | Update customer. |
| DELETE | `/api/v1/customers/{id}` | `customers.delete` | Soft delete. |
| GET | `/api/v1/customers/{id}/activities` | `customers.read` | ActivityLog history. |

Create example:

```json
{
  "fullName": "Tran Thi B",
  "phone": "0912345678",
  "source": "Facebook",
  "company": "RealSync"
}
```

## Notifications

Notification APIs always operate on the current authenticated user's notifications only. Notifications owned by another user return `404`.

| Method | Endpoint | Permission |
| --- | --- | --- |
| GET | `/api/v1/notifications` | `notifications.read` |
| GET | `/api/v1/notifications/{id}` | `notifications.read` |
| GET | `/api/v1/notifications/unread-count` | `notifications.read` |
| GET | `/api/v1/notifications/summary` | `notifications.read` |
| PATCH | `/api/v1/notifications/{id}/read` | `notifications.update` |
| PATCH | `/api/v1/notifications/read-all` | `notifications.update` |
| DELETE | `/api/v1/notifications/{id}` | `notifications.delete` |

List filters:

- `isRead`
- `type`
- `fromDate`
- `toDate`
- `search`

Delete is soft delete. Deleted notifications no longer appear in list/detail.

## CRM Analytics

All analytics endpoints require `dashboard.analytics`.

| Method | Endpoint |
| --- | --- |
| GET | `/api/v1/crm/analytics/summary` |
| GET | `/api/v1/crm/analytics/leads-by-status` |
| GET | `/api/v1/crm/analytics/leads-by-source` |
| GET | `/api/v1/crm/analytics/conversion` |
| GET | `/api/v1/crm/analytics/follow-ups` |
| GET | `/api/v1/crm/analytics/customers` |
| GET | `/api/v1/crm/analytics/monthly-trend?year=2026` |

Supported analytics filters:

- `fromDate`
- `toDate`
- `assignedToId`
- `sourceChannel`
- `status`

Lead temperature:

- Hot: score >= 70
- Warm: score >= 40 and < 70
- Cold: score < 40

Conversion rates return `0` when total leads is `0`.

## Non-versioned Routes

CRM controllers also expose non-versioned routes where configured:

- `/api/leads`
- `/api/customers`
- `/api/notifications`
- `/api/crm/analytics`
