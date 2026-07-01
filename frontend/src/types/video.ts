export interface VideoScene {
  id: string;
  videoProjectId: string;
  sequence: number;
  durationSeconds: number;
  narration: string;
  onScreenText: string;
  visualPrompt: string;
  negativePrompt: string | null;
  cameraDirection: string | null;
  referenceAssetIdsJson: string | null;
  status: string; // Pending, Generating, Completed, Failed
  generatedAssetId: string | null;
  generatedAsset?: {
    id: string;
    url: string;
    originalFileName: string;
    sizeBytes?: number;
  } | null;
}

export interface VideoProject {
  id: string;
  postId: string;
  contentVariantId: string;
  title: string;
  targetChannel: string;
  aspectRatio: string;
  targetDurationSeconds: number;
  status: string; // Draft, StoryboardGenerating, StoryboardGenerated, GeneratingScenes, Rendering, Completed, Failed
  approvedStoryboardVersion: number;
  finalAssetId: string | null;
  finalAsset?: {
    id: string;
    url: string;
    originalFileName: string;
    sizeBytes?: number;
  } | null;
  scenes: VideoScene[];
}

export interface VideoGenerationJob {
  id: string;
  videoProjectId: string;
  videoSceneId: string | null;
  provider: string;
  model: string;
  operationId: string | null;
  status: string; // Pending, Processing, Completed, Failed, Cancelled
  progressPercent: number | null;
  outputAssetId: string | null;
  errorCategory: string | null;
  errorCode: string | null;
  errorMessage: string | null;
  retryCount: number;
  startedAt: string | null;
  completedAt: string | null;
}
