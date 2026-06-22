import { Injectable } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { UserDto } from '../../../core/models/user.model';
import { CreateUserDto, UpdateUserDto } from '../../../core/models/create-user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private api: ApiService) {}

  getUsers() {
    return this.api.get<UserDto[]>('users');
  }

  getQaUsers() {
    return this.api.get<UserDto[]>('users/qa');
  }

  getMentionTargets() {
    return this.api.get<UserDto[]>('users/mention-targets');
  }

  getUserById(id: number) {
    return this.api.get<UserDto>(`users/${id}`);
  }

  createUser(data: CreateUserDto) {
    return this.api.post<UserDto>('users', data);
  }

  updateUser(id: number, data: UpdateUserDto) {
    return this.api.put<any>(`users/${id}`, data);
  }

  deleteUser(id: number) {
    return this.api.delete<any>(`users/${id}`);
  }
}
