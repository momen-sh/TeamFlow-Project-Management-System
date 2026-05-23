import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environments';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }
    return this.http.get<ApiResponse<T> | T>(`${this.apiUrl}/${path}`, { params: httpParams }).pipe(
      map(response => this.unwrapResponse<T>(response))
    );
  }

  post<T>(path: string, body: any): Observable<T> {
    return this.http.post<ApiResponse<T> | T>(`${this.apiUrl}/${path}`, body).pipe(
      map(response => this.unwrapResponse<T>(response))
    );
  }

  put<T>(path: string, body: any): Observable<T> {
    return this.http.put<ApiResponse<T> | T>(`${this.apiUrl}/${path}`, body).pipe(
      map(response => this.unwrapResponse<T>(response))
    );
  }

  patch<T>(path: string, body: any): Observable<T> {
    return this.http.patch<ApiResponse<T> | T>(`${this.apiUrl}/${path}`, body).pipe(
      map(response => this.unwrapResponse<T>(response))
    );
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<ApiResponse<T> | T>(`${this.apiUrl}/${path}`).pipe(
      map(response => this.unwrapResponse<T>(response))
    );
  }

  private unwrapResponse<T>(response: ApiResponse<T> | T): T {
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as ApiResponse<T>).data;
    }

    return response as T;
  }
}
