<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { Close, Delete, UploadFilled } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';

import type { PropertyImage } from '@/types/property';

const props = withDefaults(defineProps<{
  modelValue: File[];
  existingImages?: PropertyImage[];
  uploading?: boolean;
  readonly?: boolean;
}>(), {
  modelValue: () => [],
  existingImages: () => [],
  uploading: false,
  readonly: false
});

const emit = defineEmits<{
  (event: 'update:modelValue', value: File[]): void;
  (event: 'remove-existing', image: PropertyImage): void;
}>();

interface LocalPreview {
  id: string;
  file: File;
  url: string;
}

const inputRef = ref<HTMLInputElement | null>(null);
const previews = ref<LocalPreview[]>([]);

const acceptedTypes = ['image/jpeg', 'image/png', 'image/webp'];
const maxSize = 5 * 1024 * 1024;
const accept = acceptedTypes.join(',');
const fallbackImage =
  'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="480" height="320" viewBox="0 0 480 320"><rect width="480" height="320" fill="%23f8fafc"/><rect x="96" y="78" width="288" height="164" rx="14" fill="%23e2e8f0"/><path d="M145 214l58-64 42 42 28-30 62 52H145z" fill="%2394a3b8"/><circle cx="305" cy="127" r="20" fill="%23cbd5e1"/></svg>';

const hasImages = computed(() => props.existingImages.length > 0 || previews.value.length > 0);

function revokePreviews() {
  previews.value.forEach((preview) => URL.revokeObjectURL(preview.url));
}

watch(
  () => props.modelValue,
  (files) => {
    revokePreviews();
    previews.value = files.map((file, index) => ({
      id: `${file.name}-${file.lastModified}-${index}`,
      file,
      url: URL.createObjectURL(file)
    }));
  },
  { immediate: true }
);

onBeforeUnmount(() => {
  revokePreviews();
});

function openPicker() {
  if (props.readonly || props.uploading) return;
  inputRef.value?.click();
}

function validateFile(file: File) {
  if (!acceptedTypes.includes(file.type)) {
    ElMessage.warning('Chỉ hỗ trợ ảnh JPG, JPEG, PNG hoặc WEBP.');
    return false;
  }

  if (file.size > maxSize) {
    ElMessage.warning(`Ảnh ${file.name} vượt quá giới hạn 5MB.`);
    return false;
  }

  return true;
}

function handleFilesSelected(event: Event) {
  const target = event.target as HTMLInputElement;
  const selected = Array.from(target.files ?? []);
  const validFiles = selected.filter(validateFile);

  if (validFiles.length > 0) {
    emit('update:modelValue', [...props.modelValue, ...validFiles]);
  }

  target.value = '';
}

function removeLocal(index: number) {
  const next = [...props.modelValue];
  next.splice(index, 1);
  emit('update:modelValue', next);
}

function handleImageError(event: Event) {
  const target = event.target as HTMLImageElement;
  target.src = fallbackImage;
}
</script>

<template>
  <div class="property-uploader" :class="{ 'is-readonly': readonly }">
    <input
      ref="inputRef"
      class="property-uploader__input"
      type="file"
      multiple
      :accept="accept"
      :disabled="readonly || uploading"
      @change="handleFilesSelected"
    />

    <button
      v-if="!readonly"
      type="button"
      class="property-uploader__dropzone"
      :disabled="uploading"
      @click="openPicker"
    >
      <el-icon><UploadFilled /></el-icon>
      <span class="property-uploader__title">
        {{ uploading ? 'Đang upload ảnh...' : 'Chọn ảnh bất động sản' }}
      </span>
      <span class="property-uploader__hint">JPG, JPEG, PNG, WEBP. Mỗi ảnh tối đa 5MB.</span>
    </button>

    <div v-if="hasImages" class="property-uploader__grid">
      <article
        v-for="image in existingImages"
        :key="image.id"
        class="property-uploader__item"
      >
        <img :src="image.url" :alt="image.originalFileName || image.fileName" @error="handleImageError" />
        <span v-if="image.isThumbnail" class="property-uploader__badge">Thumbnail</span>
        <button
          v-if="!readonly"
          type="button"
          class="property-uploader__remove"
          :disabled="uploading"
          :title="`Xóa ảnh ${image.originalFileName || image.fileName}`"
          @click="emit('remove-existing', image)"
        >
          <el-icon><Delete /></el-icon>
        </button>
      </article>

      <article
        v-for="(preview, index) in previews"
        :key="preview.id"
        class="property-uploader__item"
      >
        <img :src="preview.url" :alt="preview.file.name" @error="handleImageError" />
        <span class="property-uploader__badge property-uploader__badge--new">New</span>
        <button
          type="button"
          class="property-uploader__remove"
          :disabled="uploading"
          :title="`Bỏ ảnh ${preview.file.name}`"
          @click="removeLocal(index)"
        >
          <el-icon><Close /></el-icon>
        </button>
      </article>
    </div>

    <div v-else class="property-uploader__empty">
      Chưa có ảnh nào được chọn.
    </div>
  </div>
</template>

<style scoped>
.property-uploader {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.property-uploader__input {
  display: none;
}

.property-uploader__dropzone {
  align-items: center;
  background: #ffffff;
  border: 1px dashed var(--color-border-strong);
  border-radius: 8px;
  color: var(--color-text-primary);
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-height: 118px;
  justify-content: center;
  padding: 18px;
  transition: border-color var(--duration-fast), background var(--duration-fast);
  width: 100%;
}

.property-uploader__dropzone:hover {
  background: #fafafa;
  border-color: var(--color-yellow);
}

.property-uploader__dropzone:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.property-uploader__dropzone .el-icon {
  color: var(--color-text-muted);
  font-size: 24px;
}

.property-uploader__title {
  font-size: 13px;
  font-weight: 700;
}

.property-uploader__hint,
.property-uploader__empty {
  color: var(--color-text-muted);
  font-size: 12px;
}

.property-uploader__empty {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 18px;
  text-align: center;
}

.property-uploader__grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fill, minmax(136px, 1fr));
}

.property-uploader__item {
  aspect-ratio: 4 / 3;
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.property-uploader__item img {
  height: 100%;
  object-fit: cover;
  width: 100%;
}

.property-uploader__badge {
  background: rgba(15, 23, 42, 0.78);
  border-radius: 5px;
  color: #ffffff;
  font-size: 10px;
  font-weight: 700;
  left: 8px;
  padding: 3px 7px;
  position: absolute;
  top: 8px;
}

.property-uploader__badge--new {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
}

.property-uploader__remove {
  align-items: center;
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  color: var(--color-danger);
  cursor: pointer;
  display: flex;
  height: 28px;
  justify-content: center;
  position: absolute;
  right: 8px;
  top: 8px;
  transition: background var(--duration-fast), border-color var(--duration-fast);
  width: 28px;
}

.property-uploader__remove:hover {
  background: var(--color-danger-bg);
  border-color: var(--color-danger-border);
}
</style>
