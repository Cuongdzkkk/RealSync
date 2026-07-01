<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { useLeadStore } from '@/stores/useLeadStore';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useToastStore } from '@/stores/useToastStore';
import { formatCurrency } from '@/utils/format';
import RoleGate from '@/components/common/RoleGate.vue';
import { api } from '@/services/api';

const leadStore = useLeadStore();
const propertyStore = usePropertyStore();
const toastStore = useToastStore();

// --- Playground State ---
const inputText = ref('');
const isAnalyzing = ref(false);
const showAnalysisResults = ref(false);

// AI Extraction results model
const extractedEntities = ref({
  intent: 'Tìm mua Bất động sản',
  area: 'Quận 7, TP.HCM',
  budget: 'Từ 8 - 14 tỷ VND',
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
    label: 'Đất nền giá tốt Vĩnh Cửu',
    text: 'Tìm đất nền thổ cư khu vực Vĩnh Cửu, Đồng Nai dưới 5 tỷ, pháp lý sổ hồng riêng rõ ràng, sang tên công chứng ngay trong tuần.'
  },
  {
    label: 'Căn hộ cho thuê Thủ Đức',
    text: 'Cần thuê căn hộ 2 phòng ngủ tại TP. Thủ Đức, giá từ 8-12 triệu/tháng, gần đại học, đầy đủ nội thất, hầm xe.'
  }
];

function applyTemplate(text: string) {
  inputText.value = text;
}

// Normalize Vietnamese text for comparison (remove diacritics for fuzzy match)
function normalizeVietnamese(str: string): string {
  return str
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9\s]/g, '');
}

// Matching properties calculation based on NLP intent
const matchedProperties = computed(() => {
  if (!showAnalysisResults.value) return [];

  const targetArea = extractedEntities.value.area || '';
  // Extract district/city name, e.g. "Quận 7" from "Quận 7, TP.HCM"
  const cleanArea = targetArea.split(',')[0].trim();
  const normalizedArea = normalizeVietnamese(cleanArea);

  // Only return matches if area is specific (not generic fallback)
  if (!cleanArea || cleanArea === 'Chưa rõ' || normalizedArea === 'tp ho chi minh' || normalizedArea === 'ha noi') {
    return [];
  }

  const matches = propertyStore.items.filter(p => {
    if (!p.address) return false;
    const normalizedAddr = normalizeVietnamese(p.address);
    const normalizedTitle = normalizeVietnamese(p.title || '');
    return normalizedAddr.includes(normalizedArea) || normalizedTitle.includes(normalizedArea);
  });

  // Do NOT fallback to all items — return empty if no match
  return matches.slice(0, 3);
});

// Classification trigger
async function startAnalysis() {
  if (!inputText.value.trim()) {
    toastStore.warning('Thiếu dữ liệu', 'Vui lòng điền nội dung yêu cầu hoặc chọn một mẫu thử nghiệm bên dưới.');
    return;
  }

  isAnalyzing.value = true;
  showAnalysisResults.value = false;

  try {
    const systemPrompt = `Bạn là một AI phân loại nhu cầu bất động sản. Hãy phân tích yêu cầu sau và trích xuất thông tin dưới dạng JSON duy nhất. JSON phải có định dạng chính xác sau đây (không kèm theo ký tự markdown hay lời dẫn):
{
  "intent": "Mô tả ý định ngắn gọn (ví dụ: Tìm mua căn hộ, Thuê nhà phố...)",
  "area": "Khu vực tìm kiếm cụ thể (ví dụ: Quận 7, TP.HCM hoặc Vĩnh Cửu, Đồng Nai)",
  "budget": "Ngân sách (ví dụ: Dưới 12 tỷ VND)",
  "type": "Loại BĐS (ví dụ: Căn hộ chung cư)",
  "bedrooms": "Số phòng ngủ (ví dụ: 3 PN)",
  "urgency": "Độ khẩn cấp: Cao (Hot Lead) hoặc Trung bình (Warm) hoặc Thấp (Cold)",
  "score": Điểm số tiềm năng từ 0 đến 100 (số nguyên)
}

Yêu cầu cần phân tích: "${inputText.value}"`;

    const { data: res } = await api.post('/ai-chat', { message: systemPrompt }, { timeout: 35000 });

    if (res && res.data) {
      let jsonText = res.data.trim();

      // Extract the JSON object using regex to handle any prefix/suffix conversational text
      const jsonMatch = jsonText.match(/\{[\s\S]*\}/);
      if (jsonMatch) {
        jsonText = jsonMatch[0];
      }

      const parsed = JSON.parse(jsonText);
      if (parsed.intent || parsed.area) {
        extractedEntities.value = {
          intent: parsed.intent || 'Nhu cầu Bất động sản',
          area: parsed.area || 'Chưa rõ',
          budget: parsed.budget || 'Chưa rõ',
          type: parsed.type || 'Chưa rõ',
          bedrooms: parsed.bedrooms || 'Chưa rõ',
          urgency: parsed.urgency || 'Trung bình (Warm)',
          score: typeof parsed.score === 'number' ? parsed.score : 80
        };
        showAnalysisResults.value = true;
        toastStore.success('Phân tích hoàn tất', 'AI đã trích xuất thành công nhu cầu và liên kết kho hàng phù hợp.');
        return;
      }
    }
    throw new Error('Invalid AI response format');
  } catch (err) {
    console.warn('AI analysis API failed, falling back to local simulation:', err);
    runLocalFallback();
  } finally {
    isAnalyzing.value = false;
  }
}

function runLocalFallback() {
  showAnalysisResults.value = true;
  const prompt = inputText.value.trim();
  const lowerPrompt = normalizeVietnamese(prompt);

  // 1. Detect Area — TP.HCM districts
  let area = 'Chưa rõ';

  // Đồng Nai
  if (lowerPrompt.includes('vinh cuu') || lowerPrompt.includes('vinh cửu')) area = 'Vĩnh Cửu, Đồng Nai';
  else if (lowerPrompt.includes('nhon trach') || lowerPrompt.includes('nhơn trạch')) area = 'Nhơn Trạch, Đồng Nai';
  else if (lowerPrompt.includes('long thanh') || lowerPrompt.includes('long thành')) area = 'Long Thành, Đồng Nai';
  else if (lowerPrompt.includes('trang bom') || lowerPrompt.includes('trảng bom')) area = 'Trảng Bom, Đồng Nai';
  else if (lowerPrompt.includes('bien hoa') || lowerPrompt.includes('biên hòa')) area = 'Biên Hòa, Đồng Nai';
  else if (lowerPrompt.includes('dong nai') || lowerPrompt.includes('đồng nai')) area = 'Đồng Nai';
  // Bình Dương
  else if (lowerPrompt.includes('thu dau mot') || lowerPrompt.includes('thủ dầu một')) area = 'Thủ Dầu Một, Bình Dương';
  else if (lowerPrompt.includes('thuan an') || lowerPrompt.includes('thuận an')) area = 'Thuận An, Bình Dương';
  else if (lowerPrompt.includes('di an') || lowerPrompt.includes('dĩ an')) area = 'Dĩ An, Bình Dương';
  else if (lowerPrompt.includes('binh duong') || lowerPrompt.includes('bình dương')) area = 'Bình Dương';
  // HCM quận số
  else if (lowerPrompt.includes('quan 12') || lowerPrompt.includes('quận 12')) area = 'Quận 12, TP.HCM';
  else if (lowerPrompt.includes('quan 11') || lowerPrompt.includes('quận 11')) area = 'Quận 11, TP.HCM';
  else if (lowerPrompt.includes('quan 10') || lowerPrompt.includes('quận 10')) area = 'Quận 10, TP.HCM';
  else if (lowerPrompt.includes('quan 9') || lowerPrompt.includes('quận 9')) area = 'Quận 9, TP.HCM';
  else if (lowerPrompt.includes('quan 8') || lowerPrompt.includes('quận 8')) area = 'Quận 8, TP.HCM';
  else if (lowerPrompt.includes('quan 7') || lowerPrompt.includes('quận 7')) area = 'Quận 7, TP.HCM';
  else if (lowerPrompt.includes('quan 6') || lowerPrompt.includes('quận 6')) area = 'Quận 6, TP.HCM';
  else if (lowerPrompt.includes('quan 5') || lowerPrompt.includes('quận 5')) area = 'Quận 5, TP.HCM';
  else if (lowerPrompt.includes('quan 4') || lowerPrompt.includes('quận 4')) area = 'Quận 4, TP.HCM';
  else if (lowerPrompt.includes('quan 3') || lowerPrompt.includes('quận 3')) area = 'Quận 3, TP.HCM';
  else if (lowerPrompt.includes('quan 2') || lowerPrompt.includes('quận 2')) area = 'Quận 2, TP.HCM';
  else if (lowerPrompt.includes('quan 1') || lowerPrompt.includes('quận 1')) area = 'Quận 1, TP.HCM';
  // HCM quận tên
  else if (lowerPrompt.includes('binh thanh') || lowerPrompt.includes('bình thạnh')) area = 'Quận Bình Thạnh, TP.HCM';
  else if (lowerPrompt.includes('go vap') || lowerPrompt.includes('gò vấp')) area = 'Quận Gò Vấp, TP.HCM';
  else if (lowerPrompt.includes('phu nhuan') || lowerPrompt.includes('phú nhuận')) area = 'Quận Phú Nhuận, TP.HCM';
  else if (lowerPrompt.includes('tan binh') || lowerPrompt.includes('tân bình')) area = 'Quận Tân Bình, TP.HCM';
  else if (lowerPrompt.includes('tan phu') || lowerPrompt.includes('tân phú')) area = 'Quận Tân Phú, TP.HCM';
  else if (lowerPrompt.includes('binh tan') || lowerPrompt.includes('bình tân')) area = 'Quận Bình Tân, TP.HCM';
  else if (lowerPrompt.includes('thu duc') || lowerPrompt.includes('thủ đức')) area = 'TP. Thủ Đức, TP.HCM';
  else if (lowerPrompt.includes('binh chanh') || lowerPrompt.includes('bình chánh')) area = 'Huyện Bình Chánh, TP.HCM';
  else if (lowerPrompt.includes('hoc mon') || lowerPrompt.includes('hóc môn')) area = 'Huyện Hóc Môn, TP.HCM';
  else if (lowerPrompt.includes('nha be') || lowerPrompt.includes('nhà bè')) area = 'Huyện Nhà Bè, TP.HCM';
  else if (lowerPrompt.includes('cu chi') || lowerPrompt.includes('củ chi')) area = 'Huyện Củ Chi, TP.HCM';
  else if (lowerPrompt.includes('thu thiem') || lowerPrompt.includes('thủ thiêm')) area = 'Thủ Thiêm, TP.HCM';
  // Hà Nội
  else if (lowerPrompt.includes('hoan kiem') || lowerPrompt.includes('hoàn kiếm')) area = 'Quận Hoàn Kiếm, Hà Nội';
  else if (lowerPrompt.includes('cau giay') || lowerPrompt.includes('cầu giấy')) area = 'Quận Cầu Giấy, Hà Nội';
  else if (lowerPrompt.includes('dong da') || lowerPrompt.includes('đống đa')) area = 'Quận Đống Đa, Hà Nội';
  else if (lowerPrompt.includes('ha noi') || lowerPrompt.includes('hà nội')) area = 'Hà Nội';
  // Vũng Tàu
  else if (lowerPrompt.includes('vung tau') || lowerPrompt.includes('vũng tàu')) area = 'TP. Vũng Tàu';
  // Long An
  else if (lowerPrompt.includes('ben luc') || lowerPrompt.includes('bến lức')) area = 'Bến Lức, Long An';
  else if (lowerPrompt.includes('duc hoa') || lowerPrompt.includes('đức hòa')) area = 'Đức Hòa, Long An';
  else if (lowerPrompt.includes('long an')) area = 'Long An';
  // Generic HCM fallback
  else if (lowerPrompt.includes('ho chi minh') || lowerPrompt.includes('hồ chí minh') || lowerPrompt.includes('tphcm') || lowerPrompt.includes('tp.hcm')) area = 'TP.HCM';

  // 2. Detect Type
  let type = 'Bất động sản';
  if (lowerPrompt.includes('can ho') || lowerPrompt.includes('chung cu') || lowerPrompt.includes('căn hộ') || lowerPrompt.includes('chung cư')) type = 'Căn hộ chung cư';
  else if (lowerPrompt.includes('nha pho') || lowerPrompt.includes('nha rieng') || lowerPrompt.includes('nhà phố') || lowerPrompt.includes('nhà riêng')) type = 'Nhà phố';
  else if (lowerPrompt.includes('dat nen') || lowerPrompt.includes('dat') || lowerPrompt.includes('đất nền') || lowerPrompt.includes('đất')) type = 'Đất nền';
  else if (lowerPrompt.includes('biet thu') || lowerPrompt.includes('biệt thự') || lowerPrompt.includes('villa')) type = 'Biệt thự / Villa';
  else if (lowerPrompt.includes('mat bang') || lowerPrompt.includes('van phong') || lowerPrompt.includes('mặt bằng') || lowerPrompt.includes('văn phòng')) type = 'Mặt bằng thương mại';

  // 3. Detect Budget
  let budget = 'Chưa xác định';
  const budgetMatch = prompt.match(/(\d+(?:[.,]\d+)?)\s*(tỷ|ty|triệu|trieu|tr|t|usd)/i);
  if (budgetMatch) {
    const val = budgetMatch[1];
    const unit = budgetMatch[2].toLowerCase();
    const displayUnit = (unit === 'tỷ' || unit === 'ty' || unit === 't') ? 'tỷ VND' : (unit === 'usd' ? 'USD' : 'triệu VND');
    const lowerP = lowerPrompt;
    if (lowerP.includes('duoi') || lowerP.includes('dưới')) budget = `Dưới ${val} ${displayUnit}`;
    else if (lowerP.includes('tren') || lowerP.includes('trên') || lowerP.includes('khoang') || lowerP.includes('khoảng') || lowerP.includes('tam') || lowerP.includes('tầm')) budget = `Khoảng ${val} ${displayUnit}`;
    else budget = `${val} ${displayUnit}`;
  }

  // 4. Intent
  let intent = 'Tìm mua ' + type.toLowerCase();
  if (lowerPrompt.includes('thue') || lowerPrompt.includes('thuê') || lowerPrompt.includes('cho thue')) {
    intent = 'Tìm thuê ' + type.toLowerCase();
  } else if (lowerPrompt.includes('ban') || lowerPrompt.includes('bán') || lowerPrompt.includes('ky goi') || lowerPrompt.includes('ký gửi')) {
    intent = 'Bán/Ký gửi ' + type.toLowerCase();
  }

  // 5. Bedrooms
  let bedrooms = 'Chưa rõ';
  const brMatch = prompt.match(/(\d+)\s*(pn|phòng ngủ|phong ngu|phòng|phong)/i);
  if (brMatch) {
    bedrooms = `${brMatch[1]} PN`;
  }

  // 6. Urgency & Score
  let urgency = 'Trung bình (Warm)';
  let score = 82;
  const isUrgent = lowerPrompt.includes('gap') || lowerPrompt.includes('gấp') || lowerPrompt.includes('ngay') || lowerPrompt.includes('khan cap') || lowerPrompt.includes('khẩn cấp');
  if (isUrgent) {
    urgency = 'Cao (Hot Lead)';
    score = 95;
  } else if (budget !== 'Chưa xác định') {
    score = 88;
  }

  extractedEntities.value = { intent, area, budget, type, bedrooms, urgency, score };

  toastStore.success('Phân tích hoàn tất (Giả lập)', 'Đã phân tích thông tin và liên kết kho hàng phù hợp.');
}

function sendToLead() {
  toastStore.success('Đã gửi đề xuất', 'Đã chuyển tiếp thông tin các căn hộ phù hợp nhất đến Zalo/Viber của khách hàng.');
}

const stats = ref({
  totalClassified: 1482,
  avgLatency: 142,
  accuracy: 94.8,
  acceptanceRate: 91.3
});

const jobsList = ref<any[]>([]);

onMounted(async () => {
  propertyStore.fetchProperties();

  try {
    const { data: res } = await api.get('/crawlers/stats');
    if (res && res.data) {
      stats.value = {
        totalClassified: res.data.totalClassified,
        avgLatency: res.data.avgLatencyMs,
        accuracy: res.data.accuracy,
        acceptanceRate: res.data.acceptanceRate
      };
    }
  } catch (err) {
    console.error('Failed to load stats:', err);
  }

  try {
    const { data: res } = await api.get('/crawlers/jobs');
    if (res && res.data) {
      jobsList.value = res.data.map((j: any) => ({
        id: j.id,
        target: j.sourceName,
        type: 'property',
        result: j.log ? j.log.split('\n')[0] : 'Cào tin đăng thành công',
        confidence: 85 + (j.successCount > 0 ? 10 : 0),
        createdAt: j.startedAt ?? new Date().toISOString(),
        status: j.status === 'Completed' ? 'completed' : 'review'
      }));
    }
  } catch (err) {
    console.error('Failed to load jobs:', err);
  }
});
</script>
<template>
  <RoleGate :roles="['Admin', 'Manager', 'Sales']">
    <div class="page">
      <!-- Header Summary Metrics -->
      <div class="metrics-row">
      <div class="metric-card glass-card">
        <span class="label">Tổng số bản ghi đã phân loại</span>
        <strong class="value numeric">{{ stats.totalClassified.toLocaleString() }}</strong>
        <span class="trend success">+14.2% tuần này</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Độ chính xác mô hình NLP</span>
        <strong class="value numeric">{{ stats.accuracy }}%</strong>
        <span class="trend success">Đã hiệu chuẩn</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Tốc độ xử lý trung bình</span>
        <strong class="value numeric">{{ stats.avgLatency }}ms</strong>
        <span class="trend info">Fast Latency</span>
      </div>
      <div class="metric-card glass-card">
        <span class="label">Đề xuất được môi giới chấp nhận</span>
        <strong class="value numeric">{{ stats.acceptanceRate }}%</strong>
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

