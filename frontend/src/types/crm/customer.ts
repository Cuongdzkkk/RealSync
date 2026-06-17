import type { ApiMeta, ApiResponse } from './lead';

export type CustomerSource = 'Website' | 'Facebook' | 'Zalo' | 'Phone' | 'Referral' | 'Other';

export type CustomerActivityAction = 'Create' | 'Update' | 'Delete' | 'Assignment' | 'StatusChange' | 'View' | 'Export';

export type CustomerOriginFilter = 'all' | 'converted' | 'direct';

export interface CustomerActivityLog {
  id: string;
  userId?: string | null;
  userName?: string | null;
  entityType: 'Customer';
  entityId: string;
  action: CustomerActivityAction;
  description?: string | null;
  oldValues?: string | null;
  newValues?: string | null;
  createdAt: string;
}

export interface CrmCustomerListItem {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  company?: string | null;
  notes?: string | null;
  source?: CustomerSource | null;
  assignedToId?: string | null;
  assignedToName?: string | null;
  convertedFromLeadId?: string | null;
  convertedFromLeadName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  activities: CustomerActivityLog[];
}

export interface CrmCustomerDetail extends CrmCustomerListItem {}

export interface CustomerQuery {
  page: number;
  pageSize: number;
  search: string;
  source?: CustomerSource | null;
  assignedToId?: string | null;
  convertedFromLeadId?: string | null;
  convertedFromLead?: boolean | null;
  origin?: CustomerOriginFilter;
  fromDate?: string | null;
  toDate?: string | null;
  sortBy?: 'createdAt' | 'updatedAt' | 'fullName' | 'company';
  sortDirection?: 'asc' | 'desc';
}

export interface CustomerCreateModel {
  fullName: string;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  company?: string | null;
  notes?: string | null;
  source?: CustomerSource | null;
  assignedToId?: string | null;
}

export interface CustomerUpdateModel extends CustomerCreateModel {}

export interface CustomerAssignmentModel {
  assignedToId: string;
  note?: string | null;
}

export type CustomerApiResponse<T> = ApiResponse<T>;
export type CustomerApiMeta = ApiMeta;

export const CUSTOMER_SOURCES: CustomerSource[] = ['Website', 'Facebook', 'Zalo', 'Phone', 'Referral', 'Other'];
export const CUSTOMER_ACTIVITY_ACTIONS: CustomerActivityAction[] = [
  'Create',
  'Update',
  'Delete',
  'Assignment',
  'StatusChange',
  'View',
  'Export'
];
