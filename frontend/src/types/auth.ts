export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Sales' | 'Marketing' | 'Data Analyst' | 'Agent' | 'Viewer';
}

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  user: UserProfile;
}
