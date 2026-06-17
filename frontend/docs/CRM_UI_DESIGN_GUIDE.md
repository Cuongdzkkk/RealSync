# CRM UI Design Guide

## Brand

- Primary CTA and selected state: Electric Yellow `#FACC15`.
- Hover yellow: `#EAB308`.
- AI accent: Cyan `#0EA5E9`, reserved for AI score or AI-related content.
- Do not flood dashboards with yellow. Yellow should guide the primary action.

## Semantic Colors

- Success: won, converted, completed actions.
- Warning: proposal, follow-up due today, high attention.
- Danger: lost, overdue, urgent priority, destructive actions.
- Info: contacted, upcoming follow-up, neutral operational state.
- Muted: missing values, unassigned state, inactive content.

## Typography

- UI font: Plus Jakarta Sans.
- Numeric and technical values: JetBrains Mono through `.numeric`.
- Keep dashboard text compact and scannable.

## Shape And Surface

- Cards use existing `glass-card` and `glass-card--hoverable`.
- Radius should stay around 8-12px.
- Borders use `--color-border` and `--color-border-strong`.
- Shadows remain light through `--elevation-surface` and `--elevation-raised`.

## Button Hierarchy

- Primary: Element Plus primary or yellow CTA for the main action.
- Secondary: default Element Plus button.
- Danger: delete or destructive confirmation only.
- Icon-only buttons need a tooltip or an aria label.

## Lead Status Mapping

- `New`: Mới, info/muted blue.
- `Contacted`: Đã liên hệ, info.
- `Qualified`: Đủ điều kiện, AI/cyan.
- `Proposal`: Đang đề xuất, warning.
- `Won`: Thành công, success.
- `Lost`: Thất bại, danger.

## Priority Mapping

- `Low`: Thấp, muted.
- `Normal`: Bình thường, info.
- `High`: Cao, warning.
- `Urgent`: Khẩn cấp, danger.

## Score Mapping

- `Hot`: score >= 70, danger.
- `Warm`: score >= 40 and < 70, warning.
- `Cold`: score < 40, info.

## Follow-up Mapping

- Overdue: danger.
- Today: warning.
- Upcoming within 7 days: info.
- No schedule: muted.
- Won/Lost leads can show follow-up as inactive instead of active overdue.

## Component Naming

- Shared CRM shell components live in `components/crm/common`.
- Lead-specific components live in `components/crm/leads`.
- Views compose components and call Pinia actions only.

## Mock-first Architecture

- Phase 1 uses `crmLeadMockService`.
- The service simulates the backend `ApiResponse` shape, async delay, filtering, sorting, pagination and mutation.
- Mock data persists in `localStorage` key `realsync.crm.mock.leads`.
- API integration must replace service calls later without changing page composition.

## API Boundary

- CRM Lead UI must not import Axios in this phase.
- No backend API request should be made from Lead List or Lead Detail.
- Customer, Notification, Analytics and API integration remain separate phases.

## Customer Source Mapping

- `Website`, `Facebook`, `Zalo`, `Phone`, `Referral`, `Other` use compact info badges.
- Converted customers use a success badge labeled `Chuyển đổi từ Lead`.
- Direct customers use muted/info treatment labeled `Khách hàng trực tiếp`.

## Customer Table Pattern

- First column combines initials, full name and phone/email.
- Company falls back to `Cá nhân`.
- Converted source should show the Lead name as a navigation action.
- Viewer role is read-only and must not see create, edit, assign or delete actions.

## Customer Activity History Pattern

- Customer activity is read-only.
- `Create` uses success, `Update` and `View` use info, `Assignment` uses AI/cyan, `Delete` uses danger, `Export` uses warning.
- Old/New values are parsed defensively; invalid JSON is rendered as plain text.
- There is no manual Customer activity creation in the UI because the backend contract only exposes read history.

## Notification Center Pattern

- Notification UI is mock-first and uses `realsync.crm.mock.notifications`.
- Notification bell reads unread count from Pinia; zero count hides the badge and counts above 99 render as `99+`.
- Dropdown shows 5-8 newest items with all/unread tabs, no polling, no SignalR and no backend calls.
- Notification links are resolved through a safe internal-path helper. External URLs, `javascript:`, `data:` and unknown paths fall back to `/admin/notifications`.
- Notification data JSON is parsed defensively; invalid JSON is shown as plain text in the detail drawer.
- Type mapping: `System` muted, `Lead` AI/cyan, `Property` success, `Task` warning, `Assignment` info.
- Viewer role can read notifications but cannot mark read, mark all read or delete.
- Follow-up notifications show warning for upcoming and danger for overdue based on `scheduledFor`.

## Responsive Rules

- Desktop: data-dense table and detail two-column layout.
- Tablet: filters move into drawer, detail becomes one column if space is tight.
- Mobile: drawer full width, Kanban scrolls horizontally, table may scroll horizontally.
- Activity timeline panel is closed by default on Lead routes through route meta.
