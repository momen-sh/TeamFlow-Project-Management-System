export interface CommentDto {
  id: number;
  taskId: number;
  content: string;
  createdAt: string;
  userId?: number;
  userEmail?: string;
  userName?: string;
  mentionedUserIds?: number[];
}

export interface CreateCommentDto {
  content: string;
  mentionedUserIds?: number[];
}
