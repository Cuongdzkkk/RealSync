import type { PROPERTY_STATUSES } from '@/utils/constants';

export type PropertyStatus = (typeof PROPERTY_STATUSES)[number];

export interface Property {
  id: string;
  title: string;
  address: string;
  area: string;
  price: number;
  acreage: number;
  bedrooms: number;
  status: PropertyStatus;
  source: string;
  imageUrl: string;
  aiScore: number;
  createdAt: string;
}

export interface PropertyFilter {
  keyword?: string;
  status?: PropertyStatus;
  area?: string;
}
