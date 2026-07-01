<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { usePostStore } from '@/stores/usePostStore';
import { useVideoStore } from '@/stores/useVideoStore';
import { useToastStore } from '@/stores/useToastStore';
import type { VideoProject, VideoScene } from '@/types/video';

const postStore = usePostStore();
const videoStore = useVideoStore();
const toastStore = useToastStore();

// --- State ---
const selectedPostId = ref('');
const isPolling = ref(false);
let pollInterval: number | null = null;

// Lấy danh sách bài viết từ store
const postsList = computed(() => {
  return postStore.posts.map(p => ({
    id: p.id,
    title: p.title,
    summary: p.summary || 'Không có mô tả'
  }));
});

// Post được chọn hiện tại
const selectedPost = computed(() => {
  return postStore.posts.find(p => p.id === selectedPostId.value);
});

// Theo dõi thay đổi của dự án hiện tại
const project = computed<VideoProject | null>(() => videoStore.currentProject);

// Trạng thái các scenes đã được sinh xong hay chưa
const isAllScenesGenerated = computed(() => {
  if (!project.value || !project.value.scenes.length) return false;
  return project.value.scenes.every(s => s.status === 'Completed');
});

// Trạng thái có phân cảnh nào bị lỗi hay không
const isAnySceneFailed = computed(() => {
  if (!project.value || !project.value.scenes.length) return false;
  return project.value.scenes.some(s => s.status === 'Failed');
});

// --- Khởi tạo ---
onMounted(async () => {
  await postStore.fetchAllHistory();
  // Nếu có bài đăng đang soạn thảo, chọn luôn làm mặc định
  if (postStore.currentPost) {
    selectedPostId.value = postStore.currentPost.id;
    await checkOrCreateProjectForPost(postStore.currentPost.id);
  } else if (postsList.value.length > 0) {
    selectedPostId.value = postsList.value[0].id;
    await checkOrCreateProjectForPost(postsList.value[0].id);
  }
});

onUnmounted(() => {
  stopPolling();
});

// Watch thay đổi bài viết được chọn
watch(selectedPostId, async (newPostId) => {
  if (newPostId) {
    stopPolling();
    await checkOrCreateProjectForPost(newPostId);
  } else {
    videoStore.currentProject = null;
  }
});

// Kiểm tra xem bài viết đã có dự án video nào chưa, nếu chưa thì tạo
async function checkOrCreateProjectForPost(postId: string) {
  try {
    // Để kiểm tra, ta lấy danh sách hoặc thử fetch một dự án đã lưu.
    // Vì backend chưa cung cấp endpoint GET /videoprojects/by-post, ta có thể thử tạo trực tiếp.
    // Backend service sẽ tự động tìm kiếm variant hoặc tự động tạo variant cho postId
    await videoStore.createProject(
      '00000000-0000-0000-0000-000000000000', // Guid.Empty
      postId
    );
    toastStore.success('Dự án Video', 'Đã tải/khởi tạo dự án video thành công cho bài viết này.');
    
    // Nếu dự án đang trong tiến trình xử lý, kích hoạt polling
    if (project.value && ['GeneratingScenes', 'Rendering'].includes(project.value.status)) {
      startPolling();
    }
  } catch (err: any) {
    console.error('Lỗi khi tải hoặc tạo dự án video:', err);
    toastStore.error('Lỗi', 'Không thể khởi tạo dự án video AI. Chi tiết: ' + (err.response?.data?.message || err.message));
  }
}

// --- Storyboard Actions ---
async function saveStoryboard() {
  if (!project.value) return;
  try {
    const sceneUpdates = project.value.scenes.map(s => ({
      id: s.id,
      narration: s.narration,
      onScreenText: s.onScreenText,
      visualPrompt: s.visualPrompt,
      negativePrompt: s.negativePrompt,
      cameraDirection: s.cameraDirection,
      durationSeconds: s.durationSeconds
    }));

    await videoStore.updateStoryboard(project.value.id, sceneUpdates);
    toastStore.success('Thành công', 'Đã lưu kịch bản phân cảnh (storyboard) thành công.');
  } catch (err: any) {
    toastStore.error('Lỗi', 'Không thể lưu kịch bản phân cảnh: ' + (err.response?.data?.message || err.message));
  }
}

async function triggerGenerateScenes() {
  if (!project.value) return;
  try {
    // Tự động lưu storyboard trước khi chạy generate
    await saveStoryboard();

    await videoStore.generateVideo(project.value.id);
    toastStore.success('Đã kích hoạt', 'Bắt đầu gửi yêu cầu sinh video phân cảnh AI ngầm.');
    startPolling();
  } catch (err: any) {
    toastStore.error('Lỗi', 'Không thể kích hoạt sinh video: ' + (err.response?.data?.message || err.message));
  }
}

async function triggerRenderVideo() {
  if (!project.value) return;
  try {
    await videoStore.renderVideo(project.value.id);
    toastStore.success('Đã kích hoạt', 'Bắt đầu ghép nối và render video thành phẩm.');
    startPolling();
  } catch (err: any) {
    toastStore.error('Lỗi', 'Không thể bắt đầu ghép nối: ' + (err.response?.data?.message || err.message));
  }
}

// --- Polling Logic ---
function startPolling() {
  if (isPolling.value) return;
  isPolling.value = true;
  pollInterval = window.setInterval(async () => {
    if (!project.value) {
      stopPolling();
      return;
    }
    try {
      await videoStore.fetchProject(project.value.id);
      
      // Dừng polling khi hoàn thành hoặc thất bại
      if (project.value && ['Completed', 'Failed', 'StoryboardGenerated'].includes(project.value.status)) {
        // Nếu chuyển từ trạng thái sinh scenes sang StoryboardGenerated
        if (project.value.status === 'StoryboardGenerated') {
          toastStore.success('Hoàn tất phân cảnh', 'Tất cả các phân cảnh video đã được sinh thành công!');
        } else if (project.value.status === 'Completed') {
          toastStore.success('Hoàn thành video', 'Video thành phẩm 9:16 đã được render thành công!');
        } else if (project.value.status === 'Failed') {
          toastStore.error('Lỗi', 'Tiến trình xử lý video thất bại. Vui lòng kiểm tra lại kịch bản.');
        }
        stopPolling();
      }
    } catch (e) {
      console.error('Lỗi polling trạng thái video:', e);
    }
  }, 3000);
}

function stopPolling() {
  isPolling.value = false;
  if (pollInterval) {
    clearInterval(pollInterval);
    pollInterval = null;
  }
}

// Làm mới thủ công
async function forceRefresh() {
  if (!project.value) return;
  try {
    await videoStore.fetchProject(project.value.id);
    toastStore.success('Đã cập nhật', 'Trạng thái dự án video đã được làm mới.');
    if (['GeneratingScenes', 'Rendering'].includes(project.value.status)) {
      startPolling();
    }
  } catch (e: any) {
    toastStore.error('Lỗi', 'Không thể làm mới trạng thái: ' + e.message);
  }
}

// Giả lập tiến trình ở local (cho mục đích test nhanh UI)
async function simulateDevelopmentProgress() {
  if (!project.value) return;
  toastStore.info('Thông báo', 'Đang yêu cầu hệ thống xử lý nhanh các phân cảnh...');
  try {
    await videoStore.fetchProject(project.value.id);
  } catch (e) {}
}

async function attachVideoToPost() {
  if (!project.value || !project.value.finalAsset) return;
  try {
    // Đính kèm url video vào thumbnail/video của post
    await postStore.applyAiContent(project.value.postId, selectedPost.value?.content || '', project.value.finalAsset.url);
    toastStore.success('Liên kết thành công', 'Video thành phẩm đã được liên kết trực tiếp vào bài đăng.');
  } catch (err: any) {
    toastStore.error('Lỗi', 'Không thể liên kết video: ' + (err.response?.data?.message || err.message));
  }
}

// Trả về url hiển thị cho tệp tin video hoặc asset
function getAssetUrl(url: string) {
  if (!url) return '';
  if (url.startsWith('/')) {
    return `http://localhost:5000${url}`;
  }
  return url;
}
</script>

<template>
  <div class="video-studio-panel">
    <div class="header-container glass-card">
      <div class="left-desc">
        <h3>🎬 Video Studio AI — Trình tạo Clip 9:16</h3>
        <p class="subtitle">Sinh kịch bản phân cảnh tự động bằng Google Veo và biên soạn video ngắn (TikTok, Reels, Shorts) đa phân cảnh.</p>
      </div>
      <div class="right-selector">
        <label>Liên kết bài viết:</label>
        <select v-model="selectedPostId" class="post-select">
          <option v-for="p in postsList" :key="p.id" :value="p.id">
            {{ p.title }}
          </option>
        </select>
      </div>
    </div>

    <!-- Main Workspace -->
    <div v-if="project" class="studio-grid animate-fade">
      <!-- Left Column: Storyboard Editor -->
      <div class="storyboard-panel glass-card">
        <div class="panel-header-actions">
          <h4>📝 Kịch bản phân cảnh (Storyboard)</h4>
          <div class="buttons">
            <button 
              type="button" 
              class="btn-secondary" 
              @click="saveStoryboard"
              :disabled="videoStore.actionLoading"
            >
              💾 Lưu kịch bản
            </button>
            <button 
              type="button" 
              class="btn-refresh" 
              @click="forceRefresh"
              title="Làm mới trạng thái"
            >
              🔄
            </button>
          </div>
        </div>

        <div class="scenes-list">
          <div 
            v-for="(scene, idx) in project.scenes" 
            :key="scene.id" 
            class="scene-card"
            :class="(scene.status ?? '').toString().toLowerCase()"
          >
            <div class="scene-header">
              <span class="scene-number">PHÂN CẢNH #{{ scene.sequence }}</span>
              <span class="scene-status-badge" :class="(scene.status ?? '').toString().toLowerCase()">
                {{ 
                  scene.status === 'Pending' ? '⏳ Chờ xử lý' :
                  scene.status === 'Generating' ? '⚡ AI đang vẽ...' :
                  scene.status === 'Completed' ? '✅ Đã hoàn thành' : '❌ Lỗi' 
                }}
              </span>
            </div>

            <div class="scene-body">
              <div class="input-row">
                <div class="input-group flex-2">
                  <label>Mô tả hình ảnh (Visual Prompt cho Veo)</label>
                  <textarea 
                    v-model="scene.visualPrompt" 
                    rows="2" 
                    placeholder="Mô tả chi tiết những gì sẽ xuất hiện trên màn hình..."
                  ></textarea>
                </div>
                <div class="input-group flex-1">
                  <label>Góc quay (Camera)</label>
                  <input type="text" v-model="scene.cameraDirection" placeholder="Zoom in slow, Pan left...">
                </div>
              </div>

              <div class="input-row margin-top">
                <div class="input-group flex-2">
                  <label>Thuyết minh (Voice-over Narration)</label>
                  <input type="text" v-model="scene.narration" placeholder="Lời bình của MC ảo phát ra...">
                </div>
                <div class="input-group flex-1">
                  <label>Phụ đề (Subtitle)</label>
                  <input type="text" v-model="scene.onScreenText" placeholder="Chữ chạy trên màn hình...">
                </div>
                <div class="input-group width-90">
                  <label>Giây</label>
                  <input type="number" v-model.number="scene.durationSeconds" class="number-input" min="3" max="15">
                </div>
              </div>

              <!-- Preview single clip generated by Veo -->
              <div v-if="scene.generatedAsset" class="scene-preview-container">
                <span class="preview-tag">Xem thử phân cảnh</span>
                <video 
                  :src="getAssetUrl(scene.generatedAsset.url)" 
                  controls 
                  class="scene-clip-player"
                  preload="metadata"
                ></video>
              </div>
            </div>
          </div>
        </div>

        <!-- Controls Footer -->
        <div class="action-footer">
          <div class="status-summary">
            Trạng thái dự án: 
            <span class="status-badge" :class="(project.status ?? '').toString().toLowerCase()">
              {{ 
                project.status === 'Draft' ? 'Bản nháp' :
                project.status === 'GeneratingScenes' ? 'Đang sinh video phân cảnh...' :
                project.status === 'StoryboardGenerated' ? 'Đã sinh xong phân cảnh' :
                project.status === 'Rendering' ? 'Đang render video thành phẩm...' :
                project.status === 'Completed' ? 'Render hoàn tất' : 'Xảy ra lỗi'
              }}
            </span>
          </div>

          <div class="footer-buttons">
            <button 
              type="button" 
              class="btn-primary glow-yellow" 
              @click="triggerGenerateScenes"
              :disabled="videoStore.actionLoading || project.status === 'GeneratingScenes' || project.status === 'Rendering'"
            >
              ⚡ Kích hoạt sinh phân cảnh AI
            </button>

            <button 
              type="button" 
              class="btn-success glow-green" 
              @click="triggerRenderVideo"
              :disabled="videoStore.actionLoading || !isAllScenesGenerated || project.status === 'Rendering'"
            >
              🎬 Ghép nối & Render Video
            </button>
          </div>
        </div>
      </div>

      <!-- Right Column: Final Video Renderer Mockup -->
      <div class="renderer-panel glass-card">
        <h4>📺 Màn hình Render & Trình phát Video 9:16</h4>
        <p class="section-desc">Video thành phẩm sẽ có định dạng dọc 9:16 với kịch bản kịch bản phân cảnh phụ đề và CTA đính kèm.</p>

        <!-- Video Player State -->
        <div v-if="project.finalAsset" class="player-container animate-fade">
          <div class="phone-frame-container">
            <div class="phone-notch"></div>
            <div class="phone-screen">
              <video 
                :src="getAssetUrl(project.finalAsset.url)" 
                controls 
                autoplay
                loop
                class="final-video-player"
              ></video>
            </div>
          </div>

          <div class="video-meta-details">
            <p class="video-title"><strong>{{ project.title }}</strong></p>
            <p class="file-spec">Định dạng: MP4 (H.264) | Tỷ lệ dọc 9:16 | Kích thước: {{ ((project.finalAsset.sizeBytes || 15920000) / 1024 / 1024).toFixed(2) }} MB</p>
            
            <div class="btn-group-center">
              <a :href="getAssetUrl(project.finalAsset.url)" download target="_blank" class="btn-download">
                📥 Tải xuống Video
              </a>
              <button type="button" class="btn-attach" @click="attachVideoToPost">
                🔗 Đính kèm vào bài đăng
              </button>
            </div>
          </div>
        </div>

        <!-- Rendering Loading state -->
        <div v-else-if="project.status === 'Rendering' || project.status === 'GeneratingScenes'" class="render-loading-state">
          <div class="loading-spinner-glow"></div>
          <div class="progress-info">
            <p class="loading-text">{{ project.status === 'Rendering' ? 'Đang ghép nối và đè chữ lên video...' : 'Google Veo đang vẽ các phân cảnh...' }}</p>
            <div class="custom-progress-bar">
              <div class="fill" :style="{ width: project.status === 'Rendering' ? '70%' : '35%' }"></div>
            </div>
            <p class="sub-text">Tiến trình nền chạy ngầm (Hangfire Background Worker). Bạn có thể đóng tab này hoặc làm việc khác trong khi xử lý.</p>
            <button type="button" class="btn-check-status" @click="simulateDevelopmentProgress">
              Kiểm tra nhanh tiến độ 🔄
            </button>
          </div>
        </div>

        <!-- Idle Waiting state -->
        <div v-else class="idle-renderer-state">
          <div class="render-placeholder-icon">🎥</div>
          <p v-if="isAnySceneFailed" class="error-msg">⚠️ Có phân cảnh bị lỗi khi AI vẽ. Vui lòng bấm lưu kịch bản và thử lại.</p>
          <p v-else-if="!isAllScenesGenerated">Vui lòng bấm <strong>"Kích hoạt sinh phân cảnh AI"</strong> để gửi kịch bản mô tả cho Google Veo vẽ video ngắn.</p>
          <p v-else>Tất cả phân cảnh đã vẽ xong! Hãy bấm <strong>"Ghép nối & Render Video"</strong> để hoàn thiện video ngắn dọc 9:16.</p>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-studio glass-card">
      <div class="empty-icon">🎬</div>
      <p>Vui lòng tạo hoặc chọn một bài viết có nội dung tiếp thị để bắt đầu Video Studio AI.</p>
    </div>
  </div>
</template>

<style scoped>
.video-studio-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 24px;
}

.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: var(--color-surface-glass);
}

.left-desc h3 {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.left-desc .subtitle {
  margin: 0;
  font-size: 12.5px;
  color: var(--color-text-secondary);
}

.right-selector {
  display: flex;
  align-items: center;
  gap: 10px;
}

.right-selector label {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.post-select {
  width: 260px;
  height: 36px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 10px;
  font-size: 13px;
  color: var(--color-text-primary);
}

/* Studio Grid Layout */
.studio-grid {
  display: grid;
  grid-template-columns: 1.3fr 0.7fr;
  gap: 20px;
  align-items: start;
}

.storyboard-panel,
.renderer-panel {
  padding: 20px;
  min-height: 520px;
  display: flex;
  flex-direction: column;
}

.panel-header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.panel-header-actions h4,
.renderer-panel h4 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.section-desc {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  margin: 4px 0 20px 0;
}

.buttons {
  display: flex;
  gap: 8px;
}

.btn-secondary {
  background: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  font-size: 11.5px;
  font-weight: 600;
  height: 30px;
  padding: 0 12px;
  border-radius: 6px;
  cursor: pointer;
}

.btn-secondary:hover {
  background: var(--color-divider);
}

.btn-refresh {
  background: transparent;
  border: 1px solid var(--color-border);
  cursor: pointer;
  border-radius: 6px;
  padding: 0 8px;
}

/* Scene Cards */
.scenes-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
  overflow-y: auto;
  max-height: 580px;
  padding-right: 4px;
}

.scene-card {
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 16px;
  transition: all var(--duration-fast);
}

.scene-card:hover {
  background: rgba(255, 255, 255, 0.04);
  border-color: var(--color-border-strong);
}

.scene-card.completed {
  border-left: 4px solid var(--color-green);
}

.scene-card.generating {
  border-left: 4px solid var(--color-yellow);
  background: rgba(245, 158, 11, 0.02);
}

.scene-card.failed {
  border-left: 4px solid var(--color-red);
  background: rgba(239, 68, 68, 0.02);
}

.scene-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.scene-number {
  font-size: 12px;
  font-weight: 800;
  color: var(--color-text-muted);
}

.scene-status-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
}

.scene-status-badge.pending {
  background: rgba(255, 255, 255, 0.06);
  color: var(--color-text-secondary);
}

.scene-status-badge.generating {
  background: rgba(245, 158, 11, 0.15);
  color: var(--color-yellow);
}

.scene-status-badge.completed {
  background: rgba(16, 185, 129, 0.15);
  color: var(--color-green);
}

.scene-status-badge.failed {
  background: rgba(239, 68, 68, 0.15);
  color: var(--color-red);
}

.input-row {
  display: flex;
  gap: 12px;
}

.input-row.margin-top {
  margin-top: 12px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.input-group.flex-2 { flex: 2; }
.input-group.flex-1 { flex: 1; }
.input-group.width-90 { width: 60px; }

.input-group label {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.input-group textarea,
.input-group input {
  background-color: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 12.5px;
  color: var(--color-text-primary);
  width: 100%;
}

.input-group textarea:focus,
.input-group input:focus {
  border-color: var(--color-yellow);
  outline: none;
}

.number-input {
  text-align: center;
}

/* Scene single clip player */
.scene-preview-container {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed var(--color-border);
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.preview-tag {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.scene-clip-player {
  width: 100%;
  max-width: 320px;
  aspect-ratio: 9/16;
  border-radius: 8px;
  background: #000;
  max-height: 150px;
}

/* Footer Control block */
.action-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid var(--color-border);
}

.status-summary {
  font-size: 12.5px;
  color: var(--color-text-secondary);
}

.status-badge {
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
  margin-left: 6px;
}

.status-badge.draft { background: rgba(255,255,255,0.06); color: var(--color-text-secondary); }
.status-badge.generatingscenes { background: rgba(245,158,11,0.15); color: var(--color-yellow); }
.status-badge.storyboardgenerated { background: rgba(59,130,246,0.15); color: #3b82f6; }
.status-badge.rendering { background: rgba(147,51,234,0.15); color: #c084fc; }
.status-badge.completed { background: rgba(16,185,129,0.15); color: var(--color-green); }
.status-badge.failed { background: rgba(239,68,68,0.15); color: var(--color-red); }

.footer-buttons {
  display: flex;
  gap: 12px;
}

.btn-primary {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12.5px;
  font-weight: 700;
  height: 38px;
  padding: 0 16px;
  border-radius: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.btn-primary:hover:not(:disabled) {
  opacity: 0.9;
}

.btn-primary:disabled,
.btn-success:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-success {
  background: var(--color-green);
  color: white;
  border: none;
  font-size: 12.5px;
  font-weight: 700;
  height: 38px;
  padding: 0 16px;
  border-radius: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.btn-success:hover:not(:disabled) {
  opacity: 0.9;
}

.glow-yellow {
  box-shadow: 0 0 12px rgba(245, 158, 11, 0.2);
}

.glow-green {
  box-shadow: 0 0 12px rgba(16, 185, 129, 0.2);
}

/* Renderer Panel styles */
.phone-frame-container {
  width: 250px;
  height: 480px;
  background: #000;
  border: 12px solid #1a1e2e;
  border-radius: 36px;
  box-shadow: 0 15px 35px rgba(0,0,0,0.8), 0 0 20px rgba(59, 130, 246, 0.2);
  margin: 10px auto 20px auto;
  position: relative;
  overflow: hidden;
}

.phone-notch {
  width: 100px;
  height: 18px;
  background: #1a1e2e;
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
  position: absolute;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2;
}

.phone-screen {
  width: 100%;
  height: 100%;
  position: relative;
  background: #080b11;
}

.final-video-player {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.video-meta-details {
  text-align: center;
}

.video-title {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.file-spec {
  font-size: 11px;
  color: var(--color-text-muted);
  margin: 0 0 16px 0;
}

.btn-group-center {
  display: flex;
  gap: 10px;
  justify-content: center;
}

.btn-download {
  background: #3b82f6;
  color: white;
  text-decoration: none;
  font-size: 12px;
  font-weight: 600;
  height: 32px;
  padding: 0 16px;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.3);
}

.btn-download:hover {
  opacity: 0.9;
}

.btn-attach {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12px;
  font-weight: 600;
  height: 32px;
  padding: 0 16px;
  border-radius: 6px;
  cursor: pointer;
  box-shadow: var(--color-yellow-glow);
}

.btn-attach:hover {
  opacity: 0.9;
}

/* Loading state render */
.render-loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  text-align: center;
  padding: 20px;
}

.loading-spinner-glow {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(147, 51, 234, 0.1);
  border-top-color: #c084fc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
  box-shadow: 0 0 20px rgba(192, 132, 252, 0.4);
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.progress-info {
  width: 100%;
}

.loading-text {
  font-size: 13.5px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 12px 0;
}

.custom-progress-bar {
  width: 80%;
  height: 8px;
  background: rgba(255,255,255,0.06);
  border-radius: 99px;
  margin: 0 auto 16px auto;
  overflow: hidden;
}

.custom-progress-bar .fill {
  height: 100%;
  background: linear-gradient(90deg, #c084fc, #9333ea);
  border-radius: 99px;
  animation: progress-shimmer 2s ease infinite;
}

@keyframes progress-shimmer {
  0% { opacity: 0.7; }
  50% { opacity: 1; }
  100% { opacity: 0.7; }
}

.sub-text {
  font-size: 11px;
  color: var(--color-text-muted);
  line-height: 1.5;
  margin: 0 0 16px 0;
}

.btn-check-status {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  font-size: 11.5px;
  height: 28px;
  padding: 0 12px;
  border-radius: 6px;
  cursor: pointer;
}

/* Idle state render */
.idle-renderer-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  text-align: center;
  color: var(--color-text-muted);
  padding: 30px;
}

.render-placeholder-icon {
  font-size: 50px;
  opacity: 0.3;
  margin-bottom: 16px;
}

.error-msg {
  color: var(--color-red);
  font-size: 12.5px;
  font-weight: 600;
}

/* Empty Studio state */
.empty-studio {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px;
  text-align: center;
  color: var(--color-text-muted);
  min-height: 300px;
}

.empty-icon {
  font-size: 48px;
  opacity: 0.4;
  margin-bottom: 16px;
}
</style>
