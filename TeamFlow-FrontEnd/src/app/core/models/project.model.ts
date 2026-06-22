export interface CreateProjectDto {
  name: string;
  description?: string;
  workspaceId?: number;
}

export interface ProjectDto {
  id: number;
  name: string;
  description?: string;
  ownerId: number;
  ownerName?: string;
  workspaceId?: number;
  members?: ProjectMemberDto[];
  memberIds?: number[];
}

export interface AssignProjectMemberDto {
  userId: number;
  role?: string;
}

export interface ProjectMemberDto {
  userId: number;
  fullName?: string;
  email?: string;
  role?: string;
}
