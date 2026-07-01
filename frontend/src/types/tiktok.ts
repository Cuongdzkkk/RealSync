export interface TikTokOAuthAuthorizeResponse {
  authorizeUrl: string;
  state: string;
}

export interface TikTokCreatorInfo {
  creatorUsername: string | null;
  creatorNickname: string | null;
  creatorAvatarUrl: string | null;
  privacyLevelOptions: string[];
  commentDisabled: boolean;
  duetDisabled: boolean;
  stitchDisabled: boolean;
  maxVideoPostDurationSec: number;
  isAppAudited: boolean;
  restrictionNote: string | null;
}

export interface ChannelCapabilities {
  supportsDirectPublish: boolean;
  supportsDraftUpload: boolean;
  supportsScheduling: boolean;
  supportsVideo: boolean;
  supportsImages: boolean;
  requiresFinalUserConfirmation: boolean;
  isAppAudited: boolean;
  restrictionReason: string | null;
  grantedScopes: string[];
}

export interface TikTokMediaManifest {
  videoUrl: string;
  videoSizeBytes?: number;
  durationSeconds?: number;
  isAigc: boolean;
  privacyLevel: string;
  userConsentConfirmed: boolean;
  disableComment?: boolean;
  disableDuet?: boolean;
  disableStitch?: boolean;
  videoCoverTimestampMs?: number;
}
