import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { fetchCurrentUser, loginRequest, registerRequest } from './api'
import { useAuthStore } from './auth-store'
import type { AuthResponse } from './types'

export const authKeys = {
  me: ['auth', 'me'] as const,
}

function useApplySession() {
  const setSession = useAuthStore((state) => state.setSession)
  const queryClient = useQueryClient()

  return (data: AuthResponse) => {
    setSession({ token: data.accessToken, user: data.user, expiresAt: data.expiresAt })
    queryClient.setQueryData(authKeys.me, data.user)
  }
}

export function useLogin() {
  const applySession = useApplySession()
  return useMutation({ mutationFn: loginRequest, onSuccess: applySession })
}

export function useRegister() {
  const applySession = useApplySession()
  return useMutation({ mutationFn: registerRequest, onSuccess: applySession })
}

export function useLogout() {
  const clear = useAuthStore((state) => state.clear)
  const queryClient = useQueryClient()

  return () => {
    clear()
    queryClient.clear()
  }
}

/**
 * Loads the authenticated user's profile from <c>/auth/me</c>. Only runs when a token is present;
 * a 401 response causes the API client to clear the stale session automatically.
 */
export function useCurrentUser() {
  const token = useAuthStore((state) => state.token)
  const setUser = useAuthStore((state) => state.setUser)

  return useQuery({
    queryKey: authKeys.me,
    queryFn: async () => {
      const user = await fetchCurrentUser()
      setUser(user)
      return user
    },
    enabled: Boolean(token),
    staleTime: 5 * 60_000,
  })
}
