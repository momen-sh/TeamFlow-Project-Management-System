import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

interface TokenPayload {
  nameid?: string;
  email?: string;
  role?: string;
  exp: number;
  [claim: string]: any;
}

const NAME_IDENTIFIER_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'token';

  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  hasToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const decoded = this.decodeToken(token);
      return !this.isTokenExpired(decoded);
    } catch {
      return false;
    }
  }

  private decodeToken(token: string): TokenPayload {
    return jwtDecode<TokenPayload>(token);
  }

  private isTokenExpired(payload: TokenPayload): boolean {
    const expirationDate = new Date(payload.exp * 1000);
    return expirationDate < new Date();
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    
    try {
      const decoded = this.decodeToken(token);
      return decoded.nameid ?? decoded[NAME_IDENTIFIER_CLAIM] ?? null;
    } catch {
      return null;
    }
  }

  getEmail(): string | null {
    const token = this.getToken();
    if (!token) return null;
    
    try {
      const decoded = this.decodeToken(token);
      return decoded.email ?? decoded[EMAIL_CLAIM] ?? null;
    } catch {
      return null;
    }
  }

  getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    
    try {
      const decoded = this.decodeToken(token);
      return decoded.role ?? decoded[ROLE_CLAIM] ?? null;
    } catch {
      return null;
    }
  }
}
