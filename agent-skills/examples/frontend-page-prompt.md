# 🎨 Frontend Page Prompt Template

> Dùng prompt này khi cần agent tạo trang Frontend mới.

---

## Prompt Template

```
[FRONTEND] Tạo trang "{PageName}" đầy đủ

Yêu cầu:
1. View component trong src/views/{module}/
   - Sử dụng <script setup lang="ts">
   - Element Plus components
   - Responsive layout

2. Child components trong src/components/{module}/
   - Tách thành components nhỏ, reusable
   - Props + Emits typed

3. Pinia store trong src/stores/
   - State, getters, actions
   - API integration

4. API service trong src/services/
   - Axios calls
   - Type definitions

5. Router config
   - Thêm route mới
   - Route guard nếu cần

6. Types trong src/types/
   - Interface/Type definitions
   - Request/Response types

Tuân thủ:
- Composition API + <script setup>
- TypeScript bắt buộc
- Element Plus components
- Loading states
- Error handling (ElMessage)
- Naming convention trong SKILL.md
```

---

## Ví dụ: Trang Quản lý BĐS

```
[FRONTEND] Tạo trang "PropertyList" — Danh sách Bất Động Sản

Layout:
┌────────────────────────────────────────────────────┐
│  Header: "Quản lý Bất Động Sản"    [+ Thêm mới]  │
├────────────────────────────────────────────────────┤
│  Filters:                                          │
│  [Loại BĐS ▼] [Trạng thái ▼] [Khu vực ▼]        │
│  [Giá từ ___] [đến ___]  [🔍 Tìm kiếm]          │
├────────────────────────────────────────────────────┤
│  Table:                                            │
│  | Mã | Tiêu đề | Loại | Giá | DT | TT | Hành động│
│  |----|---------|----|------|----|----|-----------|  │
│  | P001| Nhà... | Nhà | 5 tỷ| 80 | ✅ | 👁 ✏ 🗑 │  │
│  | P002| Đất... | Đất | 3 tỷ| 200| 📝 | 👁 ✏ 🗑 │  │
├────────────────────────────────────────────────────┤
│  Pagination: [← 1 2 3 ... 8 →]   20/trang ▼      │
└────────────────────────────────────────────────────┘

Features:
- Table với ElTable + sort + pagination
- Filter bar với ElForm
- Search debounce 300ms
- Status badge (Draft=grey, Active=green, Sold=blue, Expired=red)
- Actions: View, Edit, Delete (with confirm)
- Responsive: Table → Card view trên mobile
- Loading skeleton khi fetch data
- Empty state khi không có data
```

---

## Expected Output Structure

```
Files to create/modify:

✅ src/views/properties/PropertyListView.vue      # Main page
✅ src/views/properties/PropertyDetailView.vue     # Detail page
✅ src/views/properties/PropertyCreateView.vue     # Create/Edit form
✅ src/components/property/PropertyTable.vue       # Table component
✅ src/components/property/PropertyFilter.vue      # Filter bar
✅ src/components/property/PropertyCard.vue        # Card view
✅ src/components/property/PropertyStatusBadge.vue # Status badge
✅ src/stores/usePropertyStore.ts                  # Pinia store
✅ src/services/propertyService.ts                 # API service
✅ src/types/property.ts                           # TypeScript types
✅ src/router/index.ts                             # Add routes
```

---

## Component Template

```vue
<!-- src/views/properties/PropertyListView.vue -->
<template>
  <div class="property-list-page">
    <div class="page-header">
      <h1>Quản lý Bất Động Sản</h1>
      <el-button type="primary" @click="handleCreate">
        <el-icon><Plus /></el-icon>
        Thêm mới
      </el-button>
    </div>

    <PropertyFilter
      v-model="filterParams"
      @search="handleSearch"
      @reset="handleReset"
    />

    <PropertyTable
      :data="propertyStore.properties"
      :loading="propertyStore.loading"
      :pagination="propertyStore.pagination"
      @view="handleView"
      @edit="handleEdit"
      @delete="handleDelete"
      @page-change="handlePageChange"
      @sort-change="handleSortChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox, ElMessage } from 'element-plus';
import { Plus } from '@element-plus/icons-vue';
import { usePropertyStore } from '@/stores/usePropertyStore';
import PropertyFilter from '@/components/property/PropertyFilter.vue';
import PropertyTable from '@/components/property/PropertyTable.vue';
import type { PropertyFilter as FilterType } from '@/types/property';

const router = useRouter();
const propertyStore = usePropertyStore();
const filterParams = ref<FilterType>({});

onMounted(() => {
  propertyStore.fetchProperties();
});

const handleSearch = (params: FilterType) => {
  propertyStore.fetchProperties({ ...params, page: 1 });
};

const handleReset = () => {
  filterParams.value = {};
  propertyStore.fetchProperties({ page: 1 });
};

const handleCreate = () => {
  router.push('/properties/create');
};

const handleView = (id: string) => {
  router.push(`/properties/${id}`);
};

const handleEdit = (id: string) => {
  router.push(`/properties/${id}/edit`);
};

const handleDelete = async (id: string) => {
  try {
    await ElMessageBox.confirm('Bạn có chắc muốn xóa BĐS này?', 'Xác nhận', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning',
    });
    await propertyStore.deleteProperty(id);
    ElMessage.success('Đã xóa thành công');
  } catch {
    // User cancelled
  }
};

const handlePageChange = (page: number) => {
  propertyStore.fetchProperties({ ...filterParams.value, page });
};

const handleSortChange = ({ prop, order }: { prop: string; order: string }) => {
  propertyStore.fetchProperties({
    ...filterParams.value,
    sortBy: prop,
    sortOrder: order === 'ascending' ? 'asc' : 'desc',
  });
};
</script>

<style scoped>
.property-list-page {
  padding: 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
</style>
```
