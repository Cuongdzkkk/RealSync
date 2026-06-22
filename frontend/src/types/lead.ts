export type {
  ApiMeta,
  ApiResponse,
  CrmLead,
  CrmUserMock,
  LeadActivity,
  LeadActivityCreateModel,
  LeadActivityType,
  LeadAssignModel,
  LeadConvertModel,
  LeadCreateModel,
  LeadFollowUpModel,
  LeadFollowUpState,
  LeadPriority,
  LeadQuery,
  LeadSourceChannel,
  LeadStatus,
  LeadStatusUpdateModel,
  LeadTemperature,
  LeadUpdateModel,
  LeadViewMode
} from './crm/lead';

export {
  LEAD_PRIORITIES,
  LEAD_SOURCE_CHANNELS,
  LEAD_STATUSES,
  USER_ACTIVITY_TYPES
} from './crm/lead';

export type LeadStage = 'new' | 'contacted' | 'qualified' | 'proposal' | 'won' | 'lost';
export type LegacyLeadTemperature = 'hot' | 'warm' | 'cold';

export interface Lead {
  id: string;
  fullName: string;
  phone: string;
  demand: string;
  budget: number;
  stage: LeadStage;
  temperature: LegacyLeadTemperature;
  assignedTo: string;
  lastContactAt: string;
}
