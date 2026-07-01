export type PublicationJobStatus = 
  | 'Pending' 
  | 'Queued' 
  | 'Validating' 
  | 'NeedsReview' 
  | 'Publishing' 
  | 'RemoteProcessing'
  | 'Published' 
  | 'RetryScheduled' 
  | 'Failed' 
  | 'Cancelled';

export type PublishMode = 'Direct' | 'DraftUpload' | 'Assisted';

export interface PublicationJob {
  id: string;
  postId: string;
  contentVariantId: string;
  connectedAccountId: string | null;
  publishMode: PublishMode;
  scheduledAtUtc: string | null;
  status: PublicationJobStatus;
  idempotencyKey: string;
  externalPostId: string | null;
  publishedUrl: string | null;
  publishedAt: string | null;
  lastErrorMessage: string | null;
  createdAt: string;
}

export interface PublicationAttempt {
  id: string;
  publicationJobId: string;
  attemptNumber: number;
  startedAt: string;
  completedAt: string | null;
  durationMs: number | null;
  providerHttpStatus: number | null;
  providerErrorCode: string | null;
  normalizedErrorCategory: string | null;
  providerRequestId: string | null;
  requestMetadataJson: string | null;
  responseMetadataJson: string | null;
  isSuccess: boolean;
  retryDecision: string | null;
  createdAt: string;
}

export interface QueuePublicationRequest {
  postId: string;
  contentVariantId: string;
  connectedAccountId: string | null;
  publishMode: PublishMode;
  scheduledAtUtc: string | null;
  mediaManifestJson?: string | null;
}
