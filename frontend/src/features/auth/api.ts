import { apiFetch } from '@/lib/api'

import type { AuthResponse, LoginRequest, RegisterRequest, User } from './types'

export const registerRequest = (body: RegisterRequest) =>
  apiFetch<AuthResponse>('/auth/register', { method: 'POST', json: body, auth: false })

export const loginRequest = (body: LoginRequest) =>
  apiFetch<AuthResponse>('/auth/login', { method: 'POST', json: body, auth: false })

export const fetchCurrentUser = () => apiFetch<User>('/auth/me')
