import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAuthStore } from './auth-store'

/**
 * Route guard for protected sections. Redirects unauthenticated users to the login page and
 * remembers the attempted location so they can be returned there after signing in.
 */
export function RequireAuth() {
  const token = useAuthStore((state) => state.token)
  const location = useLocation()

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <Outlet />
}
