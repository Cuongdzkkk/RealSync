<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Delete, Edit, Picture } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';

import { propertyService } from '@/services/propertyService';
import { useAuthStore } from '@/stores/useAuthStore';
import type { Property } from '@/types/property';
import { formatCurrency, formatDate, formatNumber } from '@/utils/format';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const property = ref<Property | null>(null);
const loading = ref(false);
const deleting = ref(false);
const errorMessage = ref('');
const activeImage = ref('');

const propertyId = computed(() => String(route.params.id));
const canMutate = computed(() => {
  const role = authStore.user?.role;
  return Boolean(role && ['Admin', 'Manager', 'Marketing'].includes(role));
});

const fallbackImage =
  'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="900" height="600" viewBox="0 0 900 600"><rect width="900" height="600" fill="%23f8fafc"/><rect x="180" y="140" width="540" height="320" rx="24" fill="%23e2e8f0"/><path d="M260 405l112-128 92 92 58-66 126 102H260z" fill="%2394a3b8"/><circle cx="590" cy="235" r="38" fill="%23cbd5e1"/></svg>';

const galleryImages = computed(() => {
  const images = property.value?.images ?? [];
  if (images.length > 0) {
    return images.map((image) => image.url).filter(Boolean);
  }

  return property.value?.imageUrl ? [property.value.imageUrl] : [fallbackImage];
});

const fullAddress = computed(() => {
  if (!property.value) return '-';
  const location = [
    property.value.address,
    property.value.wardName,
    property.value.districtName,
    property.value.provinceName
  ].filter(Boolean);
  return location.length > 0 ? location.join(', ') : '-';
});

const categoryText = computed(() => {
  if (!property.value) return '-';
  return property.value.categoryName ?? property.value.propertyCategoryName ?? '-';
});

const typeText = computed(() => {
  if (!property.value) return '-';
  return property.value.typeName ?? property.value.propertyTypeName ?? '-';
});

function getErrorMessage(error: any) {
  if (error?.response?.status === 403) {
    return 'Bạn không có quyền thực hiện thao tác này.';
  }

  return error?.response?.data?.message ?? 'Không tải được chi tiết bất động sản.';
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

function handleImageError(event: Event) {
  const target = event.target as HTMLImageElement;
  target.src = fallbackImage;
}

async function loadProperty() {
  loading.value = true;
  errorMessage.value = '';

  try {
    property.value = await propertyService.getPropertyById(propertyId.value);
    activeImage.value = galleryImages.value[0] ?? fallbackImage;
  } catch (error: any) {
    errorMessage.value = getErrorMessage(error);
  } finally {
    loading.value = false;
  }
}

async function deleteProperty() {
  if (!property.value || !canMutate.value) return;

  try {
    await ElMessageBox.confirm(
      `Xóa bất động sản "${property.value.title}"? Hành động này không thể hoàn tác.`,
      'Xác nhận xóa',
      {
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }
    );

    deleting.value = true;
    await propertyService.deleteProperty(property.value.id);
    ElMessage.success('Đã xóa bất động sản.');
    router.push({ name: 'properties' });
  } catch (error: any) {
    if (error === 'cancel' || error === 'close') return;
    ElMessage.error(getErrorMessage(error));
  } finally {
    deleting.value = false;
  }
}

onMounted(loadProperty);
</script>

<template>
  <section class="property-detail-page">
    <header class="detail-header">
      <el-button :icon="ArrowLeft" @click="router.push({ name: 'properties' })">Back</el-button>
      <div class="detail-header__actions">
        <el-button
          v-if="canMutate && property"
          :icon="Edit"
          @click="router.push({ name: 'property-edit', params: { id: property.id } })"
        >
          Edit
        </el-button>
        <el-button v-if="canMutate && property" :icon="Delete" :loading="deleting" @click="deleteProperty">
          Delete
        </el-button>
      </div>
    </header>

    <section v-if="loading" class="detail-card loading-card">
      <div class="skeleton detail-skeleton-media"></div>
      <div class="detail-skeleton-lines">
        <span class="skeleton"></span>
        <span class="skeleton"></span>
        <span class="skeleton"></span>
      </div>
    </section>

    <section v-else-if="errorMessage" class="detail-card state-panel">
      <h3>Không tải được chi tiết</h3>
      <p>{{ errorMessage }}</p>
      <el-button class="primary-action" @click="loadProperty">Retry</el-button>
    </section>

    <section v-else-if="!property" class="detail-card state-panel">
      <h3>Không tìm thấy bất động sản</h3>
      <p>Bản ghi có thể đã bị xóa hoặc bạn không còn quyền truy cập.</p>
      <el-button @click="router.push({ name: 'properties' })">Back to list</el-button>
    </section>

    <template v-else>
      <section class="detail-hero">
        <div class="gallery-card">
          <div class="gallery-main">
            <img :src="activeImage || fallbackImage" :alt="property.title" @error="handleImageError" />
          </div>
          <div class="gallery-strip">
            <button
              v-for="image in galleryImages"
              :key="image"
              type="button"
              class="gallery-thumb"
              :class="{ 'is-active': activeImage === image }"
              @click="activeImage = image"
            >
              <img :src="image" :alt="property.title" @error="handleImageError" />
            </button>
          </div>
        </div>

        <aside class="summary-card">
          <span class="status-badge" :class="getStatusClass(property.status)">
            {{ getStatusLabel(property.status) }}
          </span>
          <h1>{{ property.title }}</h1>
          <div class="summary-code mono">{{ property.code || '-' }}</div>

          <div class="summary-price numeric">{{ formatCurrency(property.price) }}</div>
          <div class="summary-address">{{ fullAddress }}</div>

          <div class="summary-grid">
            <div>
              <span>Diện tích</span>
              <strong class="numeric">{{ formatNumber(Number(property.area ?? property.acreage ?? 0)) }} m2</strong>
            </div>
            <div>
              <span>Danh mục</span>
              <strong>{{ categoryText }}</strong>
            </div>
            <div>
              <span>Loại</span>
              <strong>{{ typeText }}</strong>
            </div>
            <div>
              <span>Listing</span>
              <strong>{{ property.listingType || '-' }}</strong>
            </div>
          </div>
        </aside>
      </section>

      <section class="detail-grid">
        <article class="detail-card">
          <div class="detail-card__header">
            <h2>Property Details</h2>
          </div>
          <dl class="info-list">
            <div>
              <dt>Bedrooms</dt>
              <dd>{{ property.bedrooms ?? 0 }}</dd>
            </div>
            <div>
              <dt>Bathrooms</dt>
              <dd>{{ property.bathrooms ?? 0 }}</dd>
            </div>
            <div>
              <dt>Floors</dt>
              <dd>{{ property.floors ?? 0 }}</dd>
            </div>
            <div>
              <dt>Direction</dt>
              <dd>{{ property.direction || '-' }}</dd>
            </div>
            <div>
              <dt>Legal Status</dt>
              <dd>{{ property.legalStatus || '-' }}</dd>
            </div>
            <div>
              <dt>Created At</dt>
              <dd>{{ formatDate(property.createdAt) }}</dd>
            </div>
            <div>
              <dt>Updated At</dt>
              <dd>{{ property.updatedAt ? formatDate(property.updatedAt) : '-' }}</dd>
            </div>
          </dl>
        </article>

        <article class="detail-card description-card">
          <div class="detail-card__header">
            <h2>Description</h2>
          </div>
          <p v-if="property.description">{{ property.description }}</p>
          <el-empty v-else :image-size="72" description="Chưa có mô tả." />
        </article>

        <article class="detail-card metadata-card">
          <div class="detail-card__header">
            <h2>Images</h2>
          </div>
          <div class="metadata-image-count">
            <el-icon><Picture /></el-icon>
            <span>{{ property.images?.length ?? 0 }} ảnh đã upload</span>
          </div>
        </article>
      </section>
    </template>
  </section>
</template>

<style scoped>
.property-detail-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.detail-header {
  align-items: center;
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.detail-header__actions {
  display: flex;
  gap: 8px;
}

.detail-hero {
  display: grid;
  gap: 18px;
  grid-template-columns: minmax(0, 1.35fr) minmax(320px, 0.65fr);
}

.gallery-card,
.summary-card,
.detail-card {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
}

.gallery-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
}

.gallery-main {
  aspect-ratio: 16 / 10;
  background: #f8fafc;
  border-radius: 8px;
  overflow: hidden;
}

.gallery-main img,
.gallery-thumb img {
  height: 100%;
  object-fit: cover;
  width: 100%;
}

.gallery-strip {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(auto-fill, minmax(86px, 1fr));
}

.gallery-thumb {
  aspect-ratio: 4 / 3;
  background: #f8fafc;
  border: 2px solid transparent;
  border-radius: 8px;
  cursor: pointer;
  overflow: hidden;
  padding: 0;
}

.gallery-thumb.is-active {
  border-color: var(--color-yellow);
}

.summary-card {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding: 22px;
}

.summary-card h1 {
  font-size: 24px;
  line-height: 1.25;
  margin: 0;
}

.summary-code {
  color: var(--color-text-muted);
  font-size: 12px;
}

.summary-price {
  color: var(--color-text-primary);
  font-size: 26px;
  font-weight: 800;
}

.summary-address {
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.summary-grid {
  border-top: 1px solid var(--color-divider);
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  padding-top: 16px;
}

.summary-grid div,
.info-list div {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.summary-grid span,
.info-list dt {
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.summary-grid strong,
.info-list dd {
  color: var(--color-text-primary);
  font-size: 13px;
  font-weight: 700;
  margin: 0;
}

.detail-grid {
  display: grid;
  gap: 18px;
  grid-template-columns: minmax(0, 0.8fr) minmax(0, 1.2fr);
}

.detail-card {
  padding: 20px;
}

.description-card,
.metadata-card {
  min-height: 180px;
}

.metadata-card {
  grid-column: 1 / -1;
}

.detail-card__header {
  border-bottom: 1px solid var(--color-divider);
  margin-bottom: 16px;
  padding-bottom: 12px;
}

.detail-card__header h2 {
  font-size: 15px;
  margin: 0;
}

.info-list {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin: 0;
}

.description-card p {
  color: var(--color-text-secondary);
  line-height: 1.7;
  margin: 0;
  white-space: pre-wrap;
}

.metadata-image-count {
  align-items: center;
  color: var(--color-text-secondary);
  display: flex;
  gap: 8px;
}

.status-badge {
  align-self: flex-start;
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

.primary-action {
  background: var(--color-yellow);
  border-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-weight: 700;
}

.loading-card {
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 1fr) 320px;
  min-height: 360px;
  padding: 18px;
}

.detail-skeleton-media {
  border-radius: 8px;
  min-height: 320px;
}

.detail-skeleton-lines {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.detail-skeleton-lines span {
  height: 22px;
}

.detail-skeleton-lines span:first-child {
  height: 42px;
}

.state-panel {
  align-items: center;
  display: flex;
  flex-direction: column;
  justify-content: center;
  min-height: 320px;
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

@media (max-width: 980px) {
  .detail-hero,
  .detail-grid,
  .loading-card {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 580px) {
  .detail-header {
    align-items: stretch;
    flex-direction: column;
  }

  .detail-header__actions {
    width: 100%;
  }

  .detail-header__actions .el-button {
    flex: 1;
  }

  .summary-grid,
  .info-list {
    grid-template-columns: 1fr;
  }
}
</style>
