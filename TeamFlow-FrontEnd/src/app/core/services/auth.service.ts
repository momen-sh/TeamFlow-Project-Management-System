import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from './token.service';
import { environment } from '../../../environments/environments';
import { ApiResponse } from '../models/api-response.model';
import { AppRoles } from '../models/app-roles';

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

interface AuthResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private loggedIn = new BehaviorSubject<boolean>(this.tokenService.hasToken());

  isLoggedIn$ = this.loggedIn.asObservable();

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private router: Router
  ) {}

  register(data: RegisterDto): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, data).pipe(
      map(response => response.data),
      tap(response => {
        this.tokenService.saveToken(response.token);
        this.loggedIn.next(true);
      })
    );
  }

  login(data: LoginDto): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, data).pipe(
      map(response => response.data),
      tap(response => {
        this.tokenService.saveToken(response.token);
        this.loggedIn.next(true);
      })
    );
  }

  logout(): void {
    this.tokenService.removeToken();
    this.loggedIn.next(false);
    this.router.navigate(['/auth/login']);
  }

  isLoggedIn(): boolean {
    return this.tokenService.hasToken();
  }

  getUserRole(): string | null {
    return this.tokenService.getRole();
  }

  getUserId(): string | null {
    return this.tokenService.getUserId();
  }

  getUserEmail(): string | null {
    return this.tokenService.getEmail();
  }

  isAdmin(): boolean {
    return this.getUserRole() === AppRoles.Admin;
  }
}
