export type PostStatus = 'Draft' | 'Scheduled' | 'Published' | 'Failed' | 'Archived';

export interface Post {
  id: string;
  title: string;
  content?: string;
  summary?: string;
  thumbnailUrl?: string;
  status: PostStatus;
  publishedAt?: string;
  authorId: string;
  authorName: string;
  propertyId?: string;
  propertyTitle?: string;
  createdAt: string;
  updatedAt?: string;
  channelCount: number;
  scheduleCount: number;
}

export interface PostCreateRequest {
  title: string;
  content?: string;
  summary?: string;
  thumbnailUrl?: string;
  propertyId?: string;
}

export interface AIContentGenerateRequest {
  prompt: string;
  propertyId?: string;
}

export interface AIContentGeneration {
  id: string;
  postId: string;
  prompt: string;
  generatedContent: string;
  createdAt: string;
  promptTokens?: number;
  completionTokens?: number;
  estimatedCost?: number;
  factsUsedJson?: string;
}
