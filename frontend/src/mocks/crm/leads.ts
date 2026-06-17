import type { CrmLead, LeadActivity, LeadPriority, LeadSourceChannel, LeadStatus } from '@/types/crm/lead';
import { getLeadTemperature } from '@/utils/crm';
import { mockCrmUsers } from './users';

const now = new Date();

function daysFromNow(days: number, hours = 9): string {
  const date = new Date(now);
  date.setDate(now.getDate() + days);
  date.setHours(hours, 0, 0, 0);
  return date.toISOString();
}

function userName(id?: string | null): string | null {
  return mockCrmUsers.find((user) => user.id === id)?.fullName ?? null;
}

function activity(
  leadId: string,
  activityType: LeadActivity['activityType'],
  description: string,
  daysAgo: number,
  oldValue?: string | null,
  newValue?: string | null
): LeadActivity {
  return {
    id: `${leadId}-${activityType}-${daysAgo}`,
    leadId,
    activityType,
    description,
    oldValue,
    newValue,
    performedById: 'user-sales-01',
    performedByName: 'Lê Anh Sales',
    createdAt: daysFromNow(-daysAgo, 10)
  };
}

interface LeadSeed {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  status: LeadStatus;
  priority: LeadPriority;
  score: number;
  requirements: string;
  preferredArea: string;
  preferredType: string;
  budget?: number | null;
  assignedToId?: string | null;
  sourceChannel?: LeadSourceChannel | null;
  nextFollowUpAt?: string | null;
}

const seeds: LeadSeed[] = [
  ['lead-001', 'Nguyễn Quốc Huy', '0901234567', 'huy.nguyen@example.com', 'New', 'High', 82, 'Căn hộ 2PN Thủ Thiêm, view sông', 'Thủ Thiêm', 'Căn hộ', 6200000000, 'user-sales-01', 'Website', daysFromNow(-1)],
  ['lead-002', 'Trần Mai Anh', '0912345678', 'maianh@example.com', 'Contacted', 'Normal', 58, 'Nhà phố Quận 7 gần trường quốc tế', 'Quận 7', 'Nhà phố', 12800000000, 'user-agent-01', 'Facebook', daysFromNow(0, 15)],
  ['lead-003', 'Lê Văn Kiên', null, 'kien.le@example.com', 'Qualified', 'Urgent', 91, 'Đất nền Đồng Nai pháp lý rõ', 'Đồng Nai', 'Đất nền', 2300000000, 'user-manager', 'Referral', daysFromNow(2)],
  ['lead-004', 'Phạm Thảo Vy', '0934567890', null, 'Proposal', 'High', 74, 'Shophouse mặt tiền khu dân cư mới', 'Bình Thạnh', 'Shophouse', 18500000000, 'user-sales-01', 'Phone', daysFromNow(5)],
  ['lead-005', 'Hoàng Minh Đức', '0945678901', 'duc.hoang@example.com', 'Won', 'Normal', 66, 'Chung cư Biên Hòa cho gia đình trẻ', 'Biên Hòa', 'Chung cư', 2100000000, 'user-agent-01', 'Zalo', null],
  ['lead-006', 'Đặng Bảo Trâm', '0956789012', 'tram.dang@example.com', 'Lost', 'Low', 32, 'Nhà cho thuê gần khu công nghiệp', 'Dĩ An', 'Nhà cho thuê', 900000000, 'user-sales-01', 'Other', daysFromNow(-3)],
  ['lead-007', 'Bùi Gia Bảo', '0967890123', 'bao.bui@example.com', 'New', 'Normal', 43, 'Bất động sản đầu tư giữ tiền', 'Quận 9', 'Đầu tư', 4500000000, null, 'Website', null],
  ['lead-008', 'Vũ Ngọc Linh', '0978901234', 'linh.vu@example.com', 'Contacted', 'Urgent', 88, 'Penthouse trung tâm có hồ bơi', 'Quận 1', 'Penthouse', 26000000000, 'user-manager', 'Facebook', daysFromNow(-2)],
  ['lead-009', 'Đỗ Thanh Sơn', '0989012345', null, 'Qualified', 'High', 72, 'Căn hộ 1PN cho thuê Airbnb', 'Quận 4', 'Căn hộ', 3900000000, 'user-sales-01', 'Zalo', daysFromNow(1)],
  ['lead-010', 'Mai Hồng Phúc', null, 'phuc.mai@example.com', 'Proposal', 'Normal', 61, 'Nhà phố hẻm xe hơi Bình Thạnh', 'Bình Thạnh', 'Nhà phố', 9300000000, 'user-agent-01', 'Phone', daysFromNow(7)],
  ['lead-011', 'Ngô Khánh An', '0909988776', 'an.ngo@example.com', 'New', 'Low', 28, 'Đất vườn ven đô nghỉ dưỡng', 'Long An', 'Đất vườn', 1600000000, null, 'Other', null],
  ['lead-012', 'Hồ Nhật Nam', '0918877665', 'nam.ho@example.com', 'Contacted', 'High', 69, 'Căn hộ 3PN gần metro', 'Thủ Đức', 'Căn hộ', 7200000000, 'user-sales-01', 'Website', daysFromNow(0, 17)],
  ['lead-013', 'Cao Minh Châu', '0927766554', null, 'Qualified', 'Urgent', 95, 'Biệt thự compound an ninh', 'Nhà Bè', 'Biệt thự', 32000000000, 'user-manager', 'Referral', daysFromNow(3)],
  ['lead-014', 'Tạ Quỳnh Như', null, 'nhu.ta@example.com', 'Proposal', 'High', 77, 'Officetel cho công ty mới', 'Quận 2', 'Officetel', 5100000000, 'user-agent-01', 'Facebook', daysFromNow(-1, 8)],
  ['lead-015', 'Đinh Tuấn Anh', '0936655443', 'tuananh@example.com', 'Won', 'High', 84, 'Shophouse khai thác cafe', 'Phú Mỹ Hưng', 'Shophouse', 22000000000, 'user-sales-01', 'Website', null],
  ['lead-016', 'Lý Bảo Ngọc', '0945544332', 'ngoc.ly@example.com', 'Lost', 'Normal', 37, 'Căn hộ studio giá tốt', 'Tân Bình', 'Studio', 1800000000, 'user-agent-01', 'Zalo', null],
  ['lead-017', 'Chu Hoàng Long', '0954433221', null, 'New', 'Urgent', 81, 'Mua nhanh nhà phố Quận 3', 'Quận 3', 'Nhà phố', 15000000000, 'user-manager', 'Phone', daysFromNow(1, 11)],
  ['lead-018', 'Lâm Minh Thư', null, 'thu.lam@example.com', 'Contacted', 'Low', 39, 'Thuê mặt bằng spa nhỏ', 'Gò Vấp', 'Mặt bằng', 700000000, 'user-sales-01', 'Other', daysFromNow(12)],
  ['lead-019', 'Phan Đức Tài', '0963322110', 'tai.phan@example.com', 'Qualified', 'Normal', 54, 'Đầu tư căn hộ biển Hồ Tràm', 'Hồ Tràm', 'Resort', 5800000000, 'user-agent-01', 'Referral', daysFromNow(4)],
  ['lead-020', 'Kiều Thanh Hà', '0972211009', 'ha.kieu@example.com', 'Proposal', 'Urgent', 89, 'Mua căn hộ bàn giao ngay', 'Quận 10', 'Căn hộ', 6400000000, 'user-sales-01', 'Website', daysFromNow(-4)],
  ['lead-021', 'Tống Minh Khang', '0981100998', null, 'New', 'Normal', 46, 'Đất nền quanh sân bay Long Thành', 'Long Thành', 'Đất nền', 3100000000, null, 'Facebook', null],
  ['lead-022', 'Văn Hải Yến', null, 'yen.van@example.com', 'Contacted', 'High', 71, 'Căn hộ cao cấp cho chuyên gia thuê', 'Quận 2', 'Căn hộ', 8300000000, 'user-manager', 'Zalo', daysFromNow(6)],
  ['lead-023', 'Dương Nhật Minh', '0901122334', 'minh.duong@example.com', 'Qualified', 'Normal', 57, 'Nhà phố khu dân cư an ninh', 'Tân Phú', 'Nhà phố', 7600000000, 'user-agent-01', 'Phone', daysFromNow(8)],
  ['lead-024', 'Hà Phương Uyên', '0912233445', 'uyen.ha@example.com', 'Proposal', 'High', 79, 'Bất động sản đầu tư dòng tiền thuê', 'Thủ Đức', 'Đầu tư', 11800000000, 'user-sales-01', 'Referral', daysFromNow(0, 13)]
].map((row) => {
  const [
    id,
    fullName,
    phone,
    email,
    status,
    priority,
    score,
    requirements,
    preferredArea,
    preferredType,
    budget,
    assignedToId,
    sourceChannel,
    nextFollowUpAt
  ] = row;
  return {
    id,
    fullName,
    phone,
    email,
    status,
    priority,
    score,
    requirements,
    preferredArea,
    preferredType,
    budget,
    assignedToId,
    sourceChannel,
    nextFollowUpAt
  };
}) as LeadSeed[];

export const mockLeads: CrmLead[] = seeds.map((seed, index) => {
  const createdAt = daysFromNow(-20 + index, 9);
  const assignedToName = userName(seed.assignedToId);

  return {
    ...seed,
    interestedPropertyId: `property-${(index % 8) + 1}`,
    interestedPropertyTitle: seed.requirements,
    assignedToName,
    lastContactedAt: index % 3 === 0 ? daysFromNow(-2, 14) : null,
    convertedAt: seed.status === 'Won' ? daysFromNow(-1, 16) : null,
    createdAt,
    updatedAt: daysFromNow(-Math.max(0, 12 - index), 11),
    activities: [
      activity(seed.id, 'Note', `Tiếp nhận nhu cầu: ${seed.requirements}.`, 4),
      ...(seed.assignedToId ? [activity(seed.id, 'Assigned', `Phân công cho ${assignedToName}.`, 3, null, assignedToName)] : []),
      ...(seed.status !== 'New' ? [activity(seed.id, 'StatusChange', `Cập nhật trạng thái sang ${seed.status}.`, 2, 'New', seed.status)] : []),
      ...(seed.nextFollowUpAt ? [activity(seed.id, 'FollowUp', 'Đặt lịch chăm sóc tiếp theo.', 1, null, seed.nextFollowUpAt)] : [])
    ],
    leadTemperature: getLeadTemperature(seed.score)
  } as CrmLead;
});
