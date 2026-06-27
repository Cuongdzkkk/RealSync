<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useProjectStore } from '@/stores/useProjectStore';
import { useToastStore } from '@/stores/useToastStore';
import { usePostStore } from '@/stores/usePostStore';
import RoleGate from '@/components/common/RoleGate.vue';

const propertyStore = usePropertyStore();
const projectStore = useProjectStore();
const toastStore = useToastStore();
const postStore = usePostStore();

onMounted(() => {
  postStore.fetchAllHistory();
});

// --- Input States ---
const selectedType = ref<'property' | 'project'>('property');
const selectedPropertyId = ref(propertyStore.items[0]?.id || '');
const selectedProjectId = ref(projectStore.items[0]?.id || '');
const selectedChannel = ref<'facebook' | 'listing' | 'zalo' | 'seo'>('facebook');
const selectedTone = ref<string>('persuasive');
const includeEmojis = ref(true);
const includeContact = ref(true);

// --- Generator States ---
const isGenerating = ref(false);
const generatedText = ref('');
const displayedText = ref(''); // For typing animation
const showPreview = ref(false);

const selectedTargetItem = computed(() => {
  if (selectedType.value === 'property') {
    return propertyStore.items.find(p => p.id === selectedPropertyId.value);
  } else {
    return projectStore.items.find(p => p.id === selectedProjectId.value);
  }
});

// Watch to sync initial defaults
watch(selectedType, (newType) => {
  if (newType === 'property') {
    selectedPropertyId.value = propertyStore.items[0]?.id || '';
  } else {
    selectedProjectId.value = projectStore.items[0]?.id || '';
  }
});

// Gọi API backend để tạo Post + sinh nội dung AI
async function triggerGeneration() {
  if (!selectedTargetItem.value) {
    toastStore.warning('Thiếu đối tượng', 'Vui lòng chọn sản phẩm BĐS hoặc dự án để tạo nội dung.');
    return;
  }

  isGenerating.value = true;
  showPreview.value = false;
  displayedText.value = '';

  const item = selectedTargetItem.value as any;
  const itemName = item.name || item.title || 'BĐS';
  const itemId = item.id;
  const channel = selectedChannel.value;

  // Xây dựng prompt dựa trên channel + tone
  const prompt = `Viết nội dung ${channel === 'facebook' ? 'Facebook Post' : channel === 'zalo' ? 'Zalo Broadcast' : channel === 'seo' ? 'bài viết SEO' : 'tin đăng BĐS'} cho "${itemName}", giọng văn ${selectedTone.value}, ${includeEmojis.value ? 'có emoji' : 'không emoji'}${includeContact.value ? ', kèm thông tin liên hệ' : ''}`;

  try {
    // Gọi API: tạo Post → sinh AI content
    const generation = await postStore.generateContent(itemName, prompt, itemId, channel);

    // Hiển thị nội dung AI trả về với hiệu ứng typing
    generatedText.value = generation.generatedContent;

    setTimeout(() => {
      isGenerating.value = false;
      showPreview.value = true;

      let index = 0;
      const interval = setInterval(() => {
        if (index < generation.generatedContent.length) {
          displayedText.value += generation.generatedContent.slice(index, index + 3);
          index += 3;
        } else {
          clearInterval(interval);
        }
      }, 8);
    }, 600);
  } catch (error: any) {
    isGenerating.value = false;
    toastStore.error('Lỗi', error?.response?.data?.message || 'Không thể tạo nội dung AI. Vui lòng thử lại.');
  }
}

function copyContent() {
  navigator.clipboard.writeText(displayedText.value);
  toastStore.success('Đã sao chép', 'Nội dung đã được sao chép vào bộ nhớ tạm.');
}

// History: lấy từ API posts thay vì mock
const historyList = computed(() =>
  postStore.posts.map(p => ({
    id: p.id,
    title: p.title,
    channel: extractChannel(p.summary),
    status: mapPostStatus(p.status),
    owner: p.authorName,
    updatedAt: p.updatedAt ?? p.createdAt,
  }))
);

function extractChannel(summary?: string): 'seo' | 'facebook' | 'zalo' | 'listing' {
  if (!summary) return 'listing';
  if (summary.includes('facebook')) return 'facebook';
  if (summary.includes('zalo')) return 'zalo';
  if (summary.includes('seo')) return 'seo';
  return 'listing';
}

function mapPostStatus(status: string): 'draft' | 'review' | 'approved' | 'published' {
  switch (status) {
    case 'Published': return 'published';
    case 'Archived': return 'approved';
    case 'Failed': return 'review';
    default: return 'draft';
  }
}
</script>

<template>
  <RoleGate :roles="['Admin', 'Sales', 'Marketing']">
    <div class="page">
      <div class="workspace-grid">
      <!-- Left Controls Panel -->
      <div class="panel-controls glass-card">
        <div class="panel-header">
          <h3>Trình biên soạn Nội dung AI</h3>
          <p class="subtitle">Liên kết dữ liệu BĐS trong hệ thống để tự động soạn bài đăng mạng xã hội, tin quảng cáo đa kênh.</p>
        </div>

        <form class="generator-form" @submit.prevent="triggerGeneration">
          <!-- Target database linkage selector -->
          <div class="form-group">
            <label>Chọn nguồn dữ liệu liên kết</label>
            <div class="toggle-group">
              <button 
                type="button" 
                class="toggle-btn"
                :class="{ active: selectedType === 'property' }"
                @click="selectedType = 'property'"
              >
                Bất động sản
              </button>
              <button 
                type="button" 
                class="toggle-btn"
                :class="{ active: selectedType === 'project' }"
                @click="selectedType = 'project'"
              >
                Dự án (Project)
              </button>
            </div>
          </div>

          <!-- Target select list -->
          <div v-if="selectedType === 'property'" class="form-group">
            <label>Chọn tin đăng BĐS</label>
            <select v-model="selectedPropertyId">
              <option 
                v-for="p in propertyStore.items" 
                :key="p.id" 
                :value="p.id"
              >
                {{ p.title }} ({{ (p.price / 1000000000).toFixed(1) }} tỷ)
              </option>
            </select>
          </div>

          <div v-else class="form-group">
            <label>Chọn dự án bất động sản</label>
            <select v-model="selectedProjectId">
              <option 
                v-for="prj in projectStore.items" 
                :key="prj.id" 
                :value="prj.id"
              >
                {{ prj.name }} - {{ prj.developer }}
              </option>
            </select>
          </div>

          <!-- Channel selection tabs -->
          <div class="form-group">
            <label>Kênh đăng bài (Channel)</label>
            <div class="channels-grid">
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'facebook' }"
                @click="selectedChannel = 'facebook'"
              >
                <span class="icon">👥</span>
                <span>Facebook Post</span>
              </div>
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'listing' }"
                @click="selectedChannel = 'listing'"
              >
                <span class="icon">📰</span>
                <span>Tin đăng BĐS</span>
              </div>
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'zalo' }"
                @click="selectedChannel = 'zalo'"
              >
                <span class="icon">💬</span>
                <span>Zalo Broadcast</span>
              </div>
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'seo' }"
                @click="selectedChannel = 'seo'"
              >
                <span class="icon">🔍</span>
                <span>Bài viết SEO</span>
              </div>
            </div>
          </div>

          <!-- Tone and options -->
          <div class="form-row">
            <div class="form-group">
              <label>Giọng văn (Tone)</label>
              <select v-model="selectedTone">
                <option value="persuasive">Thuyết phục (Persuasive)</option>
                <option value="professional">Chuyên nghiệp (Professional)</option>
                <option value="excited">Hào hứng (Excited)</option>
                <option value="friendly">Thân thiện (Friendly)</option>
              </select>
            </div>
          </div>

          <div class="options-row">
            <label class="checkbox-wrapper">
              <input type="checkbox" v-model="includeEmojis" />
              <span>Chèn Emojis tự động</span>
            </label>
            <label class="checkbox-wrapper">
              <input type="checkbox" v-model="includeContact" />
              <span>Kèm thông tin liên hệ</span>
            </label>
          </div>

          <button 
            type="submit" 
            class="generate-btn glow-yellow" 
            :disabled="isGenerating"
          >
            <span v-if="isGenerating" class="spinner-inline"></span>
            {{ isGenerating ? 'AI đang tạo nội dung...' : '⚡ Bắt đầu tạo bài bằng AI' }}
          </button>
        </form>
      </div>

      <!-- Right Mockup Preview Panel -->
      <div class="panel-preview glass-card">
        <div class="panel-header">
          <h3>Xem trước bài đăng (Mockup Preview)</h3>
        </div>

        <!-- Waiting State -->
        <div v-if="!showPreview && !isGenerating" class="waiting-state">
          <div class="preview-placeholder-icon">📝</div>
          <p>Thiết lập thông tin bên trái và nhấn tạo bài viết để xem trước giao diện bài đăng trực quan.</p>
        </div>

        <!-- Generating Loader State -->
        <div v-if="isGenerating" class="generating-state">
          <div class="shimmer-card">
            <div class="shimmer-header">
              <div class="circle shimmer"></div>
              <div class="lines">
                <div class="line shimmer"></div>
                <div class="line short shimmer"></div>
              </div>
            </div>
            <div class="shimmer-body">
              <div class="line long shimmer"></div>
              <div class="line long shimmer"></div>
              <div class="line short shimmer"></div>
            </div>
          </div>
          <p class="loader-lbl">AI đang soạn thảo nội dung tiếp thị...</p>
        </div>

        <!-- Output Mockups -->
        <div v-if="showPreview && !isGenerating" class="preview-workspace animate-fade">
          <div class="mockup-header-actions">
            <span class="mockup-tag">{{ selectedChannel.toUpperCase() }} MOCKUP</span>
            <button class="copy-btn glow-yellow" @click="copyContent">Sao chép nội dung</button>
          </div>

          <!-- Facebook Mockup -->
          <div v-if="selectedChannel === 'facebook'" class="facebook-mockup glass-card">
            <div class="fb-header">
              <div class="fb-avatar">RS</div>
              <div class="fb-meta">
                <strong>RealSync CRM Admin</strong>
                <span>Vừa xong · 🌐</span>
              </div>
            </div>
            <div class="fb-text">
              <textarea v-model="displayedText" rows="10" class="editor-textarea"></textarea>
            </div>
            <div class="fb-image-placeholder" v-if="selectedTargetItem?.imageUrl">
              <img :src="selectedTargetItem.imageUrl" />
            </div>
            <div class="fb-actions">
              <span>👍 Thích</span>
              <span>💬 Bình luận</span>
              <span>🗣️ Chia sẻ</span>
            </div>
          </div>

          <!-- Zalo Mockup -->
          <div v-else-if="selectedChannel === 'zalo'" class="zalo-mockup">
            <div class="zalo-phone-frame">
              <div class="zalo-chat-header">RealSync Broadcast</div>
              <div class="zalo-bubble">
                <textarea v-model="displayedText" rows="10" class="editor-textarea"></textarea>
                <div class="zalo-meta">15:52 · Đã gửi</div>
              </div>
            </div>
          </div>

          <!-- Default Text/SEO Mockup -->
          <div v-else class="generic-mockup glass-card">
            <div class="document-header">
              <span class="dot red"></span><span class="dot yellow"></span><span class="dot green"></span>
            </div>
            <div class="document-body">
              <textarea v-model="displayedText" rows="12" class="editor-textarea"></textarea>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- History list table -->
    <div class="history-section glass-card">
      <div class="section-header">
        <h3>Lịch sử nội dung tiếp thị đã xuất bản</h3>
        <p class="subtitle">Danh sách các mô tả tin đăng và bài social được tạo trước đó.</p>
      </div>

      <table class="history-table">
        <thead>
          <tr>
            <th>Tiêu đề nội dung</th>
            <th>Kênh đăng tải</th>
            <th>Trạng thái duyệt</th>
            <th>Người tạo</th>
            <th>Thời gian</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in historyList" :key="item.id">
            <td class="item-title">{{ item.title }}</td>
            <td>
              <span class="channel-tag" :class="item.channel">
                {{ item.channel === 'facebook' ? 'Facebook' : item.channel === 'zalo' ? 'Zalo' : item.channel === 'seo' ? 'SEO' : 'Tin đăng' }}
              </span>
            </td>
            <td>
              <span class="status-lbl" :class="item.status">
                {{ item.status === 'draft' ? 'Nháp' : item.status === 'review' ? 'Chờ duyệt' : item.status === 'approved' ? 'Đã duyệt' : 'Đã đăng' }}
              </span>
            </td>
            <td>{{ item.owner }}</td>
            <td>{{ new Date(item.updatedAt).toLocaleDateString('vi-VN') }}</td>
            <td>
              <button class="action-btn-copy" @click="toastStore.success('Đã sao chép', 'Đã copy nội dung lịch sử.')">Copy</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  </RoleGate>
</template>

<style scoped>
.workspace-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-bottom: 24px;
}

.panel-controls,
.panel-preview {
  display: flex;
  flex-direction: column;
  padding: 24px;
  min-height: 480px;
}

.panel-header h3 {
  margin: 0 0 4px 0;
  font-size: 15px;
}

.panel-header .subtitle {
  margin: 0 0 20px 0;
  font-size: 12.5px;
  color: var(--color-text-secondary);
}

.generator-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.form-group select {
  height: 38px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

/* Toggle Group */
.toggle-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 4px;
  border-radius: 8px;
}

.toggle-btn {
  height: 30px;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 11.5px;
  font-weight: 600;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.toggle-btn.active {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  box-shadow: var(--color-yellow-glow);
}

/* Channel selection */
.channels-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}

.channel-card {
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.channel-card:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border-strong);
}

.channel-card.active {
  border-color: var(--color-ai);
  background: var(--color-ai-bg);
  color: var(--color-ai);
}

.channel-card .icon {
  font-size: 16px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr;
}

.options-row {
  display: flex;
  gap: 16px;
  margin-top: 4px;
}

.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--color-text-secondary);
  cursor: pointer;
}

.generate-btn {
  height: 40px;
  border: none;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12.5px;
  font-weight: 700;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all var(--duration-fast);
  margin-top: auto;
}

.generate-btn:hover {
  background-color: var(--color-yellow-hover);
}

.generate-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-inline {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
}

/* Waiting State */
.waiting-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  flex: 1;
  text-align: center;
  color: var(--color-text-muted);
}

.preview-placeholder-icon {
  font-size: 40px;
  opacity: 0.5;
}

/* Shimmer Card Loader */
.generating-state {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  flex: 1;
  gap: 16px;
}

.shimmer-card {
  width: 100%;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.shimmer-header {
  display: flex;
  gap: 12px;
}

.shimmer-header .circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
}

.shimmer-header .lines {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
}

.shimmer-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.shimmer {
  background: linear-gradient(90deg, var(--color-divider) 25%, var(--color-surface-hover) 50%, var(--color-divider) 75%);
  background-size: 200% 100%;
  animation: loading-shimmer 1.5s infinite;
  border-radius: 4px;
  height: 10px;
}

.shimmer.line.short { width: 40%; }
.shimmer.line.long { width: 100%; }

@keyframes loading-shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.loader-lbl {
  font-size: 12px;
  color: var(--color-text-secondary);
}

/* Output Preview area */
.preview-workspace {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
}

.mockup-header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.mockup-tag {
  font-size: 9px;
  font-weight: 700;
  background-color: var(--color-divider);
  padding: 3px 8px;
  border-radius: 4px;
  color: var(--color-text-secondary);
}

.copy-btn {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 11px;
  font-weight: 600;
  height: 28px;
  padding: 0 12px;
  border-radius: 6px;
  cursor: pointer;
}

/* Facebook mockup */
.facebook-mockup {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.fb-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px;
  border-bottom: 1px solid var(--color-divider);
}

.fb-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: var(--color-info-bg);
  color: var(--color-info);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 11px;
}

.fb-meta {
  display: flex;
  flex-direction: column;
  font-size: 12px;
}

.fb-meta span {
  font-size: 10px;
  color: var(--color-text-muted);
}

.fb-text {
  padding: 12px;
}

.editor-textarea {
  width: 100%;
  background: transparent;
  border: none;
  font-size: 12.5px;
  color: var(--color-text-primary);
  line-height: 1.5;
  resize: none;
}

.editor-textarea:focus {
  outline: none;
}

.fb-image-placeholder {
  width: 100%;
  aspect-ratio: 16 / 9;
  overflow: hidden;
  background-color: var(--color-divider);
}

.fb-image-placeholder img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.fb-actions {
  display: flex;
  border-top: 1px solid var(--color-divider);
  padding: 10px 12px;
  justify-content: space-around;
  font-size: 11.5px;
  color: var(--color-text-secondary);
}

/* Zalo Mockup */
.zalo-mockup {
  background-color: var(--color-divider);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 16px;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.zalo-phone-frame {
  width: 280px;
  background-color: var(--color-canvas);
  border: 4px solid var(--color-border-strong);
  border-radius: 16px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: var(--elevation-floating);
}

.zalo-chat-header {
  background-color: #0068ff;
  color: #ffffff;
  padding: 8px 12px;
  font-size: 12px;
  font-weight: 600;
  text-align: center;
}

.zalo-bubble {
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 10px;
  border-radius: 8px;
  margin: 12px;
  align-self: flex-start;
  max-width: 85%;
  display: flex;
  flex-direction: column;
}

.zalo-bubble textarea {
  font-size: 11.5px;
  color: var(--color-text-primary);
}

.zalo-meta {
  font-size: 9px;
  color: var(--color-text-muted);
  text-align: right;
  margin-top: 4px;
}

/* Document / SEO Mockup */
.generic-mockup {
  display: flex;
  flex-direction: column;
}

.document-header {
  background: var(--color-surface-hover);
  padding: 6px 10px;
  display: flex;
  gap: 6px;
  border-bottom: 1px solid var(--color-divider);
}

.document-header .dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.document-header .dot.red { background-color: var(--color-danger); }
.document-header .dot.yellow { background-color: var(--color-warning); }
.document-header .dot.green { background-color: var(--color-success); }

.document-body {
  padding: 16px;
}

/* History Section */
.history-section {
  padding: 20px;
}

.history-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12px;
  margin-top: 12px;
}

.history-table th {
  padding: 10px 12px;
  font-size: 10px;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border);
}

.history-table td {
  padding: 12px;
  border-bottom: 1px solid var(--color-divider);
  color: var(--color-text-secondary);
}

.item-title {
  font-weight: 600;
  color: var(--color-text-primary);
}

.channel-tag {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}
.channel-tag.facebook { background-color: rgba(59, 130, 246, 0.15); color: #3b82f6; }
.channel-tag.zalo { background-color: rgba(14, 165, 233, 0.15); color: #0ea5e9; }
.channel-tag.seo { background-color: rgba(168, 85, 247, 0.15); color: #a855f7; }
.channel-tag.listing { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }

.status-lbl {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
}
.status-lbl.draft { background-color: rgba(148, 163, 184, 0.15); color: #94a3b8; }
.status-lbl.review { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.status-lbl.approved { background-color: rgba(59, 130, 246, 0.15); color: #3b82f6; }
.status-lbl.published { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }

.action-btn-copy {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
}

.action-btn-copy:hover {
  background: var(--color-surface-hover);
}
</style>
