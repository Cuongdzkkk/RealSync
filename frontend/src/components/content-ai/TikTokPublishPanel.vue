<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useConnectedAccountStore } from '@/stores/useConnectedAccountStore';
import { usePublicationStore } from '@/stores/usePublicationStore';
import { useToastStore } from '@/stores/useToastStore';
import { connectedAccountService } from '@/services/connectedAccountService';
import type { TikTokCreatorInfo, TikTokMediaManifest, ChannelCapabilities } from '@/types/tiktok';

const props = defineProps<{
  postId: string;
  caption: string;
  videoUrl?: string;
  isAigc?: boolean;
}>();

const connectedAccountStore = useConnectedAccountStore();
const publicationStore = usePublicationStore();
const toastStore = useToastStore();

const selectedAccountId = ref('');
const privacyLevel = ref('');
const userConsent = ref(false);
const publishMode = ref<'Direct' | 'DraftUpload'>('DraftUpload');
const creatorInfo = ref<TikTokCreatorInfo | null>(null);
const capabilities = ref<ChannelCapabilities | null>(null);
const loadingCreator = ref(false);
const publishing = ref(false);

const tiktokAccounts = computed(() =>
  connectedAccountStore.accounts.filter(a => a.provider === 'TikTok' && a.status === 'Active')
);

const privacyLabels: Record<string, string> = {
  PUBLIC_TO_EVERYONE: 'Công khai (Mọi người)',
  MUTUAL_FOLLOW_FRIENDS: 'Bạn bè qua lại',
  FOLLOWER_OF_CREATOR: 'Người theo dõi',
  SELF_ONLY: 'Chỉ mình tôi (Riêng tư)'
};

onMounted(async () => {
  await connectedAccountStore.fetchAccounts();
  if (tiktokAccounts.value.length > 0) {
    selectedAccountId.value = tiktokAccounts.value[0].id;
  }
});

watch(selectedAccountId, async (id) => {
  privacyLevel.value = '';
  creatorInfo.value = null;
  capabilities.value = null;
  if (!id) return;
  await loadAccountMeta(id);
});

async function loadAccountMeta(accountId: string) {
  loadingCreator.value = true;
  try {
    const [capRes, creatorRes] = await Promise.all([
      connectedAccountService.getCapabilities(accountId),
      connectedAccountService.getTikTokCreatorInfo(accountId)
    ]);
    capabilities.value = capRes.data ?? null;
    creatorInfo.value = creatorRes.data ?? null;

    if (capabilities.value?.supportsDirectPublish) {
      publishMode.value = 'Direct';
    } else {
      publishMode.value = 'DraftUpload';
    }
  } catch (error: any) {
    toastStore.error('TikTok', error?.response?.data?.message || 'Không thể tải thông tin creator TikTok.');
  } finally {
    loadingCreator.value = false;
  }
}

async function handlePublish() {
  if (!props.postId) {
    toastStore.warning('TikTok', 'Chưa có bài đăng để xuất bản.');
    return;
  }
  if (!selectedAccountId.value) {
    toastStore.warning('TikTok', 'Vui lòng chọn tài khoản TikTok đã liên kết.');
    return;
  }
  if (!props.videoUrl) {
    toastStore.warning('TikTok', 'Cần URL video công khai (9:16). Tạo video tại tab Video Studio trước.');
    return;
  }
  if (!privacyLevel.value) {
    toastStore.warning('TikTok', 'Bạn phải chọn mức riêng tư — TikTok không cho phép giá trị mặc định.');
    return;
  }
  if (!userConsent.value) {
    toastStore.warning('TikTok', 'Bạn phải xác nhận đồng ý gửi video lên TikTok.');
    return;
  }
  if (publishMode.value === 'Direct' && !capabilities.value?.supportsDirectPublish) {
    toastStore.warning('TikTok', 'Direct post không khả dụng. App chưa audit hoặc thiếu scope video.publish.');
    return;
  }

  const manifest: TikTokMediaManifest = {
    videoUrl: props.videoUrl,
    isAigc: props.isAigc ?? false,
    privacyLevel: privacyLevel.value,
    userConsentConfirmed: true,
    disableComment: creatorInfo.value?.commentDisabled ?? false,
    disableDuet: creatorInfo.value?.duetDisabled ?? false,
    disableStitch: creatorInfo.value?.stitchDisabled ?? false
  };

  publishing.value = true;
  try {
    await publicationStore.queueJob({
      postId: props.postId,
      contentVariantId: '00000000-0000-0000-0000-000000000000',
      connectedAccountId: selectedAccountId.value,
      publishMode: publishMode.value,
      scheduledAtUtc: null,
      mediaManifestJson: JSON.stringify(manifest)
    });
    const modeLabel = publishMode.value === 'DraftUpload'
      ? 'Upload/Draft — kiểm tra inbox TikTok để hoàn tất'
      : 'Direct post — đang xử lý trên TikTok';
    toastStore.success('TikTok', `Đã đưa video vào hàng đợi. ${modeLabel}`);
  } catch (error: any) {
    toastStore.error('TikTok', error?.response?.data?.message || 'Không thể tạo job xuất bản TikTok.');
  } finally {
    publishing.value = false;
  }
}
</script>

<template>
  <div class="tiktok-publish-panel glass-card">
    <h4>🎵 Xuất bản TikTok</h4>

    <div v-if="creatorInfo?.restrictionNote || (capabilities && !capabilities.isAppAudited)" class="audit-banner">
      <strong>⚠️ Giới hạn app chưa audit</strong>
      <p>{{ creatorInfo?.restrictionNote || capabilities?.restrictionReason }}</p>
      <p class="hint">Direct post công khai chỉ khả dụng sau Content Posting API audit. Hiện tại dùng <strong>Upload/Draft</strong>.</p>
    </div>

    <div class="form-group">
      <label>Tài khoản TikTok</label>
      <select v-model="selectedAccountId" :disabled="tiktokAccounts.length === 0">
        <option value="">-- Chọn tài khoản --</option>
        <option v-for="acc in tiktokAccounts" :key="acc.id" :value="acc.id">
          {{ acc.displayName }}
        </option>
      </select>
      <p v-if="tiktokAccounts.length === 0" class="hint">
        Chưa có tài khoản TikTok. Liên kết tại <strong>Cài đặt hệ thống → Tài khoản liên kết</strong>.
      </p>
    </div>

    <div v-if="loadingCreator" class="loading-hint">Đang tải creator info...</div>

    <div v-else-if="creatorInfo" class="creator-card">
      <img v-if="creatorInfo.creatorAvatarUrl" :src="creatorInfo.creatorAvatarUrl" class="creator-avatar" alt="" />
      <div>
        <strong>{{ creatorInfo.creatorNickname || creatorInfo.creatorUsername }}</strong>
        <span v-if="creatorInfo.creatorUsername" class="muted">@{{ creatorInfo.creatorUsername }}</span>
        <p class="hint">Tối đa {{ creatorInfo.maxVideoPostDurationSec }}s / video</p>
      </div>
    </div>

    <div class="form-group">
      <label>Chế độ xuất bản</label>
      <select v-model="publishMode">
        <option value="DraftUpload" :disabled="!capabilities?.supportsDraftUpload">Upload/Draft (inbox TikTok)</option>
        <option value="Direct" :disabled="!capabilities?.supportsDirectPublish">Direct Post</option>
      </select>
    </div>

    <div class="form-group">
      <label>Mức riêng tư (bắt buộc chọn thủ công)</label>
      <select v-model="privacyLevel">
        <option value="">-- Chọn mức riêng tư --</option>
        <option
          v-for="opt in creatorInfo?.privacyLevelOptions || []"
          :key="opt"
          :value="opt"
        >
          {{ privacyLabels[opt] || opt }}
        </option>
      </select>
    </div>

    <div class="form-group checkbox-row">
      <label>
        <input v-model="userConsent" type="checkbox" />
        Tôi đồng ý gửi video này lên tài khoản TikTok đã chọn
      </label>
    </div>

    <div v-if="isAigc" class="aigc-badge">🤖 Nội dung AI — sẽ khai báo <code>is_aigc</code> cho TikTok</div>

    <div v-if="videoUrl" class="video-preview-hint">
      Video: <code>{{ videoUrl }}</code>
    </div>
    <p v-else class="hint warn">Chưa có URL video. Dùng Video Studio để render video 9:16 trước.</p>

    <button
      type="button"
      class="publish-btn"
      :disabled="publishing || !selectedAccountId || !videoUrl"
      @click="handlePublish"
    >
      {{ publishing ? 'Đang gửi...' : 'Gửi lên TikTok' }}
    </button>
  </div>
</template>

<style scoped>
.tiktok-publish-panel {
  padding: 1.25rem;
  margin-top: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.audit-banner {
  background: rgba(245, 158, 11, 0.12);
  border: 1px solid rgba(245, 158, 11, 0.35);
  border-radius: 8px;
  padding: 0.75rem;
  font-size: 0.9rem;
}
.creator-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
.creator-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  object-fit: cover;
}
.form-group label { display: block; margin-bottom: 0.35rem; font-size: 0.85rem; }
.form-group select { width: 100%; padding: 0.5rem; border-radius: 6px; }
.hint { font-size: 0.8rem; color: #94a3b8; margin: 0.25rem 0 0; }
.hint.warn { color: #f59e0b; }
.muted { color: #64748b; margin-left: 0.5rem; font-size: 0.85rem; }
.checkbox-row label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
.aigc-badge { font-size: 0.85rem; color: #a78bfa; }
.video-preview-hint code { font-size: 0.75rem; word-break: break-all; }
.publish-btn {
  margin-top: 0.5rem;
  padding: 0.65rem 1rem;
  background: linear-gradient(135deg, #010101, #333);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}
.publish-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.loading-hint { color: #94a3b8; font-size: 0.85rem; }
</style>
