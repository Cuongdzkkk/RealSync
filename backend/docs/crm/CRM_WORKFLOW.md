# CRM Workflow

## High-Level Flow

```text
Login
  -> Create Lead
  -> Update Lead / Status / Assignment / Activities / Follow-up
  -> Internal notifications are created for assignment and follow-up
  -> Convert Lead to Customer
  -> Customer history is available through ActivityLog
  -> CRM Analytics summarizes Leads, Customers, Follow-ups, and Conversion
```

## Lead Lifecycle

1. A user creates a lead with at least phone or email.
2. The lead starts with default status `New` unless a valid status is supplied.
3. The lead can move through:
   - `New`
   - `Contacted`
   - `Qualified`
   - `Proposal`
   - `Won`
   - `Lost`
4. Status changes create system lead activities.
5. Manual activities support:
   - `Call`
   - `Email`
   - `Meeting`
   - `Note`

## Assignment and Notifications

1. Assigning a lead requires an active user.
2. Assignment updates `AssignedToId`.
3. Assignment creates a lead activity of type `Assigned`.
4. Assignment sends an internal notification.
5. Notification send failure does not roll back the lead workflow.

## Follow-Up

1. Follow-up date must be in the future.
2. Setting follow-up updates `NextFollowUpAt`.
3. A lead activity of type `FollowUp` is created.
4. If the lead has an assigned user, an internal notification is created.
5. Overdue follow-up filters ignore `Won` and `Lost` leads.

## Convert Lead to Customer

1. Conversion is called from `POST /api/v1/leads/{id}/convert-to-customer`.
2. The service creates a Customer linked by `ConvertedFromLeadId`.
3. Lead status is set to `Won`.
4. `ConvertedAt` is set when missing.
5. A lead activity of type `Converted` is created.
6. Duplicate conversion for the same lead is blocked with validation error.

## Customer Workflow

1. Customers can be created directly or from lead conversion.
2. A customer must have `FullName` and at least phone or email.
3. Assigned user, if provided, must be active.
4. Customer delete is soft delete.
5. Customer activity history is read from `ActivityLog`; no separate customer activity table is used.

## Notification Workflow

1. Notifications are created internally by workflows.
2. API users can list and manage only their own notifications.
3. Reading, marking, and deleting another user's notification returns not found.
4. Delete is soft delete.

## CRM Analytics

Analytics APIs aggregate:

- Lead status counts
- Lead source counts
- Lead temperature
- Conversion rates
- Follow-up buckets
- Customer source and conversion stats
- Monthly lead/customer trends

## Edge Cases

| Case | Expected Result |
| --- | --- |
| Duplicate lead conversion | Validation error |
| Invalid lead status | Validation error |
| Assign inactive user | Validation error |
| Follow-up in the past | Validation error |
| Notification belongs to another user | Not found |
| Deleted notification detail | Not found |
| Deleted customer/lead | Hidden by soft-delete query filter |
| No leads in analytics | Conversion rates return `0` |
