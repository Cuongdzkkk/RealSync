# CRM Testing Checklist

## Swagger Checklist

- [ ] Start API with `dotnet run --project src/RealSync.Api/RealSync.Api.csproj`
- [ ] Open `http://localhost:5000/swagger`
- [ ] Login admin and authorize Bearer token
- [ ] Create lead returns `201`
- [ ] Lead list returns `200` with pagination metadata
- [ ] Lead detail returns `200`
- [ ] Lead update returns `200`
- [ ] Lead status update returns `200`
- [ ] Lead assign returns `200`
- [ ] Add lead activity returns `201`
- [ ] Get lead activities returns `200`
- [ ] Set follow-up returns `200`
- [ ] Automatic follow-up reminder creates exactly one due notification
- [ ] Automatic follow-up reminder does not duplicate after additional polling cycles
- [ ] Rescheduled follow-up creates one reminder for the new timestamp
- [ ] Overdue follow-up filter returns `200`
- [ ] Convert lead to customer returns `201`
- [ ] Duplicate conversion returns validation error
- [ ] Customer list/detail/update/activities return `200`
- [ ] Notification list/detail/count/summary return `200`
- [ ] Mark notification read returns `204`
- [ ] Mark all notifications read returns `204`
- [ ] Delete notification returns `204`
- [ ] Deleted notification detail returns `404`
- [ ] All 7 CRM analytics endpoints return `200`
- [ ] Leads by status returns 6 statuses
- [ ] Monthly trend returns 12 months

## Postman Checklist

- [ ] Import `RealSync_CRM.postman_collection.json`
- [ ] Import `RealSync_Local.postman_environment.json`
- [ ] Select RealSync Local environment
- [ ] Run Auth / Login Admin and confirm `token` is set
- [ ] Run Leads CRUD / Create Lead and confirm `leadId` is set
- [ ] Run Lead Workflow requests in order
- [ ] Set `assignedUserId` manually if testing assign to another user
- [ ] Run Customers / Convert Lead To Customer and confirm `customerId` is set
- [ ] Run Notifications list and set `notificationId` manually if needed
- [ ] Run CRM Analytics folder
- [ ] Run Negative Tests folder selectively

## Permission Checklist

- [ ] Admin can use all CRM endpoints
- [ ] Manager can use configured CRM read/update/delete endpoints
- [ ] Agent can use lead/customer workflow endpoints granted by seed
- [ ] Viewer can read configured read-only endpoints
- [ ] User cannot read/update/delete notifications owned by another user
- [ ] Analytics endpoints require `dashboard.analytics`

## Response Format Checklist

- [ ] All successful responses use `ApiResponse<T>` except `204 No Content`
- [ ] Paginated list responses include `meta`
- [ ] POST create endpoints return `201`
- [ ] DELETE endpoints return `204`
- [ ] Validation failures return the existing validation error format
- [ ] Missing resources return the existing not-found error format

## Regression Checklist

- [ ] Existing Lead CRUD tests pass
- [ ] Existing Lead workflow tests pass
- [ ] Existing Customer tests pass
- [ ] Existing Notification tests pass
- [ ] Existing CRM Analytics tests pass
- [ ] Follow-up reminder service/background tests pass
- [ ] Final CRM workflow test passes
- [ ] Release CI-style test passes

## Before Merge

- [ ] `dotnet restore` pass
- [ ] `dotnet build --no-restore` pass
- [ ] `dotnet test --no-build` pass
- [ ] `dotnet build --no-restore --configuration Release` pass
- [ ] `dotnet test RealSync.slnx --no-build --configuration Release --verbosity normal` pass
- [ ] `backend/src/RealSync.Api/appsettings.Development.json` is not staged
- [ ] No migration/schema change unless explicitly required
- [ ] PR description includes manual QA results
