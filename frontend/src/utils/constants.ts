export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api/v1';

export const APP_NAME = 'RealSync';

export const LEAD_STAGES = ['new', 'contacted', 'qualified', 'proposal', 'won', 'lost'] as const;

export const PROPERTY_STATUSES = ['draft', 'verified', 'published', 'expired'] as const;
