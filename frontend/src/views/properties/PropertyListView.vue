<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { Delete, Edit, Plus, Refresh, Search, View as ViewIcon } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';

import PropertyAnalytics from '@/components/property/PropertyAnalytics.vue';
import { propertyService } from '@/services/propertyService';
import { useAuthStore } from '@/stores/useAuthStore';
import type { LocationOption, Property, PropertyCategory, PropertyQuery, PropertyType } from '@/types/property';
import { formatCurrency, formatDate, formatNumber } from '@/utils/format';

const router = useRouter();
const authStore = useAuthStore();

const properties = ref<Property[]>([]);
const categories = ref<PropertyCategory[]>([]);
const propertyTypes = ref<PropertyType[]>([]);
const provinces = ref<LocationOption[]>([]);
const districts = ref<LocationOption[]>([]);
const wards = ref<LocationOption[]>([]);
const loading = ref(false);
const optionsLoading = ref(false);
const errorMessage = ref('');
const total = ref(0);
let searchTimer: ReturnType<typeof setTimeout> | undefined;

const query = reactive<PropertyQuery>({
  page: 1,
  pageSize: 10,
  search: '',
  minPrice: null,
  maxPrice: null,
  minArea: null,
  maxArea: null,
  provinceId: null,
  districtId: null,
  wardId: null,
  categoryId: null,
  typeId: null,
  status: null,
  sortBy: 'createdAt',
  sortDirection: 'desc'
});

const canMutate = computed(() => {
  const role = authStore.user?.role;
  return Boolean(role && ['Admin', 'Manager', 'Marketing'].includes(role));
});

const statusOptions = [
  { label: 'Draft', value: 'Draft' },
  { label: 'Active', value: 'Active' },
  { label: 'Available', value: 'Available' },
  { label: 'Sold', value: 'Sold' },
  { label: 'Rented', value: 'Rented' },
  { label: 'Expired', value: 'Expired' },
  { label: 'Hidden', value: 'Hidden' }
];

const sortOptions = [
  { label: 'Ngày tạo', value: 'createdAt' },
  { label: 'Giá', value: 'price' },
  { label: 'Diện tích', value: 'area' },
  { label: 'Tiêu đề', value: 'title' }
];

const fallbackImage =
  'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="480" height="320" viewBox="0 0 480 320"><rect width="480" height="320" fill="%23f8fafc"/><rect x="96" y="78" width="288" height="164" rx="14" fill="%23e2e8f0"/><path d="M145 214l58-64 42 42 28-30 62 52H145z" fill="%2394a3b8"/><circle cx="305" cy="127" r="20" fill="%23cbd5e1"/></svg>';

function getErrorMessage(error: any) {
  if (error?.response?.status === 403) {
    return 'Bạn không có quyền thực hiện thao tác này.';
  }

  return error?.response?.data?.message ?? 'Không tải được dữ liệu bất động sản.';
}

function getThumbnail(property: Property) {
  return property.imageUrl || property.images?.find((image) => image.isThumbnail)?.url || property.images?.[0]?.url || fallbackImage;
}

function handleImageError(event: Event) {
  const target = event.target as HTMLImageElement;
  target.src = fallbackImage;
}

function getStatusLabel(status: string) {
  const map: Record<string, string> = {
    draft: 'Draft',
    active: 'Active',
    available: 'Available',
    sold: 'Sold',
    rented: 'Rented',
    expired: 'Expired',
    hidden: 'Hidden',
    verified: 'Đã duyệt',
    published: 'Đã đăng'
  };

  return map[status.toLowerCase()] ?? status;
}

function getStatusClass(status: string) {
  return `status-${status.toLowerCase()}`;
}

function getCategoryText(property: Property) {
  const category = property.categoryName ?? property.propertyCategoryName ?? 'Chưa phân loại';
  const type = property.typeName ?? property.propertyTypeName;
  return type ? `${category} / ${type}` : category;
}

function getLocationText(property: Property) {
  const location = [property.wardName, property.districtName, property.provinceName].filter(Boolean).join(', ');
  return location || property.address || '-';
}

async function loadOptions() {
  optionsLoading.value = true;
  try {
    const [categoryItems, typeItems, provinceItems] = await Promise.all([
      propertyService.getPropertyCategories(),
      propertyService.getPropertyTypes(),
      propertyService.getProvinces()
    ]);

    categories.value = categoryItems.filter((item) => item.isActive);
    propertyTypes.value = typeItems.filter((item) => item.isActive);
    provinces.value = provinceItems.filter((item) => item.isActive);
  } catch {
    ElMessage.error('Không tải được danh mục hoặc khu vực lọc.');
  } finally {
    optionsLoading.value = false;
  }
}

async function loadProperties() {
  loading.value = true;
  errorMessage.value = '';

  try {
    const response = await propertyService.getProperties(query);
    properties.value = response.data;
    total.value = response.meta?.totalCount ?? response.data.length;
  } catch (error: any) {
    errorMessage.value = getErrorMessage(error);
  } finally {
    loading.value = false;
  }
}

function applyFilters() {
  query.page = 1;
  loadProperties();
}

function handleSearchInput() {
  if (searchTimer) {
    clearTimeout(searchTimer);
  }

  searchTimer = setTimeout(() => {
    applyFilters();
  }, 350);
}

async function handleProvinceChange(value: string | null) {
  query.districtId = null;
  query.wardId = null;
  districts.value = [];
  wards.value = [];

  if (value) {
    districts.value = (await propertyService.getDistricts(value)).filter((item) => item.isActive);
  }

  applyFilters();
}

async function handleDistrictChange(value: string | null) {
  query.wardId = null;
  wards.value = [];

  if (value) {
    wards.value = (await propertyService.getWards(value)).filter((item) => item.isActive);
  }

  applyFilters();
}

function resetFilters() {
  Object.assign(query, {
    page: 1,
    pageSize: query.pageSize,
    search: '',
    minPrice: null,
    maxPrice: null,
    minArea: null,
    maxArea: null,
    provinceId: null,
    districtId: null,
    wardId: null,
    categoryId: null,
    typeId: null,
    status: null,
    sortBy: 'createdAt',
    sortDirection: 'desc'
  });
  districts.value = [];
  wards.value = [];
  loadProperties();
}

function handlePageChange(page: number) {
  query.page = page;
  loadProperties();
}

function handlePageSizeChange(pageSize: number) {
  query.pageSize = pageSize;
  query.page = 1;
  loadProperties();
}

async function deleteProperty(property: Property) {
  if (!canMutate.value) return;

  try {
    await ElMessageBox.confirm(
      `Xóa bất động sản "${property.title}"? Hành động này không thể hoàn tác.`,
      'Xác nhận xóa',
      {
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }
    );

    await propertyService.deleteProperty(property.id);
    ElMessage.success('Đã xóa bất động sản.');

    if (properties.value.length === 1 && query.page > 1) {
      query.page -= 1;
    }

    await loadProperties();
  } catch (error: any) {
    if (error === 'cancel') return;
    ElMessage.error(getErrorMessage(error));
  }
}

onMounted(async () => {
  await Promise.all([loadOptions(), loadProperties()]);
});
</script>

<template>
  <section class="property-page">
    <header class="property-header">
      <div>
        <h1>Properties</h1>
        <p>Quản lý danh sách bất động sản</p>
      </div>
      <div class="property-header__actions">
        <el-button :icon="Refresh" :loading="loading" @click="loadProperties">Refresh</el-button>
        <el-button v-if="canMutate" class="primary-action" @click="router.push({ name: 'property-create' })">
          <el-icon><Plus /></el-icon>
          <span>Add Property</span>
        </el-button>
      </div>
    </header>

    <PropertyAnalytics :properties="properties" :total="total" :loading="loading && properties.length === 0" />

    <section class="filter-card" v-loading="optionsLoading">
      <el-input
        v-model="query.search"
        clearable
        placeholder="Search title, code, address..."
        @input="handleSearchInput"
        @clear="applyFilters"
        @keyup.enter="applyFilters"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>

      <el-select v-model="query.categoryId" clearable filterable placeholder="Category" @change="applyFilters">
        <el-option v-for="item in categories" :key="item.id" :label="item.name" :value="item.id" />
      </el-select>

      <el-select v-model="query.typeId" clearable filterable placeholder="Type" @change="applyFilters">
        <el-option v-for="item in propertyTypes" :key="item.id" :label="item.name" :value="item.id" />
      </el-select>

      <el-select v-model="query.provinceId" clearable filterable placeholder="Province" @change="handleProvinceChange">
        <el-option v-for="item in provinces" :key="item.id" :label="item.name" :value="item.id" />
      </el-select>

      <el-select
        v-model="query.districtId"
        clearable
        filterable
        placeholder="District"
        :disabled="!query.provinceId"
        @change="handleDistrictChange"
      >
        <el-option v-for="item in districts" :key="item.id" :label="item.name" :value="item.id" />
      </el-select>

      <el-select
        v-model="query.wardId"
        clearable
        filterable
        placeholder="Ward"
        :disabled="!query.districtId"
        @change="applyFilters"
      >
        <el-option v-for="item in wards" :key="item.id" :label="item.name" :value="item.id" />
      </el-select>

      <el-select v-model="query.status" clearable placeholder="Status" @change="applyFilters">
        <el-option v-for="item in statusOptions" :key="item.value" :label="item.label" :value="item.value" />
      </el-select>

      <el-input-number
        v-model="query.minPrice"
        class="filter-number"
        :min="0"
        :controls="false"
        placeholder="Min price"
        @change="applyFilters"
      />

      <el-input-number
        v-model="query.maxPrice"
        class="filter-number"
        :min="0"
        :controls="false"
        placeholder="Max price"
        @change="applyFilters"
      />

      <el-input-number
        v-model="query.minArea"
        class="filter-number"
        :min="0"
        :controls="false"
        placeholder="Min area"
        @change="applyFilters"
      />

      <el-input-number
        v-model="query.maxArea"
        class="filter-number"
        :min="0"
        :controls="false"
        placeholder="Max area"
        @change="applyFilters"
      />

      <el-select v-model="query.sortBy" placeholder="Sort by" @change="applyFilters">
        <el-option v-for="item in sortOptions" :key="item.value" :label="item.label" :value="item.value" />
      </el-select>

      <el-select v-model="query.sortDirection" placeholder="Direction" @change="applyFilters">
        <el-option label="Desc" value="desc" />
        <el-option label="Asc" value="asc" />
      </el-select>

      <el-button @click="resetFilters">Reset filter</el-button>
    </section>

    <section class="property-table-card" v-loading="loading">
      <div v-if="errorMessage" class="state-panel">
        <h3>Không tải được danh sách</h3>
        <p>{{ errorMessage }}</p>
        <el-button class="primary-action" @click="loadProperties">Retry</el-button>
      </div>

      <template v-else>
        <el-table v-if="properties.length" class="desktop-table" :data="properties" row-key="id">
          <el-table-column label="Ảnh" width="92">
            <template #default="{ row }">
              <img class="property-thumb" :src="getThumbnail(row)" :alt="row.title" @error="handleImageError" />
            </template>
          </el-table-column>

          <el-table-column label="Mã" min-width="110">
            <template #default="{ row }">
              <span class="mono table-code">{{ row.code || '-' }}</span>
            </template>
          </el-table-column>

          <el-table-column label="Tiêu đề" min-width="220">
            <template #default="{ row }">
              <button class="title-link" @click="router.push({ name: 'property-detail', params: { id: row.id } })">
                {{ row.title }}
              </button>
              <span class="table-address">{{ row.address || '-' }}</span>
            </template>
          </el-table-column>

          <el-table-column label="Loại/Danh mục" min-width="170">
            <template #default="{ row }">{{ getCategoryText(row) }}</template>
          </el-table-column>

          <el-table-column label="Giá" min-width="140">
            <template #default="{ row }">
              <strong class="numeric">{{ formatCurrency(row.price) }}</strong>
            </template>
          </el-table-column>

          <el-table-column label="Diện tích" width="110">
            <template #default="{ row }">
              <span class="numeric">{{ formatNumber(Number(row.area ?? row.acreage ?? 0)) }} m2</span>
            </template>
          </el-table-column>

          <el-table-column label="Khu vực" min-width="190">
            <template #default="{ row }">{{ getLocationText(row) }}</template>
          </el-table-column>

          <el-table-column label="Trạng thái" width="120">
            <template #default="{ row }">
              <span class="status-badge" :class="getStatusClass(row.status)">
                {{ getStatusLabel(row.status) }}
              </span>
            </template>
          </el-table-column>

          <el-table-column label="Ngày tạo" width="116">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>

          <el-table-column label="Actions" width="160" align="right">
            <template #default="{ row }">
              <div class="table-actions">
                <el-button circle :icon="ViewIcon" title="View detail" @click="router.push({ name: 'property-detail', params: { id: row.id } })" />
                <el-button
                  v-if="canMutate"
                  circle
                  :icon="Edit"
                  title="Edit"
                  @click="router.push({ name: 'property-edit', params: { id: row.id } })"
                />
                <el-button v-if="canMutate" circle :icon="Delete" title="Delete" @click="deleteProperty(row)" />
              </div>
            </template>
          </el-table-column>
        </el-table>

        <div v-if="properties.length" class="mobile-list">
          <article v-for="property in properties" :key="property.id" class="property-mobile-card">
            <img :src="getThumbnail(property)" :alt="property.title" @error="handleImageError" />
            <div class="property-mobile-card__body">
              <div class="property-mobile-card__top">
                <span class="mono">{{ property.code || '-' }}</span>
                <span class="status-badge" :class="getStatusClass(property.status)">{{ getStatusLabel(property.status) }}</span>
              </div>
              <h3>{{ property.title }}</h3>
              <p>{{ getLocationText(property) }}</p>
              <div class="property-mobile-card__meta">
                <strong>{{ formatCurrency(property.price) }}</strong>
                <span>{{ formatNumber(Number(property.area ?? property.acreage ?? 0)) }} m2</span>
              </div>
              <div class="property-mobile-card__actions">
                <el-button size="small" @click="router.push({ name: 'property-detail', params: { id: property.id } })">
                  View
                </el-button>
                <el-button
                  v-if="canMutate"
                  size="small"
                  @click="router.push({ name: 'property-edit', params: { id: property.id } })"
                >
                  Edit
                </el-button>
                <el-button v-if="canMutate" size="small" @click="deleteProperty(property)">Delete</el-button>
              </div>
            </div>
          </article>
        </div>

        <el-empty v-if="!properties.length" description="Chưa có bất động sản phù hợp với bộ lọc.">
          <el-button v-if="canMutate" class="primary-action" @click="router.push({ name: 'property-create' })">
            Add Property
          </el-button>
        </el-empty>

        <div v-if="properties.length" class="pagination-row">
          <el-pagination
            background
            layout="total, sizes, prev, pager, next"
            :current-page="query.page"
            :page-size="query.pageSize"
            :page-sizes="[10, 20, 50, 100]"
            :total="total"
            @current-change="handlePageChange"
            @size-change="handlePageSizeChange"
          />
        </div>
      </template>
    </section>
  </section>
</template>

<style scoped>
.property-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.property-header,
.filter-card,
.property-table-card {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
}

.property-header {
  align-items: center;
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 20px 22px;
}

.property-header h1 {
  color: var(--color-text-primary);
  font-size: 22px;
  line-height: 1.2;
  margin: 0 0 6px;
}

.property-header p {
  color: var(--color-text-secondary);
  margin: 0;
}

.property-header__actions,
.table-actions,
.property-mobile-card__actions {
  align-items: center;
  display: flex;
  gap: 8px;
}

.primary-action {
  background: var(--color-yellow);
  border-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-weight: 700;
}

.primary-action:hover {
  background: var(--color-yellow-hover);
  border-color: var(--color-yellow-hover);
  color: var(--color-yellow-text);
}

.filter-card {
  display: grid;
  gap: 12px;
  grid-template-columns: minmax(240px, 1.8fr) repeat(5, minmax(140px, 1fr));
  padding: 16px;
}

.filter-number {
  width: 100%;
}

.property-table-card {
  min-height: 320px;
  overflow: hidden;
  padding: 8px;
}

.property-thumb {
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  height: 58px;
  object-fit: cover;
  width: 74px;
}

.table-code {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.title-link {
  background: transparent;
  border: 0;
  color: var(--color-text-primary);
  cursor: pointer;
  display: block;
  font-weight: 700;
  line-height: 1.35;
  padding: 0;
  text-align: left;
}

.title-link:hover {
  color: #8a7a00;
}

.table-address {
  color: var(--color-text-muted);
  display: block;
  font-size: 12px;
  margin-top: 4px;
  max-width: 270px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status-badge {
  border-radius: 6px;
  display: inline-flex;
  font-size: 10px;
  font-weight: 800;
  line-height: 1;
  padding: 5px 8px;
  text-transform: uppercase;
}

.status-available,
.status-active,
.status-published,
.status-verified {
  background: var(--color-success-bg);
  border: 1px solid var(--color-success-border);
  color: var(--color-success);
}

.status-draft,
.status-hidden {
  background: #f1f5f9;
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
}

.status-sold,
.status-rented {
  background: var(--color-info-bg);
  border: 1px solid var(--color-info-border);
  color: var(--color-info);
}

.status-expired {
  background: var(--color-danger-bg);
  border: 1px solid var(--color-danger-border);
  color: var(--color-danger);
}

.state-panel {
  align-items: center;
  display: flex;
  flex-direction: column;
  justify-content: center;
  min-height: 300px;
  padding: 32px;
  text-align: center;
}

.state-panel h3 {
  font-size: 16px;
  margin: 0 0 8px;
}

.state-panel p {
  color: var(--color-text-secondary);
  margin: 0 0 16px;
}

.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 14px 8px 6px;
}

.mobile-list {
  display: none;
}

.property-mobile-card {
  border-bottom: 1px solid var(--color-divider);
  display: grid;
  gap: 12px;
  grid-template-columns: 110px minmax(0, 1fr);
  padding: 12px 4px;
}

.property-mobile-card img {
  aspect-ratio: 4 / 3;
  border-radius: 8px;
  height: auto;
  object-fit: cover;
  width: 100%;
}

.property-mobile-card__body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.property-mobile-card__top,
.property-mobile-card__meta {
  align-items: center;
  display: flex;
  justify-content: space-between;
  gap: 8px;
}

.property-mobile-card h3 {
  font-size: 14px;
  margin: 0;
}

.property-mobile-card p {
  color: var(--color-text-secondary);
  margin: 0;
}

@media (max-width: 1280px) {
  .filter-card {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 860px) {
  .property-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .filter-card {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .filter-card {
    grid-template-columns: 1fr;
  }

  .desktop-table {
    display: none;
  }

  .mobile-list {
    display: block;
  }

  .pagination-row {
    justify-content: flex-start;
    overflow-x: auto;
  }
}

@media (max-width: 520px) {
  .property-mobile-card {
    grid-template-columns: 1fr;
  }

  .property-header__actions {
    width: 100%;
  }

  .property-header__actions .el-button {
    flex: 1;
  }
}
</style>
