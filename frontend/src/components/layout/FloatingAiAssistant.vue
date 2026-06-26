<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue';

interface Message {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  timestamp: string;
  isStreaming?: boolean;
}

const isOpen = ref(false);
const isVisible = ref(true);
const isDragging = ref(false);
const didDrag = ref(false);
const position = ref({ x: 0, y: 0 });
const dragOffset = ref({ x: 0, y: 0 });
const dragStart = ref({ x: 0, y: 0 });
const inputMsg = ref('');
const messages = ref<Message[]>([
  {
    id: '1',
    sender: 'ai',
    text: 'Xin chào! Tôi là **RealSync AI Copilot**. Tôi có thể giúp bạn chấm điểm lead, soạn tin đăng BĐS, phân tích thị trường hoặc gợi ý chăm sóc khách hàng.',
    timestamp: 'Vừa xong'
  }
]);
const isResponding = ref(false);
const chatListRef = ref<HTMLElement | null>(null);

const quickActions = [
  'Chấm điểm lead mới',
  'Soạn tin đăng Quận 2',
  'Gợi ý gọi điện cho khách',
  'Phân tích nguồn lead'
];

const wrapperStyle = computed(() => ({
  left: `${position.value.x}px`,
  top: `${position.value.y}px`
}));

const chatAlignmentClass = computed(() => ({
  'ai-chat-window--align-left': position.value.x < 360
}));

onMounted(() => {
  position.value = {
    x: window.innerWidth - 84,
    y: window.innerHeight - 84
  };
});

function toggleOpen() {
  if (didDrag.value) {
    didDrag.value = false;
    return;
  }

  isOpen.value = !isOpen.value;
  if (isOpen.value) {
    scrollToBottom();
  }
}

function toggleVisible() {
  if (didDrag.value) {
    didDrag.value = false;
    return;
  }

  isVisible.value = !isVisible.value;
  if (!isVisible.value) {
    isOpen.value = false;
  }
}

function clampPosition(x: number, y: number) {
  const edgePadding = 8;
  const maxX = window.innerWidth - 60;
  const maxY = window.innerHeight - 60;

  return {
    x: Math.min(Math.max(x, edgePadding), maxX),
    y: Math.min(Math.max(y, edgePadding), maxY)
  };
}

function startDrag(event: PointerEvent) {
  if (event.button !== 0) return;

  const target = event.target as HTMLElement;
  if (target.closest('input, form, .chat-messages, .quick-action-btn, .send-btn, .close-btn, .ai-visibility-btn')) {
    return;
  }

  isDragging.value = true;
  didDrag.value = false;
  dragOffset.value = {
    x: event.clientX - position.value.x,
    y: event.clientY - position.value.y
  };
  dragStart.value = {
    x: event.clientX,
    y: event.clientY
  };

  window.addEventListener('pointermove', handleDrag);
  window.addEventListener('pointerup', stopDrag, { once: true });
}

function handleDrag(event: PointerEvent) {
  if (!isDragging.value) return;

  const distance = Math.hypot(event.clientX - dragStart.value.x, event.clientY - dragStart.value.y);
  if (distance < 4) return;

  didDrag.value = true;
  position.value = clampPosition(
    event.clientX - dragOffset.value.x,
    event.clientY - dragOffset.value.y
  );
}

function stopDrag() {
  isDragging.value = false;
  window.removeEventListener('pointermove', handleDrag);
}

function scrollToBottom() {
  nextTick(() => {
    if (chatListRef.value) {
      chatListRef.value.scrollTop = chatListRef.value.scrollHeight;
    }
  });
}

function handleQuickAction(action: string) {
  inputMsg.value = action;
  sendMessage();
}

function sendMessage() {
  if (!inputMsg.value.trim() || isResponding.value) return;

  const userText = inputMsg.value;
  messages.value.push({
    id: Date.now().toString(),
    sender: 'user',
    text: userText,
    timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  });

  inputMsg.value = '';
  isResponding.value = true;
  scrollToBottom();

  // Simulate AI response streaming
  setTimeout(() => {
    const aiMessageId = (Date.now() + 1).toString();
    const fullAiResponse = getAIResponse(userText);
    
    messages.value.push({
      id: aiMessageId,
      sender: 'ai',
      text: '',
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isStreaming: true
    });
    
    scrollToBottom();

    let currentText = '';
    let wordIndex = 0;
    const words = fullAiResponse.split(' ');
    
    const streamInterval = setInterval(() => {
      if (wordIndex < words.length) {
        currentText += (wordIndex === 0 ? '' : ' ') + words[wordIndex];
        const msgIndex = messages.value.findIndex(m => m.id === aiMessageId);
        if (msgIndex !== -1) {
          messages.value[msgIndex].text = currentText;
        }
        wordIndex++;
        scrollToBottom();
      } else {
        clearInterval(streamInterval);
        const msgIndex = messages.value.findIndex(m => m.id === aiMessageId);
        if (msgIndex !== -1) {
          messages.value[msgIndex].isStreaming = false;
        }
        isResponding.value = false;
      }
    }, 80);
  }, 1000);
}

function getAIResponse(query: string): string {
  const lowercase = query.toLowerCase();
  if (lowercase.includes('chấm điểm') || lowercase.includes('score') || lowercase.includes('lead')) {
    return 'Hệ thống AI vừa chấm điểm khách hàng **Nguyễn Văn A** đạt **92/100 điểm**. Điểm số dựa trên hành vi điền form nhanh, tương tác 3 lần trên Zalo và tìm kiếm phân khúc **Căn hộ Quận 2**. Khuyến nghị: Liên hệ ngay trong hôm nay.';
  }
  if (lowercase.includes('soạn') || lowercase.includes('tin đăng') || lowercase.includes('content')) {
    return 'Dưới đây là tin đăng mẫu do tôi soạn thảo:\n\n**[CĂN HỘ CAO CẤP QUẬN 2 - GIÁ CHỈ 4.5 TỶ]**\n- Vị trí vàng trung tâm Thạnh Mỹ Lợi.\n- Diện tích 75m², 2 phòng ngủ rộng rãi, ban công view sông Sài Gòn thoáng mát.\n- Nội thất bàn giao cao cấp, liền kề tuyến Metro.\n📞 Liên hệ ngay hôm nay để nhận thông tin chiết khấu!';
  }
  if (lowercase.includes('gợi ý') || lowercase.includes('chăm sóc')) {
    return 'Gợi ý hành động tiếp theo cho khách hàng **Trần Thị B**:\n1. Gửi tài liệu phân tích mặt bằng căn hộ qua Zalo.\n2. Lên lịch hẹn xem nhà vào lúc 9:00 sáng Thứ Bảy tuần này.\n3. Cập nhật ghi chú trên CRM.';
  }
  return 'Tôi đã ghi nhận yêu cầu phân tích dữ liệu. AI đang liên kết nguồn tin từ crawler và lịch sử tương tác khách hàng của bạn để chuẩn bị câu trả lời tối ưu nhất.';
}
</script>

<template>
  <div
    class="floating-ai-wrapper"
    :class="{ 'is-hidden': !isVisible, 'is-dragging': isDragging }"
    :style="wrapperStyle"
    @pointerdown="startDrag"
  >
    <button
      v-if="!isVisible"
      class="ai-restore-btn"
      type="button"
      title="Hiện AI Copilot"
      aria-label="Hiện AI Copilot"
      @click="toggleVisible"
    >
      AI
    </button>

    <template v-else>
    <button
      class="ai-visibility-btn"
      type="button"
      title="Ẩn AI Copilot"
      aria-label="Ẩn AI Copilot"
      @click="toggleVisible"
    >
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M18 6 6 18" />
        <path d="m6 6 12 12" />
      </svg>
    </button>

    <!-- Trigger Button -->
    <button 
      class="ai-trigger-btn glow-ai" 
      :class="{ 'is-open': isOpen }" 
      @click="toggleOpen"
      aria-label="RealSync AI Copilot"
    >
      <svg v-if="!isOpen" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
      </svg>
      <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18"/>
        <line x1="6" y1="6" x2="18" y2="18"/>
      </svg>
      <span class="ai-pulse-dot"></span>
    </button>

    <!-- AI Chat Window -->
    <div v-if="isOpen" class="ai-chat-window glass-card" :class="chatAlignmentClass">
      <div class="chat-header" title="Kéo để di chuyển">
        <div class="header-info">
          <div class="ai-avatar">AI</div>
          <div>
            <div class="chat-title">RealSync AI Copilot</div>
            <div class="chat-status">
              <span class="status-dot"></span>
              Đang hoạt động (v2.4)
            </div>
          </div>
        </div>
        <button class="close-btn" @click="toggleOpen">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18"/>
            <line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
      </div>

      <!-- Messages Log -->
      <div class="chat-messages" ref="chatListRef">
        <div 
          v-for="msg in messages" 
          :key="msg.id" 
          class="chat-bubble-wrapper"
          :class="`chat-bubble-wrapper--${msg.sender}`"
        >
          <div class="chat-bubble" :class="{ 'ai-streaming-cursor': msg.isStreaming }">
            <p v-html="msg.text.replace(/\n/g, '<br>').replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')"></p>
          </div>
          <span class="bubble-time">{{ msg.timestamp }}</span>
        </div>
      </div>

      <!-- Quick Action Prompts -->
      <div v-if="messages.length === 1 && !isResponding" class="quick-actions-list">
        <button 
          v-for="action in quickActions" 
          :key="action"
          class="quick-action-btn"
          @click="handleQuickAction(action)"
        >
          {{ action }}
        </button>
      </div>

      <!-- Chat Input -->
      <form class="chat-input-form" @submit.prevent="sendMessage">
        <input 
          v-model="inputMsg" 
          type="text" 
          placeholder="Hỏi AI Copilot của RealSync..." 
          :disabled="isResponding"
        />
        <button type="submit" class="send-btn" :disabled="!inputMsg.trim() || isResponding">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="22" y1="2" x2="11" y2="13"/>
            <polygon points="22 2 15 22 11 13 2 9 22 2"/>
          </svg>
        </button>
      </form>
    </div>
    </template>
  </div>
</template>

<style scoped>
.floating-ai-wrapper {
  position: fixed;
  z-index: var(--z-toast);
  touch-action: none;
  user-select: none;
}

.floating-ai-wrapper.is-hidden {
  right: auto;
}

.floating-ai-wrapper.is-dragging {
  cursor: grabbing;
}

.ai-restore-btn,
.ai-visibility-btn {
  align-items: center;
  border: 1px solid var(--color-border);
  box-shadow: var(--elevation-surface);
  cursor: pointer;
  display: flex;
  justify-content: center;
  transition: all var(--duration-fast) var(--ease-standard);
}

.ai-restore-btn {
  background: var(--color-text-primary);
  border-radius: 10px 0 0 10px;
  color: var(--color-canvas);
  font-size: 11px;
  font-weight: 800;
  height: 42px;
  letter-spacing: 0;
  width: 42px;
}

.ai-restore-btn:hover {
  transform: translateX(-4px);
}

.ai-visibility-btn {
  background: var(--color-surface);
  border-radius: 50%;
  color: var(--color-text-muted);
  height: 26px;
  position: absolute;
  right: -8px;
  top: -10px;
  width: 26px;
  z-index: 2;
}

.ai-visibility-btn:hover {
  color: var(--color-text-primary);
  transform: scale(1.06);
}

.ai-trigger-btn {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  border: 1px solid var(--color-border);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  transition: all var(--duration-base) var(--ease-spring);
}

.ai-trigger-btn,
.chat-header {
  cursor: grab;
}

.floating-ai-wrapper.is-dragging .ai-trigger-btn,
.floating-ai-wrapper.is-dragging .chat-header {
  cursor: grabbing;
}

.ai-trigger-btn:hover {
  transform: scale(1.08);
}

.ai-trigger-btn.is-open {
  background-color: var(--color-surface);
  color: var(--color-text-primary);
}

.ai-pulse-dot {
  position: absolute;
  top: 0;
  right: 0;
  width: 12px;
  height: 12px;
  background-color: var(--color-ai);
  border: 2px solid var(--color-canvas);
  border-radius: 50%;
  animation: pulse-ring 2s infinite;
}

/* AI Chat Window */
.ai-chat-window {
  position: absolute;
  bottom: 64px;
  right: 0;
  width: 360px;
  height: 480px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: var(--elevation-floating);
  animation: fadein var(--duration-base) var(--ease-spring);
}

.ai-chat-window--align-left {
  left: 0;
  right: auto;
}

.chat-header {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(255, 255, 255, 0.05);
}

.header-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.ai-avatar {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  background: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  color: var(--color-ai);
  font-weight: 700;
  font-size: 11px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chat-title {
  font-size: 12.5px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.chat-status {
  font-size: 10px;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 4px;
}

.status-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background-color: var(--color-success);
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
}

.close-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.chat-bubble-wrapper {
  display: flex;
  flex-direction: column;
  max-width: 80%;
}

.chat-bubble-wrapper--user {
  align-self: flex-end;
}

.chat-bubble-wrapper--ai {
  align-self: flex-start;
}

.chat-bubble {
  padding: 10px 12px;
  border-radius: 10px;
  font-size: 12px;
  line-height: 1.4;
}

.chat-bubble-wrapper--user .chat-bubble {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border-bottom-right-radius: 2px;
}

.chat-bubble-wrapper--ai .chat-bubble {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
  border-bottom-left-radius: 2px;
}

.chat-bubble p {
  margin: 0;
}

.bubble-time {
  font-size: 9px;
  color: var(--color-text-muted);
  margin-top: 4px;
  align-self: flex-end;
}

.chat-bubble-wrapper--ai .bubble-time {
  align-self: flex-start;
}

/* Quick Actions */
.quick-actions-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: 8px 16px;
  border-top: 1px solid var(--color-divider);
}

.quick-action-btn {
  font-size: 11px;
  background-color: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  padding: 4px 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.quick-action-btn:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border-strong);
  color: var(--color-text-primary);
}

/* Chat Input Form */
.chat-input-form {
  padding: 12px;
  border-top: 1px solid var(--color-border);
  display: flex;
  gap: 8px;
  background: rgba(255, 255, 255, 0.03);
}

.chat-input-form input {
  flex: 1;
  border: 1px solid var(--color-border);
  background: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  height: 36px;
  font-size: 12px;
  transition: all var(--duration-fast);
}

.chat-input-form input:focus {
  border-color: var(--color-border-strong);
  outline: none;
}

.send-btn {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: none;
  background-color: var(--color-text-primary);
  color: var(--color-canvas);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.send-btn:hover:not(:disabled) {
  transform: scale(1.04);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
