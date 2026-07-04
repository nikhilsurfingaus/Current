import { UserRole } from '../enums';

export interface AuthResponse {
  userId: string;
  email: string;
  role: UserRole;
  token: string;
  expiresAt: string;
}
