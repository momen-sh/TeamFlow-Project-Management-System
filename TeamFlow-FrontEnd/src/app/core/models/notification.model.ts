export enum NotificationType {
  Info = 'Info',
  TaskAssigned = 'TaskAssigned',
  TaskSentToQA = 'TaskSentToQA',
  Mention = 'Mention'
}

export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  userId: number;
  relatedEntityId?: number | null;
  relatedEntityType?: string | null;
}
