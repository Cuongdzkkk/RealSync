export interface ApiMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
  errors?: string[] | null;
  meta?: ApiMeta;
}

export interface PaginationQuery {
  page: number;
  pageSize: number;
  keyword?: string;
}

export type StatusVariant = 'success' | 'warning' | 'danger' | 'info' | 'ai' | 'muted';
