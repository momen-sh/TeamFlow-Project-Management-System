import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import { environment } from '../../../../environments/environments';
import { TaskAttachmentDto } from '../../../core/models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskAttachmentService {
  constructor(private api: ApiService) {}

  getAttachments(taskId: number): Observable<TaskAttachmentDto[]> {
    return this.api.get<TaskAttachmentDto[]>(`tasks/${taskId}/attachments`).pipe(
      map(attachments => attachments.map(attachment => this.normalizeAttachment(attachment)))
    );
  }

  uploadAttachments(taskId: number, files: File[]): Observable<TaskAttachmentDto[]> {
    if (files.length === 0) return of([]);

    const uploads = files.map(file => {
      const formData = new FormData();
      formData.append('file', file, file.name);
      return this.api.post<TaskAttachmentDto>(`tasks/${taskId}/attachments`, formData).pipe(
        map(attachment => this.normalizeAttachment(attachment))
      );
    });

    return forkJoin(uploads);
  }

  deleteAttachment(taskId: number, attachmentId: number): Observable<void> {
    return this.api.delete<void>(`tasks/${taskId}/attachments/${attachmentId}`);
  }

  private normalizeAttachment(attachment: TaskAttachmentDto): TaskAttachmentDto {
    const raw = attachment as TaskAttachmentDto & Record<string, unknown>;
    const url = attachment.url ?? attachment.fileUrl ?? raw['FileUrl'] as string ?? '';
    return {
      ...attachment,
      fileName: attachment.fileName ?? raw['FileName'] as string ?? '',
      contentType: attachment.contentType ?? attachment.fileType ?? raw['FileType'] as string ?? '',
      fileType: attachment.fileType ?? attachment.contentType ?? raw['FileType'] as string ?? '',
      size: attachment.size ?? attachment.fileSize ?? (Number(raw['FileSize']) || 0),
      fileSize: attachment.fileSize ?? attachment.size ?? (Number(raw['FileSize']) || 0),
      url: this.absoluteFileUrl(url),
      fileUrl: this.absoluteFileUrl(url),
      createdAt: attachment.createdAt ?? raw['CreatedAt'] as string,
      uploadedAt: attachment.uploadedAt ?? attachment.createdAt ?? raw['CreatedAt'] as string
    };
  }

  private absoluteFileUrl(url: string): string {
    if (!url) return '';
    if (/^https?:\/\//i.test(url)) return url;
    const apiOrigin = environment.apiUrl.replace(/\/api\/?$/i, '');
    return `${apiOrigin}${url.startsWith('/') ? url : `/${url}`}`;
  }
}
