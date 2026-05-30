import type { ApiResponse, PaginationQuery } from '@/types/common';
import type { Property, PropertyFilter } from '@/types/property';

import { api } from './api';

export const propertyService = {
  async getAll(query: PaginationQuery & PropertyFilter): Promise<ApiResponse<Property[]>> {
    const { data } = await api.get('/properties', { params: query });
    return data;
  },

  async getById(id: string): Promise<Property> {
    const { data } = await api.get(`/properties/${id}`);
    return data.data;
  }
};
