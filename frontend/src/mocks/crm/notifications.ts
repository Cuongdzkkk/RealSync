import type { CrmNotification, NotificationType } from '@/types/crm/notification';

const now = new Date();

function at(offsetMs: number): string {
  return new Date(now.getTime() + offsetMs).toISOString();
}

const hour = 60 * 60 * 1000;
const day = 24 * hour;

function data(value: Record<string, unknown>): string {
  return JSON.stringify(value);
}

function item(
  id: string,
  userId: string,
  type: NotificationType,
  title: string,
  message: string,
  createdAt: string,
  options: Pick<CrmNotification, 'isRead' | 'readAt' | 'data' | 'link'>
): CrmNotification {
  return { id, userId, type, title, message, createdAt, ...options };
}

export const mockNotifications: CrmNotification[] = [
  item('noti-admin-001', 'user-admin', 'System', 'Bản build CRM đã sẵn sàng', 'Module khách hàng đã được bật trong workspace.', at(-30 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'SystemRelease', version: 'crm-ui-phase-2' }),
    link: '/admin/customers'
  }),
  item('noti-admin-002', 'user-admin', 'Lead', 'Lead mới được phân công', 'Lead Nguyễn Hoàng Minh đã được chuyển về nhóm Admin xử lý.', at(-2 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadAssigned', leadId: 'lead-001', assignedToId: 'user-admin' }),
    link: '/leads/lead-001'
  }),
  item('noti-admin-003', 'user-admin', 'Task', 'Lịch follow-up quá hạn', 'Lead Trần Minh Anh đã quá hạn chăm sóc 1 ngày.', at(-27 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'FollowUpDue', leadId: 'lead-002', scheduledFor: at(-day) }),
    link: '/admin/leads/lead-002'
  }),
  item('noti-admin-004', 'user-admin', 'Property', 'Sản phẩm được cập nhật', 'Căn hộ The River đã cập nhật trạng thái pháp lý.', at(-2 * day), {
    isRead: true,
    readAt: at(-36 * hour),
    data: data({ eventType: 'PropertyUpdated', propertyId: 'property-001' }),
    link: '/properties/property-001'
  }),
  item('noti-admin-005', 'user-admin', 'Assignment', 'Phân công khách hàng', 'Khách hàng Lê Thu Trang được gán cho Sales Anh.', at(-3 * day), {
    isRead: true,
    readAt: at(-60 * hour),
    data: data({ eventType: 'CustomerAssigned', customerId: 'customer-001', assignedToId: 'user-sales-01' }),
    link: '/customers/customer-001'
  }),
  item('noti-admin-006', 'user-admin', 'Lead', 'Lead đổi trạng thái', 'Lead Phạm Quốc Huy chuyển sang Qualified.', at(-4 * day), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadStatusChanged', leadId: 'lead-003', oldStatus: 'Contacted', newStatus: 'Qualified' }),
    link: '/admin/leads/lead-003'
  }),
  item('noti-admin-007', 'user-admin', 'System', 'Dữ liệu thông báo lỗi JSON', 'Thông báo này dùng để kiểm tra xử lý data không hợp lệ.', at(-5 * day), {
    isRead: true,
    readAt: at(-4 * day),
    data: '{ "eventType": "BrokenJson", ',
    link: '/admin/notifications'
  }),
  item('noti-admin-008', 'user-admin', 'System', 'Đường dẫn ngoài bị chặn', 'Liên kết ngoài sẽ quay về trang thông báo.', at(-6 * day), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'UnsafeLink' }),
    link: 'https://example.com/phishing'
  }),
  item('noti-admin-009', 'user-admin', 'Lead', 'Đường dẫn script bị chặn', 'Thông báo kiểm thử fallback an toàn.', at(-7 * day), {
    isRead: true,
    readAt: at(-6 * day),
    data: data({ eventType: 'UnsafeLink', leadId: 'lead-004' }),
    link: 'javascript:alert(1)'
  }),
  item('noti-manager-001', 'user-manager', 'Lead', 'Lead cần duyệt chất lượng', 'Lead Bùi Gia Hân đạt score Hot và cần review.', at(-45 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadScoreHot', leadId: 'lead-005', score: 82 }),
    link: '/leads/lead-005'
  }),
  item('noti-manager-002', 'user-manager', 'Task', 'Follow-up hôm nay', 'Cuộc gọi với khách hàng Nguyễn Lan Anh đến hạn trong hôm nay.', at(-5 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'FollowUpDue', leadId: 'lead-006', scheduledFor: at(3 * hour) }),
    link: null
  }),
  item('noti-manager-003', 'user-manager', 'Property', 'Sản phẩm được phân công', 'Biệt thự Palm Garden được gán cho nhóm của bạn.', at(-26 * hour), {
    isRead: true,
    readAt: at(-20 * hour),
    data: data({ eventType: 'PropertyAssigned', propertyId: 'property-002', assignedToId: 'user-manager' }),
    link: '/properties/property-002'
  }),
  item('noti-manager-004', 'user-manager', 'Assignment', 'Sales Anh nhận khách hàng mới', 'Khách hàng từ Lead đã chuyển cho Sales Anh.', at(-2 * day), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'CustomerAssigned', customerId: 'customer-002', assignedToId: 'user-sales-01' }),
    link: '/admin/customers/customer-002'
  }),
  item('noti-manager-005', 'user-manager', 'System', 'Kiểm tra phân quyền Viewer', 'Thông báo chỉ đọc cho nhóm quản lý.', at(-3 * day), {
    isRead: true,
    readAt: at(-2 * day),
    data: data({ eventType: 'PermissionReview' }),
    link: '/admin/settings'
  }),
  item('noti-manager-006', 'user-manager', 'Lead', 'Lead mất cơ hội', 'Lead Đặng Minh chuyển sang Lost.', at(-5 * day), {
    isRead: true,
    readAt: at(-4 * day),
    data: data({ eventType: 'LeadStatusChanged', leadId: 'lead-007', newStatus: 'Lost' }),
    link: '/admin/leads/lead-007'
  }),
  item('noti-sales-001', 'user-sales-01', 'Lead', 'Bạn được giao Lead mới', 'Lead Võ Thanh Tùng đang quan tâm căn hộ Quận 2.', at(-20 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadAssigned', leadId: 'lead-008', assignedToId: 'user-sales-01' }),
    link: '/leads/lead-008'
  }),
  item('noti-sales-002', 'user-sales-01', 'Task', 'Follow-up quá hạn', 'Cuộc hẹn với Lead Mai Hà đã quá hạn 3 giờ.', at(-4 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'FollowUpDue', leadId: 'lead-009', scheduledFor: at(-3 * hour) }),
    link: '/admin/leads/lead-009'
  }),
  item('noti-sales-003', 'user-sales-01', 'Lead', 'Lead đổi trạng thái', 'Lead Trương Khoa chuyển sang Proposal.', at(-30 * hour), {
    isRead: true,
    readAt: at(-26 * hour),
    data: data({ eventType: 'LeadStatusChanged', leadId: 'lead-010', oldStatus: 'Qualified', newStatus: 'Proposal' }),
    link: '/admin/leads/lead-010'
  }),
  item('noti-sales-004', 'user-sales-01', 'Property', 'Sản phẩm phù hợp Lead', 'Nhà phố Lakeview có thể phù hợp với Lead hiện tại.', at(-2 * day), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'PropertyMatched', leadId: 'lead-011', propertyId: 'property-003' }),
    link: '/properties/property-003'
  }),
  item('noti-sales-005', 'user-sales-01', 'Assignment', 'Khách hàng được phân công', 'Bạn nhận chăm sóc khách hàng Đỗ Hồng Nhung.', at(-3 * day), {
    isRead: true,
    readAt: at(-2 * day),
    data: data({ eventType: 'CustomerAssigned', customerId: 'customer-003', assignedToId: 'user-sales-01' }),
    link: '/customers/customer-003'
  }),
  item('noti-sales-006', 'user-sales-01', 'System', 'Đường dẫn data bị chặn', 'Thông báo kiểm thử link data URI.', at(-6 * day), {
    isRead: true,
    readAt: at(-5 * day),
    data: data({ eventType: 'UnsafeLink' }),
    link: 'data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg=='
  }),
  item('noti-agent-001', 'user-agent-01', 'Lead', 'Lead mới trong khu vực', 'Lead Hồ Minh cần tư vấn dự án Thủ Đức.', at(-55 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadAssigned', leadId: 'lead-012', assignedToId: 'user-agent-01' }),
    link: '/admin/leads/lead-012'
  }),
  item('noti-agent-002', 'user-agent-01', 'Task', 'Lịch hẹn sắp tới', 'Cuộc hẹn xem nhà bắt đầu trong 2 giờ.', at(-3 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'TaskDue', taskId: 'task-001', leadId: 'lead-013', scheduledFor: at(2 * hour) }),
    link: null
  }),
  item('noti-agent-003', 'user-agent-01', 'Property', 'Cập nhật giá bán', 'Property Eco Green cập nhật giá mới.', at(-25 * hour), {
    isRead: true,
    readAt: at(-18 * hour),
    data: data({ eventType: 'PropertyUpdated', propertyId: 'property-004' }),
    link: '/admin/properties/property-004'
  }),
  item('noti-agent-004', 'user-agent-01', 'Assignment', 'Bạn được gán nhiệm vụ', 'Chuẩn bị hồ sơ tư vấn cho Lead Nguyễn Phúc.', at(-2 * day), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'TaskAssigned', taskId: 'task-002', assignedToId: 'user-agent-01' }),
    link: '/admin/notifications'
  }),
  item('noti-agent-005', 'user-agent-01', 'System', 'Thông báo hệ thống', 'Workspace sẽ bảo trì mock data trong đêm nay.', at(-4 * day), {
    isRead: true,
    readAt: at(-3 * day),
    data: data({ eventType: 'Maintenance' }),
    link: '/admin/notifications'
  }),
  item('noti-viewer-001', 'user-viewer', 'System', 'Chế độ chỉ đọc', 'Bạn đang xem notification center với quyền Viewer.', at(-35 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'ViewerMode' }),
    link: '/admin/notifications'
  }),
  item('noti-viewer-002', 'user-viewer', 'Lead', 'Lead có cập nhật mới', 'Lead demo đã chuyển sang Contacted.', at(-4 * hour), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'LeadStatusChanged', leadId: 'lead-014', newStatus: 'Contacted' }),
    link: '/leads/lead-014'
  }),
  item('noti-viewer-003', 'user-viewer', 'Property', 'Property link không hợp lệ', 'Đường dẫn lạ sẽ được đưa về notification center.', at(-2 * day), {
    isRead: true,
    readAt: at(-36 * hour),
    data: data({ eventType: 'UnknownPath', propertyId: 'property-005' }),
    link: '/unknown/place'
  }),
  item('noti-marketing-001', 'user-marketing', 'System', 'Chiến dịch mới', 'Landing page dự án mới đã sẵn sàng để review.', at(-90 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'CampaignReady' }),
    link: '/admin/content-ai'
  }),
  item('noti-marketing-002', 'user-marketing', 'Lead', 'Lead từ Facebook', 'Lead mới đến từ chiến dịch Facebook Ads.', at(-32 * hour), {
    isRead: true,
    readAt: at(-28 * hour),
    data: data({ eventType: 'LeadCreated', leadId: 'lead-015', source: 'Facebook' }),
    link: '/admin/leads/lead-015'
  }),
  item('noti-analyst-001', 'user-data-analyst', 'System', 'Báo cáo dữ liệu', 'Bộ lọc dashboard đã được cập nhật.', at(-75 * 60 * 1000), {
    isRead: false,
    readAt: null,
    data: data({ eventType: 'AnalyticsUpdated' }),
    link: '/admin/dashboard'
  }),
  item('noti-analyst-002', 'user-data-analyst', 'Property', 'Crawler cập nhật property', 'Crawler ghi nhận 18 sản phẩm mới.', at(-3 * day), {
    isRead: true,
    readAt: at(-2 * day),
    data: data({ eventType: 'CrawlerPropertyUpdate', propertyId: 'property-006' }),
    link: '/admin/crawlers'
  })
];
