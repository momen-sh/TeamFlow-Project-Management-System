export interface Task {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  type?: TaskType;
  projectId: number;
  assignedUserId?: number | null;
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  type: TaskType;
  projectId: number;
  assignedUserId?: number | null;
}

export interface UpdateTaskStatusDto {
  status: TaskStatus;
}

export interface TaskDto {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  type: TaskType;
  projectId: number;
  projectName?: string;
  assignedUserId?: number | null;
  assignedUserName?: string;
  attachments?: TaskAttachmentDto[];
  workRecords?: TaskWorkRecordDto[];
  qaTestCases?: QaTestCaseDto[];
  qaAssignments?: TaskQaAssignmentDto[];
  permissions?: TaskPermissionsDto;
  sentToQaAt?: string | null;
  sentToQaByUserId?: number | null;
  createdAt?: string;
  updatedAt?: string;
}

export enum TaskStatus {
  ToDo = 'ToDo',
  InProgress = 'InProgress',
  Done = 'Done'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export enum TaskType {
  Task = 'Task',
  Bug = 'Bug',
  Feature = 'Feature'
}

export interface UpdateTaskDto {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  type: TaskType;
  projectId: number;
  assignedUserId?: number | null;
}

export interface TaskAttachmentDto {
  id: number;
  taskId: number;
  fileName: string;
  contentType?: string;
  fileType?: string;
  size?: number;
  fileSize?: number;
  url?: string;
  fileUrl?: string;
  uploadedAt?: string;
  createdAt?: string;
}

export interface TaskWorkRecordDto {
  id: number;
  title: string;
  description?: string | null;
  timeSpentMinutes: number;
  branchNumber: string;
  createdAt: string;
  createdByUserId: number;
  createdByUserName?: string | null;
  taskId: number;
}

export interface CreateTaskWorkRecordDto {
  title: string;
  description?: string | null;
  timeSpentMinutes: number;
  branchNumber: string;
}

export enum QaTestCaseStatus {
  Pass = 'Pass',
  Fail = 'Fail',
  Blocked = 'Blocked'
}

export interface QaTestCaseDto {
  id: number;
  title: string;
  steps: string;
  expectedResult: string;
  actualResult?: string | null;
  status: QaTestCaseStatus;
  createdAt: string;
  taskId: number;
  createdByUserId: number;
  createdByUserName?: string | null;
}

export interface CreateQaTestCaseDto {
  title: string;
  steps: string;
  expectedResult: string;
  actualResult?: string | null;
  status: QaTestCaseStatus;
}

export interface SendTaskToQaDto {
  qaUserIds: number[];
}

export interface TaskQaAssignmentDto {
  taskId: number;
  qaUserId: number;
  qaUserName?: string | null;
  qaUserEmail?: string | null;
  assignedByUserId: number;
  assignedAt: string;
}

export interface TaskPermissionsDto {
  canManage: boolean;
  canAddWorkRecord: boolean;
  canSendToQa: boolean;
  canAddQaTestCase: boolean;
  canComment: boolean;
  canUnassign: boolean;
}

export interface TaskFilters {
  search: string;
  priority: TaskPriority | 'all';
  projectId: number | 'all';
  type: TaskType | 'all';
}
