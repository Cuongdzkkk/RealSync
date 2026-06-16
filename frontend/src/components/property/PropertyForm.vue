<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { FormInstance, FormRules } from 'element-plus';
import { ElMessage } from 'element-plus';

import PropertyImageUploader from '@/components/property/PropertyImageUploader.vue';
import { propertyService } from '@/services/propertyService';
import type {
  LocationOption,
  Property,
  PropertyImage,
  PropertyPayload,
  PropertyCategory,
  PropertyType
} from '@/types/property';

const props = withDefaults(defineProps<{
  mode: 'create' | 'edit';
  property?: Property | null;
  submitting?: boolean;
  imageUploading?: boolean;
}>(), {
  property: null,
  submitting: false,
  imageUploading: false
});

const emit = defineEmits<{
  (event: 'submit', payload: PropertyPayload, files: File[]): void;
  (event: 'cancel'): void;
  (event: 'delete-image', image: PropertyImage): void;
}>();

interface PropertyFormState {
  title: string;
  code: string;
  description: string;
  price: number;
  area: number;
  priceUnit: string;
  address: string;
  provinceId: string;
  districtId: string;
  wardId: string;
  categoryId: string;
  typeId: string;
  status: string;
  listingType: string;
  bedrooms: number;
  bathrooms: number;
  floors: number;
  direction: string;
  legalStatus: string;
}

const formRef = ref<FormInstance>();
const dictionariesLoading = ref(false);
const locationLoading = ref(false);
const imageFiles = ref<File[]>([]);

const categories = ref<PropertyCategory[]>([]);
const propertyTypes = ref<PropertyType[]>([]);
const provinces = ref<LocationOption[]>([]);
const districts = ref<LocationOption[]>([]);
const wards = ref<LocationOption[]>([]);

const form = reactive<PropertyFormState>({
  title: '',
  code: '',
  description: '',
  price: 0,
  area: 0,
  priceUnit: 'VND',
  address: '',
  provinceId: '',
  districtId: '',
  wardId: '',
  categoryId: '',
  typeId: '',
  status: 'Available',
  listingType: 'Sale',
  bedrooms: 0,
  bathrooms: 0,
  floors: 0,
  direction: '',
  legalStatus: ''
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

const listingTypeOptions = [
  { label: 'Bán', value: 'Sale' },
  { label: 'Cho thuê', value: 'Rent' }
];

const directionOptions = [
  'Đông',
  'Tây',
  'Nam',
  'Bắc',
  'Đông Bắc',
  'Đông Nam',
  'Tây Bắc',
  'Tây Nam'
];

const rules: FormRules<PropertyFormState> = {
  title: [{ required: true, message: 'Vui lòng nhập tiêu đề bất động sản.', trigger: 'blur' }],
  price: [{ type: 'number', min: 0, message: 'Giá phải lớn hơn hoặc bằng 0.', trigger: 'change' }],
  area: [{ type: 'number', min: 0, message: 'Diện tích phải lớn hơn hoặc bằng 0.', trigger: 'change' }],
  categoryId: [{ required: true, message: 'Vui lòng chọn danh mục.', trigger: 'change' }],
  typeId: [{ required: true, message: 'Vui lòng chọn loại bất động sản.', trigger: 'change' }]
};

const submitLabel = computed(() => (props.mode === 'create' ? 'Tạo bất động sản' : 'Lưu thay đổi'));
const existingImages = computed(() => props.property?.images ?? []);

function resetForm() {
  Object.assign(form, {
    title: '',
    code: '',
    description: '',
    price: 0,
    area: 0,
    priceUnit: 'VND',
    address: '',
    provinceId: '',
    districtId: '',
    wardId: '',
    categoryId: '',
    typeId: '',
    status: 'Available',
    listingType: 'Sale',
    bedrooms: 0,
    bathrooms: 0,
    floors: 0,
    direction: '',
    legalStatus: ''
  });
  districts.value = [];
  wards.value = [];
  imageFiles.value = [];
}

async function hydrateForm(property: Property) {
  Object.assign(form, {
    title: property.title ?? '',
    code: property.code ?? '',
    description: property.description ?? '',
    price: Number(property.price ?? 0),
    area: Number(property.area ?? property.acreage ?? 0),
    priceUnit: property.priceUnit ?? 'VND',
    address: property.address ?? '',
    provinceId: property.provinceId ?? '',
    districtId: property.districtId ?? '',
    wardId: property.wardId ?? '',
    categoryId: property.categoryId ?? property.propertyCategoryId ?? '',
    typeId: property.typeId ?? property.propertyTypeId ?? '',
    status: property.status ?? 'Available',
    listingType: property.listingType ?? 'Sale',
    bedrooms: property.bedrooms ?? 0,
    bathrooms: property.bathrooms ?? 0,
    floors: property.floors ?? 0,
    direction: property.direction ?? '',
    legalStatus: property.legalStatus ?? ''
  });

  imageFiles.value = [];

  if (form.provinceId) {
    await loadDistricts(form.provinceId);
  }

  if (form.districtId) {
    await loadWards(form.districtId);
  }
}

async function loadDictionaries() {
  dictionariesLoading.value = true;
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
    ElMessage.error('Không tải được danh mục hoặc khu vực. Vui lòng thử lại.');
  } finally {
    dictionariesLoading.value = false;
  }
}

async function loadDistricts(provinceId: string) {
  if (!provinceId) {
    districts.value = [];
    return;
  }

  locationLoading.value = true;
  try {
    districts.value = (await propertyService.getDistricts(provinceId)).filter((item) => item.isActive);
  } finally {
    locationLoading.value = false;
  }
}

async function loadWards(districtId: string) {
  if (!districtId) {
    wards.value = [];
    return;
  }

  locationLoading.value = true;
  try {
    wards.value = (await propertyService.getWards(districtId)).filter((item) => item.isActive);
  } finally {
    locationLoading.value = false;
  }
}

async function handleProvinceChange(value: string) {
  form.districtId = '';
  form.wardId = '';
  wards.value = [];
  await loadDistricts(value);
}

async function handleDistrictChange(value: string) {
  form.wardId = '';
  await loadWards(value);
}

async function handleSubmit() {
  const valid = await formRef.value?.validate().catch(() => false);
  if (!valid) return;

  emit('submit', {
    title: form.title.trim(),
    code: form.code.trim() || null,
    description: form.description.trim() || null,
    price: form.price,
    area: form.area,
    priceUnit: form.priceUnit,
    address: form.address.trim() || null,
    provinceId: form.provinceId || null,
    districtId: form.districtId || null,
    wardId: form.wardId || null,
    categoryId: form.categoryId,
    typeId: form.typeId,
    status: form.status,
    listingType: form.listingType,
    bedrooms: form.bedrooms,
    bathrooms: form.bathrooms,
    floors: form.floors,
    direction: form.direction || null,
    legalStatus: form.legalStatus.trim() || null
  }, imageFiles.value);
}

watch(
  () => props.property,
  async (property) => {
    if (property) {
      await hydrateForm(property);
    } else {
      resetForm();
    }
  },
  { immediate: true }
);

loadDictionaries();
</script>

<template>
  <el-form
    ref="formRef"
    v-loading="dictionariesLoading"
    class="property-form"
    :model="form"
    :rules="rules"
    label-position="top"
    @submit.prevent="handleSubmit"
  >
    <section class="form-section">
      <div class="form-section__header">
        <h3>Basic Information</h3>
        <p>Thông tin nhận diện, mô tả và giá trị chính của bất động sản.</p>
      </div>

      <div class="form-grid">
        <el-form-item label="Title" prop="title" class="form-grid__wide">
          <el-input v-model="form.title" maxlength="200" show-word-limit placeholder="Căn hộ 2PN view sông..." />
        </el-form-item>

        <el-form-item label="Code">
          <el-input v-model="form.code" placeholder="RS-PROP-001" />
        </el-form-item>

        <el-form-item label="Listing Type">
          <el-select v-model="form.listingType" class="full-control">
            <el-option
              v-for="option in listingTypeOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="Price" prop="price">
          <el-input-number v-model="form.price" class="full-control" :min="0" :controls="false" />
        </el-form-item>

        <el-form-item label="Area (m2)" prop="area">
          <el-input-number v-model="form.area" class="full-control" :min="0" :controls="false" />
        </el-form-item>

        <el-form-item label="Description" class="form-grid__wide">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="5"
            placeholder="Mô tả điểm nổi bật, tình trạng bàn giao, tiện ích..."
          />
        </el-form-item>
      </div>
    </section>

    <section class="form-section">
      <div class="form-section__header">
        <h3>Location</h3>
        <p>Chọn khu vực theo thứ tự tỉnh/thành, quận/huyện, phường/xã.</p>
      </div>

      <div class="form-grid">
        <el-form-item label="Province">
          <el-select
            v-model="form.provinceId"
            clearable
            filterable
            class="full-control"
            placeholder="Chọn tỉnh/thành"
            @change="handleProvinceChange"
          >
            <el-option v-for="item in provinces" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="District">
          <el-select
            v-model="form.districtId"
            clearable
            filterable
            class="full-control"
            placeholder="Chọn quận/huyện"
            :disabled="!form.provinceId"
            :loading="locationLoading"
            @change="handleDistrictChange"
          >
            <el-option v-for="item in districts" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="Ward">
          <el-select
            v-model="form.wardId"
            clearable
            filterable
            class="full-control"
            placeholder="Chọn phường/xã"
            :disabled="!form.districtId"
            :loading="locationLoading"
          >
            <el-option v-for="item in wards" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="Address" class="form-grid__wide">
          <el-input v-model="form.address" placeholder="Số nhà, tên đường, block/tòa..." />
        </el-form-item>
      </div>
    </section>

    <section class="form-section">
      <div class="form-section__header">
        <h3>Property Details</h3>
        <p>Phân loại, trạng thái và các thông số vận hành cần theo dõi.</p>
      </div>

      <div class="form-grid">
        <el-form-item label="Category" prop="categoryId">
          <el-select v-model="form.categoryId" filterable class="full-control" placeholder="Chọn danh mục">
            <el-option v-for="item in categories" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="Type" prop="typeId">
          <el-select v-model="form.typeId" filterable class="full-control" placeholder="Chọn loại BĐS">
            <el-option v-for="item in propertyTypes" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="Status">
          <el-select v-model="form.status" class="full-control">
            <el-option v-for="option in statusOptions" :key="option.value" :label="option.label" :value="option.value" />
          </el-select>
        </el-form-item>

        <el-form-item label="Direction">
          <el-select v-model="form.direction" clearable class="full-control" placeholder="Chọn hướng">
            <el-option v-for="item in directionOptions" :key="item" :label="item" :value="item" />
          </el-select>
        </el-form-item>

        <el-form-item label="Bedrooms">
          <el-input-number v-model="form.bedrooms" class="full-control" :min="0" :controls="false" />
        </el-form-item>

        <el-form-item label="Bathrooms">
          <el-input-number v-model="form.bathrooms" class="full-control" :min="0" :controls="false" />
        </el-form-item>

        <el-form-item label="Floors">
          <el-input-number v-model="form.floors" class="full-control" :min="0" :controls="false" />
        </el-form-item>

        <el-form-item label="Legal Status">
          <el-input v-model="form.legalStatus" placeholder="Sổ hồng, hợp đồng mua bán..." />
        </el-form-item>
      </div>
    </section>

    <section class="form-section">
      <div class="form-section__header">
        <h3>Images</h3>
        <p>Ảnh mới sẽ upload sau khi thông tin bất động sản được lưu thành công.</p>
      </div>

      <PropertyImageUploader
        v-model="imageFiles"
        :existing-images="existingImages"
        :uploading="imageUploading"
        @remove-existing="emit('delete-image', $event)"
      />
    </section>

    <div class="form-footer">
      <el-button :disabled="submitting || imageUploading" @click="emit('cancel')">Cancel</el-button>
      <el-button
        native-type="submit"
        :loading="submitting || imageUploading"
        class="primary-action"
      >
        {{ submitLabel }}
      </el-button>
    </div>
  </el-form>
</template>

<style scoped>
.property-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-section {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
  display: grid;
  gap: 18px;
  grid-template-columns: minmax(190px, 260px) minmax(0, 1fr);
  padding: 20px;
}

.form-section__header h3 {
  color: var(--color-text-primary);
  font-size: 15px;
  margin: 0 0 6px;
}

.form-section__header p {
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 1.5;
  margin: 0;
}

.form-grid {
  display: grid;
  gap: 12px 14px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid__wide {
  grid-column: 1 / -1;
}

.full-control {
  width: 100%;
}

.form-footer {
  align-items: center;
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding: 14px 16px;
  position: sticky;
  bottom: 16px;
  z-index: var(--z-sticky);
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

:deep(.el-form-item) {
  margin-bottom: 0;
}

:deep(.el-input-number .el-input__inner) {
  text-align: left;
}

@media (max-width: 920px) {
  .form-section {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .form-grid {
    grid-template-columns: 1fr;
  }

  .form-section {
    padding: 16px;
  }

  .form-footer {
    bottom: 8px;
  }
}
</style>
