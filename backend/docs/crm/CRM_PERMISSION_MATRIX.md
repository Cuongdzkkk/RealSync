# CRM Permission Matrix

This matrix reflects the current `DataSeeder` behavior.

## Endpoint Permissions

| Module | Action | Permission |
| --- | --- | --- |
| Leads | List/detail | `leads.read` |
| Leads | Create | `leads.create` |
| Leads | Update basic info | `leads.update` |
| Leads | Delete | `leads.delete` |
| Lead Workflow | Update status | `leads.update` |
| Lead Workflow | Assign lead | `leads.assign` |
| Lead Workflow | Add activity | `leads.update` |
| Lead Workflow | Get activities | `leads.read` |
| Lead Workflow | Set follow-up | `leads.update` |
| Lead Workflow | Convert to customer | `customers.create` |
| Customers | List/detail/activities | `customers.read` |
| Customers | Create | `customers.create` |
| Customers | Update | `customers.update` |
| Customers | Delete | `customers.delete` |
| Notifications | List/detail/unread-count/summary | `notifications.read` |
| Notifications | Mark read/read all | `notifications.update` |
| Notifications | Delete | `notifications.delete` |
| CRM Analytics | All analytics endpoints | `dashboard.analytics` |

## Role Matrix

| Permission | Admin | Manager | Agent | Viewer |
| --- | --- | --- | --- | --- |
| `leads.read` | Yes | Yes | Yes | Yes |
| `leads.create` | Yes | Yes | Yes | No |
| `leads.update` | Yes | Yes | Yes | No |
| `leads.delete` | Yes | Yes | No | No |
| `leads.assign` | Yes | Yes | No | No |
| `customers.read` | Yes | Yes | Yes | Yes |
| `customers.create` | Yes | Yes | Yes | No |
| `customers.update` | Yes | Yes | Yes | No |
| `customers.delete` | Yes | Yes | No | No |
| `notifications.read` | Yes | Yes | Yes | Yes |
| `notifications.update` | Yes | Yes | Yes | No |
| `notifications.delete` | Yes | Yes | Yes | No |
| `dashboard.analytics` | Yes | Yes | No | No |

## Notes

- Admin receives all permissions in the seed.
- Manager receives all permissions except `system.settings` and `users.delete`.
- Agent receives CRUD permissions for properties/leads/customers except delete/assign, plus notification read/update/delete from the Phase 4 additive seed.
- Viewer receives read-only permissions for properties/leads/customers/dashboard view, plus `notifications.read` from the Phase 4 additive seed.
- CRM Analytics intentionally reuses `dashboard.analytics`; no `crm.analytics` permission exists.
- Notification APIs enforce ownership in service methods, independent of role.
