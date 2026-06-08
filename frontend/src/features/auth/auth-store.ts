import { create } from 'zustand'
import { persist } from 'zustand/middleware'

import type { User } from './types'

interface Session {
  token: string
  user: User
  expiresAt: string
}

interface AuthState {
  token: string | null
  user: User | null
  expiresAt: string | null
  setSession: (session: Session) => void
  setUser: (user: User) => void
  clear: () => void
}

/**
 * Holds the JWT access token and current user, persisted to localStorage so the session
 * survives reloads. The token is read by the API client to attach the Authorization header.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      expiresAt: null,
      setSession: ({ token, user, expiresAt }) => set({ token, user, expiresAt }),
      setUser: (user) => set({ user }),
      clear: () => set({ token: null, user: null, expiresAt: null }),
    }),
    {
      name: 'documind.auth',
      partialize: (state) => ({
        token: state.token,
        user: state.user,
        expiresAt: state.expiresAt,
      }),
    },
  ),
)
