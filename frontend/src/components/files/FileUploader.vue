<script setup lang="ts">
import { ref, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { UploadFilled, Document, CircleCheck, CircleClose, Loading } from '@element-plus/icons-vue';
import { fileService } from '@/services/fileService';
import type { StoredFileResult } from '@/services/fileService';

const props = withDefaults(defineProps<{
  category?: 'properties' | 'projects' | 'avatars' | 'contracts' | 'documents';
  isPublic?: boolean;
  maxSizeMb?: number;
  accept?: string;
  entityId?: string;
}>(), {
  category: 'properties',
  isPublic: true,
  maxSizeMb: 15,
  accept: '.jpg,.jpeg,.png,.webp'
});

const emit = defineEmits<{
  (event: 'uploaded', result: StoredFileResult): void;
  (event: 'error', message: string): void;
  (event: 'clear'): void;
}>();

const fileInputRef = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);
const progress = ref(0);
const uploading = ref(false);
const errorMsg = ref('');
const uploadSuccess = ref(false);
const uploadedFile = ref<StoredFileResult | null>(null);

let abortController: AbortController | null = null;

const sizeText = computed(() => {
  if (!selectedFile.value) return '';
  const bytes = selectedFile.value.size;
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
});

function openFilePicker() {
  if (uploading.value) return;
  fileInputRef.value?.click();
}

function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    selectFile(target.files[0]);
  }
}

function selectFile(file: File) {
  errorMsg.value = '';
  uploadSuccess.value = false;
  uploadedFile.value = null;

  // Validate size
  const maxBytes = props.maxSizeMb * 1024 * 1024;
  if (file.size > maxBytes) {
    errorMsg.value = `Dung lượng file vượt quá giới hạn cho phép (${props.maxSizeMb} MB).`;
    emit('error', errorMsg.value);
    return;
  }

  // Validate extension
  const extension = '.' + file.name.split('.').pop()?.toLowerCase();
  const allowedExtensions = props.accept.split(',').map(ext => ext.trim().toLowerCase());
  if (!allowedExtensions.includes(extension)) {
    errorMsg.value = `Định dạng file không được hỗ trợ. Các định dạng cho phép: ${props.accept}`;
    emit('error', errorMsg.value);
    return;
  }

  selectedFile.value = file;
}

function handleDragOver(event: DragEvent) {
  event.preventDefault();
}

function handleDrop(event: DragEvent) {
  event.preventDefault();
  if (uploading.value) return;
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    selectFile(event.dataTransfer.files[0]);
  }
}

async function startUpload() {
  if (!selectedFile.value || uploading.value) return;

  uploading.value = true;
  progress.value = 0;
  errorMsg.value = '';
  abortController = new AbortController();

  try {
    const onProgress = (progressEvent: any) => {
      if (progressEvent.total) {
        progress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total);
      }
    };

    let response;
    if (props.isPublic) {
      response = await fileService.uploadPublicImage(
        selectedFile.value,
        props.category as any,
        props.entityId,
        onProgress
      );
    } else {
      response = await fileService.uploadPrivateDocument(
        selectedFile.value,
        props.category as any,
        props.entityId,
        onProgress
      );
    }

    if (response.success && response.data) {
      uploadSuccess.value = true;
      uploadedFile.value = response.data;
      emit('uploaded', response.data);
      ElMessage.success('Upload file thành công!');
    } else {
      throw new Error(response.message || 'Không thể upload file.');
    }
  } catch (error: any) {
    if (error.name === 'CanceledError' || error.name === 'AbortError') {
      errorMsg.value = 'Đã hủy quá trình upload.';
    } else {
      errorMsg.value = error.response?.data?.message || error.message || 'Có lỗi xảy ra khi upload file.';
      emit('error', errorMsg.value);
    }
  } finally {
    uploading.value = false;
    abortController = null;
  }
}

function cancelUpload() {
  if (abortController) {
    abortController.abort();
  }
}

function clear() {
  selectedFile.value = null;
  progress.value = 0;
  uploading.value = false;
  errorMsg.value = '';
  uploadSuccess.value = false;
  uploadedFile.value = null;
  if (fileInputRef.value) {
    fileInputRef.value.value = '';
  }
  emit('clear');
}
</script>

<template>
  <div class="file-uploader">
    <input
      ref="fileInputRef"
      type="file"
      class="file-uploader__input"
      :accept="accept"
      @change="handleFileChange"
    />

    <div
      v-if="!selectedFile"
      class="file-uploader__dropzone"
      @click="openFilePicker"
      @dragover="handleDragOver"
      @drop="handleDrop"
    >
      <el-icon class="file-uploader__icon"><UploadFilled /></el-icon>
      <div class="file-uploader__text">
        <span>Kéo thả file vào đây hoặc </span>
        <span class="file-uploader__link">chọn file từ máy</span>
      </div>
      <div class="file-uploader__hint">
        Cho phép các định dạng: {{ accept }} (Tối đa {{ maxSizeMb }}MB)
      </div>
    </div>

    <div v-else class="file-uploader__preview">
      <div class="file-uploader__file-info">
        <el-icon class="file-uploader__file-icon"><Document /></el-icon>
        <div class="file-uploader__meta">
          <div class="file-uploader__filename" :title="selectedFile.name">
            {{ selectedFile.name }}
          </div>
          <div class="file-uploader__size">{{ sizeText }}</div>
        </div>
        <button
          v-if="!uploading"
          type="button"
          class="file-uploader__clear-btn"
          @click="clear"
        >
          <el-icon><CircleClose /></el-icon>
        </button>
      </div>

      <!-- Upload Progress -->
      <div v-if="uploading" class="file-uploader__progress-section">
        <div class="file-uploader__progress-header">
          <span class="file-uploader__status-text">
            <el-icon class="is-loading"><Loading /></el-icon> Đang tải lên...
          </span>
          <span class="file-uploader__percentage">{{ progress }}%</span>
        </div>
        <div class="file-uploader__progress-bar">
          <div class="file-uploader__progress-fill" :style="{ width: `${progress}%` }"></div>
        </div>
        <button
          type="button"
          class="file-uploader__cancel-btn"
          @click="cancelUpload"
        >
          Hủy upload
        </button>
      </div>

      <!-- Actions -->
      <div v-if="!uploading && !uploadSuccess" class="file-uploader__actions">
        <button
          type="button"
          class="file-uploader__btn file-uploader__btn--primary"
          @click="startUpload"
        >
          Tải lên Server
        </button>
      </div>

      <!-- Success State -->
      <div v-if="uploadSuccess" class="file-uploader__success">
        <el-icon class="file-uploader__success-icon"><CircleCheck /></el-icon>
        <span>Tải lên thành công!</span>
      </div>

      <!-- Error State -->
      <div v-if="errorMsg" class="file-uploader__error">
        {{ errorMsg }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.file-uploader {
  width: 100%;
}

.file-uploader__input {
  display: none;
}

.file-uploader__dropzone {
  border: 1px dashed var(--color-border-strong, #cbd5e1);
  border-radius: 8px;
  background-color: #ffffff;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: all var(--duration-fast, 0.2s) ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.file-uploader__dropzone:hover {
  background-color: #fafafa;
  border-color: var(--color-yellow, #e2a000);
}

.file-uploader__icon {
  font-size: 32px;
  color: var(--color-text-muted, #94a3b8);
}

.file-uploader__text {
  font-size: 13px;
  color: var(--color-text-primary, #1e293b);
  font-weight: 500;
}

.file-uploader__link {
  color: var(--color-yellow, #e2a000);
  font-weight: 600;
}

.file-uploader__hint {
  font-size: 11px;
  color: var(--color-text-muted, #94a3b8);
}

.file-uploader__preview {
  border: 1px solid var(--color-border, #e2e8f0);
  border-radius: 8px;
  background-color: #ffffff;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.file-uploader__file-info {
  display: flex;
  align-items: center;
  gap: 12px;
  background-color: #f8fafc;
  padding: 10px 12px;
  border-radius: 6px;
  position: relative;
}

.file-uploader__file-icon {
  font-size: 24px;
  color: var(--color-yellow, #e2a000);
}

.file-uploader__meta {
  flex: 1;
  min-width: 0;
}

.file-uploader__filename {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary, #1e293b);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-uploader__size {
  font-size: 11px;
  color: var(--color-text-muted, #94a3b8);
}

.file-uploader__clear-btn {
  background: none;
  border: none;
  padding: 4px;
  color: var(--color-danger, #ef4444);
  cursor: pointer;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s;
}

.file-uploader__clear-btn:hover {
  opacity: 0.8;
}

.file-uploader__progress-section {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.file-uploader__progress-header {
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  font-weight: 600;
}

.file-uploader__status-text {
  color: var(--color-text-primary, #1e293b);
  display: flex;
  align-items: center;
  gap: 4px;
}

.file-uploader__percentage {
  color: var(--color-yellow, #e2a000);
}

.file-uploader__progress-bar {
  height: 6px;
  background-color: #f1f5f9;
  border-radius: 3px;
  overflow: hidden;
}

.file-uploader__progress-fill {
  height: 100%;
  background-color: var(--color-yellow, #e2a000);
  border-radius: 3px;
  transition: width 0.1s ease;
}

.file-uploader__cancel-btn {
  align-self: flex-end;
  background: none;
  border: none;
  color: var(--color-text-muted, #94a3b8);
  font-size: 11px;
  cursor: pointer;
  font-weight: 600;
  text-decoration: underline;
}

.file-uploader__cancel-btn:hover {
  color: var(--color-danger, #ef4444);
}

.file-uploader__actions {
  display: flex;
  justify-content: flex-end;
}

.file-uploader__btn {
  padding: 8px 16px;
  font-size: 12px;
  font-weight: 700;
  border-radius: 6px;
  cursor: pointer;
  border: none;
  transition: opacity 0.2s;
}

.file-uploader__btn--primary {
  background-color: var(--color-yellow, #e2a000);
  color: var(--color-yellow-text, #ffffff);
}

.file-uploader__btn:hover {
  opacity: 0.9;
}

.file-uploader__success {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #10b981;
  font-size: 12px;
  font-weight: 700;
}

.file-uploader__success-icon {
  font-size: 16px;
}

.file-uploader__error {
  color: var(--color-danger, #ef4444);
  font-size: 11px;
  font-weight: 600;
  margin-top: 4px;
}
</style>
