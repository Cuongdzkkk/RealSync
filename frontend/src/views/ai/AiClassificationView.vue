<script setup lang="ts">
import { computed, ref } from 'vue';
import { useLeadStore } from '@/stores/useLeadStore';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useToastStore } from '@/stores/useToastStore';
import { mockAiJobs } from '@/utils/mockData';
import { formatCurrency } from '@/utils/format';
import RoleGate from '@/components/common/RoleGate.vue';

const leadStore = useLeadStore();
const propertyStore = usePropertyStore();
const toastStore = useToastStore();

// --- Playground State ---
const inputText = ref('');
const isAnalyzing = ref(false);
const showAnalysisResults = ref(false);

// AI Extraction results model
const extractedEntities = ref({
  intent: 'Mua bán Bất động sản',
  area: 'Quận 7, TP.HCM',
  budget: 'Từ 8 - 14 tỷ VNĐ',
  type: 'Căn hộ chung cư / Nhà phố',
  bedrooms: '3 Phòng ngủ',
  urgency: 'Cao (Hot Lead)',
  score: 92
});

// Templates for quick testing
const templates = [
  {
    label: 'Tìm chung cư 3PN Quận 7',
    text: 'Tôi muốn tìm mua căn hộ 3 phòng ngủ ở Quận 7, ngân sách khoảng 12 tỷ, ưu tiên tầng cao view đẹp, bàn giao nội thất cơ bản, đã có sổ hồng.'
  },
  {
    label: 'Mua nhà phố kinh doanh Bình Thạnh',
    text: 'Cần mua nhà phố kinh doanh mặt tiền hoặc hẻm xe hơi lớn tại Bình Thạnh, ngân sách tối đa 30 tỷ, hướng Đông Nam tốt cho kinh doanh.'
  },
  {
    label: 'Đất nền giá tốt Bình Chánh',
    text: 'Tìm đất nền thổ cư khu vực Bình Chánh dưới 5 tỷ, pháp lý sổ hồng riêng rõ ràng, sang tên công chứng ngay trong tuần.'
  }
];

function applyTemplate(text: string) {
  inputText.value = text;
}

// Matching properties calculation based on simulated NLP intent
const matchedProperties = computed(() => {
  if (!showAnalysisResults.value) return [];
  
  // Return some matching properties from propertyStore
  if (inputText.value.includes('Quận 7')) {
    return propertyStore.items.filter(p => p.address.includes('Quận 7') || p.price > 10000000000);
  } else if (inputText.value.includes('Bình Chánh')) {
    return propertyStore.items.filter(p => p.address.includes('Bình Chánh') || p.price < 6000000000);
  }
  // Default fallback matching
  return propertyStore.items.slice(0, 2);
});

// Classification trigger
function startAnalysis() {
  if (!inputText.value.trim()) {
    toastStore.warning('Thiếu dữ liệu', 'Vui lòng điền nội dung yêu cầu hoặc chọn một mẫu thử nghiệm bên dưới.');
    return;
  }

  isAnalyzing.value = true;
  showAnalysisResults.value = false;

  // Simulate AI parsing delay
  setTimeout(() => {
    isAnalyzing.value = false;
    showAnalysisResults.value = true;
    
    // Dynamic extraction based on keywords
    if (inputText.value.toLowerCase().includes('quận 7')) {
      extractedEntities.value = {
        intent: 'Tìm mua căn hộ',
        area: 'Quận 7, TP.HCM',
        budget: 'Khoảng 10 - 15 tỷ VNĐ',
        type: 'Căn hộ cao cấp 3PN',
        bedrooms: '3 PN',
        urgency: 'Cao (Hot Lead)',
        score: 95
      };
    } else if (inputText.value.toLowerCase().includes('bình thạnh')) {
      extractedEntities.value = {
        intent: 'Đầu tư nhà phố kinh doanh',
        area: 'Quận Bình Thạnh, TP.HCM',
        budget: 'Dưới 30 tỷ VNĐ',
        type: 'Nhà phố thương mại',
        bedrooms: '4+ PN',
        urgency: 'Trung bình (Warm)',
        score: 89
      };
    } else if (inputText.value.toLowerCase().includes('bình chánh')) {
      extractedEntities.value = {
        intent: 'Mua đất nền thổ cư',
        area: 'Bình Chánh, TP.HCM',
        budget: 'Dưới 5 tỷ VNĐ',
        type: 'Đất nền dự án',
        bedrooms: 'N/A',
        urgency: 'Thấp (Cold)',
        score: 74
      };
    } else {
      extractedEntities.value = {
        intent: 'Nhu cầu Bất động sản',
        area: 'TP. Hồ Chí Minh',
        budget: 'Chưa xác định',
        type: 'Căn hộ chung cư',
        bedrooms: '2-3 PN',
        urgency: 'Trung bình',
        score: 82
      };
    }

    toastStore.success('Phân tích hoàn tất', 'AI đã trích xuất thành công nhu cầu và liên kết kho hàng phù hợp.');
  }, 1500);
}

function sendToLead() {
  toastStore.success('Đã gửi đề xuất', 'Đã chuyển tiếp thông tin các căn hộ phù hợp nhất đến Zalo/Viber của khách hàng.');
}

// History Logs state
const jobsList = ref(mockAiJobs);
</script>

<template>
  <RoleGate :roles="['Admin', 'Manager', 'Sales']">
    <div class="page">
      <!-- Header Summary Metrics -->
      <div class="metrics-row">
      <div class="metric-card glass-card">
        <span class="label">Tổng số bản ghi đã phân loại</span>
        <strong class="value numeric">1,482</strong>
        <span class="trend success">+14.2% tuần này</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Độ chính xác mô hình NLP</span>
        <strong class="value numeric">94.8%</strong>
        <span class="trend success">Đã hiệu chuẩn</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Tốc độ xử lý trung bình</span>
        <strong class="value numeric">142ms</strong>
        <span class="trend info">Fast Latency</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Đề xuất được môi giới chấp nhận</span>
        <strong class="value numeric">91.3%</strong>
        <span class="trend success">+2.1% so với tháng trước</span>
      </div>
    </div>

    <!-- Main Playground Workspace -->
    <div class="playground-layout">
      <!-- Left input panel -->
      <div class="panel-input glass-card">
        <div class="panel-header">
          <h3>AI Classification Playground</h3>
          <p class="subtitle">Nhập văn bản yêu cầu của khách hàng hoặc tin đăng cào về để AI bóc tách thực thể tự động.</p>
        </div>

        <div class="playground-form">
          <div class="form-group">
            <label>Nội dung yêu cầu / Tin đăng</label>
            <textarea 
              v-model="inputText" 
              placeholder="Ví dụ: Khách hàng cần tìm căn hộ 3PN view sông tại Thủ Thiêm, ngân sách tầm 12 tỷ, sẵn sàng cọc ngay..."
              rows="6"
            ></textarea>
          </div>

          <!-- Quick Templates -->
          <div class="templates-section">
            <span class="section-label">Mẫu thử nhanh:</span>
            <div class="templates-grid">
              <button 
                v-for="t in templates" 
                :key="t.label" 
                type="button" 
                class="template-tag"
                @click="applyTemplate(t.text)"
              >
                {{ t.label }}
              </button>
            </div>
          </div>

          <button 
            class="analyze-btn glow-yellow" 
            :disabled="isAnalyzing"
            @click="startAnalysis"
          >
            <span v-if="isAnalyzing" class="spinner-inline"></span>
            {{ isAnalyzing ? 'Đang trích xuất thực thể...' : '⚡ Bắt đầu phân tích AI' }}
          </button>
        </div>
      </div>

      <!-- Right Output Panel -->
      <div class="panel-output glass-card">
        <div class="panel-header">
          <h3>Kết quả phân tích NLP & Đối sánh kho</h3>
        </div>

        <!-- Waiting State -->
        <div v-if="!showAnalysisResults && !isAnalyzing" class="waiting-state">
          <div class="ai-glow-ring">
            <div class="ring-pulse"></div>
            <span>AI</span>
          </div>
          <p>Nhập dữ liệu và nhấn nút phân tích để kích hoạt mô hình RealSync NLP Engine.</p>
        </div>

        <!-- Analyzing Loader State -->
        <div v-if="isAnalyzing" class="loader-state">
          <div class="wave-loader">
            <div></div><div></div><div></div>
          </div>
          <p>AI đang phân tích ý định, ngân sách, khu vực và tìm kiếm sản phẩm phù hợp...</p>
        </div>

        <!-- Results Output State -->
        <div v-if="showAnalysisResults && !isAnalyzing" class="results-state animate-fade">
          <!-- Score Badge Header -->
          <div class="score-banner glass-card glow-ai">
            <div class="score-info">
              <span class="lbl">Điểm tiềm năng (AI Score)</span>
              <strong class="val numeric">{{ extractedEntities.score }}%</strong>
            </div>
            <span class="status-pill" :class="extractedEntities.urgency.toLowerCase()">
              {{ extractedEntities.urgency }}
            </span>
          </div>

          <!-- Extracted Entities Grid -->
          <div class="entities-grid">
            <div class="entity-item">
              <span class="lbl">Ý định (Intent)</span>
              <span class="val">{{ extractedEntities.intent }}</span>
            </div>
            <div class="entity-item">
              <span class="lbl">Khu vực tìm kiếm</span>
              <span class="val">{{ extractedEntities.area }}</span>
            </div>
            <div class="entity-item">
              <span class="lbl">Ngân sách dự kiến</span>
              <span class="val">{{ extractedEntities.budget }}</span>
            </div>
            <div class="entity-item">
              <span class="lbl">Loại hình sản phẩm</span>
              <span class="val">{{ extractedEntities.type }}</span>
            </div>
          </div>

          <!-- Matched Inventory Section -->
          <div class="matched-inventory-block">
            <div class="block-header">
              <span>🏠 Bất động sản đề xuất tự động</span>
              <button class="send-all-btn" @click="sendToLead">Gửi tất cả</button>
            </div>

            <div class="matched-grid">
              <div 
                v-for="p in matchedProperties" 
                :key="p.id" 
                class="matched-card glass-card"
              >
                <img :src="p.imageUrl" :alt="p.title" />
                <div class="matched-card-body">
                  <h4 class="matched-title">{{ p.title }}</h4>
                  <span class="matched-price numeric">{{ formatCurrency(p.price) }}</span>
                  <span class="matched-pct">⚡ Tương thích {{ p.aiScore }}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- History Logs Table -->
    <div class="history-table-section glass-card">
      <div class="section-header">
        <h3>Lịch sử phân tích & phân loại gần đây</h3>
        <p class="subtitle">Danh sách cào tin và đăng ký lead được phân loại tự động chạy ngầm.</p>
      </div>

      <div class="table-container">
        <table class="history-table">
          <thead>
            <tr>
              <th>Đối tượng (Target)</th>
              <th>Loại hình</th>
              <th>Kết quả phân loại</th>
              <th>Độ tin cậy (Confidence)</th>
              <th>Thời gian thực hiện</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="job in jobsList" :key="job.id">
              <td class="target-name">{{ job.target }}</td>
              <td>
                <span class="type-badge" :class="job.type">
                  {{ job.type === 'property' ? 'BĐS' : 'Khách hàng' }}
                </span>
              </td>
              <td class="result-text">{{ job.result }}</td>
              <td>
                <div class="confidence-cell">
                  <div class="progress-bar-tiny">
                    <div class="fill" :style="{ width: `${job.confidence}%` }"></div>
                  </div>
                  <span class="numeric">{{ job.confidence }}%</span>
                </div>
              </td>
              <td>{{ new Date(job.createdAt).toLocaleString('vi-VN') }}</td>
              <td>
                <span class="status-indicator" :class="job.status">
                  {{ job.status === 'completed' ? 'Thành công' : job.status === 'review' ? 'Cần duyệt' : 'Đang xử lý' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  </RoleGate>
</template>

<style scoped>
.metrics-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.metric-card {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.metric-card .label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.metric-card .value {
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.metric-card .trend {
  font-size: 11px;
  font-weight: 600;
}

.metric-card .trend.success { color: var(--color-success); }
.metric-card .trend.info { color: var(--color-info); }

/* Playground layout */
.playground-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-bottom: 24px;
}

.panel-input,
.panel-output {
  display: flex;
  flex-direction: column;
  min-height: 440px;
  padding: 24px;
}

.panel-header h3 {
  margin: 0 0 4px 0;
  font-size: 15px;
  font-weight: 700;
}

.panel-header .subtitle {
  margin: 0 0 20px 0;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.playground-form {
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

.form-group textarea {
  background-color: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
  resize: vertical;
  line-height: 1.5;
}

.form-group textarea:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

.templates-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.templates-section .section-label {
  font-size: 11px;
  color: var(--color-text-muted);
}

.templates-grid {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.template-tag {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  font-size: 11.5px;
  color: var(--color-text-secondary);
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.template-tag:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-strong);
  color: var(--color-text-primary);
}

.analyze-btn {
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

.analyze-btn:hover {
  background-color: var(--color-yellow-hover);
}

.analyze-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Spinner */
.spinner-inline {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
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
  padding: 32px;
}

.ai-glow-ring {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.2), rgba(168, 85, 247, 0.2));
  border: 1px solid var(--color-ai-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  color: var(--color-ai);
  font-size: 14px;
  position: relative;
}

.ring-pulse {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  border-radius: 50%;
  border: 2px solid var(--color-ai);
  opacity: 0.4;
  animation: pulse-ring 2s infinite ease-in-out;
}

@keyframes pulse-ring {
  0% { transform: scale(1); opacity: 0.4; }
  100% { transform: scale(1.3); opacity: 0; }
}

/* Loader State */
.loader-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  gap: 16px;
  color: var(--color-text-secondary);
  text-align: center;
  font-size: 12px;
}

.wave-loader {
  display: flex;
  gap: 6px;
}

.wave-loader div {
  width: 8px;
  height: 8px;
  background-color: var(--color-ai);
  border-radius: 50%;
  animation: wave 1.2s infinite ease-in-out;
}

.wave-loader div:nth-child(2) { animation-delay: -1.1s; }
.wave-loader div:nth-child(3) { animation-delay: -1.0s; }

@keyframes wave {
  0%, 40%, 100% { transform: translateY(0); }
  20% { transform: translateY(-10px); }
}

/* Results State */
.results-state {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
}

.score-banner {
  padding: 12px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.score-info {
  display: flex;
  flex-direction: column;
}

.score-info .lbl {
  font-size: 10px;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.score-info .val {
  font-size: 18px;
  font-weight: 800;
  color: var(--color-ai);
}

.status-pill {
  font-size: 9px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
}
.status-pill.cao\ \(hot\ lead\) { background-color: rgba(239, 68, 68, 0.15); color: #ef4444; }
.status-pill.trung\ bình\ \(warm\) { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.status-pill.thấp\ \(cold\) { background-color: rgba(148, 163, 184, 0.15); color: #94a3b8; }

.entities-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.entity-item {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 10px 12px;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.entity-item .lbl {
  font-size: 9px;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.entity-item .val {
  font-size: 12.5px;
  font-weight: 600;
  color: var(--color-text-primary);
}

/* Matched block */
.matched-inventory-block {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 8px;
}

.block-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.send-all-btn {
  background: transparent;
  border: 1px solid var(--color-border);
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-secondary);
  padding: 2px 8px;
  border-radius: 4px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.send-all-btn:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.matched-grid {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.matched-card {
  display: flex;
  gap: 12px;
  padding: 8px;
  border-radius: 8px;
}

.matched-card img {
  width: 72px;
  height: 48px;
  object-fit: cover;
  border-radius: 6px;
  flex-shrink: 0;
}

.matched-card-body {
  display: flex;
  flex-direction: column;
  justify-content: center;
  flex: 1;
}

.matched-title {
  margin: 0;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-primary);
  line-height: 1.3;
}

.matched-price {
  font-size: 11px;
  color: var(--color-text-secondary);
  font-weight: 600;
  margin-top: 2px;
}

.matched-pct {
  font-size: 9.5px;
  font-weight: 700;
  color: var(--color-ai);
  margin-left: auto;
}

/* History logs section */
.history-table-section {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-header h3 {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.section-header .subtitle {
  margin: 0;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.table-container {
  overflow-x: auto;
}

.history-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12px;
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

.history-table tr:last-child td {
  border-bottom: none;
}

.target-name {
  font-weight: 600;
  color: var(--color-text-primary);
}

.type-badge {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}
.type-badge.property { background-color: rgba(59, 130, 246, 0.15); color: #3b82f6; }
.type-badge.lead { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }

.result-text {
  font-family: var(--font-mono);
  font-size: 11.5px;
}

.confidence-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.progress-bar-tiny {
  width: 50px;
  height: 4px;
  background-color: var(--color-divider);
  border-radius: 2px;
  overflow: hidden;
}

.progress-bar-tiny .fill {
  height: 100%;
  background-color: var(--color-yellow);
}

.status-indicator {
  font-size: 10px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 6px;
}

.status-indicator::before {
  content: '';
  width: 6px;
  height: 6px;
  border-radius: 50%;
}

.status-indicator.completed { color: var(--color-success); }
.status-indicator.completed::before { background-color: var(--color-success); }

.status-indicator.review { color: var(--color-warning); }
.status-indicator.review::before { background-color: var(--color-warning); }

.status-indicator.processing { color: var(--color-info); }
.status-indicator.processing::before { background-color: var(--color-info); }
</style>
