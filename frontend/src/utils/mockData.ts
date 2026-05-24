import type { CrawlSource } from '@/types/crawler';
import type { AiClassificationJob, AiContentItem } from '@/types/ai';
import type { DashboardMetric, MarketPoint } from '@/types/dashboard';
import type { Lead } from '@/types/lead';
import type { MessageThread } from '@/types/message';
import type { Project } from '@/types/project';
import type { Property } from '@/types/property';
import type { RoleCapability, WorkspaceUser } from '@/types/user';

export const mockMetrics: DashboardMetric[] = [
  { label: 'Listing mới', value: 128, trend: '+12%', variant: 'success' },
  { label: 'Lead nóng', value: 34, trend: '+8%', variant: 'danger' },
  { label: 'Máy thu thập', value: 6, trend: 'live', variant: 'info' },
  { label: 'Điểm AI TB', value: 86, suffix: '%', trend: '+3%', variant: 'success' }
];

export const mockMarketTrend: MarketPoint[] = [
  { label: 'Quận 1', value: 92 },
  { label: 'Thủ Đức', value: 118 },
  { label: 'Bình Thạnh', value: 76 },
  { label: 'Quận 7', value: 84 },
  { label: 'Tân Bình', value: 61 }
];

export const mockProperties: Property[] = [
  {
    id: 'p-001',
    title: 'Căn hộ 3PN view sông tại Thủ Thiêm',
    address: 'Thủ Thiêm, TP. Thủ Đức',
    area: 'TP. Thủ Đức',
    price: 12500000000,
    acreage: 118,
    bedrooms: 3,
    status: 'verified',
    source: 'batdongsan.com.vn',
    imageUrl: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=900&q=80',
    aiScore: 94,
    createdAt: '2026-05-24T09:00:00Z'
  },
  {
    id: 'p-002',
    title: 'Nhà phố thương mại mặt tiền Quận 7',
    address: 'Nguyễn Lương Bằng, Quận 7',
    area: 'Quận 7',
    price: 28600000000,
    acreage: 168,
    bedrooms: 5,
    status: 'published',
    source: 'internal',
    imageUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=900&q=80',
    aiScore: 89,
    createdAt: '2026-05-22T11:30:00Z'
  },
  {
    id: 'p-003',
    title: 'Đất nền khu dân cư Bình Chánh',
    address: 'Vĩnh Lộc B, Bình Chánh',
    area: 'Bình Chánh',
    price: 4200000000,
    acreage: 96,
    bedrooms: 0,
    status: 'draft',
    source: 'chotot.com',
    imageUrl: 'https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=900&q=80',
    aiScore: 72,
    createdAt: '2026-05-21T08:15:00Z'
  }
];

export const mockLeads: Lead[] = [
  {
    id: 'l-001',
    fullName: 'Nguyễn Minh Anh',
    phone: '0901234567',
    demand: 'Tìm căn hộ 2-3PN khu Thủ Thiêm',
    budget: 12000000000,
    stage: 'new',
    temperature: 'hot',
    assignedTo: 'Lan',
    lastContactAt: '2026-05-24T06:00:00Z'
  },
  {
    id: 'l-002',
    fullName: 'Trần Quốc Huy',
    phone: '0912345678',
    demand: 'Mua nhà phố khai thác cho thuê',
    budget: 30000000000,
    stage: 'qualified',
    temperature: 'warm',
    assignedTo: 'Huy',
    lastContactAt: '2026-05-23T10:20:00Z'
  },
  {
    id: 'l-003',
    fullName: 'Lê Hoàng Nam',
    phone: '0923456789',
    demand: 'Đất nền vùng ven, pháp lý rõ',
    budget: 5000000000,
    stage: 'viewing',
    temperature: 'cold',
    assignedTo: 'Mai',
    lastContactAt: '2026-05-22T14:45:00Z'
  }
];

export const mockCrawlSources: CrawlSource[] = [
  {
    id: 'c-001',
    name: 'Batdongsan',
    baseUrl: 'https://batdongsan.com.vn',
    isActive: true,
    successRate: 96,
    lastRunAt: '2026-05-24T08:00:00Z',
    listingsToday: 342
  },
  {
    id: 'c-002',
    name: 'Nhadat24h',
    baseUrl: 'https://nhadat24h.net',
    isActive: true,
    successRate: 91,
    lastRunAt: '2026-05-24T07:40:00Z',
    listingsToday: 118
  },
  {
    id: 'c-003',
    name: 'Chotot BĐS',
    baseUrl: 'https://www.chotot.com',
    isActive: false,
    successRate: 78,
    lastRunAt: '2026-05-23T19:10:00Z',
    listingsToday: 0
  }
];

export const mockProjects: Project[] = [
  {
    id: 'prj-001',
    name: 'The River Thủ Thiêm',
    area: 'TP. Thủ Đức',
    status: 'active',
    propertyCount: 38,
    leadCount: 64,
    updatedAt: '2026-05-24T06:20:00Z'
  },
  {
    id: 'prj-002',
    name: 'Phú Mỹ Hưng Midtown',
    area: 'Quận 7',
    status: 'active',
    propertyCount: 24,
    leadCount: 41,
    updatedAt: '2026-05-23T15:10:00Z'
  },
  {
    id: 'prj-003',
    name: 'Khu dân cư Bình Chánh',
    area: 'Bình Chánh',
    status: 'planning',
    propertyCount: 12,
    leadCount: 18,
    updatedAt: '2026-05-22T09:35:00Z'
  }
];

export const mockAiJobs: AiClassificationJob[] = [
  {
    id: 'ai-001',
    target: 'Căn hộ 3PN view sông tại Thủ Thiêm',
    type: 'property',
    status: 'completed',
    confidence: 94,
    result: 'Căn hộ cao cấp · Sale · Premium',
    createdAt: '2026-05-24T09:12:00Z'
  },
  {
    id: 'ai-002',
    target: 'Nguyễn Minh Anh',
    type: 'lead',
    status: 'review',
    confidence: 78,
    result: 'Lead A · High priority · Follow-up 24h',
    createdAt: '2026-05-24T08:40:00Z'
  },
  {
    id: 'ai-003',
    target: 'Đất nền khu dân cư Bình Chánh',
    type: 'property',
    status: 'processing',
    confidence: 62,
    result: 'Chờ kiểm duyệt vì confidence thấp',
    createdAt: '2026-05-24T08:10:00Z'
  }
];

export const mockAiContents: AiContentItem[] = [
  {
    id: 'cnt-001',
    title: 'Mô tả SEO căn hộ Thủ Thiêm 3PN',
    channel: 'seo',
    status: 'review',
    owner: 'Lan',
    updatedAt: '2026-05-24T09:30:00Z'
  },
  {
    id: 'cnt-002',
    title: 'Bài Zalo nhà phố thương mại Quận 7',
    channel: 'zalo',
    status: 'draft',
    owner: 'Mai',
    updatedAt: '2026-05-24T08:50:00Z'
  },
  {
    id: 'cnt-003',
    title: 'Facebook post đất nền Bình Chánh',
    channel: 'facebook',
    status: 'approved',
    owner: 'Huy',
    updatedAt: '2026-05-23T16:00:00Z'
  }
];

export const mockUsers: WorkspaceUser[] = [
  {
    id: 'u-001',
    fullName: 'Nguyễn Quỳnh Lan',
    email: 'lan@realsync.vn',
    role: 'Admin',
    status: 'active',
    lastSeenAt: '2026-05-24T09:45:00Z'
  },
  {
    id: 'u-002',
    fullName: 'Trần Quốc Huy',
    email: 'huy@realsync.vn',
    role: 'Manager',
    status: 'active',
    lastSeenAt: '2026-05-24T08:18:00Z'
  },
  {
    id: 'u-003',
    fullName: 'Lê Mai Anh',
    email: 'mai@realsync.vn',
    role: 'Agent',
    status: 'invited',
    lastSeenAt: '2026-05-22T11:20:00Z'
  },
  {
    id: 'u-004',
    fullName: 'Phạm Minh Khoa',
    email: 'khoa@realsync.vn',
    role: 'Viewer',
    status: 'locked',
    lastSeenAt: '2026-05-20T13:10:00Z'
  }
];

export const mockRoleCapabilities: RoleCapability[] = [
  { module: 'Dashboard & Insight', admin: true, manager: true, agent: true, viewer: true },
  { module: 'Product Management', admin: true, manager: true, agent: false, viewer: true },
  { module: 'Lead Management', admin: true, manager: true, agent: true, viewer: true },
  { module: 'Crawler Engine', admin: true, manager: false, agent: false, viewer: false },
  { module: 'AI Classification', admin: true, manager: true, agent: false, viewer: true },
  { module: 'Content AI Engine', admin: true, manager: true, agent: true, viewer: false },
  { module: 'Users & Roles', admin: true, manager: false, agent: false, viewer: false }
];

export const mockMessages: MessageThread[] = [
  {
    id: 'msg-001',
    senderName: 'Trần Quốc Huy',
    avatarUrl: 'https://images.unsplash.com/photo-1599566150163-29194dcaad36?auto=format&fit=crop&w=100&q=80',
    preview: 'Đã check lại pháp lý cho mảnh đất...',
    timestamp: '2026-05-24T09:15:00Z',
    unread: true
  },
  {
    id: 'msg-002',
    senderName: 'Lê Mai Anh',
    avatarUrl: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=100&q=80',
    preview: 'Khách hàng hẹn xem nhà chiều nay',
    timestamp: '2026-05-24T08:30:00Z',
    unread: true
  },
  {
    id: 'msg-003',
    senderName: 'Phạm Minh Khoa',
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=100&q=80',
    preview: 'Mình đã cập nhật báo cáo thị trường tháng',
    timestamp: '2026-05-23T16:45:00Z',
    unread: false
  },
  {
    id: 'msg-004',
    senderName: 'Nguyễn Quỳnh Lan',
    avatarUrl: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?auto=format&fit=crop&w=100&q=80',
    preview: 'Cần duyệt bài viết content AI cho dự án',
    timestamp: '2026-05-23T14:20:00Z',
    unread: false
  },
  {
    id: 'msg-005',
    senderName: 'Đội Hỗ Trợ Kỹ Thuật',
    avatarUrl: 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?auto=format&fit=crop&w=100&q=80',
    preview: 'Hệ thống crawler đã được nâng cấp',
    timestamp: '2026-05-22T09:00:00Z',
    unread: false
  }
];
