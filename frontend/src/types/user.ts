import type { UserProfile } from './auth';

export interface WorkspaceUser extends UserProfile {
  status: 'active' | 'invited' | 'locked';
  lastSeenAt: string;
}

export interface RoleCapability {
  module: string;
  admin: boolean;
  manager: boolean;
  agent: boolean;
  viewer: boolean;
}
