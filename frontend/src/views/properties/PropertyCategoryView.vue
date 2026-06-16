<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { Delete, Edit, Plus, Refresh } from '@element-plus/icons-vue';
import type { FormInstance, FormRules } from 'element-plus';
import { ElMessage, ElMessageBox } from 'element-plus';

import { propertyService } from '@/services/propertyService';
import { useAuthStore } from '@/stores/useAuthStore';
import type { PropertyCategory, PropertyCategoryPayload } from '@/types/property';
import { formatDate } from '@/utils/format';

const authStore = useAuthStore();

const categories = ref<PropertyCategory[]>([]);
const loading = ref(false);
const submitting = ref(false);
const errorMessage = ref('');
const dialogVisible = ref(false);
const editingCategory = ref<PropertyCategory | null>(null);
const formRef = ref<FormInstance>();

const form = reactive<PropertyCategoryPayload>({
  name: '',
  slug: '',
  description: '',
  isActive: true
});

const canMutate = computed(() => {
  const role = authStore.user?.role;
  return Boolean(role && ['Admin', 'Manager', 'Marketing'].includes(role));
});

const dialogTitle = computed(() => (editingCategory.value ? 'Edit Category' : 'Create Category'));

const rules: FormRules<PropertyCategoryPayload> = {
  name: [{ required: true, message: 'Vui lòng nhập tên danh mục.', trigger: 'blur' }]
};

function getErrorMessage(error: any) {
  if (error?.response?.status === 403) {
    return 'Bạn không có quyền thực hiện thao tác này.';
  }

  return error?.response?.data?.message ?? 'Không thực hiện được thao tác. Vui lòng thử lại.';
}

function toSlug(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

async function loadCategories() {
  loading.value = true;
  errorMessage.value = '';

  try {
    categories.value = await propertyService.getPropertyCategories();
  } catch (error: any) {
    errorMessage.value = getErrorMessage(error);
  } finally {
    loading.value = false;
  }
}

function openCreateDialog() {
  editingCategory.value = null;
  Object.assign(form, {
    name: '',
    slug: '',
    description: '',
    isActive: true
  });
  dialogVisible.value = true;
}

function openEditDialog(category: PropertyCategory) {
  editingCategory.value = category;
  Object.assign(form, {
    name: category.name,
    slug: category.slug,
    description: category.description ?? '',
    isActive: category.isActive
  });
  dialogVisible.value = true;
}

async function submitCategory() {
  const valid = await formRef.value?.validate().catch(() => false);
  if (!valid) return;

  submitting.value = true;
  try {
    const payload = {
      name: form.name.trim(),
      slug: form.slug?.trim() || toSlug(form.name),
      description: form.description?.trim() || null,
      isActive: form.isActive
    };

    if (editingCategory.value) {
      await propertyService.updatePropertyCategory(editingCategory.value.id, payload);
      ElMessage.success('Đã cập nhật danh mục.');
    } else {
      await propertyService.createPropertyCategory(payload);
      ElMessage.success('Đã tạo danh mục mới.');
    }

    dialogVisible.value = false;
    await loadCategories();
  } catch (error: any) {
    ElMessage.error(getErrorMessage(error));
  } finally {
    submitting.value = false;
  }
}

async function deleteCategory(category: PropertyCategory) {
  if (!canMutate.value) return;

  try {
    await ElMessageBox.confirm(
      `Xóa danh mục "${category.name}"?`,
      'Xác nhận xóa',
      {
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }
    );

    await propertyService.deletePropertyCategory(category.id);
    ElMessage.success('Đã xóa danh mục.');
    await loadCategories();
  } catch (error: any) {
    if (error === 'cancel' || error === 'close') return;
    ElMessage.error(getErrorMessage(error));
  }
}

onMounted(loadCategories);
</script>

<template>
  <section class="category-page">
    <header class="category-header">
      <div>
        <h1>Property Categories</h1>
        <p>Quản lý danh mục bất động sản</p>
      </div>
      <div class="category-header__actions">
        <el-button :icon="Refresh" :loading="loading" @click="loadCategories">Refresh</el-button>
        <el-button v-if="canMutate" class="primary-action" @click="openCreateDialog">
          <el-icon><Plus /></el-icon>
          <span>Create category</span>
        </el-button>
      </div>
    </header>

    <section class="category-table-card" v-loading="loading">
      <div v-if="errorMessage" class="state-panel">
        <h3>Không tải được danh mục</h3>
        <p>{{ errorMessage }}</p>
        <el-button class="primary-action" @click="loadCategories">Retry</el-button>
      </div>

      <template v-else>
        <el-table v-if="categories.length" :data="categories" row-key="id">
          <el-table-column label="Name" min-width="180">
            <template #default="{ row }">
              <strong>{{ row.name }}</strong>
            </template>
          </el-table-column>
          <el-table-column label="Slug" min-width="180">
            <template #default="{ row }">
              <span class="mono muted-text">{{ row.slug }}</span>
            </template>
          </el-table-column>
          <el-table-column label="Description" min-width="240">
            <template #default="{ row }">{{ row.description || '-' }}</template>
          </el-table-column>
          <el-table-column label="Status" width="120">
            <template #default="{ row }">
              <span class="category-status" :class="{ 'is-active': row.isActive }">
                {{ row.isActive ? 'Active' : 'Inactive' }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="Created" width="120">
            <template #default="{ row }">{{ row.createdAt ? formatDate(row.createdAt) : '-' }}</template>
          </el-table-column>
          <el-table-column v-if="canMutate" label="Actions" width="112" align="right">
            <template #default="{ row }">
              <div class="table-actions">
                <el-button circle :icon="Edit" title="Edit" @click="openEditDialog(row)" />
                <el-button circle :icon="Delete" title="Delete" @click="deleteCategory(row)" />
              </div>
            </template>
          </el-table-column>
        </el-table>

        <el-empty v-else description="Chưa có danh mục bất động sản.">
          <el-button v-if="canMutate" class="primary-action" @click="openCreateDialog">Create category</el-button>
        </el-empty>
      </template>
    </section>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="460px">
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
        <el-form-item label="Name" prop="name">
          <el-input v-model="form.name" placeholder="Nhà đất bán" />
        </el-form-item>
        <el-form-item label="Slug">
          <el-input v-model="form.slug" placeholder="nha-dat-ban" />
        </el-form-item>
        <el-form-item label="Description">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="Status">
          <el-switch v-model="form.isActive" active-text="Active" inactive-text="Inactive" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button :disabled="submitting" @click="dialogVisible = false">Cancel</el-button>
        <el-button class="primary-action" :loading="submitting" @click="submitCategory">Save</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.category-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.category-header,
.category-table-card {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
}

.category-header {
  align-items: center;
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 20px 22px;
}

.category-header h1 {
  font-size: 22px;
  line-height: 1.2;
  margin: 0 0 6px;
}

.category-header p {
  color: var(--color-text-secondary);
  margin: 0;
}

.category-header__actions,
.table-actions {
  display: flex;
  gap: 8px;
}

.category-table-card {
  min-height: 320px;
  overflow: hidden;
  padding: 8px;
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

.muted-text {
  color: var(--color-text-muted);
}

.category-status {
  background: #f1f5f9;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  color: var(--color-text-secondary);
  display: inline-flex;
  font-size: 10px;
  font-weight: 800;
  line-height: 1;
  padding: 5px 8px;
  text-transform: uppercase;
}

.category-status.is-active {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
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

@media (max-width: 640px) {
  .category-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .category-header__actions {
    width: 100%;
  }
}
</style>
