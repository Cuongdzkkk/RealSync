<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useToastStore } from '@/stores/useToastStore';
import { useAuthStore } from '@/stores/useAuthStore';
import { mockUsers, mockRoleCapabilities } from '@/utils/mockData';
import type { WorkspaceUser } from '@/types/user';
import { api } from '@/services/api';

const toastStore = useToastStore();
const authStore = useAuthStore();

// --- Active Tab State ---
const activeTab = ref<'general' | 'ai' | 'crawler' | 'users' | 'channels'>('general');

// --- General Config State ---
const generalForm = ref({
  companyName: 'RealSync AI CRM',
  primaryCurrency: 'VND',
  language: 'vi',
  timezone: 'Asia/Ho_Chi_Minh',
  autoBackup: true,
  notifyLeads: true
});

// --- AI Center Config State ---
const aiForm = ref({
  defaultModel: 'gpt-4o',
  temperature: 0.2,
  apiKey: 'sk-proj-••••••••••••••••••••••••3A8f',
  showKey: false,
  budgetWeight: 40,
  locationWeight: 30,
  typeWeight: 30
});

// --- Crawler Config State ---
const crawlerForm = ref({
  rotateProxies: true,
  crawlDelay: 5,
  listingsLimit: 2000,
  autoImport: false,
  bypassCaptcha: true
});

// --- Users & Roles State ---
const usersList = ref<WorkspaceUser[]>(mockUsers);
const capabilitiesList = ref(JSON.parse(JSON.stringify(mockRoleCapabilities)));
const showAddUserModal = ref(false);
const newUser = ref({
  fullName: '',
  email: '',
  role: 'Agent' as any
});

function handleSaveGeneral() {
  toastStore.success('Cập nhật cấu hình', 'Đã lưu cấu hình chung của hệ thống.');
}

function handleSaveAi() {
  toastStore.success('Lưu cấu hình AI', 'Đã cập nhật các thông số trọng số khớp lệnh và mô hình AI.');
}

function handleSaveCrawler() {
  toastStore.success('Lưu cấu hình cào', 'Thiết lập điều phối proxy và tiến trình cào đã được cập nhật.');
}

function handleAddUser() {
  if (!newUser.value.fullName || !newUser.value.email) {
    toastStore.warning('Thiếu thông tin', 'Vui lòng nhập đầy đủ họ tên và email của nhân sự.');
    return;
  }
  
  const created: WorkspaceUser = {
    id: `u-${Date.now()}`,
    fullName: newUser.value.fullName,
    email: newUser.value.email,
    role: newUser.value.role,
    status: 'invited',
    lastSeenAt: new Date().toISOString()
  };

  usersList.value.push(created);
  toastStore.success('Đã gửi lời mời', `Lời mời tham gia đã được gửi tới email ${created.email}`);
  showAddUserModal.value = false;
  newUser.value = { fullName: '', email: '', role: 'Agent' };
}

function toggleLockUser(user: WorkspaceUser) {
  const previousStatus = user.status;
  user.status = previousStatus === 'locked' ? 'active' : 'locked';
  toastStore.success(
    user.status === 'locked' ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản',
    `Trạng thái tài khoản của ${user.fullName} đã được cập nhật.`
  );
}

function toggleCapability(cap: any, roleKey: 'admin' | 'manager' | 'agent' | 'viewer') {
  if (authStore.user?.role !== 'Admin') {
    toastStore.warning('Từ chối truy cập', 'Chỉ tài khoản quyền Quản trị viên (Admin) mới có thể thay đổi ma trận phân quyền.');
    return;
  }
  
  cap[roleKey] = !cap[roleKey];
  toastStore.success(
    'Cập nhật ma trận phân quyền',
    `Đã ${cap[roleKey] ? 'bật' : 'tắt'} quyền ${roleKey.toUpperCase()} cho module "${cap.module}".`
  );
}

function changeUserRole(user: WorkspaceUser, newRole: any) {
  user.role = newRole;
  toastStore.success('Đổi vai trò', `Đã chuyển vai trò của ${user.fullName} thành ${newRole}.`);
}

const isSavingChannels = ref(false);
const channelsForm = ref({
  facebookPageId: '',
  facebookAccessToken: '',
  metaAppId: '',
  metaAppSecret: '',
  zaloPageId: '',
  zaloTargetGroupId: '',
  facebookGroupIds: ''
});

const fbUserToken = ref('');
const fbPages = ref<any[]>([]);
const isFetchingFbPages = ref(false);
const showFbHelper = ref(false);

async function handleFetchFbPages() {
  if (!fbUserToken.value.trim()) {
    toastStore.warning('Thiếu Token', 'Vui lòng nhập User Access Token.');
    return;
  }
  isFetchingFbPages.value = true;
  try {
    const { data: res } = await api.post('/settings/facebook/fetch-pages', {
      userAccessToken: fbUserToken.value,
      appId: channelsForm.value.metaAppId,
      appSecret: channelsForm.value.metaAppSecret
    });
    if (res.data && res.data.length > 0) {
      fbPages.value = res.data;
      toastStore.success('Thành công', `Tìm thấy ${res.data.length} trang Facebook.`);
    } else {
      fbPages.value = [];
      toastStore.info('Không tìm thấy', 'Không tìm thấy trang nào được quản lý bởi token này.');
    }
  } catch (error: any) {
    toastStore.error('Lỗi', error.response?.data?.message || 'Không thể lấy danh sách trang.');
  } finally {
    isFetchingFbPages.value = false;
  }
}

function selectFbPage(page: any) {
  channelsForm.value.facebookPageId = page.id;
  channelsForm.value.facebookAccessToken = page.accessToken;
  toastStore.success('Đã điền', `Đã tự động điền ID và Access Token cho trang "${page.name}".`);
}

onMounted(async () => {
  try {
    const { data: res } = await api.get('/settings/channels');
    if (res.data) {
      channelsForm.value = {
        facebookPageId: res.data.facebookPageId || '',
        facebookAccessToken: res.data.facebookAccessToken || '',
        metaAppId: res.data.metaAppId || '',
        metaAppSecret: res.data.metaAppSecret || '',
        zaloPageId: res.data.zaloPageId || '',
        zaloTargetGroupId: res.data.zaloTargetGroupId || '',
        facebookGroupIds: res.data.facebookGroupIds || ''
      };
    }
  } catch (error) {
    // API interceptor will show the error toast
  }
});

async function handleSaveChannels() {
  isSavingChannels.value = true;
  try {
    await api.post('/settings/channels', channelsForm.value);
    toastStore.success('Đã lưu cấu hình', 'Thiết lập cấu hình các kênh đăng bài đã được lưu thành công.');
  } catch (error) {
    // API interceptor will show the error toast
  } finally {
    isSavingChannels.value = false;
  }
}
</script>

<template>
  <div class="page">
    <div class="settings-container">
      <!-- Settings Tabs sidebar -->
      <div class="settings-sidebar glass-card">
        <button 
          class="tab-link" 
          :class="{ active: activeTab === 'general' }"
          @click="activeTab = 'general'"
        >
          <span class="icon">⚙️</span>
          <span>Cấu hình chung</span>
        </button>
        <button 
          class="tab-link" 
          :class="{ active: activeTab === 'ai' }"
          @click="activeTab = 'ai'"
        >
          <span class="icon">🧠</span>
          <span>Cấu hình AI Center</span>
        </button>
        <button 
          class="tab-link" 
          :class="{ active: activeTab === 'crawler' }"
          @click="activeTab = 'crawler'"
        >
          <span class="icon">🕸️</span>
          <span>Cấu hình Crawler</span>
        </button>
        <button 
          class="tab-link" 
          :class="{ active: activeTab === 'users' }"
          @click="activeTab = 'users'"
        >
          <span class="icon">👥</span>
          <span>Nhân sự & Phân quyền</span>
        </button>
        <button 
          class="tab-link" 
          :class="{ active: activeTab === 'channels' }"
          @click="activeTab = 'channels'"
        >
          <span class="icon">📢</span>
          <span>Cấu hình Kênh đăng</span>
        </button>
      </div>

      <!-- Settings Panels content -->
      <div class="settings-content glass-card">
        <!-- 1. GENERAL SETTINGS PANEL -->
        <div v-if="activeTab === 'general'" class="panel-section animate-fade">
          <div class="panel-header">
            <h3>Cấu hình chung hệ thống</h3>
            <p class="subtitle">Thiết lập các thông số cơ bản cho sàn giao dịch và ngôn ngữ mặc định.</p>
          </div>

          <form @submit.prevent="handleSaveGeneral" class="settings-form">
            <div class="form-group">
              <label>Tên Doanh nghiệp / Sàn BĐS</label>
              <input v-model="generalForm.companyName" type="text" placeholder="Nhập tên sàn..." required />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Ngôn ngữ hiển thị</label>
                <select v-model="generalForm.language">
                  <option value="vi">Tiếng Việt</option>
                  <option value="en">English</option>
                </select>
              </div>
              <div class="form-group">
                <label>Tiền tệ chính</label>
                <select v-model="generalForm.primaryCurrency">
                  <option value="VND">VNĐ (Việt Nam Đồng)</option>
                  <option value="USD">USD (Đô la Mỹ)</option>
                </select>
              </div>
            </div>

            <div class="form-group">
              <label>Múi giờ hệ thống</label>
              <select v-model="generalForm.timezone">
                <option value="Asia/Ho_Chi_Minh">Asia/Ho_Chi_Minh (GMT+7)</option>
                <option value="Asia/Singapore">Asia/Singapore (GMT+8)</option>
              </select>
            </div>

            <div class="toggle-list">
              <label class="toggle-item">
                <div class="toggle-info">
                  <strong>Tự động sao lưu dữ liệu</strong>
                  <span>Sao lưu đám mây tự động mỗi 24 giờ để tránh mất dữ liệu khách hàng.</span>
                </div>
                <label class="switch">
                  <input type="checkbox" v-model="generalForm.autoBackup" />
                  <span class="slider round"></span>
                </label>
              </label>

              <label class="toggle-item">
                <div class="toggle-info">
                  <strong>Thông báo Leads mới ngay lập tức</strong>
                  <span>Gửi tin nhắn thông báo đẩy cho Sales khi có Leads nóng từ Crawler đổ về.</span>
                </div>
                <label class="switch">
                  <input type="checkbox" v-model="generalForm.notifyLeads" />
                  <span class="slider round"></span>
                </label>
              </label>
            </div>

            <button type="submit" class="save-btn glow-yellow">Lưu thay đổi</button>
          </form>
        </div>

        <!-- 2. AI CENTER SETTINGS PANEL -->
        <div v-if="activeTab === 'ai'" class="panel-section animate-fade">
          <div class="panel-header">
            <h3>Cấu hình Trí tuệ Nhân tạo (AI Center)</h3>
            <p class="subtitle">Điều chỉnh mô hình ngôn ngữ lớn (LLM), API keys và các trọng số đối sánh khách hàng tiềm năng.</p>
          </div>

          <form @submit.prevent="handleSaveAi" class="settings-form">
            <div class="form-row">
              <div class="form-group">
                <label>Mô hình NLP chủ đạo (LLM Model)</label>
                <select v-model="aiForm.defaultModel">
                  <option value="gpt-4o">GPT-4o (Độ chính xác cao nhất)</option>
                  <option value="gpt-3.5-turbo">GPT-3.5 Turbo (Tốc độ nhanh)</option>
                  <option value="claude-3.5-sonnet">Claude 3.5 Sonnet</option>
                  <option value="llama-3">Meta Llama 3 (Mã nguồn mở)</option>
                </select>
              </div>

              <div class="form-group">
                <label>Độ sáng tạo (Temperature): <span class="numeric">{{ aiForm.temperature }}</span></label>
                <input 
                  type="range" 
                  min="0" 
                  max="1" 
                  step="0.1" 
                  v-model.number="aiForm.temperature" 
                  class="styled-range"
                />
              </div>
            </div>

            <div class="form-group">
              <label>OpenAI API Key kết nối</label>
              <div class="input-password-wrapper">
                <input 
                  :type="aiForm.showKey ? 'text' : 'password'" 
                  v-model="aiForm.apiKey" 
                  placeholder="sk-..." 
                />
                <button 
                  type="button" 
                  class="pwd-toggle-btn"
                  @click="aiForm.showKey = !aiForm.showKey"
                >
                  {{ aiForm.showKey ? '👁️' : '🕶️' }}
                </button>
              </div>
            </div>

            <div class="section-divider"></div>

            <div class="weights-section">
              <h4>Trọng số đối sánh AI (AI Matcher Weights)</h4>
              <p class="section-desc">Phân bổ tỷ trọng điểm số khi AI đối khớp nhu cầu Lead với kho Bất động sản hiện có.</p>

              <div class="weight-sliders">
                <div class="weight-slider-group">
                  <div class="slider-labels">
                    <span>Ngân sách dự kiến (Budget)</span>
                    <span class="numeric">{{ aiForm.budgetWeight }}%</span>
                  </div>
                  <input type="range" min="0" max="100" v-model.number="aiForm.budgetWeight" class="styled-range yellow" />
                </div>

                <div class="weight-slider-group">
                  <div class="slider-labels">
                    <span>Vị trí / Khu vực (Location)</span>
                    <span class="numeric">{{ aiForm.locationWeight }}%</span>
                  </div>
                  <input type="range" min="0" max="100" v-model.number="aiForm.locationWeight" class="styled-range yellow" />
                </div>

                <div class="weight-slider-group">
                  <div class="slider-labels">
                    <span>Loại hình sản phẩm (Type)</span>
                    <span class="numeric">{{ aiForm.typeWeight }}%</span>
                  </div>
                  <input type="range" min="0" max="100" v-model.number="aiForm.typeWeight" class="styled-range yellow" />
                </div>
              </div>

              <div class="total-weight-warning" :class="{ error: aiForm.budgetWeight + aiForm.locationWeight + aiForm.typeWeight !== 100 }">
                Tổng trọng số: <span class="numeric">{{ aiForm.budgetWeight + aiForm.locationWeight + aiForm.typeWeight }}%</span>
                (Phải bằng 100%)
              </div>
            </div>

            <button 
              type="submit" 
              class="save-btn glow-yellow"
              :disabled="aiForm.budgetWeight + aiForm.locationWeight + aiForm.typeWeight !== 100"
            >
              Lưu cấu hình AI
            </button>
          </form>
        </div>

        <!-- 3. CRAWLER SETTINGS PANEL -->
        <div v-if="activeTab === 'crawler'" class="panel-section animate-fade">
          <div class="panel-header">
            <h3>Cấu hình hệ thống Crawler (Thu thập tự động)</h3>
            <p class="subtitle">Điều tiết băng thông quét dữ liệu, tần suất xoay proxy để tránh bị chặn IP.</p>
          </div>

          <form @submit.prevent="handleSaveCrawler" class="settings-form">
            <div class="form-row">
              <div class="form-group">
                <label>Thời gian trễ giữa các lượt request: <span class="numeric">{{ crawlerForm.crawlDelay }}s</span></label>
                <input type="range" min="1" max="30" v-model.number="crawlerForm.crawlDelay" class="styled-range" />
              </div>

              <div class="form-group">
                <label>Giới hạn tin cào/ngày: <span class="numeric">{{ crawlerForm.listingsLimit }} tin</span></label>
                <input type="range" min="100" max="5000" step="100" v-model.number="crawlerForm.listingsLimit" class="styled-range" />
              </div>
            </div>

            <div class="toggle-list">
              <label class="toggle-item">
                <div class="toggle-info">
                  <strong>Tự động xoay vòng danh sách Proxies</strong>
                  <span>Thay đổi địa chỉ IP liên tục thông qua hệ thống proxy quốc gia để phòng ngừa bị tường lửa các trang BĐS chặn.</span>
                </div>
                <label class="switch">
                  <input type="checkbox" v-model="crawlerForm.rotateProxies" />
                  <span class="slider round"></span>
                </label>
              </label>

              <label class="toggle-item">
                <div class="toggle-info">
                  <strong>Tự động vượt CAPTCHA bằng AI</strong>
                  <span>Sử dụng dịch vụ nhận dạng để giải mã mã CAPTCHA hình ảnh tự động khi bị bot phát hiện.</span>
                </div>
                <label class="switch">
                  <input type="checkbox" v-model="crawlerForm.bypassCaptcha" />
                  <span class="slider round"></span>
                </label>
              </label>

              <label class="toggle-item">
                <div class="toggle-info">
                  <strong>Tự động duyệt tin cào vào kho chính thức</strong>
                  <span>Nếu bật, các tin có AI Score > 90% sẽ tự động được hiển thị trên kho BĐS của sàn mà không cần môi giới phê duyệt thủ công.</span>
                </div>
                <label class="switch">
                  <input type="checkbox" v-model="crawlerForm.autoImport" />
                  <span class="slider round"></span>
                </label>
              </label>
            </div>

            <button type="submit" class="save-btn glow-yellow">Lưu cấu hình cào</button>
          </form>
        </div>

        <!-- 4. USERS & ROLES SETTINGS PANEL -->
        <div v-if="activeTab === 'users'" class="panel-section animate-fade">
          <div class="panel-header">
            <div>
              <h3>Danh sách nhân sự & Phân quyền</h3>
              <p class="subtitle">Quản lý tài khoản các môi giới (Agents), trưởng phòng (Managers) và cấu hình bảng phân quyền hạn (RBAC).</p>
            </div>
            <button class="add-user-btn glow-yellow" @click="showAddUserModal = true">
              + Mời thành viên
            </button>
          </div>

          <!-- Users List Table -->
          <div class="table-container">
            <table class="users-table">
              <thead>
                <tr>
                  <th>Họ và tên</th>
                  <th>Địa chỉ Email</th>
                  <th>Vai trò</th>
                  <th>Trạng thái</th>
                  <th>Lần cuối hoạt động</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in usersList" :key="user.id">
                  <td class="user-name-cell">
                    <div class="avatar-sm">{{ user.fullName[0] }}</div>
                    <strong>{{ user.fullName }}</strong>
                  </td>
                  <td>{{ user.email }}</td>
                  <td>
                    <select 
                      :value="user.role" 
                      @change="changeUserRole(user, ($event.target as HTMLSelectElement).value)"
                      class="role-select"
                    >
                      <option value="Admin">Admin (Quản trị viên)</option>
                      <option value="Manager">Manager (Quản lý)</option>
                      <option value="Agent">Agent (Môi giới)</option>
                      <option value="Viewer">Viewer (Chỉ xem)</option>
                    </select>
                  </td>
                  <td>
                    <span class="status-badge" :class="user.status">
                      {{ user.status === 'active' ? 'Hoạt động' : user.status === 'invited' ? 'Đã mời' : 'Đã khóa' }}
                    </span>
                  </td>
                  <td>{{ new Date(user.lastSeenAt).toLocaleDateString('vi-VN') }}</td>
                  <td>
                    <button 
                      class="lock-toggle-btn"
                      :class="{ locked: user.status === 'locked' }"
                      @click="toggleLockUser(user)"
                    >
                      {{ user.status === 'locked' ? 'Mở khóa' : 'Khóa' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- RBAC Capabilities table -->
          <div class="rbac-section">
            <h4>Bảng phân quyền chức năng (Role Capabilities Matrix)</h4>
            <div class="table-container">
              <table class="rbac-table">
                <thead>
                  <tr>
                    <th>Chức năng quản lý (Module)</th>
                    <th class="text-center">Admin</th>
                    <th class="text-center">Manager</th>
                    <th class="text-center">Agent</th>
                    <th class="text-center">Viewer</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="cap in capabilitiesList" :key="cap.module">
                    <td class="module-name">{{ cap.module }}</td>
                    <td class="text-center">
                      <button 
                        type="button"
                        class="cap-toggle-btn"
                        :class="{ active: cap.admin, disabled: authStore.user?.role !== 'Admin' }"
                        :disabled="authStore.user?.role !== 'Admin'"
                        @click="toggleCapability(cap, 'admin')"
                        :title="authStore.user?.role === 'Admin' ? 'Click để bật/tắt' : 'Chỉ Admin mới có quyền sửa'"
                      >
                        {{ cap.admin ? '✓' : '✗' }}
                      </button>
                    </td>
                    <td class="text-center">
                      <button 
                        type="button"
                        class="cap-toggle-btn"
                        :class="{ active: cap.manager, disabled: authStore.user?.role !== 'Admin' }"
                        :disabled="authStore.user?.role !== 'Admin'"
                        @click="toggleCapability(cap, 'manager')"
                        :title="authStore.user?.role === 'Admin' ? 'Click để bật/tắt' : 'Chỉ Admin mới có quyền sửa'"
                      >
                        {{ cap.manager ? '✓' : '✗' }}
                      </button>
                    </td>
                    <td class="text-center">
                      <button 
                        type="button"
                        class="cap-toggle-btn"
                        :class="{ active: cap.agent, disabled: authStore.user?.role !== 'Admin' }"
                        :disabled="authStore.user?.role !== 'Admin'"
                        @click="toggleCapability(cap, 'agent')"
                        :title="authStore.user?.role === 'Admin' ? 'Click để bật/tắt' : 'Chỉ Admin mới có quyền sửa'"
                      >
                        {{ cap.agent ? '✓' : '✗' }}
                      </button>
                    </td>
                    <td class="text-center">
                      <button 
                        type="button"
                        class="cap-toggle-btn"
                        :class="{ active: cap.viewer, disabled: authStore.user?.role !== 'Admin' }"
                        :disabled="authStore.user?.role !== 'Admin'"
                        @click="toggleCapability(cap, 'viewer')"
                        :title="authStore.user?.role === 'Admin' ? 'Click để bật/tắt' : 'Chỉ Admin mới có quyền sửa'"
                      >
                        {{ cap.viewer ? '✓' : '✗' }}
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- 5. SOCIAL CHANNELS SETTINGS PANEL -->
        <div v-if="activeTab === 'channels'" class="panel-section animate-fade">
          <div class="panel-header">
            <h3>Cấu hình Kênh đăng tải Social</h3>
            <p class="subtitle">Nhập thông tin ID Trang, Nhóm và các Access Token để tự động xuất bản bài viết mà không cần sửa code.</p>
          </div>

          <form @submit.prevent="handleSaveChannels" class="settings-form">
            <!-- Facebook Section -->
            <div class="channel-setting-section">
              <h4 class="section-title">👥 Facebook Configuration</h4>
              <div class="form-group">
                <label>ID Trang Facebook hoặc ID Trang cá nhân (Facebook Page/Profile ID)</label>
                <input v-model="channelsForm.facebookPageId" type="text" placeholder="Nhập ID Trang hoặc Profile ID (ví dụ: 1234567890)..." />
              </div>
              <div class="form-group">
                <label>Mã truy cập trang (Page Access Token - Để trống để chạy chế độ giả lập đăng bài)</label>
                <input v-model="channelsForm.facebookAccessToken" type="text" placeholder="Nhập EAAOTc0... (Nếu không nhập, hệ thống sẽ tự động đăng giả lập)" />
              </div>
              <div class="form-group">
                <label>ID nhóm Facebook đăng bài (cách nhau bởi dấu phẩy)</label>
                <input v-model="channelsForm.facebookGroupIds" type="text" placeholder="Nhập ID các nhóm Facebook (ví dụ: 123456789, 987654321)..." />
              </div>

              <!-- Fanpage Token Helper Accordion/Block -->
              <div class="fb-token-helper glass-card">
                <div class="helper-header" @click="showFbHelper = !showFbHelper">
                  <span class="helper-title">🔑 Trình hỗ trợ liên kết Fanpage tự động</span>
                  <span class="helper-toggle-icon">{{ showFbHelper ? '▼' : '▶' }}</span>
                </div>
                <div v-show="showFbHelper" class="helper-content animate-fade">
                  <p class="helper-desc">Dán User Access Token (từ Meta Graph Explorer) để tự động lấy Page ID và Page Access Token vĩnh viễn mà không cần thao tác thủ công.</p>
                  <div class="form-group">
                    <label>Mã truy cập người dùng (User Access Token)</label>
                    <div class="input-with-btn">
                      <input v-model="fbUserToken" type="text" placeholder="Nhập mã truy cập người dùng EAA..." />
                      <button type="button" class="btn-submit glow-yellow" :disabled="isFetchingFbPages" @click="handleFetchFbPages">
                        {{ isFetchingFbPages ? 'Đang tải...' : 'Lấy danh sách Trang' }}
                      </button>
                    </div>
                  </div>

                  <div v-if="fbPages.length > 0" class="fb-pages-list">
                    <label>Chọn Fanpage của bạn:</label>
                    <div class="pages-grid">
                      <div v-for="page in fbPages" :key="page.id" class="fb-page-item" @click="selectFbPage(page)">
                        <div class="page-info">
                          <span class="page-name">🚩 {{ page.name }}</span>
                          <span class="page-id">ID: {{ page.id }}</span>
                        </div>
                        <button type="button" class="select-btn">Chọn</button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>ID ứng dụng Meta (Meta App ID - Tùy chọn)</label>
                  <input v-model="channelsForm.metaAppId" type="text" placeholder="Nhập App ID..." />
                </div>
                <div class="form-group">
                  <label>Khóa bảo mật App (Meta App Secret - Tùy chọn)</label>
                  <input v-model="channelsForm.metaAppSecret" type="text" placeholder="Nhập App Secret..." />
                </div>
              </div>
            </div>

            <div class="section-divider"></div>

            <!-- Zalo Section -->
            <div class="channel-setting-section">
              <h4 class="section-title">💬 Zalo Configuration</h4>
              <div class="form-group">
                <label>ID Trang Zalo OA hoặc Số điện thoại cá nhân (Zalo Page/Profile ID)</label>
                <input v-model="channelsForm.zaloPageId" type="text" placeholder="Nhập ID Trang OA hoặc Số điện thoại Zalo cá nhân (ví dụ: 0987654321)..." />
              </div>
            </div>

            <button type="submit" class="save-btn glow-yellow" :disabled="isSavingChannels">
              {{ isSavingChannels ? 'Đang lưu...' : 'Lưu cấu hình kênh' }}
            </button>
          </form>
        </div>
      </div>
    </div>

    <!-- INVITE USER MODAL -->
    <div v-if="showAddUserModal" class="modal-overlay" @click.self="showAddUserModal = false">
      <div class="modal-content glass-card">
        <h3>Mời thành viên mới vào sàn</h3>

        <form class="modal-form" @submit.prevent="handleAddUser">
          <div class="form-group">
            <label>Họ và tên nhân sự</label>
            <input v-model="newUser.fullName" type="text" placeholder="Nhập tên nhân sự..." required />
          </div>

          <div class="form-group">
            <label>Địa chỉ Email</label>
            <input v-model="newUser.email" type="email" placeholder="email@realsync.vn" required />
          </div>

          <div class="form-group">
            <label>Phân quyền ban đầu</label>
            <select v-model="newUser.role">
              <option value="Manager">Manager (Quản lý)</option>
              <option value="Agent">Agent (Môi giới)</option>
              <option value="Viewer">Viewer (Chỉ xem)</option>
            </select>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="showAddUserModal = false">Hủy</button>
            <button type="submit" class="btn-submit glow-yellow">Gửi lời mời</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.settings-container {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 24px;
  align-items: start;
}

.settings-sidebar {
  display: flex;
  flex-direction: column;
  padding: 16px;
  gap: 8px;
}

.tab-link {
  height: 40px;
  padding: 0 16px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 12.5px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  transition: all var(--duration-fast);
  text-align: left;
}

.tab-link:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.tab-link.active {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  box-shadow: var(--color-yellow-glow);
}

.tab-link .icon {
  font-size: 15px;
}

.settings-content {
  padding: 24px;
  min-height: 520px;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--color-divider);
  padding-bottom: 16px;
}

.panel-header h3 {
  margin: 0 0 4px 0;
  font-size: 16px;
}

.panel-header .subtitle {
  margin: 0;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.settings-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
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

.form-group input,
.form-group select {
  height: 38px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

/* Range input styling */
.styled-range {
  -webkit-appearance: none;
  width: 100%;
  height: 6px;
  background: var(--color-divider);
  border-radius: 99px;
  outline: none;
  margin: auto 0;
}

.styled-range::-webkit-slider-thumb {
  -webkit-appearance: none;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--color-yellow);
  cursor: pointer;
  box-shadow: var(--color-yellow-glow);
}

/* Password eye button wrapper */
.input-password-wrapper {
  position: relative;
  display: flex;
}

.input-password-wrapper input {
  width: 100%;
  padding-right: 40px;
}

.pwd-toggle-btn {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 14px;
}

.section-divider {
  height: 1px;
  background-color: var(--color-divider);
  margin: 8px 0;
}

/* Toggle lists options */
.toggle-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.toggle-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 12px 16px;
  border-radius: 8px;
  cursor: pointer;
}

.toggle-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  max-width: 80%;
}

.toggle-info strong {
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.toggle-info span {
  font-size: 11.5px;
  color: var(--color-text-secondary);
}

/* Switch styling */
.switch {
  position: relative;
  display: inline-block;
  width: 34px;
  height: 20px;
  flex-shrink: 0;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: var(--color-divider);
  transition: .3s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 14px;
  width: 14px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .3s;
}

input:checked + .slider {
  background-color: var(--color-yellow);
}

input:checked + .slider:before {
  transform: translateX(14px);
}

.slider.round {
  border-radius: 34px;
}

.slider.round:before {
  border-radius: 50%;
}

/* Weights sliders */
.weights-section h4 {
  font-size: 13.5px;
  margin: 0 0 4px 0;
}

.weights-section .section-desc {
  font-size: 12px;
  color: var(--color-text-secondary);
  margin: 0 0 16px 0;
}

.weight-sliders {
  display: flex;
  flex-direction: column;
  gap: 12px;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 8px;
}

.weight-slider-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.slider-labels {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.total-weight-warning {
  font-size: 11px;
  color: var(--color-success);
  font-weight: 600;
  text-align: right;
}

.total-weight-warning.error {
  color: var(--color-danger);
}

.save-btn {
  height: 40px;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12.5px;
  font-weight: 700;
  border-radius: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
  margin-top: 8px;
}

.save-btn:hover {
  background-color: var(--color-yellow-hover);
}

.save-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Users List */
.add-user-btn {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 12px;
  font-weight: 600;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
}

.table-container {
  overflow-x: auto;
  border: 1px solid var(--color-border);
  background-color: var(--color-surface-glass);
  border-radius: 8px;
  margin-bottom: 24px;
}

.users-table,
.rbac-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12.5px;
}

.users-table th,
.rbac-table th {
  padding: 12px 16px;
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
}

.users-table td,
.rbac-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-divider);
  color: var(--color-text-secondary);
}

.user-name-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar-sm {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background-color: var(--color-divider);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 11px;
  color: var(--color-text-primary);
}

.role-select {
  height: 28px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 4px;
  font-size: 11.5px;
  padding: 0 4px;
  color: var(--color-text-primary);
}

.status-badge {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
  white-space: nowrap;
  display: inline-block;
}

.status-badge.active { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }
.status-badge.invited { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.status-badge.locked { background-color: rgba(239, 68, 68, 0.15); color: #ef4444; }

.lock-toggle-btn {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
}

.lock-toggle-btn:hover {
  background-color: var(--color-surface-hover);
}

.lock-toggle-btn.locked {
  background-color: rgba(239, 68, 68, 0.15);
  border-color: rgba(239, 68, 68, 0.3);
  color: #ef4444;
}

/* RBAC Table styling */
.rbac-section h4 {
  font-size: 13.5px;
  margin: 0 0 12px 0;
}

.rbac-table th.text-center,
.rbac-table td.text-center {
  text-align: center;
}

.cap-toggle-btn {
  background: transparent;
  border: none;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-danger);
  padding: 4px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.cap-toggle-btn.active {
  color: var(--color-success);
}

.cap-toggle-btn:not(.disabled):hover {
  background-color: var(--color-surface-hover);
  transform: scale(1.15);
}

.cap-toggle-btn.disabled {
  cursor: not-allowed;
}

.module-name {
  font-weight: 600;
  color: var(--color-text-primary);
}

/* Modals */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
}

.modal-content {
  width: 400px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: var(--elevation-floating);
}

.modal-content h3 {
  margin: 0;
  font-size: 15px;
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

.btn-cancel {
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: transparent;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.btn-submit {
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
  border: none;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.channel-setting-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-title {
  margin: 0 0 8px 0;
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-primary);
}

/* FB Token Helper Styles */
.fb-token-helper {
  margin-top: 4px;
  border: 1px dashed rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.015);
  overflow: hidden;
}

.helper-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  cursor: pointer;
  user-select: none;
  background: rgba(255, 255, 255, 0.02);
  transition: background 0.2s;
}

.helper-header:hover {
  background: rgba(255, 255, 255, 0.04);
}

.helper-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-yellow);
}

.helper-toggle-icon {
  font-size: 10px;
  color: var(--color-text-secondary);
}

.helper-content {
  padding: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
}

.helper-desc {
  font-size: 11px;
  color: var(--color-text-secondary);
  margin: 0 0 12px 0;
  line-height: 1.4;
}

.input-with-btn {
  display: flex;
  gap: 10px;
  align-items: center;
}

.input-with-btn input {
  flex: 1;
}

.fb-pages-list {
  margin-top: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding-top: 12px;
}

.fb-pages-list > label {
  display: block;
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: 8px;
}

.pages-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 180px;
  overflow-y: auto;
  padding-right: 4px;
}

.fb-page-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.fb-page-item:hover {
  background: rgba(255, 255, 255, 0.06);
  border-color: var(--color-yellow);
}

.fb-page-item .page-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.fb-page-item .page-name {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.fb-page-item .page-id {
  font-size: 10px;
  color: var(--color-text-secondary);
}

.fb-page-item .select-btn {
  height: 28px;
  padding: 0 12px;
  border-radius: 6px;
  border: none;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 10px;
  font-weight: 600;
  cursor: pointer;
}
</style>
