export type AiJobStatus = 'queued' | 'processing' | 'review' | 'completed' | 'failed';

export interface AiClassificationJob {
  id: string;
  target: string;
  type: 'property' | 'lead' | 'content';
  status: AiJobStatus;
  confidence: number;
  result: string;
  createdAt: string;
}

export interface AiContentItem {
  id: string;
  title: string;
  channel: 'seo' | 'facebook' | 'zalo' | 'listing';
  status: 'draft' | 'review' | 'approved' | 'published';
  owner: string;
  updatedAt: string;
}
