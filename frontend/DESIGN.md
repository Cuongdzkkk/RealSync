---
# ─── NAGFY / REALSYNC — DESIGN SYSTEM v3 ───────────────────────────────────
# Vietnamese Real Estate Data & Content Operating System
# Stack: Vue 3 + Vite + Element Plus + Tailwind CSS v4
# Style: "Minimal Editorial Light" — Urban.com × Propse Admin fusion
# Updated: 2026-05

meta:
  name: RealSync
  version: "3.0"
  style-direction: "Minimal Editorial Light — NOT dark, NOT glassmorphism"
  reference-images: "Urban.com (layout, typography, yellow accent) + Propse Admin (icon sidebar, pastel metrics)"
  context: dual-surface — Admin "War Room" + Public "Showroom"
  stack: Vue 3 + Element Plus + Tailwind CSS v4
  base-unit: 4px
  color-model: oklch + hex fallback

# ─── CRITICAL STYLE RULES (READ FIRST) ──────────────────────────────────────
style-rules:
  - "Light mode is THE default — no dark sidebar, no dark backgrounds in admin"
  - "Electric Yellow #F5E642 is the ONE AND ONLY accent — use once per screen max"
  - "Icon-only sidebar, 56px wide, no text labels (Propse pattern)"
  - "3-column asymmetric layout: Left 40% | Center 35% | Right 25%"
  - "Metric numbers 32–40px weight 600–700 — everything else small and muted"
  - "Zero glassmorphism, zero WebGL, zero mesh gradients, zero decorative effects"
  - "Card borders: 0.5–1px solid #E8E8E8 OR background contrast — no heavy shadows"
  - "Property images never have text overlay — only small badge in corner"
  - "No Cormorant Garamond anywhere — admin or public (this is editorial minimal, not luxury serif)"
  - "Inter or Plus Jakarta Sans only — clean, geometric, functional"
---

# Design System v3: RealSync (Nagfy)

## Overview

**Style Direction**: Minimal Editorial Light  
**Inspired by**: Urban.com (editorial density, Electric Yellow, large numbers) × Propse Admin (icon sidebar, pastel metric cards, data-dense whitespace)  
**Core principle**: Dữ liệu là ngôi sao. White space là không khí. Yellow là signal duy nhất.

**What this is NOT**:
- ❌ NOT dark mode
- ❌ NOT glassmorphism
- ❌ NOT luxury serif (Cormorant, etc.)
- ❌ NOT Techno-futurist (WebGL, mesh gradients)
- ❌ NOT bento grid tiles

**What this IS**:
- ✅ Flat white/off-white surfaces
- ✅ Single Electric Yellow accent
- ✅ Asymmetric 3-column layout
- ✅ Giant metric numbers, everything else small
- ✅ Icon-only compact sidebar
- ✅ Property cards with clean image + small badge only

---

## 1. Color System

### Core Palette

```css
@layer base {
  :root {
    /* ── SURFACES ─────────────────────────────────────────────── */
    --color-canvas:         #FAFAFA;   /* App background — off-white */
    --color-surface:        #FFFFFF;   /* Cards, panels */
    --color-surface-hover:  #F5F5F5;   /* Row hover, subtle hover state */

    /* ── TEXT ─────────────────────────────────────────────────── */
    --color-text-primary:   #0D0D0D;   /* Near black — body, headlines */
    --color-text-secondary: #6B6B6B;   /* Supporting, labels, meta */
    --color-text-muted:     #ABABAB;   /* Placeholders, disabled, timestamps */

    /* ── BORDERS ──────────────────────────────────────────────── */
    --color-border:         #E8E8E8;   /* Universal border — 0.5–1px */
    --color-border-strong:  #D0D0D0;   /* Table headers, section dividers */
    --color-divider:        #F0F0F0;   /* Ultra-subtle row separators */

    /* ── ACCENT — ELECTRIC YELLOW (Urban DNA) ─────────────────── */
    --color-yellow:         #F5E642;   /* THE single accent color */
    --color-yellow-hover:   #EDD800;   /* Yellow on hover/active */
    --color-yellow-muted:   #FEFBCC;   /* Very light yellow tint bg */
    --color-yellow-text:    #0D0D0D;   /* Text ON yellow background */

    /* ── METRIC CARD PASTELS (Propse DNA) ────────────────────── */
    --color-pastel-pink:    #F0D0DC;   /* Total Revenue card bg */
    --color-pastel-blue:    #D4E8F0;   /* Cost/expense card bg */
    --color-pastel-green:   #D4EDD8;   /* Growth/positive card bg */
    --color-pastel-peach:   #F0E0D0;   /* Neutral metric card bg */

    /* ── STATUS SEMANTIC ──────────────────────────────────────── */
    --color-success:        #16A34A;
    --color-success-bg:     #F0FDF4;
    --color-success-border: #BBF7D0;
    --color-warning:        #D97706;
    --color-warning-bg:     #FFFBEB;
    --color-warning-border: #FDE68A;
    --color-danger:         #DC2626;
    --color-danger-bg:      #FEF2F2;
    --color-danger-border:  #FECACA;
    --color-info:           #2563EB;
    --color-info-bg:        #EFF6FF;
    --color-info-border:    #BFDBFE;

    /* ── AI MODULE ────────────────────────────────────────────── */
    --color-ai:             #0891B2;
    --color-ai-bg:          #ECFEFF;
    --color-ai-border:      #A5F3FC;

    /* ── CHART COLORS (limited palette) ──────────────────────── */
    --color-chart-primary:  #0D0D0D;   /* Active bars */
    --color-chart-secondary:#4CAF50;   /* Second data series (green only) */
    --color-chart-inactive: #E0E0E0;   /* Inactive/background bars */
    --color-chart-yellow:   #F5E642;   /* Highlighted bar/point */

    /* ── SIDEBAR ──────────────────────────────────────────────── */
    --color-sidebar-bg:     #FFFFFF;   /* White sidebar, not dark */
    --color-sidebar-border: #E8E8E8;   /* Right border of sidebar */
    --color-sidebar-icon:   #9E9E9E;   /* Default icon color */
    --color-sidebar-active: #0D0D0D;   /* Active icon color */

    /* ── FOCUS RING ───────────────────────────────────────────── */
    --ring-color:           rgba(245,230,66,0.60);   /* Yellow focus ring */
    --ring-offset:          2px;

    /* ── MOTION ───────────────────────────────────────────────── */
    --duration-fast:        120ms;
    --duration-base:        200ms;
    --duration-slow:        350ms;
    --ease-standard:        cubic-bezier(0.4, 0, 0.2, 1);
  }
}
```

### Element Plus Overrides

```css
/* assets/styles/element-override.scss */
:root {
  --el-color-primary:           #0D0D0D;
  --el-bg-color:                #FFFFFF;
  --el-bg-color-page:           #FAFAFA;
  --el-border-color:            #E8E8E8;
  --el-border-color-darker:     #D0D0D0;
  --el-text-color-primary:      #0D0D0D;
  --el-text-color-secondary:    #6B6B6B;
  --el-text-color-placeholder:  #ABABAB;
  --el-border-radius-base:      8px;
  --el-border-radius-small:     6px;
  --el-font-size-base:          13px;
  --el-font-family:             'Plus Jakarta Sans', system-ui, sans-serif;
  --el-fill-color-blank:        #FFFFFF;
  --el-fill-color-light:        #FAFAFA;
}
```

---

## 2. Typography

### Font Stack

```html
<!-- Only these 2 font families. No serif. No Cormorant. -->
<link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&family=JetBrains+Mono:wght@400&display=swap" rel="stylesheet">
```

### Typography Scale

| Role | Font | Size | Weight | LH | Notes |
|---|---|---|---|---|---|
| **Hero Metric** | Plus Jakarta Sans | 38px | 700 | 1.0 | Tabular nums, `–0.02em` LS |
| **Large Metric** | Plus Jakarta Sans | 28px | 700 | 1.0 | Tabular nums |
| **Section Title** | Plus Jakarta Sans | 20px | 700 | 1.2 | Page-level heading |
| **Card Title** | Plus Jakarta Sans | 15px | 600 | 1.3 | |
| **Body Default** | Plus Jakarta Sans | 13px | 400 | 1.5 | Main body size |
| **Body Medium** | Plus Jakarta Sans | 13px | 500 | 1.5 | Navigation, labels |
| **Caption** | Plus Jakarta Sans | 12px | 400 | 1.4 | Meta, timestamps |
| **Micro Label** | Plus Jakarta Sans | 10px | 500 | 1.0 | Uppercase, `0.05em` LS |
| **Badge** | Plus Jakarta Sans | 11px | 600 | 1.0 | Uppercase, `0.04em` LS |
| **Mono** | JetBrains Mono | 12px | 400 | 1.6 | URLs, AI scores, logs |

### Rules
- **Font size minimum**: 12px for any displayed text (13px for body — Vietnamese diacritics need space)
- **Tabular nums**: `font-variant-numeric: tabular-nums` — ALL prices and metrics
- **Max metric size**: 40px — larger than that loses admin density contract
- **Uppercase labels**: always `letter-spacing: 0.04–0.05em`
- **NO Cormorant Garamond** — anywhere, admin or public

---

## 3. Layout System

### Admin Layout Structure

```
┌──────────────────────────────────────────────────────────────────┐
│  SIDEBAR   │  TOP BAR (60px, sticky)                             │
│  56px      ├─────────────────────────────────────────────────────┤
│  icon-only │                                                       │
│            │  LEFT (40%)          CENTER (35%)    RIGHT (25%)    │
│  [icon]    │  ─────────────────────────────────────────────────  │
│  [icon]    │  Main list/table     Charts/maps     Quick stats    │
│  [icon]    │  Property cards      Analytics       Messages       │
│  [icon]    │  Lead list           AI outputs      Alerts         │
│            │                                                       │
│  [icon]    │  Sticky right panel — always visible, no drawer     │
│  [icon]    │                                                       │
└──────────────────────────────────────────────────────────────────┘
```

```css
.admin-shell {
  display: flex;
  min-height: 100dvh;
  background: var(--color-canvas);
}

/* SIDEBAR: icon-only, 56px, white, left border-right */
.admin-sidebar {
  width: 56px;
  min-height: 100dvh;
  background: var(--color-surface);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 12px 0;
  gap: 4px;
  position: sticky;
  top: 0;
  z-index: 100;
  flex-shrink: 0;
}

/* TOP BAR */
.admin-topbar {
  height: 60px;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  padding: 0 24px;
  gap: 16px;
  position: sticky;
  top: 0;
  z-index: 90;
}

/* MAIN AREA */
.admin-main {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

/* 3-COLUMN ASYMMETRIC CONTENT AREA */
.admin-content {
  flex: 1;
  display: grid;
  grid-template-columns: 2fr 1.75fr 1.25fr;   /* ~40% 35% 25% */
  min-height: calc(100dvh - 60px);
}

/* Column dividers */
.col-left   { padding: 24px; border-right: 1px solid var(--color-border); }
.col-center { padding: 24px; border-right: 1px solid var(--color-border); }
.col-right  { padding: 24px; }

/* Responsive: collapse to 2-col at 1200px, 1-col at 768px */
@media (max-width: 1200px) {
  .admin-content { grid-template-columns: 1fr 1fr; }
  .col-right { border-top: 1px solid var(--color-border); grid-column: 1/-1; }
}
@media (max-width: 768px) {
  .admin-content { grid-template-columns: 1fr; }
}
```

---

## 4. Components

### 4.1 Sidebar (Icon-Only — Propse Pattern)

```css
.sidebar-logo {
  width: 36px;
  height: 36px;
  background: var(--color-yellow);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
}

.sidebar-icon-btn {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  color: var(--color-sidebar-icon);   /* #9E9E9E muted */
  border: none;
  background: transparent;
  cursor: pointer;
  transition: background var(--duration-fast) var(--ease-standard),
              color var(--duration-fast) var(--ease-standard);
  position: relative;
}

.sidebar-icon-btn:hover {
  background: var(--color-surface-hover);   /* #F5F5F5 */
  color: var(--color-text-primary);
}

.sidebar-icon-btn.active {
  background: var(--color-yellow);   /* Electric yellow fill */
  color: var(--color-yellow-text);   /* #0D0D0D */
}

/* Tooltip on hover — replaces text labels */
.sidebar-icon-btn::after {
  content: attr(data-tooltip);
  position: absolute;
  left: calc(100% + 8px);
  top: 50%;
  transform: translateY(-50%);
  background: #0D0D0D;
  color: #FFFFFF;
  font: 500 12px/1 'Plus Jakarta Sans';
  padding: 4px 8px;
  border-radius: 4px;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity var(--duration-fast);
  z-index: 200;
}
.sidebar-icon-btn:hover::after { opacity: 1; }

/* Sidebar divider */
.sidebar-divider {
  width: 32px;
  height: 1px;
  background: var(--color-border);
  margin: 4px 0;
}

/* Sidebar bottom — avatar */
.sidebar-avatar {
  margin-top: auto;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  overflow: hidden;
  cursor: pointer;
}
```

### 4.2 Top Navigation Bar (Urban Pattern)

```css
.admin-topbar {
  /* See layout above */
}

/* Page title in topbar */
.topbar-title {
  font: 700 18px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

/* Tab navigation — Urban active pill style */
.topbar-tabs {
  display: flex;
  gap: 4px;
  align-items: center;
}

.topbar-tab {
  font: 500 13px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  padding: 6px 14px;
  border-radius: 20px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
  white-space: nowrap;
}

.topbar-tab:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

.topbar-tab.active {
  background: var(--color-text-primary);   /* Dark filled pill */
  color: #FFFFFF;
}

/* CTA in topbar — Urban "+ New" style */
.topbar-cta {
  font: 600 13px/1 'Plus Jakarta Sans';
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  border-radius: 20px;
  padding: 8px 16px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background var(--duration-fast) var(--ease-standard);
  margin-left: auto;
}
.topbar-cta:hover { background: var(--color-yellow-hover); }
```

### 4.3 Metric Cards

**Pastel Metric Card (Propse Pattern)**
```css
.metric-card {
  border-radius: 12px;
  padding: 20px 24px;
  border: none;   /* No border — background contrast does the work */
}

/* Variants — use background to differentiate, not border or icon color */
.metric-card--pink   { background: var(--color-pastel-pink); }
.metric-card--blue   { background: var(--color-pastel-blue); }
.metric-card--green  { background: var(--color-pastel-green); }
.metric-card--peach  { background: var(--color-pastel-peach); }
.metric-card--white  { background: var(--color-surface); border: 1px solid var(--color-border); }

.metric-card-label {
  font: 500 11px/1 'Plus Jakarta Sans';
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  margin-bottom: 10px;
}

.metric-card-value {
  font: 700 34px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
  font-variant-numeric: tabular-nums;
}

.metric-card-delta {
  font: 500 12px/1 'Plus Jakarta Sans';
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 4px;
}
.metric-card-delta--up   { color: var(--color-success); }
.metric-card-delta--down { color: var(--color-danger); }

/* Metric grid — 2 columns in left panel */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}
```

### 4.4 Property List Item (Urban Pattern)

```css
/* Left column list item — NOT a card, a table row with image */
.property-list-item {
  display: grid;
  grid-template-columns: 88px 1fr auto;
  gap: 12px;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid var(--color-divider);
  cursor: pointer;
  transition: background var(--duration-fast);
  border-radius: 6px;
  margin: 0 -8px;
  padding-left: 8px;
  padding-right: 8px;
}
.property-list-item:hover { background: var(--color-surface-hover); }

/* Thumbnail */
.property-thumb {
  width: 88px;
  height: 64px;
  border-radius: 8px;
  object-fit: cover;
  flex-shrink: 0;
  position: relative;
}

/* Status badge — overlay BOTTOM-LEFT only, small */
.property-status-badge {
  position: absolute;
  bottom: 6px;
  left: 6px;
  font: 600 10px/1 'Plus Jakarta Sans';
  letter-spacing: 0.04em;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(13,13,13,0.75);
  color: #FFFFFF;
  backdrop-filter: blur(4px);
}

/* Property info */
.property-price {
  font: 700 16px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
  font-variant-numeric: tabular-nums;
}

.property-specs {
  font: 400 12px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  margin-top: 3px;
  display: flex;
  gap: 8px;
}

.property-address {
  font: 400 12px/1.4 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  margin-top: 4px;
}

/* Analytics sparkline column */
.property-analytics {
  text-align: right;
  min-width: 80px;
}
.property-views {
  font: 400 11px/1 'Plus Jakarta Sans';
  color: var(--color-text-muted);
}
```

### 4.5 Property Grid Card (Propse Pattern)

```css
/* Used in center/full-width views */
.property-card {
  background: var(--color-surface);
  border-radius: 12px;
  border: 1px solid var(--color-border);
  overflow: hidden;
  cursor: pointer;
  transition: border-color var(--duration-base) var(--ease-standard),
              box-shadow var(--duration-base) var(--ease-standard);
}
.property-card:hover {
  border-color: var(--color-border-strong);
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
}

.property-card-image {
  aspect-ratio: 4/3;
  width: 100%;
  object-fit: cover;
  display: block;
}

/* NO text overlay on image — only small badge bottom-left */
.property-card-badge {
  position: absolute;
  bottom: 8px;
  left: 8px;
  font: 600 10px/1 'Plus Jakarta Sans';
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding: 3px 8px;
  border-radius: 4px;
  /* Rented: success colors / Vacant: muted */
}
.property-card-badge--rented  { background: var(--color-success-bg); color: var(--color-success); }
.property-card-badge--vacant  { background: #F4F4F5; color: #71717A; }
.property-card-badge--active  { background: var(--color-yellow-muted); color: #6B5800; }

.property-card-body {
  padding: 14px 16px;
}

.property-card-name {
  font: 600 14px/1.3 'Plus Jakarta Sans';
  color: var(--color-text-primary);
}

.property-card-meta {
  font: 400 12px/1.4 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  margin-top: 4px;
}

.property-card-specs {
  font: 500 12px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  margin-top: 8px;
  display: flex;
  gap: 10px;
}

/* Grid layout for cards */
.property-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}
@media (max-width: 900px) { .property-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 580px) { .property-grid { grid-template-columns: 1fr; } }
```

### 4.6 Status Badges

```css
/* Base badge */
.badge {
  display: inline-flex;
  align-items: center;
  font: 600 10px/1 'Plus Jakarta Sans';
  letter-spacing: 0.04em;
  text-transform: uppercase;
  padding: 3px 8px;
  border-radius: 4px;
  white-space: nowrap;
}

/* Pastel variants — Propse style (no harsh colors) */
.badge--rented   { background: var(--color-success-bg); color: var(--color-success); }
.badge--vacant   { background: #F4F4F5; color: #71717A; }
.badge--active   { background: var(--color-yellow-muted); color: #6B5800; }
.badge--pending  { background: var(--color-info-bg); color: var(--color-info); }
.badge--expired  { background: #F4F4F5; color: #A1A1AA; }
.badge--hot      { background: var(--color-danger-bg); color: var(--color-danger); }
.badge--warm     { background: var(--color-warning-bg); color: var(--color-warning); }
.badge--cold     { background: var(--color-info-bg); color: var(--color-info); }
.badge--ai       { background: var(--color-ai-bg); color: var(--color-ai); }

/* Active pulse badge */
.badge--live::before {
  content: "";
  display: inline-block;
  width: 6px; height: 6px;
  background: var(--color-success);
  border-radius: 9999px;
  margin-right: 5px;
  animation: pulse 1.5s cubic-bezier(0.4,0,0.6,1) infinite;
}
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.35} }
```

### 4.7 Data Table (Admin)

```css
.data-table {
  width: 100%;
  border-collapse: collapse;
  font: 400 13px/1.5 'Plus Jakarta Sans';
}

.data-table th {
  font: 500 10px/1 'Plus Jakarta Sans';
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border-strong);
  padding: 8px 12px;
  text-align: left;
  white-space: nowrap;
}

.data-table td {
  padding: 0 12px;
  color: var(--color-text-primary);
  border-bottom: 1px solid var(--color-divider);
}

.data-table tr { height: 48px; }
.data-table tbody tr:hover td { background: var(--color-surface-hover); }

/* Mono columns */
.data-table td.mono {
  font: 400 12px/1.5 'JetBrains Mono';
  color: var(--color-text-secondary);
}

/* Pagination — Urban style */
.table-pagination {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 16px 0;
}

.page-btn {
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  font: 500 13px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  background: var(--color-surface);
  cursor: pointer;
}
.page-btn.active {
  background: var(--color-text-primary);
  color: #FFFFFF;
  border-color: var(--color-text-primary);
}
.page-btn:hover:not(.active) { background: var(--color-surface-hover); }
```

### 4.8 Charts (Urban Style)

```css
/* Chart container */
.chart-container {
  background: var(--color-surface);
  border-radius: 12px;
  border: 1px solid var(--color-border);
  padding: 20px 24px;
}

.chart-title {
  font: 600 14px/1.3 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  margin-bottom: 4px;
}
.chart-subtitle {
  font: 400 12px/1.4 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  margin-bottom: 20px;
}

/* Chart stat summary (Urban pattern — big number + legend inline) */
.chart-hero-value {
  font: 700 36px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
  font-variant-numeric: tabular-nums;
}

/* Bar chart rules */
/* - Max 2 colors per chart */
/* - Active bar: #0D0D0D (near black) or #F5E642 (yellow highlight) */
/* - Inactive bars: #E0E0E0 (gray) */
/* - No legend icons — use inline colored dots with text */

.chart-legend {
  display: flex;
  gap: 16px;
  font: 400 12px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
}
.chart-legend-dot {
  width: 8px; height: 8px;
  border-radius: 2px;
  display: inline-block;
  margin-right: 5px;
}
```

### 4.9 Right Panel — Messages / Quick Stats

```css
/* Always-visible right panel — no drawer, no click to open */
.right-panel {
  background: var(--color-surface);
  border-left: 1px solid var(--color-border);
  height: 100%;
  overflow-y: auto;
  padding: 0;
}

.right-panel-section {
  padding: 20px;
  border-bottom: 1px solid var(--color-border);
}

.right-panel-section-title {
  font: 600 13px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
  margin-bottom: 14px;
}

/* Message item */
.message-item {
  display: flex;
  gap: 10px;
  padding: 10px 0;
  border-bottom: 1px solid var(--color-divider);
  cursor: pointer;
}
.message-item:last-child { border-bottom: none; }

.message-avatar {
  width: 32px; height: 32px;
  border-radius: 50%;
  background: var(--color-surface-hover);
  flex-shrink: 0;
  overflow: hidden;
}

.message-name {
  font: 600 13px/1 'Plus Jakarta Sans';
  color: var(--color-text-primary);
}
.message-preview {
  font: 400 12px/1.4 'Plus Jakarta Sans';
  color: var(--color-text-muted);
  margin-top: 2px;
}
.message-time {
  font: 400 11px/1 'Plus Jakarta Sans';
  color: var(--color-text-muted);
  margin-left: auto;
  flex-shrink: 0;
}

/* Tab switcher in right panel */
.panel-tabs {
  display: flex;
  gap: 2px;
  padding: 12px 20px;
  border-bottom: 1px solid var(--color-border);
}
.panel-tab {
  font: 600 12px/1 'Plus Jakarta Sans';
  padding: 5px 12px;
  border-radius: 20px;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
}
.panel-tab.active {
  background: var(--color-text-primary);
  color: #FFFFFF;
}
```

### 4.10 Buttons

```css
/* Primary — Yellow (one per screen) */
.btn-primary {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  font: 600 13px/1 'Plus Jakarta Sans';
  padding: 8px 18px;
  border-radius: 20px;
  border: none;
  cursor: pointer;
  display: inline-flex; align-items: center; gap: 6px;
  transition: background var(--duration-fast) var(--ease-standard);
}
.btn-primary:hover { background: var(--color-yellow-hover); }

/* Secondary — Outlined */
.btn-secondary {
  background: var(--color-surface);
  color: var(--color-text-primary);
  font: 500 13px/1 'Plus Jakarta Sans';
  padding: 8px 16px;
  border-radius: 20px;
  border: 1px solid var(--color-border);
  cursor: pointer;
  transition: background var(--duration-fast);
}
.btn-secondary:hover { background: var(--color-surface-hover); }

/* Dark — for destructive/important actions */
.btn-dark {
  background: var(--color-text-primary);
  color: #FFFFFF;
  font: 600 13px/1 'Plus Jakarta Sans';
  padding: 8px 18px;
  border-radius: 20px;
  border: none;
  cursor: pointer;
  transition: opacity var(--duration-fast);
}
.btn-dark:hover { opacity: 0.85; }

/* Filter/tab chips (Urban listing filter) */
.filter-chip {
  font: 500 13px/1 'Plus Jakarta Sans';
  color: var(--color-text-secondary);
  padding: 5px 14px;
  border-radius: 20px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  cursor: pointer;
}
.filter-chip.active {
  background: var(--color-text-primary);
  color: #FFFFFF;
  border-color: var(--color-text-primary);
}
```

---

## 5. AI Module

```css
.ai-module {
  background: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  border-radius: 8px;
  padding: 16px 20px;
}

.ai-score {
  font: 400 12px/1.6 'JetBrains Mono';
  color: var(--color-ai);
}

/* Lead Score: 0–40 cold, 41–70 warm, 71–100 hot */
.ai-streaming-cursor::after {
  content: "▋";
  color: var(--color-ai);
  animation: blink 0.7s step-end infinite;
}
@keyframes blink { 50% { opacity: 0; } }
```

---

## 6. Focus & Accessibility

```css
:focus-visible {
  outline: 2px solid var(--ring-color);   /* Yellow focus ring */
  outline-offset: var(--ring-offset);
  border-radius: inherit;
}

@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

- Minimum font size: **12px** displayed, **13px** for interactive
- Minimum tap target: **44×44px**
- All color pairs: WCAG AA verified
- Sidebar icon-only: **required** `aria-label` + `data-tooltip`

---

## 7. Skeleton Loading

```css
.skeleton {
  background: linear-gradient(
    90deg,
    #F0F0F0 25%,
    #FAFAFA 50%,
    #F0F0F0 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 4px;
}
@keyframes shimmer {
  from { background-position: 200% 0; }
  to   { background-position: -200% 0; }
}
```

---

## 8. Breakpoints

| Name | Width | Behavior |
|---|---|---|
| `xs` | < 480px | Single column, hidden sidebar |
| `sm` | 480–767px | Sidebar → hamburger overlay |
| `md` | 768–1199px | 2-column layout, right panel moves below |
| `lg` | 1200–1439px | Full 3-column, sidebar visible |
| `xl` | ≥1440px | Max-width content areas |

### Mobile Admin (< 768px)
- Sidebar: hidden → hamburger → full-width overlay drawer
- 3-column → single column scroll
- Metric cards: 2×2 grid
- Right panel: collapsible accordion section

---

## 9. Do's and Don'ts

### ✅ Do
- White/off-white backgrounds ALWAYS in admin
- `#F5E642` yellow — one CTA or active state per screen, not more
- Icon-only sidebar, 56px, white background with border-right
- 3-column asymmetric: left 40%, center 35%, right 25%
- Metric values 28–38px, bold, tabular-nums
- Property image: clean 4:3, only small badge bottom-left
- Pastel card backgrounds to differentiate metric types (no colored borders)
- Border `0.5–1px solid #E8E8E8` — very subtle, not 2px
- Skeleton loader for data-heavy sections
- `font-variant-numeric: tabular-nums` on ALL numbers
- `:focus-visible` with yellow ring
- Chart: max 2 colors, inactive bars = #E0E0E0

### ❌ Don't
- ❌ Dark sidebar, dark backgrounds
- ❌ Glassmorphism, backdrop-filter (except tiny amounts)
- ❌ WebGL, canvas animations, mesh gradients
- ❌ Cormorant Garamond or any serif font
- ❌ Heavy box-shadows (max 1 subtle layer)
- ❌ Text overlaid on property images (only small badge)
- ❌ Multiple accent colors — yellow is the ONLY accent
- ❌ `border-radius > 16px` anywhere in admin
- ❌ Gradients on solid surfaces
- ❌ Charts with more than 2 color series
- ❌ AI teal outside AI-specific modules
- ❌ `font-size < 12px` for any visible text
- ❌ Bento grid tile layout — use asymmetric columns instead
- ❌ `outline: none` without `:focus-visible` replacement

---

## 10. Agent Prompt Quick Reference

### Token Reference (always use CSS var, never inline hex)

```
SURFACES:
--color-canvas:         #FAFAFA   (app bg, warm off-white)
--color-surface:        #FFFFFF   (cards, panels)
--color-surface-hover:  #F5F5F5   (row hover, subtle)

TEXT:
--color-text-primary:   #0D0D0D   (near black)
--color-text-secondary: #6B6B6B   (supporting)
--color-text-muted:     #ABABAB   (placeholders)

BORDERS:
--color-border:         #E8E8E8   (universal, 1px)
--color-border-strong:  #D0D0D0   (emphasis)
--color-divider:        #F0F0F0   (row separators)

ACCENT (one per screen max):
--color-yellow:         #F5E642   (Electric Yellow)
--color-yellow-hover:   #EDD800
--color-yellow-muted:   #FEFBCC   (light tint bg)
--color-yellow-text:    #0D0D0D   (text on yellow)

METRIC CARD PASTELS:
--color-pastel-pink:    #F0D0DC   (revenue)
--color-pastel-blue:    #D4E8F0   (costs)
--color-pastel-green:   #D4EDD8   (growth)
--color-pastel-peach:   #F0E0D0   (neutral)

STATUS:
--color-success:        #16A34A
--color-warning:        #D97706
--color-danger:         #DC2626
--color-info:           #2563EB
--color-ai:             #0891B2   (AI modules only)

CHART:
Active bar:    #0D0D0D or #F5E642
Inactive bar:  #E0E0E0
Second series: #4CAF50
```

### Component Prompt Templates

**Sidebar (icon-only 56px white):**
> White sidebar 56px wide, right border `1px solid #E8E8E8`. Logo: 36×36px `#F5E642` rounded square. Icons 20px, default color `#9E9E9E`, hover: `#F5F5F5` bg + `#0D0D0D` icon. Active: `#F5E642` bg + `#0D0D0D` icon. Tooltip on hover (no text labels). Avatar bottom.

**Topbar with tabs (Urban pattern):**
> White topbar 60px, bottom border `1px solid #E8E8E8`. Tabs: 13px/500 `#6B6B6B`, active = filled dark pill `#0D0D0D bg + white text`, border-radius 20px. "+ New" CTA: `#F5E642` bg, `#0D0D0D` text, 13px/600, radius 20px, right-aligned.

**Metric card pastel:**
> No border, radius 12px. Pastel bg (`#F0D0DC` pink / `#D4E8F0` blue etc). Label: 10px uppercase/500 `#6B6B6B`. Value: 34px/700 `#0D0D0D` tabular-nums. Delta: 12px success/danger color.

**Property list item (Urban left column):**
> No card border — row items separated by `1px solid #F0F0F0`. 3-col grid: 88×64px thumbnail radius 8px + info + analytics. Price: 16px/700 `#0D0D0D` tabular-nums. Specs + address: 12px `#6B6B6B`. Status badge: bottom-left of image only, 10px uppercase. Row hover: `#F5F5F5` bg.

**Property grid card (Propse pattern):**
> White card, radius 12px, `1px solid #E8E8E8` border. Image 4:3, no text overlay, only small badge bottom-left (`rented`=success, `vacant`=gray, `active`=yellow-muted). Card name: 14px/600 `#0D0D0D`. Meta: 12px `#6B6B6B`. Hover: border darkens to `#D0D0D0`, shadow `0 2px 12px rgba(0,0,0,0.06)`.

**Chart container (Urban pattern):**
> White card, radius 12px, `1px solid #E8E8E8`. Title 14px/600 `#0D0D0D`. Hero metric 36px/700 tabular-nums. Bar chart: active bars `#0D0D0D`, inactive `#E0E0E0`. Max 2 colors. Legend: small 8px square dot inline with 12px text.

**3-column layout:**
> Admin content area: `grid-template-columns: 2fr 1.75fr 1.25fr` (~40/35/25%). Dividers: `1px solid #E8E8E8` between columns. Left: main list + filters. Center: charts + overview. Right: always-visible messages panel, no drawer.

**AI module:**
> `#ECFEFF` bg, `1px solid #A5F3FC` border, radius 8px. Score in JetBrains Mono `#0891B2`. Lead: hot=danger, warm=warning, cold=info badges. Streaming cursor `▋` blink in `#0891B2`.

---

## 11. Changelog from v2

| Issue | v2 | v3 |
|---|---|---|
| Mode | Light mode (admin sidebar still dark) | **Full light mode — white sidebar** |
| Sidebar | 240px with text labels, dark bg | **56px icon-only, white bg** |
| Accent | Gold `#C9A96E` | **Electric Yellow `#F5E642`** |
| Typography | Cormorant Garamond for public | **No serif — Plus Jakarta Sans only** |
| Layout | Flex sidebar + scrollable main | **3-col asymmetric grid** |
| Metric cards | White cards with borders | **Pastel bg, no border** |
| Property cards | Text overlay on image | **No overlay — badge bottom-left only** |
| Buttons | Radius 6px squared | **Radius 20px pill style** |
| Charts | Complex multi-color | **Max 2 colors, inactive=gray** |
| Right panel | Drawer/modal | **Always-visible sticky right col** |
| Style direction | Techno-futurist dark | **Minimal Editorial Light** |