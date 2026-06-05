<script setup lang="ts">
import { ref } from 'vue';

interface TimelineItem {
  id: string;
  type: 'ai' | 'crawler' | 'crm';
  title: string;
  description: string;
  time: string;
  status: 'success' | 'warning' | 'danger' | 'info';
  meta?: string;
}

const activeTab = ref<'all' | 'ai' | 'crawler' | 'crm'>('all');

const timelineItems = ref<TimelineItem[]>([
  {
    id: '1',
    type: 'ai',
    title: 'AI Chấm Điểm Lead',
    description: 'Chấm điểm khách hàng <strong>Nguyễn Văn A</strong> đạt <strong>92 điểm</strong> (Độ ưu tiên: Cao).',
    time: '3 phút trước',
    status: 'success',
    meta: 'AI Engine v2.4'
  },
  {
    id: '2',
    type: 'crawler',
    title: 'Crawler Batdongsan.com.vn',
    description: 'Đã hoàn tất quét <strong>150 bất động sản mới</strong> khu vực Quận 2.',
    time: '12 phút trước',
    status: 'info',
    meta: 'Speed: 45 pgs/min'
  },
  {
    id: '3',
    type: 'crm',
    title: 'Phân phối Lead thành công',
    description: 'Hệ thống đã phân phối lead nóng <strong>Trần Thị B</strong> cho Sales <strong>Nguyễn Hoàng Nam</strong>.',
    time: '32 phút trước',
    status: 'success'
  },
  {
    id: '4',
    type: 'ai',
    title: 'Lỗi sinh nội dung AI',
    description: 'Tiến trình tạo tin đăng AI bị dừng do vượt hạn mức token OpenAI API.',
    time: '1 giờ trước',
    status: 'danger',
    meta: 'Error 429'
  },
  {
    id: '5',
    type: 'crawler',
    title: 'Cảnh báo Crawler Chợ Tốt',
    description: 'Crawler Chợ Tốt gặp CAPTCHA xác thực, cần cấu hình proxy xoay vòng.',
    time: '2 giờ trước',
    status: 'warning',
    meta: 'Proxy timeout'
  },
  {
    id: '6',
    type: 'crm',
    title: 'Khách hàng liên hệ lại',
    description: 'Lead <strong>Phạm Văn D</strong> vừa điền form tư vấn từ Facebook Ads Campaign.',
    time: '4 giờ trước',
    status: 'info'
  }
]);

const filterItems = (tab: typeof activeTab.value) => {
  if (tab === 'all') return timelineItems.value;
  return timelineItems.value.filter(item => item.type === tab);
};
</script>

<template>
  <aside class="activity-timeline">
    <div class="panel-header">
      <h3 class="panel-title">
        <span class="pulse-indicator"></span>
        Hoạt động Hệ thống
      </h3>
      <span class="live-badge">LIVE</span>
    </div>

    <!-- Filter Tabs -->
    <div class="timeline-tabs">
      <button 
        v-for="tab in ['all', 'ai', 'crawler', 'crm']" 
        :key="tab"
        class="tab-btn" 
        :class="{ 'is-active': activeTab === tab }"
        @click="activeTab = tab as any"
      >
        {{ tab.toUpperCase() }}
      </button>
    </div>

    <!-- Timeline Scroll Area -->
    <div class="timeline-list">
      <div 
        v-for="item in filterItems(activeTab)" 
        :key="item.id" 
        class="timeline-item"
        :class="`timeline-item--${item.status}`"
      >
        <div class="item-icon-wrapper">
          <!-- AI Icon -->
          <svg v-if="item.type === 'ai'" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/>
          </svg>
          <!-- Crawler Icon -->
          <svg v-else-if="item.type === 'crawler'" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
          </svg>
          <!-- CRM Icon -->
          <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
            <circle cx="9" cy="7" r="4"/>
            <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
            <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
        </div>

        <div class="item-content">
          <div class="item-header">
            <span class="item-title">{{ item.title }}</span>
            <span class="item-time">{{ item.time }}</span>
          </div>
          <p class="item-desc" v-html="item.description"></p>
          <div v-if="item.meta" class="item-meta">
            <span>{{ item.meta }}</span>
          </div>
        </div>
      </div>
      
      <div v-if="filterItems(activeTab).length === 0" class="empty-state">
        Không có hoạt động nào trong danh mục này.
      </div>
    </div>

    <!-- Health Checks / Metrics -->
    <div class="system-status">
      <div class="status-row">
        <span class="status-label">Trạng thái AI Engine</span>
        <span class="status-val status-val--success">Online</span>
      </div>
      <div class="status-row">
        <span class="status-label">Crawler Nodes</span>
        <span class="status-val status-val--info">3 Active</span>
      </div>
      <div class="status-row">
        <span class="status-label">Tốc độ quét dữ liệu</span>
        <span class="status-val">42 tin/phút</span>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.activity-timeline {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.panel-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.pulse-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: var(--color-success);
  box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 6px rgba(16, 185, 129, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

.live-badge {
  font-size: 9px;
  font-weight: 700;
  color: #ffffff;
  background-color: var(--color-danger);
  padding: 2px 6px;
  border-radius: 4px;
  letter-spacing: 0.05em;
}

.timeline-tabs {
  display: flex;
  gap: 4px;
  margin-bottom: 16px;
  background: rgba(148, 163, 184, 0.06);
  padding: 3px;
  border-radius: 8px;
}

.tab-btn {
  flex: 1;
  font-size: 10px;
  font-weight: 600;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  padding: 6px 4px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-standard);
}

.tab-btn:hover {
  color: var(--color-text-primary);
  background: rgba(255, 255, 255, 0.1);
}

.tab-btn.is-active {
  background: var(--color-surface);
  color: var(--color-text-primary);
  box-shadow: var(--elevation-surface);
  border: 1px solid var(--color-border);
}

.timeline-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding-right: 4px;
  margin-bottom: 20px;
}

.timeline-item {
  display: flex;
  gap: 12px;
  position: relative;
  padding-bottom: 4px;
}

.timeline-item::before {
  content: '';
  position: absolute;
  left: 14px;
  top: 28px;
  bottom: -16px;
  width: 1px;
  background-color: var(--color-divider);
}

.timeline-item:last-child::before {
  display: none;
}

.item-icon-wrapper {
  width: 28px;
  height: 28px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  z-index: 1;
  transition: all var(--duration-fast) var(--ease-standard);
}

/* Colors by status */
.timeline-item--success .item-icon-wrapper {
  background-color: var(--color-success-bg);
  border-color: var(--color-success-border);
  color: var(--color-success);
}
.timeline-item--warning .item-icon-wrapper {
  background-color: var(--color-warning-bg);
  border-color: var(--color-warning-border);
  color: var(--color-warning);
}
.timeline-item--danger .item-icon-wrapper {
  background-color: var(--color-danger-bg);
  border-color: var(--color-danger-border);
  color: var(--color-danger);
}
.timeline-item--info .item-icon-wrapper {
  background-color: var(--color-info-bg);
  border-color: var(--color-info-border);
  color: var(--color-info);
}

.item-content {
  flex: 1;
  min-width: 0;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 3px;
  gap: 8px;
}

.item-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.item-time {
  font-size: 10px;
  color: var(--color-text-muted);
  white-space: nowrap;
}

.item-desc {
  font-size: 11px;
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.4;
}

:deep(.item-desc strong) {
  font-weight: 600;
  color: var(--color-text-primary);
}

.item-meta {
  display: inline-flex;
  font-size: 9px;
  font-family: var(--font-mono);
  color: var(--color-text-muted);
  background-color: var(--color-divider);
  padding: 2px 6px;
  border-radius: 4px;
  margin-top: 6px;
}

.empty-state {
  text-align: center;
  font-size: 11px;
  color: var(--color-text-muted);
  padding: 40px 0;
}

.system-status {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.status-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11px;
}

.status-label {
  color: var(--color-text-secondary);
}

.status-val {
  font-weight: 600;
  color: var(--color-text-primary);
}

.status-val--success {
  color: var(--color-success);
}

.status-val--info {
  color: var(--color-info);
}
</style>
