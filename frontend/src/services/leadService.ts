import type { ApiResponse, PaginationQuery } from '@/types/common';
import type { Lead } from '@/types/lead';

import { api } from './api';

export const leadService = {
  async getAll(query: PaginationQuery): Promise<ApiResponse<Lead[]>> {
    const { data } = await api.get('/leads', { params: query });
    return data;
  }
};
