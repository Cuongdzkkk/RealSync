<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';

import PropertyForm from '@/components/property/PropertyForm.vue';
import { propertyService } from '@/services/propertyService';
import type { Property, PropertyImage, PropertyPayload } from '@/types/property';

const route = useRoute();
const router = useRouter();

const property = ref<Property | null>(null);
const loading = ref(false);
const submitting = ref(false);
const imageUploading = ref(false);
const errorMessage = ref('');

const isEdit = computed(() => route.name === 'property-edit');
const propertyId = computed(() => String(route.params.id ?? ''));
const pageTitle = computed(() => (isEdit.value ? 'Edit Property' : 'Create Property'));
const pageSubtitle = computed(() =>
  isEdit.value ? 'Cập nhật thông tin và hình ảnh bất động sản' : 'Thêm bất động sản mới vào kho dữ liệu'
);

function getErrorMessage(error: any) {
  if (error?.response?.status === 403) {
    return 'Bạn không có quyền thực hiện thao tác này.';
  }

  return error?.response?.data?.message ?? 'Không thực hiện được thao tác. Vui lòng thử lại.';
}

async function loadProperty() {
  if (!isEdit.value) return;

  loading.value = true;
  errorMessage.value = '';

  try {
    property.value = await propertyService.getPropertyById(propertyId.value);
  } catch (error: any) {
    errorMessage.value = getErrorMessage(error);
  } finally {
    loading.value = false;
  }
}

async function handleSubmit(payload: PropertyPayload, files: File[]) {
  submitting.value = true;

  try {
    const saved = isEdit.value
      ? await propertyService.updateProperty(propertyId.value, payload)
      : await propertyService.createProperty(payload);

    if (files.length > 0) {
      imageUploading.value = true;
      try {
        await propertyService.uploadPropertyImages(saved.id, files);
      } catch (error: any) {
        ElMessage.error(`Lưu thông tin thành công nhưng upload ảnh thất bại: ${getErrorMessage(error)}`);
      } finally {
        imageUploading.value = false;
      }
    }

    ElMessage.success(isEdit.value ? 'Đã cập nhật bất động sản.' : 'Đã tạo bất động sản mới.');
    router.push({ name: 'property-detail', params: { id: saved.id } });
  } catch (error: any) {
    ElMessage.error(getErrorMessage(error));
  } finally {
    submitting.value = false;
    imageUploading.value = false;
  }
}

async function handleDeleteImage(image: PropertyImage) {
  if (!property.value) return;

  try {
    await ElMessageBox.confirm(
      `Xóa ảnh "${image.originalFileName || image.fileName}" khỏi bất động sản này?`,
      'Xác nhận xóa ảnh',
      {
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }
    );

    imageUploading.value = true;
    await propertyService.deletePropertyImage(property.value.id, image.id);
    property.value = {
      ...property.value,
      images: property.value.images?.filter((item) => item.id !== image.id) ?? []
    };
    ElMessage.success('Đã xóa ảnh.');
  } catch (error: any) {
    if (error === 'cancel' || error === 'close') return;
    ElMessage.error(getErrorMessage(error));
  } finally {
    imageUploading.value = false;
  }
}

onMounted(loadProperty);
</script>

<template>
  <section class="property-form-page">
    <header class="form-page-header">
      <div>
        <h1>{{ pageTitle }}</h1>
        <p>{{ pageSubtitle }}</p>
      </div>
      <el-button :icon="ArrowLeft" @click="router.push(isEdit ? { name: 'property-detail', params: { id: propertyId } } : { name: 'properties' })">
        Back
      </el-button>
    </header>

    <section v-if="loading" class="form-state-card">
      <span class="skeleton skeleton-title"></span>
      <span class="skeleton skeleton-line"></span>
      <span class="skeleton skeleton-line"></span>
    </section>

    <section v-else-if="errorMessage" class="form-state-card form-state-card--center">
      <h3>Không tải được dữ liệu</h3>
      <p>{{ errorMessage }}</p>
      <el-button class="primary-action" @click="loadProperty">Retry</el-button>
    </section>

    <PropertyForm
      v-else
      :mode="isEdit ? 'edit' : 'create'"
      :property="property"
      :submitting="submitting"
      :image-uploading="imageUploading"
      @submit="handleSubmit"
      @cancel="router.push(isEdit && property ? { name: 'property-detail', params: { id: property.id } } : { name: 'properties' })"
      @delete-image="handleDeleteImage"
    />
  </section>
</template>

<style scoped>
.property-form-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.form-page-header,
.form-state-card {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
}

.form-page-header {
  align-items: center;
  display: flex;
  gap: 16px;
  justify-content: space-between;
  padding: 20px 22px;
}

.form-page-header h1 {
  font-size: 22px;
  line-height: 1.2;
  margin: 0 0 6px;
}

.form-page-header p {
  color: var(--color-text-secondary);
  margin: 0;
}

.form-state-card {
  display: flex;
  flex-direction: column;
  gap: 14px;
  min-height: 220px;
  padding: 22px;
}

.form-state-card--center {
  align-items: center;
  justify-content: center;
  text-align: center;
}

.form-state-card h3 {
  font-size: 16px;
  margin: 0;
}

.form-state-card p {
  color: var(--color-text-secondary);
  margin: 0 0 4px;
}

.skeleton-title {
  height: 30px;
  width: 240px;
}

.skeleton-line {
  height: 18px;
  max-width: 680px;
  width: 100%;
}

.primary-action {
  background: var(--color-yellow);
  border-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-weight: 700;
}

@media (max-width: 640px) {
  .form-page-header {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
