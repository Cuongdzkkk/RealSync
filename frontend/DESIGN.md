# Design System: RealSync (Nagfy)

> Version: 2.0
> Updated: 2026-05
> Format: DESIGN.md v2 compatible

## Overview

RealSync is a Vietnamese real estate data platform with two distinct faces: a power-user admin dashboard for internal operations (crawler management, AI lead scoring, listing pipeline) and a public-facing property marketplace. The design system must serve both without compromise — clinical precision for the admin, aspirational warmth for the public.

**Inspired by**: Linear (admin data density) × Airbnb (public property emotion) × Stripe (component polish)

**Stack**: Vue 3 + Element Plus + Tailwind CSS (token override layer)

---

## 1. Visual Theme & Atmosphere

RealSync's design language is built around **two distinct but harmonized contexts**:

**Admin Context — "The War Room"**
A high-information-density workspace where data is the hero. Clean white surface with subtle cool-gray structure. Every pixel is accountable. The feeling should be: a Bloomberg terminal that went to design school. Sidebar navigation is compact and persistent. Tables breathe with 14px body text and clear column hierarchy. Status badges communicate at a glance. No decorative elements — trust is built through clarity.

**Public Context — "The Showroom"**
An aspirational browsing experience where properties are the star. Generous white space. Large photography. Serif display headlines that feel premium and editorial. The feeling should be: a high-end architecture magazine that happens to let you book a viewing. Warm off-white canvas. Gold accent (not orange) signals quality, not urgency. CTAs are confident, not desperate.

**Unifying Thread**: Both contexts share the same typographic backbone (Plus Jakarta Sans), the same border-radius discipline, and the same deep midnight primary color — which reads as "corporate authority" in the admin and "quiet luxury" in the public view.

**Key Characteristics:**
- Deep midnight primary (`#0D1B2A`) — authority without aggression
- Warm gold accent (`#C9A96E`) — luxury signal, not e-commerce urgency
- Off-white warm canvas (`#FAFAF8`) — photography-friendly, not clinical
- Plus Jakarta Sans for all UI text — modern, legible, Vietnamese-friendly
- Cormorant Garamond for public display headlines only — editorial prestige
- Border radius: 6px system-wide for components, 12px for cards, 20px for public CTAs
- Shadows: Notion-style (barely there, directional) not Material (floating)
- No gradients in the admin. Subtle warm gradients allowed on public hero only

---

## 2. Color Palette & Roles

### Primary Brand

| Token | Hex | Role |
|---|---|---|
| `--color-primary` | `#0D1B2A` | Sidebar bg, nav, page titles, primary buttons |
| `--color-primary-soft` | `#1A3550` | Sidebar hover state, active nav items |
| `--color-primary-10` | `rgba(13,27,42,0.10)` | Selected row, tinted backgrounds |
| `--color-primary-06` | `rgba(13,27,42,0.06)` | Table row hover |
| `--color-primary-04` | `rgba(13,27,42,0.04)` | Ultra-subtle highlight |

### Accent (Gold System)

| Token | Hex | Role |
|---|---|---|
| `--color-gold` | `#C9A96E` | Public CTA text, price emphasis, premium badge |
| `--color-gold-light` | `#F5EDD9` | Gold tinted surface, hover bg on public |
| `--color-gold-dark` | `#9E7A42` | Gold text on light bg for contrast compliance |

### Status Colors

| Token | Hex | Role |
|---|---|---|
| `--color-success` | `#16A34A` | Active crawler, verified listing, hot lead |
| `--color-success-bg` | `#F0FDF4` | Success badge background |
| `--color-warning` | `#D97706` | Warm lead, pending AI review |
| `--color-warning-bg` | `#FFFBEB` | Warning badge background |
| `--color-danger` | `#DC2626` | Error, overwrite action, expired listing |
| `--color-danger-bg` | `#FEF2F2` | Danger badge background |
| `--color-info` | `#2563EB` | Cold lead, system info, AI processing |
| `--color-info-bg` | `#EFF6FF` | Info badge background |

### Surface & Neutral

| Token | Hex | Role |
|---|---|---|
| `--color-canvas` | `#FAFAF8` | App background (admin + public) |
| `--color-surface` | `#FFFFFF` | Cards, panels, modals |
| `--color-border` | `#E4E4E7` | All borders — subtle, consistent |
| `--color-border-strong` | `#D1D1D6` | Table headers, dividers that need weight |
| `--color-text-primary` | `#0D1B2A` | Headings, data values, active labels |
| `--color-text-secondary` | `#52525B` | Supporting text, timestamps, subtext |
| `--color-text-muted` | `#A1A1AA` | Placeholders, disabled states |

### AI Module Accent

| Token | Hex | Role |
|---|---|---|
| `--color-ai` | `#0891B2` | AI-generated badge, AI Score indicator |
| `--color-ai-bg` | `#ECFEFF` | AI module tinted surface |

---

## 3. Typography Rules

### Font Families

- **UI / Body / Admin**: `Plus Jakarta Sans` — weights 400, 500, 600, 700
- **Public Display / Hero**: `Cormorant Garamond` — weights 400, 600 (italic optional)
- **Monospace / Code / Crawler logs**: `JetBrains Mono`
- **Fallback**: `system-ui, -apple-system, sans-serif`

### Scale

| Role | Font | Size | Weight | Line Height | Letter Spacing | Context |
|---|---|---|---|---|---|---|
| Public Hero H1 | Cormorant Garamond | 56–72px | 400 | 1.05 | -0.02em | Public listing hero |
| Public H2 | Cormorant Garamond | 36px | 600 | 1.15 | -0.01em | Section titles public |
| Admin Page Title | Plus Jakarta Sans | 20px | 700 | 1.20 | -0.01em | Admin page header |
| Admin Section H2 | Plus Jakarta Sans | 16px | 600 | 1.30 | 0 | Card titles, modal titles |
| Body Default | Plus Jakarta Sans | 14px | 400 | 1.50 | 0 | Table cells, form labels |
| Body Strong | Plus Jakarta Sans | 14px | 600 | 1.50 | 0 | Data values, property price |
| Caption / Meta | Plus Jakarta Sans | 12px | 400 | 1.40 | 0.01em | Timestamps, source tags |
| Badge / Label | Plus Jakarta Sans | 11px | 600 | 1.00 | 0.04em uppercase | Status badges |
| Mono / Log | JetBrains Mono | 12px | 400 | 1.60 | 0 | Crawler logs, AI scores raw |
| Price Emphasis | Plus Jakarta Sans | 24px | 700 | 1.10 | -0.02em | Listing price display |

### Principles

- Vietnamese text renders cleanly at 14px+ with Plus Jakarta Sans — do not go below 13px
- Cormorant Garamond is **public-facing display only** — never use in admin context
- Uppercase labels always use `letter-spacing: 0.04em`
- Price and metric numbers: always `font-variant-numeric: tabular-nums`
- Line lengths: max 65 characters for body paragraphs in public view

---

## 4. Component Stylings

### Buttons

**Admin Primary**
- Background: `#0D1B2A`
- Text: `#FFFFFF`, Plus Jakarta Sans 14px weight 600
- Padding: `8px 16px`, Radius: `6px`
- Hover: `#1A3550`, Shadow: `0 1px 2px rgba(13,27,42,0.15)`

**Admin Secondary**
- Background: `#FFFFFF`, Text: `#0D1B2A`
- Border: `1px solid #E4E4E7`, Radius: `6px`
- Hover: `#FAFAF8` background

**Admin Danger Ghost**
- Background: transparent, Text: `#DC2626`
- Hover: `#FEF2F2`, No border

**Admin AI Action**
- Background: `#0891B2`, Text: white, Radius: `6px`

**Public CTA Primary**
- Background: `#0D1B2A`, Text: white, 15px weight 700
- Padding: `14px 28px`, Radius: `20px`
- Hover: `transform: scale(1.02)` + shadow lift

**Public CTA Gold**
- Background: `#C9A96E`, Text: `#0D1B2A` weight 700
- Radius: `20px`
- Use for: "Đặt lịch xem nhà", "Liên hệ môi giới"

### Status Badges

All badges: Plus Jakarta Sans 11px uppercase weight 600, letter-spacing 0.04em, `border-radius: 4px`, padding `2px 8px`

| Variant | Background | Text | Border |
|---|---|---|---|
| Hot Lead | `#FEF2F2` | `#DC2626` | `1px solid #FECACA` |
| Warm Lead | `#FFFBEB` | `#D97706` | `1px solid #FDE68A` |
| Cold Lead | `#EFF6FF` | `#2563EB` | `1px solid #BFDBFE` |
| Verified | `#F0FDF4` | `#16A34A` | `1px solid #BBF7D0` |
| Pending AI | `#ECFEFF` | `#0891B2` | `1px solid #A5F3FC` |
| Expired | `#F4F4F5` | `#71717A` | `1px solid #E4E4E7` |

**Crawler Active**: include `6px × 6px` green dot with `animate-pulse` before badge text.

### Cards

**Admin Metric Card**
- Surface white, `border: 1px solid #E4E4E7`, `border-radius: 8px`
- Shadow: `0 1px 3px rgba(0,0,0,0.05), 0 1px 2px rgba(0,0,0,0.03)`
- Padding: `20px 24px`
- Metric: 32px weight 700 `#0D1B2A`
- Label: 12px uppercase weight 500 `#52525B`

**Public Property Card**
- Surface white, `border-radius: 12px`
- Shadow: `0 2px 8px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)`
- Image: `aspect-ratio: 4/3`, `border-radius: 12px 12px 0 0`, `object-fit: cover`
- Hover: `translateY(-3px)` + shadow intensifies
- Badge overlay: top-left, `#0D1B2A` bg, white text, radius 6px

### Tables (Admin)

- Header: 11px uppercase weight 600, `#52525B`, letter-spacing 0.04em
- Header border-bottom: `2px solid #D1D1D6`
- Row height: 48px, Row border: `1px solid #E4E4E7`
- Row hover: `rgba(13,27,42,0.04)` background
- Cell: 14px weight 400, `#0D1B2A`
- Source/URL/Score columns: JetBrains Mono 12px

### Admin Sidebar

- Width: `240px` expanded / `64px` collapsed
- Background: `#0D1B2A`
- Logo zone: `64px` height, border-bottom `1px solid rgba(255,255,255,0.08)`
- Nav item default: 14px weight 500, `rgba(255,255,255,0.65)`
- Nav item active: white text + `rgba(255,255,255,0.12)` bg + `2px solid #C9A96E` left border
- Nav item hover: `rgba(255,255,255,0.08)` background
- Section labels: 10px uppercase, `rgba(255,255,255,0.35)`, weight 600

### Public Navbar

- Background: `rgba(250,250,248,0.92)` + `backdrop-filter: blur(12px)`
- Height: `72px`, sticky
- Nav links: 14px weight 600, `#52525B`, hover `#0D1B2A`
- Shadow on scroll: `0 1px 0 rgba(0,0,0,0.06)`

---

## 5. Layout Principles

### Spacing Scale (base 4px)

`4 / 8 / 12 / 16 / 20 / 24 / 32 / 40 / 48 / 64 / 96px`

### Admin Layout

- Sidebar fixed, content area `flex-1`, max-width `1600px`, padding `24px 32px`
- 12-column content grid, gap `24px`
- Metric row: 4 cards on ≥1280px, 2 on tablet

### Public Layout

- Container max `1280px`, padding `0 24px`
- Listing grid: 3-col desktop / 2-col tablet / 1-col mobile, gap `24px`
- Hero: full-bleed, content max-width `800px` centered

### Border Radius Scale

| Token | Value | Use |
|---|---|---|
| `radius.sm` | 4px | Badges, tags |
| `radius.md` | 6px | Buttons, inputs, dropdowns |
| `radius.lg` | 8px | Admin cards, modals |
| `radius.xl` | 12px | Public cards, images |
| `radius.2xl` | 20px | Public CTA buttons |
| `radius.full` | 9999px | Avatars, pulse dot |

---

## 6. Depth & Elevation

| Level | Shadow | Use |
|---|---|---|
| Flat | none | Admin backgrounds, table rows |
| Surface | `0 1px 3px rgba(0,0,0,0.05), 0 1px 2px rgba(0,0,0,0.03)` | Admin cards |
| Raised | `0 2px 8px rgba(0,0,0,0.07), 0 1px 2px rgba(0,0,0,0.04)` | Public cards, dropdowns |
| Floating | `0 8px 24px rgba(0,0,0,0.10), 0 2px 6px rgba(0,0,0,0.06)` | Modals, popovers |
| Hero | `0 20px 60px rgba(13,27,42,0.15)` | Hero featured card |

---

## 7. Do's and Don'ts

### Do
- Use `#0D1B2A` as the single authoritative dark color throughout
- Use Cormorant Garamond **only** for public-facing display headings
- Use `#C9A96E` gold for premium signals — price highlights, public CTAs, featured badges
- Keep admin components strictly flat and data-focused
- Use `font-variant-numeric: tabular-nums` for all prices and metrics
- Apply `animate-pulse` to Crawler Active badge dot
- Use JetBrains Mono for logs, URLs, raw AI scores in admin
- Maintain 4px base grid — all spacing is a multiple of 4

### Don't
- Don't use the old `#F59E0B` orange — reads as e-commerce discount, not real estate premium
- Don't use Cormorant Garamond in admin — breaks data-density contract
- Don't use more than 2 accent colors on any single screen
- Don't use `border-radius > 12px` in admin context
- Don't add decorative gradients to admin surfaces
- Don't use teal AI color (`#0891B2`) outside of AI-specific modules
- Don't expose raw internal labels (source IDs, crawler codes) in public view

---

## 8. Responsive Behavior

### Breakpoints

| Name | Width | Admin | Public |
|---|---|---|---|
| Mobile | < 640px | Sidebar hidden, overlay drawer | 1-col, full-width cards |
| Tablet | 640–1024px | Sidebar collapsed `64px` | 2-col grid |
| Desktop | 1024–1280px | Sidebar expanded `240px` | 3-col grid |
| Large | > 1280px | Max 1600px content | Max 1280px |

### Admin Collapse
- < 1024px: auto-collapse to icon-only
- < 640px: sidebar hidden → hamburger → full overlay drawer

### Public Collapse
- Navbar: hamburger < 768px, slide-in from right
- Hero headline: 72px → 48px → 36px
- Property detail: sticky sidebar → fixed bottom CTA bar on mobile

---

## 9. Agent Prompt Guide

### Quick Token Reference

```
Primary dark:     #0D1B2A
Primary soft:     #1A3550
Gold accent:      #C9A96E
Gold light:       #F5EDD9
Canvas:           #FAFAF8
Surface:          #FFFFFF
Border:           #E4E4E7
Border strong:    #D1D1D6
Text primary:     #0D1B2A
Text secondary:   #52525B
Text muted:       #A1A1AA
Success:          #16A34A
Warning:          #D97706
Danger:           #DC2626
AI teal:          #0891B2
```

### Font Import

```html
<link href="https://fonts.googleapis.com/css2?family=Cormorant+Garamond:wght@400;600&family=Plus+Jakarta+Sans:wght@400;500;600;700&family=JetBrains+Mono:wght@400&display=swap" rel="stylesheet">
```

### Example Component Prompts

**Admin sidebar nav item:**
"Sidebar on `#0D1B2A`. Nav item: Plus Jakarta Sans 14px weight 500, `rgba(255,255,255,0.65)`. Active: white + `rgba(255,255,255,0.10)` bg + `2px solid #C9A96E` left border. Icon 18px, gap 12px. Padding `10px 16px`, margin `1px 8px`, radius 6px."

**Property listing card (public):**
"White card, `border-radius: 12px`, shadow `0 2px 8px rgba(0,0,0,0.06)`. Image `aspect-ratio:4/3`, `border-radius: 12px 12px 0 0`, hover `scale(1.03)`. Price: Plus Jakarta Sans 22px weight 700 `#0D1B2A`. Address: 13px weight 500 `#52525B`. Top-left badge: `#0D1B2A` bg, white text, 11px uppercase, radius 6px. Hover: `translateY(-3px)` + shadow lift."

**Admin metric card:**
"White card, `border: 1px solid #E4E4E7`, `border-radius: 8px`, shadow `0 1px 3px rgba(0,0,0,0.05)`. Padding `20px 24px`. Metric: 32px weight 700 `#0D1B2A`. Label: 12px uppercase weight 500 `#52525B`. Trend: right-aligned badge green/red."

**Public hero:**
"Full-bleed `#FAFAF8` canvas or dark-overlay on property photo. Headline: Cormorant Garamond 64px weight 400 `#0D1B2A`, letter-spacing -0.02em. Subtitle: Plus Jakarta Sans 18px weight 400 `#52525B`. CTA: `#0D1B2A` bg, white, 15px weight 700, `border-radius: 20px`, padding `14px 32px`. Gold CTA: `#C9A96E` bg, `#0D1B2A` text."

**Admin data table:**
"Header: 11px uppercase weight 600 `#52525B`, letter-spacing 0.04em, `border-bottom: 2px solid #D1D1D6`. Row height 48px, `border-bottom: 1px solid #E4E4E7`. Row hover: `rgba(13,27,42,0.04)`. Cell: 14px weight 400 `#0D1B2A`. Source/URL cells: JetBrains Mono 12px."

### Iteration Guide

1. Always identify context first: Admin tokens ≠ Public tokens in radius and typography
2. Cormorant Garamond = public display only; Plus Jakarta Sans = everything else
3. Gold (`#C9A96E`) is a premium signal — max once per screen
4. AI modules always use teal (`#0891B2`) so users recognize AI-powered features
5. When unsure: more whitespace, less shadow, heavier font weight on labels
