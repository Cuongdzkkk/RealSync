export interface ConnectedAccount {
  id: string;
  provider: string;
  channelType: string;
  displayName: string;
  externalAccountId: string;
  externalParentAccountId: string | null;
  profileUrl: string | null;
  avatarUrl: string | null;
  status: string;
  grantedScopesJson: string | null;
  tokenExpiresAt: string | null;
  lastValidatedAt: string | null;
  lastErrorCode: string | null;
  lastErrorMessage: string | null;
  createdAt: string;
}

export interface ConnectedAccountCreateRequest {
  provider: string;
  channelType: string;
  displayName: string;
  externalAccountId: string;
  externalParentAccountId?: string | null;
  profileUrl?: string | null;
  avatarUrl?: string | null;
  accessToken: string;
  refreshToken?: string | null;
  expiresInSeconds?: number | null;
  grantedScopesJson?: string | null;
}

export interface ConnectedAccountReconnectRequest {
  accessToken: string;
  refreshToken?: string | null;
  expiresInSeconds?: number | null;
}

export interface ConnectedAccountAuditLog {
  id: string;
  userId: string | null;
  userEmail: string | null;
  action: string;
  description: string | null;
  ipAddress: string | null;
  createdAt: string;
}
