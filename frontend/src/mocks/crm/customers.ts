import type {
  CrmCustomerDetail,
  CustomerActivityAction,
  CustomerActivityLog,
  CustomerSource
} from '@/types/crm/customer';
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
  customerId: string,
  action: CustomerActivityAction,
  description: string,
  daysAgo: number,
  oldValues?: Record<string, unknown> | string | null,
  newValues?: Record<string, unknown> | string | null
): CustomerActivityLog {
  return {
    id: `${customerId}-${action}-${daysAgo}-${Math.random().toString(36).slice(2, 5)}`,
    userId: 'user-manager',
    userName: 'Trần Hà Manager',
    entityType: 'Customer',
    entityId: customerId,
    action,
    description,
    oldValues: typeof oldValues === 'string' ? oldValues : oldValues ? JSON.stringify(oldValues) : null,
    newValues: typeof newValues === 'string' ? newValues : newValues ? JSON.stringify(newValues) : null,
    createdAt: daysFromNow(-daysAgo, 11)
  };
}

interface CustomerSeed {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  company?: string | null;
  notes?: string | null;
  source?: CustomerSource | null;
  assignedToId?: string | null;
  convertedFromLeadId?: string | null;
  convertedFromLeadName?: string | null;
}

const seeds: CustomerSeed[] = [
  ['customer-001', 'Nguyễn Quốc Huy', '0901234567', 'huy.nguyen@example.com', 'Thủ Thiêm, TP. Thủ Đức', 'HQ Investment', 'Quan tâm căn hộ cao cấp view sông.', 'Website', 'user-sales-01', 'lead-001', 'Nguyễn Quốc Huy'],
  ['customer-002', 'Trần Mai Anh', '0912345678', 'maianh@example.com', 'Quận 7, TP.HCM', null, 'Gia đình cần nhà phố gần trường quốc tế.', 'Facebook', 'user-agent-01', 'lead-002', 'Trần Mai Anh'],
  ['customer-003', 'Lê Văn Kiên', null, 'kien.le@example.com', 'Biên Hòa, Đồng Nai', 'Kiên Land', 'Đầu tư đất nền pháp lý sạch.', 'Referral', 'user-manager', 'lead-003', 'Lê Văn Kiên'],
  ['customer-004', 'Phạm Thảo Vy', '0934567890', null, 'Bình Thạnh, TP.HCM', 'Vy Retail', 'Tìm shophouse mặt tiền.', 'Phone', 'user-sales-01', 'lead-004', 'Phạm Thảo Vy'],
  ['customer-005', 'Hoàng Minh Đức', '0945678901', 'duc.hoang@example.com', 'Biên Hòa', null, 'Khách đã mua chung cư cho gia đình.', 'Zalo', 'user-agent-01', 'lead-005', 'Hoàng Minh Đức'],
  ['customer-006', 'Bùi Gia Bảo', '0967890123', 'bao.bui@example.com', 'Quận 9', 'Bảo Holdings', 'Khách hàng trực tiếp tìm BĐS đầu tư.', 'Website', null, null, null],
  ['customer-007', 'Vũ Ngọc Linh', '0978901234', 'linh.vu@example.com', 'Quận 1', 'Linh Studio', 'Quan tâm penthouse trung tâm.', 'Facebook', 'user-manager', 'lead-008', 'Vũ Ngọc Linh'],
  ['customer-008', 'Đỗ Thanh Sơn', '0989012345', null, 'Quận 4', null, 'Mua căn hộ cho thuê Airbnb.', 'Zalo', 'user-sales-01', 'lead-009', 'Đỗ Thanh Sơn'],
  ['customer-009', 'Mai Hồng Phúc', null, 'phuc.mai@example.com', 'Bình Thạnh', 'Phúc Gia', 'Nhà phố hẻm xe hơi.', 'Phone', 'user-agent-01', 'lead-010', 'Mai Hồng Phúc'],
  ['customer-010', 'Ngô Khánh An', '0909988776', 'an.ngo@example.com', 'Long An', null, 'Khách trực tiếp hỏi đất vườn.', 'Other', null, null, null],
  ['customer-011', 'Hồ Nhật Nam', '0918877665', 'nam.ho@example.com', 'Thủ Đức', 'Nam Tech', 'Căn hộ 3PN gần metro.', 'Website', 'user-sales-01', null, null],
  ['customer-012', 'Cao Minh Châu', '0927766554', null, 'Nhà Bè', 'Châu Group', 'Biệt thự compound.', 'Referral', 'user-manager', 'lead-013', 'Cao Minh Châu'],
  ['customer-013', 'Tạ Quỳnh Như', null, 'nhu.ta@example.com', 'Quận 2', null, 'Officetel cho công ty mới.', 'Facebook', 'user-agent-01', 'lead-014', 'Tạ Quỳnh Như'],
  ['customer-014', 'Đinh Tuấn Anh', '0936655443', 'tuananh@example.com', 'Phú Mỹ Hưng', 'TA Coffee', 'Shophouse khai thác cafe.', 'Website', 'user-sales-01', 'lead-015', 'Đinh Tuấn Anh'],
  ['customer-015', 'Chu Hoàng Long', '0954433221', null, 'Quận 3', 'Long Build', 'Mua nhanh nhà phố Quận 3.', 'Phone', 'user-manager', 'lead-017', 'Chu Hoàng Long'],
  ['customer-016', 'Lâm Minh Thư', null, 'thu.lam@example.com', 'Gò Vấp', null, 'Thuê mặt bằng spa nhỏ.', 'Other', 'user-sales-01', null, null],
  ['customer-017', 'Phan Đức Tài', '0963322110', 'tai.phan@example.com', 'Hồ Tràm', 'Tài Resort', 'Đầu tư căn hộ biển.', 'Referral', 'user-agent-01', 'lead-019', 'Phan Đức Tài'],
  ['customer-018', 'Kiều Thanh Hà', '0972211009', 'ha.kieu@example.com', 'Quận 10', null, 'Căn hộ bàn giao ngay.', 'Website', 'user-sales-01', 'lead-020', 'Kiều Thanh Hà'],
  ['customer-019', 'Tống Minh Khang', '0981100998', null, 'Long Thành', 'Khang Logistics', 'Đất nền quanh sân bay.', 'Facebook', null, null, null],
  ['customer-020', 'Văn Hải Yến', null, 'yen.van@example.com', 'Quận 2', null, 'Căn hộ cao cấp cho chuyên gia thuê.', 'Zalo', 'user-manager', 'lead-022', 'Văn Hải Yến'],
  ['customer-021', 'Dương Nhật Minh', '0901122334', 'minh.duong@example.com', 'Tân Phú', 'Minh Decor', 'Nhà phố khu dân cư.', 'Phone', 'user-agent-01', 'lead-023', 'Dương Nhật Minh'],
  ['customer-022', 'Hà Phương Uyên', '0912233445', 'uyen.ha@example.com', 'Thủ Đức', null, 'BĐS đầu tư dòng tiền thuê.', 'Referral', 'user-sales-01', 'lead-024', 'Hà Phương Uyên'],
  ['customer-023', 'Lương Bảo Chi', '0923344556', null, 'Quận 5', 'Chi Boutique', 'Khách trực tiếp mua mặt bằng kinh doanh.', 'Phone', null, null, null],
  ['customer-024', 'Trịnh Hữu Lộc', null, 'loc.trinh@example.com', 'Bình Dương', 'Lộc Manufacturing', 'Mua đất xây kho nhỏ.', 'Website', 'user-manager', null, null]
].map((row) => {
  const [
    id,
    fullName,
    phone,
    email,
    address,
    company,
    notes,
    source,
    assignedToId,
    convertedFromLeadId,
    convertedFromLeadName
  ] = row;
  return {
    id,
    fullName,
    phone,
    email,
    address,
    company,
    notes,
    source,
    assignedToId,
    convertedFromLeadId,
    convertedFromLeadName
  };
}) as CustomerSeed[];

export const mockCustomers: CrmCustomerDetail[] = seeds.map((seed, index) => {
  const createdAt = daysFromNow(-35 + index, 9);
  const updatedAt = index % 3 === 0 ? daysFromNow(-4 + (index % 4), 15) : createdAt;
  const assignedToName = userName(seed.assignedToId);
  const activities: CustomerActivityLog[] = [
    activity(seed.id, 'Create', seed.convertedFromLeadId ? 'Tạo Customer từ Lead đã chuyển đổi.' : 'Tạo Customer trực tiếp.', 20 - (index % 10))
  ];

  if (index % 2 === 0) {
    activities.unshift(activity(seed.id, 'Update', 'Cập nhật thông tin liên hệ Customer.', 6, { phone: null }, { phone: seed.phone }));
  }
  if (seed.assignedToId) {
    activities.unshift(activity(seed.id, 'Assignment', `Phân công cho ${assignedToName}.`, 4, { assignedToName: null }, { assignedToName }));
  }
  if (index % 5 === 0) {
    activities.unshift(activity(seed.id, 'Update', 'Bổ sung ghi chú tư vấn.', 2, '{invalid-json', { notes: seed.notes }));
  }

  return {
    ...seed,
    assignedToName,
    createdAt,
    updatedAt,
    activities
  };
});
