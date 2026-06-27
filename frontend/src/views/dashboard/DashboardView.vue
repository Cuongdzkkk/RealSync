<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useAuthStore } from '@/stores/useAuthStore';
import { useToastStore } from '@/stores/useToastStore';
import { mockLeads, mockProperties, mockCrawlSources, mockAiJobs } from '@/utils/mockData';
import { formatCurrency } from '@/utils/format';

const authStore = useAuthStore();
const toastStore = useToastStore();

const role = computed(() => authStore.user?.role ?? 'Admin');

// --- State and Mock Controls ---
const selectedLead = ref<any>(null);
const showAssignModal = ref(false);
const selectedSales = ref('');
const aiGenerating = ref(false);
const generatedContent = ref('');
const contentPrompt = ref('Căn hộ chung cư 2PN tại Landmark 81, đầy đủ nội thất, view trực diện sông Sài Gòn');

// Mock statistics depending on roles
const metrics = computed(() => {
  switch (role.value) {
    case 'Manager':
      return [
        { label: 'Doanh số Đội nhóm', value: '45.2 tỷ', trend: '+14%', status: 'success', desc: 'Tháng này' },
        { label: 'Lead Chưa Phân Phối', value: '18', trend: 'Cần phân công', status: 'warning', desc: 'Có 5 lead mới' },
        { label: 'Hiệu suất Sales TB', value: '88.5%', trend: '+4%', status: 'info', desc: 'Chỉ số KPI' },
        { label: 'AI Insights Đã Tạo', value: '124', trend: 'Tự động', status: 'success', desc: 'Phân tích tự động' }
      ];
    case 'Sales':
      return [
        { label: 'Khách hàng của tôi', value: '12', trend: 'Đang chăm sóc', status: 'info', desc: '3 lead mới' },
        { label: 'Lead Nóng (Hot)', value: '4', trend: 'Liên hệ ngay!', status: 'danger', desc: 'Độ ưu tiên cao' },
        { label: 'Cuộc hẹn hôm nay', value: '3', trend: 'Lên lịch', status: 'warning', desc: 'Lịch gọi điện' },
        { label: 'Tỷ lệ chốt sales', value: '24.8%', trend: '+3.2%', status: 'success', desc: 'Tháng này' }
      ];
    case 'Marketing':
      return [
        { label: 'Tổng tin đã đăng', value: '84', trend: '+12 bài', status: 'info', desc: 'Tháng này' },
        { label: 'Lượt tiếp cận (Reach)', value: '15.4K', trend: '+28%', status: 'success', desc: 'Đa nền tảng' },
        { label: 'Leads thu hút được', value: '42', trend: '+8%', status: 'success', desc: 'Từ Facebook/Zalo' },
        { label: 'Tin nháp AI tạo', value: '19', trend: 'Chờ duyệt', status: 'warning', desc: 'AI Drafts' }
      ];
    case 'Data Analyst':
      return [
        { label: 'Tin đã thu thập (Crawled)', value: '42,810', trend: '+1,200', status: 'success', desc: 'Tổng nguồn' },
        { label: 'Tốc độ crawl TB', value: '84/phút', trend: 'Ổn định', status: 'info', desc: '4 crawler nodes' },
        { label: 'Tỷ lệ thành công', value: '98.2%', trend: '+1.4%', status: 'success', desc: '24 giờ qua' },
        { label: 'Số proxy lỗi', value: '2', trend: 'Cần kiểm tra', status: 'danger', desc: 'Xoay vòng proxy' }
      ];
    case 'Admin':
    default:
      return [
        { label: 'Tổng số Leads', value: '248', trend: '+18% tháng này', status: 'success', desc: 'Toàn hệ thống' },
        { label: 'Tổng Bất Động Sản', value: '1,429', trend: '+125 tin mới', status: 'info', desc: 'Thu thập & nội bộ' },
        { label: 'Độ chính xác AI', value: '94.2%', trend: '+1.1%', status: 'success', desc: 'Chấm điểm & Phân loại' },
        { label: 'Crawlers Đang Chạy', value: '3/4 active', trend: '96% rate', status: 'warning', desc: 'Máy thu thập' }
      ];
  }
});

// Interactive Handler: Assign Lead
function openAssign(lead: any) {
  selectedLead.value = lead;
  showAssignModal.value = true;
}

function confirmAssign() {
  if (!selectedSales.value) return;
  toastStore.success(
    'Phân phối thành công',
    `Đã giao lead ${selectedLead.value.fullName} cho ${selectedSales.value}`
  );
  showAssignModal.value = false;
  selectedSales.value = '';
}

// Interactive Handler: AI Content Generator
function generateAiContent() {
  if (aiGenerating.value) return;
  aiGenerating.value = true;
  generatedContent.value = '';

  setTimeout(() => {
    generatedContent.value = `💎 **CƠ HỘI ĐẦU TƯ & AN CƯ THƯỢNG LƯU TẠI LANDMARK 81** 💎\n\n📍 **Vị trí**: Tòa nhà Landmark 81, Vinhomes Central Park, Bình Thạnh, TP.HCM.\n📐 **Diện tích**: 86m² | Thiết kế sang trọng 2 Phòng ngủ, 2 WC rộng rãi.\n\n✨ **Đặc điểm nổi bật**:\n- Căn hộ tầng cao, ban công rộng sở hữu **view sông Sài Gòn trực diện** và công viên 14ha đắt giá.\n- Nội thất nhập khẩu cao cấp từ Châu Âu, thiết kế chuẩn chỉnh từng góc cạnh.\n- Đặc quyền cư dân: Hồ bơi vô cực, Gym & Spa chuẩn 5 sao, trung tâm thương mại lớn ngay chân tòa nhà.\n\n💵 **Giá chào thuê/bán cực tốt**: Liên hệ nhận báo giá chi tiết.\n📞 Hỗ trợ xem nhà thực tế 24/7.`;
    aiGenerating.value = false;
  }, 2000);
}

function copyToClipboard() {
  navigator.clipboard.writeText(generatedContent.value);
  toastStore.success(
    'Đã sao chép',
    'Đã sao chép nội dung tin đăng vào bộ nhớ tạm.'
  );
}
</script>

<template>
  <div class="page">
    <!-- Page Header -->
    <div class="dashboard-header glass-card">
      <div class="header-main">
        <div class="user-welcome">
          <h2>Xin chào, {{ authStore.user?.fullName }} 👋</h2>
          <p class="subtitle-text">Hệ thống RealSync hôm nay hoạt động ổn định. Dưới đây là báo cáo nhanh cho vị trí <strong>{{ role }}</strong>.</p>
        </div>
        <div class="quick-status-indicators">
          <span class="indicator-item">
            <span class="pulse-dot success"></span>
            SignalR: Kết nối
          </span>
          <span class="indicator-item">
            <span class="pulse-dot info"></span>
            AI Engine: Sẵn sàng
          </span>
        </div>
      </div>
    </div>

    <!-- 4 KPI Cards Grid -->
    <div class="metrics-grid">
      <div 
        v-for="metric in metrics" 
        :key="metric.label" 
        class="glass-card glass-card--hoverable metric-card"
      >
        <div class="metric-card-header">
          <span class="metric-label">{{ metric.label }}</span>
          <span class="trend-badge" :class="metric.status">{{ metric.trend }}</span>
        </div>
        <div class="metric-value-container">
          <span class="metric-value numeric">{{ metric.value }}</span>
        </div>
        <div class="metric-footer">
          <span class="metric-desc">{{ metric.desc }}</span>
        </div>
      </div>
    </div>

    <!-- Main Dashboard Section (Asymmetric columns) -->
    <div class="dashboard-grid">
      <!-- LEFT COLUMN: Primary Features & Charts (width ~60%) -->
      <section class="dashboard-column-left">
        <!-- ADMIN & MANAGER: Lead Flow Analysis & Performance -->
        <div v-if="role === 'Admin' || role === 'Manager'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="20" x2="18" y2="10" /><line x1="12" y1="20" x2="12" y2="4" /><line x1="6" y1="20" x2="6" y2="14" />
              </svg>
              Xu hướng phát triển Lead & Chuyển đổi
            </h3>
            <span class="time-range-badge">7 ngày qua</span>
          </div>
          <div class="glass-card__body">
            <!-- Simulated Premium Chart -->
            <div class="mock-chart-container">
              <div class="chart-y-axis">
                <span>120</span>
                <span>80</span>
                <span>40</span>
                <span>0</span>
              </div>
              <div class="chart-bars-wrapper">
                <div class="chart-day-col" v-for="(val, idx) in [34, 52, 45, 85, 92, 110, 120]" :key="idx">
                  <div class="bar-group">
                    <div class="bar bar-bg" :style="{ height: `${val}%` }">
                      <span class="bar-value">{{ val }}</span>
                    </div>
                    <div class="bar bar-accent" :style="{ height: `${val * 0.65}%` }"></div>
                  </div>
                  <span class="day-label">T.{{ idx + 2 }}</span>
                </div>
              </div>
            </div>
            <div class="chart-legends">
              <div class="legend-item">
                <span class="legend-color-dot main"></span>
                <span>Lead Mới thu nhận</span>
              </div>
              <div class="legend-item">
                <span class="legend-color-dot accent glow"></span>
                <span>AI Phân loại chất lượng cao</span>
              </div>
            </div>
          </div>
        </div>

        <!-- SALES: Hot Leads list (Focus area) -->
        <div v-if="role === 'Sales'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title text-danger">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="animate-pulse">
                <path d="M12 2c1.33 0 2.66.67 3.6 2 .94 1.33 1.4 3 1.4 5 0 2-.67 4.67-2 8l-.6 1.2c-.33.66-.67 1.33-1 2H10.6c-.33-.67-.67-1.34-1-2l-.6-1.2C7.67 13.67 7 11 7 9c0-2 .47-3.67 1.4-5 .94-1.33 2.27-2 3.6-2z"/>
              </svg>
              Khách hàng tiềm năng NÓNG (Hot Leads) cần xử lý
            </h3>
            <span class="priority-label danger">Khẩn cấp</span>
          </div>
          <div class="glass-card__body">
            <div class="leads-compact-list">
              <div 
                v-for="lead in mockLeads.filter(l => l.temperature === 'hot' || l.temperature === 'warm')" 
                :key="lead.id" 
                class="lead-row-card glass-card"
              >
                <div class="lead-row-header">
                  <div class="lead-avatar">{{ lead.fullName.charAt(0) }}</div>
                  <div>
                    <span class="lead-name">{{ lead.fullName }}</span>
                    <span class="lead-temp" :class="lead.temperature">{{ lead.temperature }}</span>
                  </div>
                  <span class="lead-budget numeric">{{ formatCurrency(lead.budget) }}</span>
                </div>
                <div class="lead-row-details">
                  <p><strong>Nhu cầu:</strong> {{ lead.demand }}</p>
                  <div class="lead-row-actions">
                    <button class="action-btn action-btn--primary">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                      Gọi điện
                    </button>
                    <button class="action-btn">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                      Zalo Chat
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- MARKETING: AI Content Writer Playground -->
        <div v-if="role === 'Marketing'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                <path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4z" />
              </svg>
              AI Content Writer - Soạn Tin Đăng BĐS Tự Động
            </h3>
            <span class="ai-badge">Power by AI</span>
          </div>
          <div class="glass-card__body">
            <div class="content-generator-layout">
              <div class="input-section">
                <label class="section-label">Thông tin thô bất động sản</label>
                <textarea 
                  v-model="contentPrompt" 
                  placeholder="Điền đặc điểm của BĐS để AI tối ưu..."
                  rows="3"
                ></textarea>
                <button 
                  class="generate-btn glow-ai" 
                  :disabled="aiGenerating || !contentPrompt"
                  @click="generateAiContent"
                >
                  <span v-if="!aiGenerating">⚡ Tạo mô tả tin đăng</span>
                  <span v-else class="ai-streaming-cursor">AI đang suy nghĩ...</span>
                </button>
              </div>

              <div class="output-section glass-card">
                <div class="output-header">
                  <span>Mẫu nội dung tối ưu tin đăng</span>
                  <button v-if="generatedContent" class="copy-btn" @click="copyToClipboard">Sao chép</button>
                </div>
                <div class="output-body">
                  <p v-if="!generatedContent" class="empty-text">Mô tả tin đăng của bạn sẽ xuất hiện ở đây sau khi AI xử lý.</p>
                  <div v-else class="generated-rich-text" v-html="generatedContent.replace(/\n/g, '<br>')"></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- DATA ANALYST: Active Crawler Status Board -->
        <div v-if="role === 'Data Analyst'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
              </svg>
              Trạng thái & Tiến trình Crawlers
            </h3>
            <span class="live-badge">REAL-TIME</span>
          </div>
          <div class="glass-card__body">
            <div class="crawler-grid-list">
              <div v-for="source in mockCrawlSources" :key="source.id" class="crawler-row-card glass-card">
                <div class="crawler-card-main">
                  <div class="crawler-meta">
                    <h4>{{ source.name }}</h4>
                    <span class="url-span mono">{{ source.baseUrl }}</span>
                  </div>
                  <span class="crawler-status-dot" :class="{ 'active': source.isActive }"></span>
                </div>
                <div class="crawler-metrics">
                  <div>
                    <span class="lbl">Tỷ lệ thành công</span>
                    <strong class="val numeric">{{ source.successRate }}%</strong>
                  </div>
                  <div>
                    <span class="lbl">Tin hôm nay</span>
                    <strong class="val numeric">{{ source.listingsToday }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Lead Listing table for Managers/Admins -->
        <div v-if="role === 'Admin' || role === 'Manager'" class="glass-card panel-item mt-4">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/>
              </svg>
              Danh sách Leads mới nhận cần theo dõi
            </h3>
            <span v-if="role === 'Manager'" class="action-hint">Click để giao việc</span>
          </div>
          <div class="glass-card__body p-0">
            <table class="dashboard-table">
              <thead>
                <tr>
                  <th>Khách hàng</th>
                  <th>Nhu cầu</th>
                  <th>Phân loại</th>
                  <th>Phân phối</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="lead in mockLeads" :key="lead.id">
                  <td>
                    <div class="table-user-cell">
                      <div class="avatar-small">{{ lead.fullName.charAt(0) }}</div>
                      <div>
                        <span class="name">{{ lead.fullName }}</span>
                        <span class="sub">{{ lead.phone }}</span>
                      </div>
                    </div>
                  </td>
                  <td>{{ lead.demand }}</td>
                  <td>
                    <span class="temp-badge" :class="lead.temperature">{{ lead.temperature }}</span>
                  </td>
                  <td>
                    <button 
                      v-if="!lead.assignedTo && role === 'Manager'" 
                      class="assign-btn" 
                      @click="openAssign(lead)"
                    >
                      Giao Sales
                    </button>
                    <span v-else class="assigned-text">{{ lead.assignedTo || 'Chưa giao' }}</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <!-- RIGHT COLUMN: AI Insights / Tasks / Sidebar widgets (width ~40%) -->
      <section class="dashboard-column-right">
        <!-- AI LEAD SCORING INSIGHTS (Admin/Manager/Sales) -->
        <div v-if="role !== 'Marketing' && role !== 'Data Analyst'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title text-ai">
              <span class="ai-spark-icon">✨</span>
              AI Lead Scoring Insights
            </h3>
          </div>
          <div class="glass-card__body">
            <div class="ai-insights-list">
              <div 
                v-for="job in mockAiJobs" 
                :key="job.id" 
                class="ai-insight-item"
              >
                <div class="insight-header">
                  <span class="insight-title">{{ job.target }}</span>
                  <span class="insight-score glow-ai">{{ job.confidence }}%</span>
                </div>
                <p class="insight-desc">{{ job.result }}</p>
                <span class="insight-time">{{ job.createdAt.substring(11,16) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- MY TASKS TODAY (Sales) -->
        <div v-if="role === 'Sales'" class="glass-card panel-item mt-4">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="9 11 12 14 22 4" /><path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
              </svg>
              Công việc hôm nay
            </h3>
          </div>
          <div class="glass-card__body">
            <div class="todo-list">
              <label class="todo-item">
                <input type="checkbox" checked />
                <span class="todo-label text-muted">Gọi điện tư vấn căn hộ The River Thủ Thiêm</span>
              </label>
              <label class="todo-item">
                <input type="checkbox" />
                <span class="todo-label">Gửi báo giá Quận 2 cho anh Minh Anh</span>
              </label>
              <label class="todo-item">
                <input type="checkbox" />
                <span class="todo-label">Hẹn xem nhà mẫu Quận 7 chị Quốc Huy</span>
              </label>
            </div>
          </div>
        </div>

        <!-- AI CONTENT SUGGESTIONS (Marketing) -->
        <div v-if="role === 'Marketing'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title text-ai">
              <span class="ai-spark-icon">✨</span>
              Ý tưởng Marketing gợi ý
            </h3>
          </div>
          <div class="glass-card__body">
            <div class="ai-insights-list">
              <div class="ai-insight-item">
                <div class="insight-header">
                  <span class="insight-title">Đăng tin đợt 2 - Quận 7</span>
                  <span class="insight-badge">Hot</span>
                </div>
                <p class="insight-desc">Phân khúc Nhà phố Q7 đang có lượt tìm kiếm tăng 14% trên Batdongsan.com.vn trong tuần này.</p>
              </div>
              <div class="ai-insight-item">
                <div class="insight-header">
                  <span class="insight-title">Tạo tiêu đề SEO mới</span>
                  <span class="insight-badge">SEO</span>
                </div>
                <p class="insight-desc">Gợi ý từ khóa: "Mua chung cư cao cấp Thủ Thiêm view sông có sổ hồng".</p>
              </div>
            </div>
          </div>
        </div>

        <!-- CRAWLER ENGINE PERFORMANCE (Data Analyst) -->
        <div v-if="role === 'Data Analyst'" class="glass-card panel-item">
          <div class="glass-card__header">
            <h3 class="glass-card__title">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="2" y="2" width="20" height="8" rx="2" ry="2" /><rect x="2" y="14" width="20" height="8" rx="2" ry="2" />
              </svg>
              Logs Engine & API Rate Limits
            </h3>
          </div>
          <div class="glass-card__body">
            <div class="logs-console mono">
              <div class="log-line success">[09:30:11] Connected to proxy pool node-4</div>
              <div class="log-line info">[09:30:15] Crawler: batdongsan.com.vn scanned page 12</div>
              <div class="log-line warning">[09:31:02] Proxy node-1 received 403 Forbidden, rotating...</div>
              <div class="log-line success">[09:31:05] Proxy rotated. New IP: 14.120.25.109</div>
              <div class="log-line info">[09:32:00] AI scoring queue: processing job ai-003</div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Manager Specific Assign Sales Modal -->
    <div v-if="showAssignModal" class="modal-overlay glass-card">
      <div class="modal-content glass-card">
        <h3>Phân phối Lead: {{ selectedLead?.fullName }}</h3>
        <p>Chọn nhân viên Sales phù hợp để giao việc chăm sóc:</p>
        <div class="sales-select-list">
          <select v-model="selectedSales">
            <option value="" disabled>-- Chọn Sales Agent --</option>
            <option value="Nguyễn Hoàng Nam (Sales 1)">Nguyễn Hoàng Nam (Chuyên khu Q2)</option>
            <option value="Lê Thị Mai (Sales 2)">Lê Thị Mai (Chuyên khu Q7)</option>
            <option value="Trần Quốc Huy (Manager)">Tự chăm sóc</option>
          </select>
        </div>
        <div class="modal-actions">
          <button class="modal-btn" @click="showAssignModal = false">Hủy</button>
          <button class="modal-btn modal-btn--primary" :disabled="!selectedSales" @click="confirmAssign">Xác nhận giao</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-header {
  padding: 20px 24px;
}

.header-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.user-welcome h2 {
  font-size: 18px;
  font-weight: 700;
  margin: 0 0 6px 0;
  color: var(--color-text-primary);
}

.subtitle-text {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  margin: 0;
}

.quick-status-indicators {
  display: flex;
  gap: 16px;
}

.indicator-item {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 11.5px;
  color: var(--color-text-secondary);
  font-weight: 600;
  background-color: var(--color-surface-hover);
  padding: 4px 10px;
  border-radius: 6px;
  border: 1px solid var(--color-border);
}

.pulse-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.pulse-dot.success { background-color: var(--color-success); animation: pulse-ring 2s infinite; }
.pulse-dot.info { background-color: var(--color-info); }

/* --- 4 KPI Cards --- */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}

.metric-card {
  padding: 18px 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.metric-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.metric-label {
  font-size: 11.5px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  color: var(--color-text-secondary);
}

.trend-badge {
  font-size: 10px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
}
.trend-badge.success { background-color: var(--color-success-bg); color: var(--color-success); }
.trend-badge.warning { background-color: var(--color-warning-bg); color: var(--color-warning); }
.trend-badge.danger { background-color: var(--color-danger-bg); color: var(--color-danger); }
.trend-badge.info { background-color: var(--color-info-bg); color: var(--color-info); }

.metric-value-container {
  margin: 4px 0;
}

.metric-value {
  font-size: 28px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1;
}

.metric-footer {
  font-size: 11px;
  color: var(--color-text-muted);
}

/* --- Layout Grid --- */
.dashboard-grid {
  display: grid;
  grid-template-columns: 2.2fr 1.3fr;
  gap: 20px;
}

.panel-item {
  display: flex;
  flex-direction: column;
}

/* --- Custom Chart Styling --- */
.mock-chart-container {
  display: flex;
  height: 200px;
  gap: 16px;
  margin-top: 10px;
}

.chart-y-axis {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  font-size: 10px;
  color: var(--color-text-muted);
  width: 24px;
  text-align: right;
  padding-bottom: 20px;
}

.chart-bars-wrapper {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  border-left: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
  padding: 0 16px 20px 16px;
}

.chart-day-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  justify-content: flex-end;
  flex: 1;
  max-width: 48px;
}

.bar-group {
  position: relative;
  width: 16px;
  height: 100%;
  display: flex;
  align-items: flex-end;
  gap: 2px;
}

.bar {
  width: 8px;
  border-top-left-radius: 4px;
  border-top-right-radius: 4px;
  position: relative;
  transition: height 0.5s ease;
}

.bar-bg {
  background-color: var(--color-text-primary);
  opacity: 0.15;
}

.bar-accent {
  background-color: var(--color-yellow);
  box-shadow: var(--color-yellow-glow);
}

.bar-value {
  position: absolute;
  top: -16px;
  left: 50%;
  transform: translateX(-50%);
  font-size: 9px;
  font-weight: 700;
  opacity: 0;
  transition: opacity 0.2s;
}

.bar-group:hover .bar-value {
  opacity: 1;
}

.day-label {
  font-size: 10px;
  color: var(--color-text-secondary);
  margin-top: 6px;
}

.chart-legends {
  display: flex;
  gap: 20px;
  justify-content: center;
  margin-top: 12px;
  font-size: 11px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--color-text-secondary);
}

.legend-color-dot {
  width: 8px;
  height: 8px;
  border-radius: 2px;
}
.legend-color-dot.main { background-color: var(--color-text-primary); }
.legend-color-dot.accent { background-color: var(--color-yellow); }

/* --- Table Styling --- */
.dashboard-table {
  width: 100%;
  border-collapse: collapse;
}

.dashboard-table th {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border);
  padding: 12px 16px;
  text-align: left;
}

.dashboard-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-divider);
  font-size: 12.5px;
}

.table-user-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar-small {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background-color: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 11px;
}

.table-user-cell .name {
  font-weight: 600;
  display: block;
  color: var(--color-text-primary);
}

.table-user-cell .sub {
  font-size: 10px;
  color: var(--color-text-muted);
}

.temp-badge {
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
}
.temp-badge.hot { background-color: var(--color-danger-bg); color: var(--color-danger); }
.temp-badge.warm { background-color: var(--color-warning-bg); color: var(--color-warning); }
.temp-badge.cold { background-color: var(--color-info-bg); color: var(--color-info); }

.assign-btn {
  font-size: 11px;
  font-weight: 600;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  padding: 4px 10px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}
.assign-btn:hover {
  background-color: var(--color-yellow-hover);
}

.assigned-text {
  font-size: 11px;
  color: var(--color-text-secondary);
}

/* --- Sales Hot Leads rows --- */
.leads-compact-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.lead-row-card {
  padding: 12px;
}

.lead-row-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
}

.lead-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--color-yellow-muted);
  color: var(--color-yellow-hover);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

.lead-name {
  font-weight: 600;
  font-size: 13px;
  display: block;
}

.lead-temp {
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
}
.lead-temp.hot { color: var(--color-danger); }
.lead-temp.warm { color: var(--color-warning); }

.lead-budget {
  margin-left: auto;
  font-weight: 700;
  color: var(--color-text-primary);
}

.lead-row-details {
  font-size: 12px;
}

.lead-row-details p {
  margin: 0 0 10px 0;
  color: var(--color-text-secondary);
}

.lead-row-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 600;
  padding: 6px 12px;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  cursor: pointer;
  transition: all var(--duration-fast);
}
.action-btn:hover {
  background: var(--color-surface-hover);
}
.action-btn--primary {
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  border-color: var(--color-text-primary);
}

/* --- Content AI Generator --- */
.content-generator-layout {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  display: block;
  margin-bottom: 6px;
}

.content-generator-layout textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  border-radius: 8px;
  resize: none;
  font-size: 12.5px;
}

.generate-btn {
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  align-self: flex-start;
  transition: all var(--duration-fast);
}
.generate-btn:hover {
  transform: translateY(-1px);
}

.output-section {
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.output-header {
  padding: 8px 12px;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11.5px;
  font-weight: 600;
}

.copy-btn {
  background: transparent;
  border: none;
  color: var(--color-info);
  cursor: pointer;
  font-weight: 600;
}

.output-body {
  padding: 16px;
  min-height: 120px;
}

.generated-rich-text {
  font-size: 12.5px;
  line-height: 1.5;
  color: var(--color-text-primary);
}

.empty-text {
  font-size: 12px;
  color: var(--color-text-muted);
  text-align: center;
  padding-top: 30px;
}

/* --- Crawler Grid List --- */
.crawler-grid-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.crawler-row-card {
  padding: 14px;
}

.crawler-card-main {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 10px;
}

.crawler-meta h4 {
  margin: 0 0 3px 0;
  font-size: 13px;
}

.url-span {
  font-size: 10.5px;
  color: var(--color-text-muted);
}

.crawler-status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: var(--color-text-muted);
}
.crawler-status-dot.active {
  background-color: var(--color-success);
  box-shadow: 0 0 8px var(--color-success);
}

.crawler-metrics {
  display: flex;
  gap: 20px;
  font-size: 12px;
  border-top: 1px solid var(--color-divider);
  padding-top: 10px;
}

.crawler-metrics .lbl {
  color: var(--color-text-muted);
  display: block;
  font-size: 10px;
}

.crawler-metrics .val {
  font-weight: 700;
  color: var(--color-text-primary);
}

/* --- AI Insights --- */
.ai-insights-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ai-insight-item {
  border-left: 2px solid var(--color-ai);
  padding-left: 12px;
  position: relative;
}

.insight-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.insight-title {
  font-size: 12px;
  font-weight: 700;
}

.insight-score {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--color-ai);
  font-family: var(--font-mono);
}

.insight-desc {
  font-size: 11.5px;
  color: var(--color-text-secondary);
  margin: 0;
}

.insight-time {
  font-size: 9px;
  color: var(--color-text-muted);
}

/* --- Todo List --- */
.todo-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.todo-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12.5px;
  cursor: pointer;
}

.todo-item input {
  cursor: pointer;
}

/* --- Console Logs --- */
.logs-console {
  background: #030712;
  color: #38bdf8;
  border-radius: 8px;
  padding: 12px;
  font-size: 10.5px;
  line-height: 1.6;
  max-height: 200px;
  overflow-y: auto;
}

.log-line.success { color: #34d399; }
.log-line.warning { color: #fbbf24; }
.log-line.info { color: #38bdf8; }

/* --- Modals --- */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
  border: none;
  border-radius: 0;
}

.modal-content {
  width: 380px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: var(--elevation-floating);
  animation: fadein var(--duration-base) var(--ease-spring);
}

.modal-content h3 {
  margin: 0;
  font-size: 15px;
}

.modal-content p {
  margin: 0;
  font-size: 12.5px;
  color: var(--color-text-secondary);
}

.sales-select-list select {
  width: 100%;
  height: 38px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  padding: 0 10px;
  background: var(--color-canvas);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

.modal-btn {
  height: 34px;
  padding: 0 14px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: transparent;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.modal-btn--primary {
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  border-color: var(--color-text-primary);
}
.modal-btn--primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* --- Breakpoints --- */
@media (max-width: 1024px) {
  .metrics-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}
@media (max-width: 580px) {
  .metrics-grid {
    grid-template-columns: 1fr;
  }
}
</style>
