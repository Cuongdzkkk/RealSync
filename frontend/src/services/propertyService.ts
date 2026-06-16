import type { ApiResponse } from '@/types/common';
import type {
  LocationOption,
  Property,
  PropertyCategory,
  PropertyCategoryPayload,
  PropertyImage,
  PropertyPayload,
  PropertyQuery,
  PropertyType
} from '@/types/property';

import { api } from './api';

type ApiEnvelope<T> = ApiResponse<T> | T;

const hasData = <T>(payload: ApiEnvelope<T>): payload is ApiResponse<T> =>
  typeof payload === 'object' && payload !== null && 'data' in payload;

const compactParams = (params: Record<string, unknown>) =>
  Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== '' && value !== null && value !== undefined)
  );

const normalizeImage = (image: any): PropertyImage => ({
  id: String(image.id),
  propertyId: image.propertyId ? String(image.propertyId) : undefined,
  url: image.url ?? image.imageUrl ?? '',
  fileName: image.fileName ?? '',
  originalFileName: image.originalFileName ?? image.fileName ?? '',
  filePath: image.filePath,
  contentType: image.contentType,
  size: Number(image.size ?? 0),
  isThumbnail: Boolean(image.isThumbnail),
  sortOrder: Number(image.sortOrder ?? 0),
  createdAt: image.createdAt
});

const normalizeProperty = (property: any): Property => {
  const images: PropertyImage[] = Array.isArray(property.images)
    ? property.images.map((image: any) => normalizeImage(image))
    : [];
  const thumbnail = images.find((image) => image.isThumbnail) ?? images[0];
  const categoryId = property.categoryId ?? property.propertyCategoryId ?? null;
  const categoryName = property.categoryName ?? property.propertyCategoryName ?? null;
  const typeId = property.typeId ?? property.propertyTypeId ?? null;
  const typeName = property.typeName ?? property.propertyTypeName ?? null;
  const area = property.area ?? property.acreage ?? 0;

  return {
    ...property,
    id: String(property.id),
    title: property.title ?? '',
    code: property.code ?? property.propertyCode ?? '',
    price: Number(property.price ?? 0),
    area,
    acreage: Number(property.acreage ?? area ?? 0),
    address: property.address ?? '',
    categoryId,
    categoryName,
    propertyCategoryId: categoryId,
    propertyCategoryName: categoryName,
    typeId,
    typeName,
    propertyTypeId: typeId,
    propertyTypeName: typeName,
    images,
    imageUrl: property.imageUrl ?? thumbnail?.url ?? '',
    source: property.source ?? property.sourceType ?? '',
    status: property.status ?? 'Draft',
    listingType: property.listingType ?? 'Sale',
    bedrooms: Number(property.bedrooms ?? 0),
    bathrooms: Number(property.bathrooms ?? 0),
    floors: Number(property.floors ?? 0),
    createdAt: property.createdAt ?? new Date().toISOString(),
    updatedAt: property.updatedAt ?? null
  };
};

const normalizeApiResponse = <TData, TNormalized>(
  payload: ApiEnvelope<TData>,
  data: TNormalized
): ApiResponse<TNormalized> => {
  if (hasData(payload)) {
    return {
      ...payload,
      data
    };
  }

  return {
    success: true,
    statusCode: 200,
    message: 'Thành công',
    data
  };
};

const toApiPayload = (payload: PropertyPayload) => ({
  title: payload.title,
  description: payload.description || null,
  code: payload.code || null,
  price: payload.price,
  area: payload.area,
  priceUnit: payload.priceUnit || 'VND',
  address: payload.address || null,
  provinceId: payload.provinceId || null,
  districtId: payload.districtId || null,
  wardId: payload.wardId || null,
  propertyCategoryId: payload.propertyCategoryId || payload.categoryId,
  propertyTypeId: payload.propertyTypeId || payload.typeId,
  projectId: payload.projectId || null,
  status: payload.status,
  listingType: payload.listingType || 'Sale',
  bedrooms: payload.bedrooms ?? 0,
  bathrooms: payload.bathrooms ?? 0,
  floors: payload.floors ?? 0,
  direction: payload.direction || null,
  legalStatus: payload.legalStatus || null
});

export const propertyService = {
  async getProperties(query: Partial<PropertyQuery> = {}): Promise<ApiResponse<Property[]>> {
    const params = compactParams({
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 20,
      search: query.search ?? query.keyword,
      minPrice: query.minPrice,
      maxPrice: query.maxPrice,
      minArea: query.minArea,
      maxArea: query.maxArea,
      provinceId: query.provinceId,
      districtId: query.districtId,
      wardId: query.wardId,
      categoryId: query.categoryId,
      typeId: query.typeId,
      status: query.status,
      sortBy: query.sortBy,
      sortDirection: query.sortDirection
    });

    const { data } = await api.get<ApiEnvelope<any[]>>('/properties', { params });
    const items = (hasData(data) ? data.data : data).map(normalizeProperty);
    return normalizeApiResponse(data, items);
  },

  async getPropertyById(id: string): Promise<Property> {
    const { data } = await api.get<ApiEnvelope<any>>(`/properties/${id}`);
    return normalizeProperty(hasData(data) ? data.data : data);
  },

  async createProperty(payload: PropertyPayload): Promise<Property> {
    const { data } = await api.post<ApiEnvelope<any>>('/properties', toApiPayload(payload));
    return normalizeProperty(hasData(data) ? data.data : data);
  },

  async updateProperty(id: string, payload: PropertyPayload): Promise<Property> {
    const { data } = await api.put<ApiEnvelope<any>>(`/properties/${id}`, toApiPayload(payload));
    return normalizeProperty(hasData(data) ? data.data : data);
  },

  async deleteProperty(id: string): Promise<void> {
    await api.delete(`/properties/${id}`);
  },

  async uploadPropertyImages(propertyId: string, files: File[]): Promise<PropertyImage[]> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));

    const { data } = await api.post<ApiEnvelope<any[]>>(`/properties/${propertyId}/images`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });

    return (hasData(data) ? data.data : data).map(normalizeImage);
  },

  async deletePropertyImage(propertyId: string, imageId: string): Promise<void> {
    await api.delete(`/properties/${propertyId}/images/${imageId}`);
  },

  async getPropertyCategories(): Promise<PropertyCategory[]> {
    const { data } = await api.get<ApiEnvelope<PropertyCategory[]>>('/property-categories');
    return hasData(data) ? data.data : data;
  },

  async createPropertyCategory(payload: PropertyCategoryPayload): Promise<PropertyCategory> {
    const { data } = await api.post<ApiEnvelope<PropertyCategory>>('/property-categories', payload);
    return hasData(data) ? data.data : data;
  },

  async updatePropertyCategory(id: string, payload: Partial<PropertyCategoryPayload>): Promise<PropertyCategory> {
    const { data } = await api.put<ApiEnvelope<PropertyCategory>>(`/property-categories/${id}`, payload);
    return hasData(data) ? data.data : data;
  },

  async deletePropertyCategory(id: string): Promise<void> {
    await api.delete(`/property-categories/${id}`);
  },

  async getPropertyTypes(): Promise<PropertyType[]> {
    const { data } = await api.get<ApiEnvelope<PropertyType[]>>('/property-types');
    return hasData(data) ? data.data : data;
  },

  async getProvinces(): Promise<LocationOption[]> {
    const { data } = await api.get<ApiEnvelope<LocationOption[]>>('/locations/provinces');
    return hasData(data) ? data.data : data;
  },

  async getDistricts(provinceId: string): Promise<LocationOption[]> {
    const { data } = await api.get<ApiEnvelope<LocationOption[]>>('/locations/districts', {
      params: { provinceId }
    });
    return hasData(data) ? data.data : data;
  },

  async getWards(districtId: string): Promise<LocationOption[]> {
    const { data } = await api.get<ApiEnvelope<LocationOption[]>>('/locations/wards', {
      params: { districtId }
    });
    return hasData(data) ? data.data : data;
  },

  async getAll(query: Partial<PropertyQuery> = {}) {
    return this.getProperties(query);
  },

  async getById(id: string) {
    return this.getPropertyById(id);
  }
};
