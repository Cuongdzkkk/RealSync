export type ProjectStatus = 'planning' | 'active' | 'paused' | 'completed';

export interface Project {
  id: string;
  name: string;
  area: string;
  status: ProjectStatus;
  propertyCount: number;
  leadCount: number;
  updatedAt: string;
}
