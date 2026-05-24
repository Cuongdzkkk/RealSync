# Frontend — RealSync UI

> Vue 3 + Vite + Element Plus + Pinia + TypeScript

## Project Structure
```
frontend/
├── src/
│   ├── views/           # Page components
│   ├── components/      # Reusable components
│   ├── stores/          # Pinia stores
│   ├── composables/     # Vue composables
│   ├── services/        # API services (Axios)
│   ├── router/          # Vue Router
│   ├── types/           # TypeScript types
│   ├── utils/           # Utilities
│   └── assets/          # Static assets
└── tests/
```

## Quick Start
```bash
npm install
npm run dev
```

## Scripts
```bash
npm run dev
npm run typecheck
npm run build
npm run preview
```

## URLs
- Dev: `http://localhost:5173`

## Routes
- Public marketplace: `/`
- Login: `/login`
- Admin dashboard: `/admin/dashboard`
- Projects: `/admin/projects`
- Properties: `/admin/properties`
- Leads: `/admin/leads`
- Crawlers: `/admin/crawlers`
- AI Classification: `/admin/ai-classification`
- Content AI: `/admin/content-ai`
- Insights: `/admin/insights`
- Users & Roles: `/admin/users`
- Settings: `/admin/settings`

## Notes
- Admin routes are accessible in local dev mode for UI development.
- API services are scaffolded against `VITE_API_BASE_URL` and currently the views use local mock data.

## Conventions
See `../agent-skills/SKILL.md` → Section 5 (Frontend Rules)
