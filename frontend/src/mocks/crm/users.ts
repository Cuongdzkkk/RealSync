import type { CrmUserMock } from '@/types/crm/lead';

export const mockCrmUsers: CrmUserMock[] = [
  {
    id: 'user-admin',
    fullName: 'Nguyễn Minh Admin',
    email: 'admin@realsync.vn',
    role: 'Admin',
    isActive: true
  },
  {
    id: 'user-manager',
    fullName: 'Trần Hà Manager',
    email: 'manager@realsync.vn',
    role: 'Manager',
    isActive: true
  },
  {
    id: 'user-sales-01',
    fullName: 'Lê Anh Sales',
    email: 'sales.anh@realsync.vn',
    role: 'Sales',
    isActive: true
  },
  {
    id: 'user-agent-01',
    fullName: 'Phạm Ngọc Agent',
    email: 'agent.ngoc@realsync.vn',
    role: 'Agent',
    isActive: true
  },
  {
    id: 'user-sales-inactive',
    fullName: 'Võ Thu Sales',
    email: 'sales.thu@realsync.vn',
    role: 'Sales',
    isActive: false
  }
];
