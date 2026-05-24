import type { LEAD_STAGES } from '@/utils/constants';

export type LeadStage = (typeof LEAD_STAGES)[number];
export type LeadTemperature = 'hot' | 'warm' | 'cold';

export interface Lead {
  id: string;
  fullName: string;
  phone: string;
  demand: string;
  budget: number;
  stage: LeadStage;
  temperature: LeadTemperature;
  assignedTo: string;
  lastContactAt: string;
}
