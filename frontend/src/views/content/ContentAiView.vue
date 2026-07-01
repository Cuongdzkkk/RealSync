<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { usePropertyStore } from '@/stores/usePropertyStore';
import { useProjectStore } from '@/stores/useProjectStore';
import { useToastStore } from '@/stores/useToastStore';
import { usePostStore } from '@/stores/usePostStore';
import { usePublicationStore } from '@/stores/usePublicationStore';
import RoleGate from '@/components/common/RoleGate.vue';
import VideoStudioPanel from '@/components/content-ai/VideoStudioPanel.vue';
import TikTokPublishPanel from '@/components/content-ai/TikTokPublishPanel.vue';
import { postingService } from '@/services/postingService';
import { api } from '@/services/api';
import { useVideoStore } from '@/stores/useVideoStore';
import { useConnectedAccountStore } from '@/stores/useConnectedAccountStore';

const propertyStore = usePropertyStore();
const projectStore = useProjectStore();
const toastStore = useToastStore();
const postStore = usePostStore();
const publicationStore = usePublicationStore();
const videoStore = useVideoStore();
const connectedAccountStore = useConnectedAccountStore();

// --- Active Tab State ---
const activeTab = ref<'compose' | 'monitor' | 'video'>('compose');

onMounted(async () => {
  postStore.fetchAllHistory();
  fetchMonitorJobs();
  connectedAccountStore.fetchAccounts();
  propertyStore.fetchProperties().then(() => {
    if (propertyStore.items.length > 0 && selectedType.value === 'property') {
      selectedPropertyId.value = propertyStore.items[0].id;
    }
  });
  try {
    const { data: res } = await api.get('/settings/channels');
    if (res.data && res.data.facebookGroupIds) {
      fbGroupIds.value = res.data.facebookGroupIds;
    }
  } catch (err) {
    console.warn('Cannot fetch system channels config', err);
  }
});

// --- Input States ---
const selectedType = ref<'property' | 'project'>('property');
const selectedPropertyId = ref(propertyStore.items[0]?.id || '');
const selectedProjectId = ref(projectStore.items[0]?.id || '');
const selectedChannel = ref<'facebook' | 'listing' | 'zalo' | 'tiktok' | 'seo' | 'batdongsan' | 'chotot' | 'alonhadat' | 'website'>('facebook');
const selectedTone = ref<string>('persuasive');
const includeEmojis = ref(true);
const includeContact = ref(true);
const fbGroupIds = ref('');
const showGroupModal = ref(false);
const activeGroups = ref<string[]>([]);

// --- Generator States ---
const isGenerating = ref(false);
const generatedText = ref('');
const displayedText = ref(''); // For typing animation
const showPreview = ref(false);
const currentAiGeneration = ref<any>(null);
const isApplying = ref(false);

const aiUsageStats = computed(() => {
  if (!currentAiGeneration.value) return null;
  return {
    promptTokens: currentAiGeneration.value.promptTokens,
    completionTokens: currentAiGeneration.value.completionTokens,
    estimatedCost: currentAiGeneration.value.estimatedCost
  };
});

const tiktokVideoUrl = computed(() => {
  const url = videoStore.currentProject?.finalAsset?.url;
  if (!url) return undefined;
  if (url.startsWith('http')) return url;
  return `http://localhost:5000${url.startsWith('/') ? url : '/' + url}`;
});

const isAiGeneratedContent = computed(() => !!currentAiGeneration.value);

const factsChecklist = computed(() => {
  if (!currentAiGeneration.value || !currentAiGeneration.value.factsUsedJson) return [];
  try {
    const parsed = JSON.parse(currentAiGeneration.value.factsUsedJson);
    return parsed.factsUsed || [];
  } catch (e) {
    console.error("Failed to parse factsUsedJson", e);
    return [];
  }
});

const contradictionWarnings = computed(() => {
  if (!currentAiGeneration.value || !currentAiGeneration.value.factsUsedJson) return [];
  try {
    const parsed = JSON.parse(currentAiGeneration.value.factsUsedJson);
    return parsed.warnings || [];
  } catch (e) {
    console.error("Failed to parse warnings from factsUsedJson", e);
    return [];
  }
});

// --- Monitor States ---
const monitorPage = ref(1);
const monitorPageSize = ref(10);
const monitorStatusFilter = ref<string>('');
const selectedJobId = ref<string | null>(null);
const queuePostId = ref('');
const queueConnectedAccountId = ref('');
const queuePublishMode = ref<'Direct' | 'DraftUpload' | 'Assisted'>('Direct');
const queueMediaUrl = ref('');

const activeConnectedAccounts = computed(() =>
  connectedAccountStore.accounts.filter(a => a.status === 'Active')
);

const selectedQueueAccount = computed(() =>
  connectedAccountStore.accounts.find(a => a.id === queueConnectedAccountId.value)
);

const tikTokPanelPostId = computed(() => postStore.currentPost?.id || historyList.value[0]?.id || '');
const tikTokPanelCaption = computed(() => postStore.currentPost?.content || displayedText.value || '');

async function fetchMonitorJobs() {
  await publicationStore.fetchJobs({
    status: monitorStatusFilter.value || undefined,
    page: monitorPage.value,
    pageSize: monitorPageSize.value
  });
}

async function handleQueuePublicationTest() {
  if (!queuePostId.value) {
    toastStore.warning('Publishing', 'Choose a post first.');
    return;
  }

  const mediaManifestJson = queueMediaUrl.value.trim()
    ? JSON.stringify({
        videoUrl: queueMediaUrl.value.trim(),
        isAigc: true,
        privacyLevel: 'SELF_ONLY',
        userConsentConfirmed: true
      })
    : null;

  try {
    await publicationStore.queueJob({
      postId: queuePostId.value,
      contentVariantId: '00000000-0000-0000-0000-000000000000',
      connectedAccountId: queueConnectedAccountId.value || null,
      publishMode: queuePublishMode.value,
      scheduledAtUtc: null,
      mediaManifestJson
    });
    toastStore.success('Publishing', 'Queued publishing job for test.');
    await fetchMonitorJobs();
  } catch (error: any) {
    toastStore.error('Publishing', error?.response?.data?.message || 'Cannot queue publishing job.');
  }
}

async function handleSelectJob(id: string) {
  selectedJobId.value = id;
  await publicationStore.fetchAttempts(id);
}

async function handleRefreshJob(id: string) {
  try {
    await publicationStore.refreshStatus(id);
    toastStore.success('Publishing', 'Refreshed provider status.');
  } catch (error: any) {
    toastStore.error('Publishing', error?.response?.data?.message || 'Cannot refresh provider status.');
  }
}

async function handleRetryJob(id: string) {
  try {
    await publicationStore.retryJob(id);
    toastStore.success('Publishing', 'Retry job queued.');
  } catch (error: any) {
    toastStore.error('Publishing', error?.response?.data?.message || 'Cannot retry this job.');
  }
}

async function handleCancelJob(id: string) {
  try {
    await publicationStore.cancelJob(id);
    toastStore.success('Publishing', 'Job cancelled.');
  } catch (error: any) {
    toastStore.error('Publishing', error?.response?.data?.message || 'Cannot cancel this job.');
  }
}

function accountDisplayName(id: string | null) {
  if (!id) return 'Website';
  const account = connectedAccountStore.accounts.find(a => a.id === id);
  return account ? `${account.provider} - ${account.displayName}` : id.slice(0, 8);
}

function shortId(id: string) {
  return id.slice(0, 8);
}

function isRetryableStatus(status: string) {
  return status === 'Failed' || status === 'NeedsReview';
}

function isCancellableStatus(status: string) {
  return status !== 'Published' && status !== 'Cancelled';
}

watch([monitorStatusFilter, monitorPage], () => {
  fetchMonitorJobs();
});

const selectedTargetItem = computed(() => {
  if (selectedType.value === 'property') {
    return propertyStore.items.find(p => p.id === selectedPropertyId.value);
  } else {
    return projectStore.items.find(p => p.id === selectedProjectId.value);
  }
});

const displayTitle = computed(() => {
  const item = selectedTargetItem.value;
  if (!item) return '';
  return 'title' in item ? item.title : item.name;
});

const displayPrice = computed(() => {
  const item = selectedTargetItem.value;
  if (!item) return '';
  if ('price' in item) {
    return item.price ? (item.price / 1000000000).toFixed(1) + ' tỷ' : 'Thỏa thuận';
  }
  return 'priceRange' in item ? item.priceRange : 'Thỏa thuận';
});

const displayLocation = computed(() => {
  const item = selectedTargetItem.value;
  if (!item) return '';
  return 'location' in item ? item.location : (item.address || item.area || '');
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

  // Xây dựng hướng dẫn chi tiết theo loại hình kênh (Platform Template)
  let platformContext = "";
  if (channel === 'facebook') {
    platformContext = "định dạng bài viết mạng xã hội Facebook hấp dẫn, có chia nhóm ý chính, câu hook gây ấn tượng ở đầu bài, hashtag ở cuối.";
  } else if (channel === 'zalo') {
    platformContext = "định dạng Zalo broadcast ngắn gọn, tập trung thẳng vào thông tin nổi bật nhất, nút call to action rõ ràng.";
  } else if (channel === 'seo') {
    platformContext = "bài viết chuẩn SEO chuyên sâu, phân tích tiềm năng bất động sản, cấu trúc heading rõ ràng, nhiều từ khóa liên quan.";
  } else if (channel === 'batdongsan' || channel === 'chotot' || channel === 'alonhadat') {
    platformContext = `tin đăng đặc thù cho sàn thương mại điện tử ${channel.toUpperCase()}, mô tả chi tiết thông số kỹ thuật (vị trí, diện tích, giá bán, pháp lý), thông tin minh bạch, chuyên nghiệp.`;
  } else {
    platformContext = "tin đăng BĐS tiêu chuẩn đầy đủ thông tin vị trí, diện tích, mức giá, tiện ích xung quanh.";
  }

  const prompt = `Hãy viết một bài đăng hoàn chỉnh cho ${channel.toUpperCase()} về bất động sản "${itemName}".
Yêu cầu định dạng: ${platformContext}
Giọng văn: ${selectedTone.value}
Kèm emoji: ${includeEmojis.value ? 'Có' : 'Không'}
Thông tin liên hệ môi giới: ${includeContact.value ? 'Có kèm' : 'Không kèm'}
`;

  try {
    // Gọi API: tạo Post → sinh AI content
    const generation = await postStore.generateContent(itemName, prompt, itemId, channel);

    // Hiển thị nội dung AI trả về với hiệu ứng typing
    generatedText.value = generation.generatedContent;
    currentAiGeneration.value = generation;

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

async function saveCurrentContent() {
  if (!postStore.currentPost) {
    toastStore.warning('Cảnh báo', 'Không tìm thấy bài đăng hoạt động để lưu.');
    return;
  }
  isApplying.value = true;
  try {
    await postStore.applyAiContent(postStore.currentPost.id, displayedText.value);
    toastStore.success('Đã lưu thành công', 'Nội dung bài viết đã được cập nhật vào cơ sở dữ liệu.');
  } catch (error: any) {
    toastStore.error('Lỗi', error?.response?.data?.message || 'Không thể lưu nội dung bài viết.');
  } finally {
    isApplying.value = false;
  }
}

// History: lấy từ API posts thay vì mock
const selectedFilterChannel = ref('all');

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

const filteredHistoryList = computed(() => {
  const list = historyList.value;
  if (selectedFilterChannel.value === 'all') {
    return list;
  }
  return list.filter(item => item.channel === selectedFilterChannel.value);
});

function extractChannel(summary?: string): 'seo' | 'facebook' | 'zalo' | 'listing' | 'batdongsan' | 'chotot' | 'alonhadat' | 'website' {
  if (!summary) return 'listing';
  const sum = summary.toLowerCase();
  if (sum.includes('facebook')) return 'facebook';
  if (sum.includes('zalo')) return 'zalo';
  if (sum.includes('seo')) return 'seo';
  if (sum.includes('batdongsan')) return 'batdongsan';
  if (sum.includes('chotot')) return 'chotot';
  if (sum.includes('alonhadat')) return 'alonhadat';
  if (sum.includes('website')) return 'website';
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

const isPublishing = ref(false);

function fallbackCopyTextToClipboard(text: string) {
  const textArea = document.createElement("textarea");
  textArea.value = text;
  textArea.style.top = "0";
  textArea.style.left = "0";
  textArea.style.position = "fixed";
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();
  try {
    document.execCommand('copy');
  } catch (err) {
    console.error('Fallback copy failed', err);
  }
  document.body.removeChild(textArea);
}

async function copyTextToClipboard(text: string) {
  if (!navigator.clipboard) {
    fallbackCopyTextToClipboard(text);
    return;
  }
  try {
    await navigator.clipboard.writeText(text);
  } catch (err) {
    fallbackCopyTextToClipboard(text);
  }
}

async function copyHistoryContent(postId: string) {
  const post = postStore.posts.find(p => p.id === postId);
  const content = post?.content || '';
  await copyTextToClipboard(content);
  toastStore.success('Đã sao chép', 'Đã copy nội dung lịch sử vào bộ nhớ tạm.');
}

async function deleteHistoryPost(postId: string) {
  try {
    await postingService.deletePost(postId);
    toastStore.success('Đã ẩn bài đăng', 'Bài đăng đã được ẩn khỏi danh sách.');
    postStore.fetchAllHistory();
  } catch (error: any) {
    toastStore.error('Lỗi', error.response?.data?.message || 'Không thể ẩn bài đăng.');
  }
}

async function publishCurrentPost() {
  if (!postStore.currentPost) {
    toastStore.warning('Lỗi', 'Không tìm thấy thông tin bài đăng để publish.');
    return;
  }
  
  isPublishing.value = true;
  const channelName = selectedChannel.value;
  const postId = postStore.currentPost.id;

  // Auto-save the edited text back to the database before publishing
  try {
    await postStore.applyAiContent(postId, displayedText.value);
  } catch (err) {
    console.warn("Failed to auto-save post content before publishing:", err);
  }

  // Tự động copy nội dung cho Facebook và Zalo trước khi chạy API
  if (channelName.toLowerCase() === 'facebook' || channelName.toLowerCase() === 'zalo') {
    await copyTextToClipboard(displayedText.value);
  }

  try {
    // Check if channel already exists for this post
    const existingChannels = await postingService.getChannels(postId);
    const existing = existingChannels.find(c => c.channel?.toLowerCase() === channelName.toLowerCase());
    
    let channelId = '';
    if (existing) {
      channelId = existing.id;
    } else {
      const channelObj = await postingService.createChannel(postId, channelName);
      channelId = channelObj.id;
    }
    
    await postingService.publishChannel(postId, channelId);
    
    // Tự động mở tab đăng bài nếu là Facebook hoặc Zalo
    if (channelName.toLowerCase() === 'facebook') {
      const groups = fbGroupIds.value.split(',').map((id: string) => id.trim()).filter((id: string) => id.length > 0);
      if (groups.length > 1) {
        activeGroups.value = groups;
        showGroupModal.value = true;
        toastStore.success('Đã copy nội dung', 'Nội dung đã được copy. Vui lòng chọn nhóm trên hộp thoại để đăng.');
      } else if (groups.length === 1) {
        window.open(`https://www.facebook.com/groups/${groups[0]}`, '_blank');
        toastStore.success('Đăng bài & Mở nhóm', 'Nội dung đã được copy. Đang mở nhóm Facebook trong tab mới.');
      } else {
        window.open('https://www.facebook.com', '_blank');
        toastStore.success('Đăng bài & Mở Facebook', 'Nội dung đã được copy. Đang mở Facebook trong tab mới.');
      }
    } else if (channelName.toLowerCase() === 'zalo') {
      window.open('https://chat.zalo.me/', '_blank');
      toastStore.success('Đăng bài & Mở Zalo', 'Nội dung đã được copy. Đang mở Zalo Web trong tab mới.');
    } else {
      toastStore.success('Đăng bài thành công', `Bài đăng đã được xuất bản lên kênh ${channelName.toUpperCase()}.`);
    }

    postStore.fetchAllHistory();
  } catch (error: any) {
    // Fallback manual redirection when API fails
    if (channelName.toLowerCase() === 'facebook' || channelName.toLowerCase() === 'zalo') {
      if (channelName.toLowerCase() === 'facebook') {
        const groups = fbGroupIds.value.split(',').map((id: string) => id.trim()).filter((id: string) => id.length > 0);
        if (groups.length > 1) {
          activeGroups.value = groups;
          showGroupModal.value = true;
          toastStore.warning('Hệ thống API lỗi - Đã mở danh sách Nhóm', 'Đã chuyển sang chế độ thủ công. Chọn nhóm trên hộp thoại để đăng.');
        } else if (groups.length === 1) {
          window.open(`https://www.facebook.com/groups/${groups[0]}`, '_blank');
          toastStore.warning('Hệ thống API lỗi - Đã mở Nhóm', 'Đã chuyển sang chế độ thủ công. Đang mở nhóm Facebook.');
        } else {
          window.open('https://www.facebook.com', '_blank');
          toastStore.warning('Hệ thống API lỗi - Đã mở Facebook', 'Đã chuyển sang chế độ thủ công. Đang mở Facebook.');
        }
      } else if (channelName.toLowerCase() === 'zalo') {
        window.open('https://chat.zalo.me/', '_blank');
        toastStore.warning('Hệ thống API lỗi - Đã mở Zalo', 'Đã chuyển sang chế độ thủ công. Đang mở Zalo Web.');
      }
    } else {
      toastStore.error('Lỗi đăng bài', error?.response?.data?.message || 'Không thể đăng bài lên kênh. Vui lòng thử lại.');
    }
  } finally {
    isPublishing.value = false;
  }
}

async function publishHistoryPost(postId: string, channelName: string) {
  // Nß║╛U L├Ç K├èNH WEBSITE -> Gß╗îI PUBLISHING ORCHESTRATOR
  if (channelName.toLowerCase() === 'website') {
    try {
      await publicationStore.queueJob({
        postId: postId,
        contentVariantId: '00000000-0000-0000-0000-000000000000',
        connectedAccountId: null,
        publishMode: 'Direct',
        scheduledAtUtc: null
      });
      toastStore.success('─É─âng b├ái th├ánh c├┤ng', 'Y├¬u cß║ºu xuß║Ñt bß║ún l├¬n Website ─æ├ú ─æ╞░ß╗úc ─æ╞░a v├áo h├áng ─æß╗úi xß╗¡ l├╜ ngß║ºm (Hangfire).');
      
      const showcaseUrl = `http://localhost:5000/api/v1/posts/${postId}/public`;
      window.open(showcaseUrl, '_blank');
      
      postStore.fetchAllHistory();
    } catch (error: any) {
      toastStore.error('Lß╗ùi ─æ─âng b├ái', error?.response?.data?.message || 'Kh├┤ng thß╗â tß║ío job xuß║Ñt bß║ún l├¬n Website.');
    }
    return;
  }

  const post = postStore.posts.find(p => p.id === postId);
  const content = post?.content || '';

  // Tự động copy nội dung cho Facebook và Zalo trước khi chạy API
  if (channelName.toLowerCase() === 'facebook' || channelName.toLowerCase() === 'zalo') {
    await copyTextToClipboard(content);
  }

  try {
    // Check if channel already exists for this post
    const existingChannels = await postingService.getChannels(postId);
    const existing = existingChannels.find(c => c.channel?.toLowerCase() === channelName.toLowerCase());
    
    let channelId = '';
    if (existing) {
      channelId = existing.id;
    } else {
      const channelObj = await postingService.createChannel(postId, channelName);
      channelId = channelObj.id;
    }
    
    await postingService.publishChannel(postId, channelId);
    
    if (channelName.toLowerCase() === 'facebook') {
      const groups = fbGroupIds.value.split(',').map((id: string) => id.trim()).filter((id: string) => id.length > 0);
      if (groups.length > 1) {
        activeGroups.value = groups;
        showGroupModal.value = true;
        toastStore.success('Đã copy nội dung', 'Nội dung đã được copy. Vui lòng chọn nhóm trên hộp thoại để đăng.');
      } else if (groups.length === 1) {
        window.open(`https://www.facebook.com/groups/${groups[0]}`, '_blank');
        toastStore.success('Đăng bài & Mở nhóm', 'Nội dung đã được copy. Đang mở nhóm Facebook trong tab mới.');
      } else {
        window.open('https://www.facebook.com', '_blank');
        toastStore.success('Đăng bài & Mở Facebook', 'Nội dung đã được copy. Đang mở Facebook trong tab mới.');
      }
    } else if (channelName.toLowerCase() === 'zalo') {
      window.open('https://chat.zalo.me/', '_blank');
      toastStore.success('Đăng bài & Mở Zalo', 'Nội dung đã được copy. Đang mở Zalo Web trong tab mới.');
    } else {
      toastStore.success('Đăng bài thành công', `Bài đăng đã được xuất bản lên kênh ${channelName.toUpperCase()}.`);
    }

    postStore.fetchAllHistory();
  } catch (error: any) {
    // Fallback manual redirection when API fails
    if (channelName.toLowerCase() === 'facebook' || channelName.toLowerCase() === 'zalo') {
      if (channelName.toLowerCase() === 'facebook') {
        const groups = fbGroupIds.value.split(',').map((id: string) => id.trim()).filter((id: string) => id.length > 0);
        if (groups.length > 1) {
          activeGroups.value = groups;
          showGroupModal.value = true;
          toastStore.warning('Hệ thống API lỗi - Đã mở danh sách Nhóm', 'Đã chuyển sang chế độ thủ công. Chọn nhóm trên hộp thoại để đăng.');
        } else if (groups.length === 1) {
          window.open(`https://www.facebook.com/groups/${groups[0]}`, '_blank');
          toastStore.warning('Hệ thống API lỗi - Đã mở Nhóm', 'Đã chuyển sang chế độ thủ công. Đang mở nhóm Facebook.');
        } else {
          window.open('https://www.facebook.com', '_blank');
          toastStore.warning('Hệ thống API lỗi - Đã mở Facebook', 'Đã chuyển sang chế độ thủ công. Đang mở Facebook.');
        }
      } else if (channelName.toLowerCase() === 'zalo') {
        window.open('https://chat.zalo.me/', '_blank');
        toastStore.warning('Hệ thống API lỗi - Đã mở Zalo', 'Đã tự động chuyển sang chế độ thủ công. Nội dung đã được copy. Đang mở Zalo Web.');
      }
    } else {
      toastStore.error('Lỗi đăng bài', error?.response?.data?.message || 'Không thể đăng bài lên kênh. Vui lòng thử lại.');
    }
  }
}
</script>

<template>
  <RoleGate :roles="['Admin', 'Sales', 'Marketing']">
    <div class="page">
      <div class="tabs-navigation glass-card">
        <button
          type="button"
          class="nav-tab-btn"
          :class="{ active: activeTab === 'compose' }"
          @click="activeTab = 'compose'"
        >
          Compose
        </button>
        <button
          type="button"
          class="nav-tab-btn"
          :class="{ active: activeTab === 'monitor' }"
          @click="activeTab = 'monitor'; fetchMonitorJobs()"
        >
          Publishing Monitor
        </button>
        <!-- Video tab hidden -->
      </div>

      <div v-if="activeTab === 'compose'">
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
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'batdongsan' }"
                @click="selectedChannel = 'batdongsan'"
              >
                <span class="icon">🏠</span>
                <span>Batdongsan.com.vn</span>
              </div>
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'chotot' }"
                @click="selectedChannel = 'chotot'"
              >
                <span class="icon">🏷️</span>
                <span>Chợ Tốt Đất</span>
              </div>
              <div 
                class="channel-card" 
                :class="{ active: selectedChannel === 'alonhadat' }"
                @click="selectedChannel = 'alonhadat'"
              >
                <span class="icon">🏘️</span>
                <span>Alo Nhà Đất</span>
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
            <div style="display: flex; gap: 8px;">
              <button class="copy-btn glow-yellow" @click="copyContent">Sao chép nội dung</button>
              <button class="copy-btn" :disabled="isApplying" style="background: rgba(255, 255, 255, 0.1); border: 1px solid rgba(255, 255, 255, 0.15);" @click="saveCurrentContent">
                {{ isApplying ? 'Đang lưu...' : '💾 Lưu nội dung' }}
              </button>
              <button class="publish-btn" :disabled="isPublishing" @click="publishCurrentPost">
                <span v-if="isPublishing" class="spinner-inline"></span>
                {{ isPublishing ? 'Đang đăng...' : 'Đăng bài (Publish)' }}
              </button>
            </div>
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

          <!-- Batdongsan Mockup -->
          <div v-else-if="selectedChannel === 'batdongsan'" class="batdongsan-mockup glass-card">
            <div class="bds-header">
              <span class="logo">🏠 Batdongsan.com.vn</span>
              <span class="tagline">Kênh thông tin số 1 về Bất động sản</span>
            </div>
            <div class="bds-content-area">
              <h4 class="bds-title">{{ displayTitle || 'Tin đăng bán nhà đất nổi bật' }}</h4>
              <div class="bds-meta-strip">
                <span class="meta-item red-text">💰 {{ displayPrice || 'Thỏa thuận' }}</span>
                <span class="meta-item">📐 85 m²</span>
                <span class="meta-item">📍 {{ displayLocation || 'Quận 2, TP. HCM' }}</span>
              </div>
              <div class="bds-body-text">
                <textarea v-model="displayedText" rows="10" class="editor-textarea"></textarea>
              </div>
            </div>
          </div>

          <!-- Chotot Mockup -->
          <div v-else-if="selectedChannel === 'chotot'" class="chotot-mockup glass-card">
            <div class="chotot-header">
              <span class="ct-logo">🏷️ CHỢ TỐT NHÀ</span>
            </div>
            <div class="chotot-content">
              <h4 class="ct-title">{{ displayTitle || 'Tin đăng bán nhà đất nổi bật' }}</h4>
              <div class="ct-price-strip">
                <span class="ct-price">{{ displayPrice || 'Thỏa thuận' }}</span>
                <span class="ct-tag-broker">Môi giới</span>
              </div>
              <div class="ct-body-text">
                <textarea v-model="displayedText" rows="10" class="editor-textarea"></textarea>
              </div>
            </div>
          </div>

          <!-- Alonhadat Mockup -->
          <div v-else-if="selectedChannel === 'alonhadat'" class="alonhadat-mockup glass-card">
            <div class="alonhadat-header">
              <span class="ald-logo">🏘️ ALONHADAT.COM.VN</span>
            </div>
            <div class="alonhadat-content">
              <h4 class="ald-title">{{ displayTitle || 'Tin đăng bán nhà đất nổi bật' }}</h4>
              <div class="ald-specs">
                <table class="ald-specs-table">
                  <tbody>
                    <tr>
                      <td><strong>Giá:</strong> {{ displayPrice || 'Thỏa thuận' }}</td>
                      <td><strong>Diện tích:</strong> 85 m²</td>
                    </tr>
                    <tr>
                      <td><strong>Pháp lý:</strong> Sổ đỏ/Sổ hồng</td>
                      <td><strong>Hướng:</strong> Đông Nam</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="ald-body-text">
                <textarea v-model="displayedText" rows="10" class="editor-textarea"></textarea>
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
      <div class="section-header" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 16px; margin-bottom: 16px;">
        <div>
          <h3>📜 Lịch sử nội dung tiếp thị đã soạn thảo</h3>
          <p class="subtitle">Danh sách các mô tả tin đăng và bài viết đã tạo trước đó.</p>
        </div>
        
        <!-- Filter dropdown -->
        <div class="filter-channel-dropdown" style="display: flex; align-items: center; gap: 8px;">
          <span style="font-size: 12px; font-weight: 600; color: var(--color-text-secondary);">Lọc theo kênh:</span>
          <select v-model="selectedFilterChannel" style="height: 32px; padding: 0 10px; border-radius: 6px; border: 1px solid var(--color-border); background: var(--color-canvas); color: var(--color-text-primary); font-size: 12px;">
            <option value="all">Tất cả các kênh</option>
            <option value="facebook">Facebook</option>
            <option value="zalo">Zalo</option>
            <option value="seo">SEO</option>
            <option value="batdongsan">Batdongsan.com.vn</option>
            <option value="chotot">Chợ Tốt</option>
            <option value="alonhadat">Alo Nhà Đất</option>
            <option value="website">Website</option>
            <option value="listing">Tin đăng khác</option>
          </select>
        </div>
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
          <tr v-for="item in filteredHistoryList" :key="item.id">
            <td class="item-title">{{ item.title }}</td>
            <td>
              <span class="channel-tag" :class="item.channel">
                {{ 
                  item.channel === 'facebook' ? 'Facebook' : 
                  item.channel === 'zalo' ? 'Zalo' : 
                  item.channel === 'seo' ? 'SEO' : 
                  item.channel === 'batdongsan' ? 'Batdongsan.com.vn' : 
                  item.channel === 'chotot' ? 'Chợ Tốt' : 
                  item.channel === 'alonhadat' ? 'Alo Nhà Đất' : 'Tin đăng BĐS' 
                }}
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
              <div style="display: flex; gap: 8px;">
                <button class="action-btn-copy" @click="copyHistoryContent(item.id)">Copy</button>
                <button class="action-btn-publish" @click="publishHistoryPost(item.id, item.channel)">Đăng bài</button>
                <button class="action-btn-delete" @click="deleteHistoryPost(item.id)">Ẩn</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL ĐĂNG NHÓM FACEBOOK -->
    </div>

    <div v-if="activeTab === 'monitor'" class="monitor-panel animate-fade">
      <div class="monitor-controls glass-card">
        <div class="section-header">
          <h3>Publishing Monitor</h3>
          <p class="subtitle">Queue a test job, refresh provider status, retry failed jobs, or cancel pending work.</p>
        </div>

        <div class="filter-bar">
          <div class="filter-group">
            <label>Status</label>
            <select v-model="monitorStatusFilter" class="filter-select">
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Queued">Queued</option>
              <option value="Validating">Validating</option>
              <option value="Publishing">Publishing</option>
              <option value="RemoteProcessing">Remote Processing</option>
              <option value="NeedsReview">Needs Review</option>
              <option value="RetryScheduled">Retry Scheduled</option>
              <option value="Failed">Failed</option>
              <option value="Published">Published</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          <button type="button" class="refresh-btn" :disabled="publicationStore.loading" @click="fetchMonitorJobs">
            Refresh jobs
          </button>
        </div>

        <form class="queue-test-form" @submit.prevent="handleQueuePublicationTest">
          <div class="form-group">
            <label>Post</label>
            <select v-model="queuePostId">
              <option value="">Choose a post</option>
              <option v-for="item in filteredHistoryList" :key="item.id" :value="item.id">
                {{ item.title }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Connected account</label>
            <select v-model="queueConnectedAccountId">
              <option value="">Website / no account</option>
              <option v-for="account in activeConnectedAccounts" :key="account.id" :value="account.id">
                {{ account.provider }} - {{ account.displayName }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Mode</label>
            <select v-model="queuePublishMode">
              <option value="Direct">Direct</option>
              <option value="DraftUpload">Draft Upload</option>
              <option value="Assisted">Assisted</option>
            </select>
          </div>
          <div class="form-group">
            <label>Video URL (optional)</label>
            <input v-model="queueMediaUrl" type="url" placeholder="https://..." />
          </div>
          <button type="submit" class="refresh-btn" :disabled="publicationStore.actionLoading">
            Queue test job
          </button>
          <p v-if="selectedQueueAccount" class="subtitle">
            Target: {{ selectedQueueAccount.provider }} / {{ selectedQueueAccount.channelType }}
          </p>
        </form>

        <table class="monitor-table">
          <thead>
            <tr>
              <th>Job</th>
              <th>Target</th>
              <th>Mode</th>
              <th>Status</th>
              <th>Published</th>
              <th>Error</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="publicationStore.jobs.length === 0">
              <td colspan="7" class="no-data">No publishing jobs yet.</td>
            </tr>
            <tr
              v-for="job in publicationStore.jobs"
              :key="job.id"
              class="clickable-row"
              :class="{ selected: selectedJobId === job.id }"
              @click="handleSelectJob(job.id)"
            >
              <td class="job-id-cell"><code>{{ shortId(job.id) }}</code></td>
              <td>{{ accountDisplayName(job.connectedAccountId) }}</td>
              <td><span class="mode-tag" :class="(job.publishMode ?? '').toString().toLowerCase()">{{ job.publishMode }}</span></td>
              <td><span class="status-lbl" :class="(job.status ?? '').toString().toLowerCase()">{{ job.status }}</span></td>
              <td>
                <a v-if="job.publishedUrl" :href="job.publishedUrl" target="_blank" class="showcase-link">Open</a>
                <span v-else>-</span>
              </td>
              <td class="error-cell">{{ job.lastErrorMessage || '-' }}</td>
              <td>
                <div class="job-actions" @click.stop>
                  <button type="button" class="action-btn-copy" @click="handleRefreshJob(job.id)">Status</button>
                  <button
                    v-if="isRetryableStatus(job.status)"
                    type="button"
                    class="action-btn-retry"
                    @click="handleRetryJob(job.id)"
                  >
                    Retry
                  </button>
                  <button
                    v-if="isCancellableStatus(job.status)"
                    type="button"
                    class="action-btn-delete"
                    @click="handleCancelJob(job.id)"
                  >
                    Cancel
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="pagination">
          <button type="button" class="page-btn" :disabled="monitorPage <= 1" @click="monitorPage--">Prev</button>
          <span>Page {{ monitorPage }}</span>
          <button
            type="button"
            class="page-btn"
            :disabled="publicationStore.jobs.length < monitorPageSize"
            @click="monitorPage++"
          >
            Next
          </button>
        </div>
      </div>

      <div v-if="selectedJobId" class="job-details-panel glass-card">
        <div class="section-header">
          <h3>Attempts for {{ shortId(selectedJobId) }}</h3>
        </div>
        <table class="attempts-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Success</th>
              <th>HTTP</th>
              <th>Category</th>
              <th>Request ID</th>
              <th>Retry</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="publicationStore.attempts.length === 0">
              <td colspan="6" class="no-data">No attempts recorded.</td>
            </tr>
            <tr v-for="attempt in publicationStore.attempts" :key="attempt.id">
              <td>{{ attempt.attemptNumber }}</td>
              <td>{{ attempt.isSuccess ? 'Yes' : 'No' }}</td>
              <td>{{ attempt.providerHttpStatus || '-' }}</td>
              <td>{{ attempt.normalizedErrorCategory || '-' }}</td>
              <td><code>{{ attempt.providerRequestId || '-' }}</code></td>
              <td>{{ attempt.retryDecision || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="activeTab === 'video'" class="video-publish-grid animate-fade">
      <VideoStudioPanel />
      <TikTokPublishPanel
        :post-id="tikTokPanelPostId"
        :caption="tikTokPanelCaption"
        :video-url="tiktokVideoUrl"
        :is-aigc="isAiGeneratedContent"
      />
    </div>

    <div v-if="showGroupModal" class="modal-overlay" @click.self="showGroupModal = false">
      <div class="modal-content glass-card animate-fade">
        <h3>📣 Đăng bài lên nhóm Facebook</h3>
        <p class="modal-desc">Nội dung đã được sao chép vào Clipboard. Bấm vào mỗi nhóm để mở Facebook và dán nội dung (Ctrl+V):</p>

        <!-- Auto-fill preview -->
        <div v-if="displayedText" class="autofill-preview">
          <div class="autofill-label">📝 Nội dung sẽ đăng:</div>
          <div class="autofill-text">{{ displayedText.substring(0, 200) }}{{ displayedText.length > 200 ? '...' : '' }}</div>
        </div>

        <div class="group-links-list">
          <a
            v-for="(groupId, index) in activeGroups"
            :key="groupId"
            :href="'https://www.facebook.com/groups/' + groupId"
            target="_blank"
            class="group-link-item"
            @click="toastStore.success('Đã mở nhóm', 'Hãy nhấn Ctrl+V để dán và đăng bài.')"
          >
            🔗 Nhóm {{ index + 1 }} — ID: {{ groupId }}
          </a>
        </div>

        <div class="modal-actions">
          <button type="button" class="btn-cancel" @click="showGroupModal = false">Đóng</button>
        </div>
      </div>
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

.form-input-text {
  height: 38px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
  transition: border-color var(--duration-fast);
}

.form-input-text:focus {
  border-color: var(--color-yellow);
  outline: none;
}

.input-help {
  margin: 0;
  font-size: 11px;
  color: var(--color-text-muted);
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

.publish-btn {
  background-color: var(--color-ai);
  color: #ffffff;
  border: none;
  font-size: 11px;
  font-weight: 600;
  height: 28px;
  padding: 0 12px;
  border-radius: 6px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.publish-btn:hover {
  opacity: 0.9;
}

.publish-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.action-btn-publish {
  background: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  color: var(--color-ai);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
}

.action-btn-publish:hover {
  opacity: 0.8;
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

/* Batdongsan Mockup */
.batdongsan-mockup {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border-color: #005a3c;
}

.bds-header {
  background-color: #005a3c;
  color: #ffffff;
  padding: 10px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.bds-header .logo {
  font-weight: 700;
  font-size: 13px;
}

.bds-header .tagline {
  font-size: 10.5px;
  opacity: 0.8;
}

.bds-content-area {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.bds-title {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.4;
}

.bds-meta-strip {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: var(--color-text-secondary);
  border-bottom: 1px dashed var(--color-border);
  padding-bottom: 12px;
}

.bds-meta-strip .red-text {
  color: #e03c3c;
  font-weight: 700;
}

.bds-body-text {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 10px;
}

/* Chotot Mockup */
.chotot-mockup {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border-color: #ffba00;
}

.chotot-header {
  background-color: #ffba00;
  color: #1e293b;
  padding: 10px 16px;
  display: flex;
  align-items: center;
}

.chotot-header .ct-logo {
  font-weight: 800;
  font-size: 13px;
}

.chotot-content {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ct-title {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.4;
}

.ct-price-strip {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px dashed var(--color-border);
  padding-bottom: 12px;
}

.ct-price {
  color: #ff5a00;
  font-weight: 800;
  font-size: 15px;
}

.ct-tag-broker {
  background-color: var(--color-divider);
  color: var(--color-text-secondary);
  font-size: 10px;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 600;
}

.ct-body-text {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 10px;
}

/* Alonhadat Mockup */
.alonhadat-mockup {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border-color: #0076a3;
}

.alonhadat-header {
  background-color: #0076a3;
  color: #ffffff;
  padding: 10px 16px;
  display: flex;
  align-items: center;
}

.alonhadat-header .ald-logo {
  font-weight: 800;
  font-size: 13px;
  letter-spacing: 0.5px;
}

.alonhadat-content {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ald-title {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.4;
}

.ald-specs {
  border-bottom: 1px dashed var(--color-border);
  padding-bottom: 12px;
}

.ald-specs-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 11.5px;
  color: var(--color-text-secondary);
}

.ald-specs-table td {
  padding: 4px 0;
  width: 50%;
}

.ald-specs-table strong {
  color: var(--color-text-muted);
}

.ald-body-text {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 10px;
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
.channel-tag.batdongsan { background-color: rgba(0, 90, 60, 0.15); color: #005a3c; }
.channel-tag.chotot { background-color: rgba(255, 186, 0, 0.15); color: #d97706; }
.channel-tag.alonhadat { background-color: rgba(0, 118, 163, 0.15); color: #0076a3; }
.channel-tag.website { background-color: rgba(251, 191, 36, 0.15); color: var(--color-yellow); }

.status-lbl {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
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

/* Modal styles for manual Facebook group links popup */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(3, 7, 18, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.modal-content {
  width: 420px;
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: var(--elevation-floating);
}

.modal-content h3 {
  margin: 0;
  font-size: 15px;
  color: var(--color-text-primary);
}

.modal-desc {
  font-size: 12.5px;
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
}

.group-links-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 10px 0;
}

.group-link-item {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 40px;
  border-radius: 8px;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-weight: 700;
  font-size: 13px;
  text-decoration: none;
  transition: all var(--duration-fast);
  box-shadow: var(--color-yellow-glow);
}

.group-link-item:hover {
  background-color: var(--color-yellow-hover);
  transform: translateY(-2px);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
}

.btn-cancel {
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.btn-cancel:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

/* History table styles */
.action-btn-delete {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.2);
  color: #ef4444;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.action-btn-delete:hover {
  background-color: #ef4444;
  color: #ffffff;
  border-color: #ef4444;
}

/* Navigation Tabs styles */
.tabs-navigation {
  display: flex;
  gap: 12px;
  padding: 12px 20px;
  margin-bottom: 24px;
  border-radius: 12px;
}

.nav-tab-btn {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  font-size: 13px;
  font-weight: 600;
  padding: 10px 20px;
  border-radius: 8px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.nav-tab-btn:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.nav-tab-btn.active {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border-color: var(--color-yellow);
  box-shadow: var(--color-yellow-glow);
}

/* Monitor Panel styles */
.monitor-panel {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.monitor-controls {
  padding: 24px;
}

.filter-bar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 20px;
  gap: 16px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-group label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.filter-select {
  height: 38px;
  width: 240px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.refresh-btn {
  height: 38px;
  border: none;
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  font-size: 12.5px;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  padding: 0 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all var(--duration-fast);
}

.refresh-btn:hover {
  background-color: var(--color-yellow-hover);
}

.monitor-table, .attempts-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12px;
}

.monitor-table th, .attempts-table th {
  padding: 12px;
  font-size: 10.5px;
  text-transform: uppercase;
  color: var(--color-text-muted);
  border-bottom: 1px solid var(--color-border);
}

.monitor-table td, .attempts-table td {
  padding: 12px;
  border-bottom: 1px solid var(--color-divider);
  color: var(--color-text-secondary);
}

.clickable-row {
  cursor: pointer;
  transition: background var(--duration-fast);
}

.clickable-row:hover {
  background-color: var(--color-surface-hover);
}

.clickable-row.selected {
  background-color: rgba(251, 191, 36, 0.08);
}

.job-id-cell code, .attempts-table code {
  background-color: var(--color-divider);
  padding: 2px 6px;
  border-radius: 4px;
  font-family: monospace;
}

.mode-tag {
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.mode-tag.direct { background-color: rgba(59, 130, 246, 0.15); color: #3b82f6; }
.mode-tag.scheduled { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.mode-tag.draftupload { background-color: rgba(168, 85, 247, 0.15); color: #a855f7; }
.mode-tag.assisted { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }

.status-lbl.pending { background-color: rgba(148, 163, 184, 0.15); color: #94a3b8; }
.status-lbl.queued { background-color: rgba(148, 163, 184, 0.25); color: #94a3b8; }
.status-lbl.validating { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.status-lbl.needsreview { background-color: rgba(239, 68, 68, 0.15); color: #ef4444; }
.status-lbl.publishing { background-color: rgba(168, 85, 247, 0.15); color: #a855f7; }
.status-lbl.published { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }
.status-lbl.failed { background-color: rgba(239, 68, 68, 0.15); color: #ef4444; }
.status-lbl.retryscheduled { background-color: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.status-lbl.cancelled { background-color: rgba(100, 116, 139, 0.15); color: #64748b; }

.showcase-link {
  color: var(--color-yellow);
  text-decoration: none;
  font-weight: 600;
}

.showcase-link:hover {
  text-decoration: underline;
}

.error-cell {
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #ef4444 !important;
}

.action-btn-retry {
  background: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  color: var(--color-ai);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
  transition: all var(--duration-fast);
}

.action-btn-retry:hover {
  background-color: var(--color-ai);
  color: #ffffff;
}

.no-data {
  text-align: center;
  padding: 24px;
  color: var(--color-text-muted);
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}

.page-btn {
  height: 30px;
  padding: 0 12px;
  border: 1px solid var(--color-border);
  background: transparent;
  color: var(--color-text-secondary);
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.job-details-panel {
  padding: 24px;
}

.queue-test-form {
  display: grid;
  grid-template-columns: 1.3fr 1.2fr 0.8fr 1.2fr auto;
  gap: 12px;
  align-items: end;
  margin-bottom: 20px;
}

.queue-test-form input,
.queue-test-form select {
  height: 38px;
  border: 1px solid var(--color-border);
  background-color: var(--color-canvas);
  border-radius: 8px;
  padding: 0 12px;
  font-size: 12.5px;
  color: var(--color-text-primary);
}

.job-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.video-publish-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) minmax(320px, 0.8fr);
  gap: 24px;
}

/* AI Diagnostics & Fact Grounding Panels */
.ai-diagnostics-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-top: 20px;
}

.contradiction-alert {
  background-color: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 8px;
  padding: 16px;
  text-align: left;
}

.contradiction-alert .alert-title {
  font-size: 13px;
  font-weight: 700;
  color: #ef4444;
  margin-bottom: 8px;
}

.contradiction-alert .alert-list {
  margin: 0;
  padding-left: 20px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.facts-grounding-card {
  padding: 16px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  text-align: left;
}

.facts-grounding-card .card-header-icon {
  font-size: 13px;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--color-text-primary);
}

.facts-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.fact-item-badge {
  background: var(--color-divider);
  border: 1px solid var(--color-border);
  padding: 6px 12px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
}

.fact-field {
  color: var(--color-text-muted);
  font-weight: 600;
  text-transform: capitalize;
}

.fact-value {
  color: var(--color-text-primary);
  font-weight: 600;
}

.no-facts-text {
  font-size: 12px;
  color: var(--color-text-muted);
  font-style: italic;
}

.usage-stats-card {
  padding: 16px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-glass);
  text-align: left;
}

.usage-stats-card .card-header-icon {
  font-size: 13px;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--color-text-primary);
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 16px;
}

.stat-box {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-lbl {
  font-size: 11px;
  color: var(--color-text-muted);
}

.stat-val {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.text-green {
  color: #10b981 !important;
}

.apply-btn {
  background-color: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  font-size: 11px;
  font-weight: 600;
  height: 28px;
  padding: 0 12px;
  border-radius: 6px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  transition: all var(--duration-fast);
}

.apply-btn:hover {
  background-color: var(--color-yellow-hover);
}

.apply-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

