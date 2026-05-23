import { Injectable } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { CommentDto, CreateCommentDto } from '../../../core/models/comment.model';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  constructor(private api: ApiService) {}

  getComments(taskId: number) {
    return this.api.get<CommentDto[]>(`tasks/${taskId}/comments`);
  }

  createComment(taskId: number, data: CreateCommentDto) {
    return this.api.post<CommentDto>(`tasks/${taskId}/comments`, data);
  }

  deleteComment(taskId: number, commentId: number) {
    return this.api.delete<void>(`tasks/${taskId}/comments/${commentId}`);
  }
}
