export type PropertyStatus =
  | 'Draft'
  | 'Active'
  | 'Available'
  | 'Sold'
  | 'Rented'
  | 'Expired'
  | 'Hidden'
  | 'draft'
  | 'verified'
  | 'published'
  | 'expired'
  | string;

export type ListingType = 'Sale' | 'Rent' | string;

export interface PropertyImage {
  id: string;
  propertyId?: string;
  url: string;
  fileName: string;
  originalFileName: string;
  filePath?: string;
  contentType?: string;
  size?: number;
  isThumbnail: boolean;
  sortOrder: number;
  createdAt?: string;
}

export interface Property {
  id: string;
  title: string;
  description?: string | null;
  code?: string;
  price: number;
  area: number | string;
  acreage?: number;
  priceUnit?: string | null;
  address: string;
  provinceId?: string | null;
  provinceName?: string | null;
  districtId?: string | null;
  districtName?: string | null;
  wardId?: string | null;
  wardName?: string | null;
  categoryId?: string | null;
  categoryName?: string | null;
  propertyCategoryId?: string | null;
  propertyCategoryName?: string | null;
  typeId?: string | null;
  typeName?: string | null;
  propertyTypeId?: string | null;
  propertyTypeName?: string | null;
  projectId?: string | null;
  status: PropertyStatus;
  listingType?: ListingType;
  bedrooms?: number;
  bathrooms?: number;
  floors?: number;
  direction?: string | null;
  legalStatus?: string | null;
  images?: PropertyImage[];
  imageUrl: string;
  source: string;
  sourceType?: string | null;
  sourceUrl?: string | null;
  slug?: string | null;
  metaTitle?: string | null;
  metaDescription?: string | null;
  aiScore?: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface PropertyQuery {
  page: number;
  pageSize: number;
  search?: string;
  keyword?: string;
  minPrice?: number | null;
  maxPrice?: number | null;
  minArea?: number | null;
  maxArea?: number | null;
  provinceId?: string | null;
  districtId?: string | null;
  wardId?: string | null;
  categoryId?: string | null;
  typeId?: string | null;
  status?: string | null;
  sortBy?: 'createdAt' | 'price' | 'area' | 'title' | string;
  sortDirection?: 'asc' | 'desc' | string;
}

export interface PropertyPayload {
  title: string;
  description?: string | null;
  code?: string | null;
  price: number;
  area: number;
  priceUnit?: string | null;
  address?: string | null;
  provinceId?: string | null;
  districtId?: string | null;
  wardId?: string | null;
  categoryId?: string | null;
  propertyCategoryId?: string | null;
  typeId?: string | null;
  propertyTypeId?: string | null;
  projectId?: string | null;
  status: string;
  listingType?: string;
  bedrooms?: number | null;
  bathrooms?: number | null;
  floors?: number | null;
  direction?: string | null;
  legalStatus?: string | null;
}

export interface PropertyCategory {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface PropertyCategoryPayload {
  name: string;
  slug?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface PropertyType {
  id: string;
  name: string;
  slug?: string | null;
  description?: string | null;
  icon?: string | null;
  sortOrder?: number;
  isActive: boolean;
}

export interface LocationOption {
  id: string;
  name: string;
  code?: string | null;
  level?: number;
  parentId?: string | null;
  isActive: boolean;
}

export interface PropertyFilter {
  keyword?: string;
  search?: string;
  status?: PropertyStatus;
  area?: string | number;
  categoryId?: string | null;
  typeId?: string | null;
  provinceId?: string | null;
  districtId?: string | null;
  wardId?: string | null;
}
