import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/features/auth/auth-store'
import { useCurrentUser, useLogout } from '@/features/auth/hooks'
import { cn } from '@/lib/utils'

const navItems = [
  { to: '/', label: 'Home', end: true },
  { to: '/documents', label: 'Documents', protected: true },
  { to: '/chat', label: 'Chat', protected: true },
]

export function AppLayout() {
  const navigate = useNavigate()
  const token = useAuthStore((state) => state.token)
  const user = useAuthStore((state) => state.user)
  const logout = useLogout()

  // Validate the persisted token against the API on load; clears the session if it has expired.
  useCurrentUser()

  const isAuthenticated = Boolean(token)

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex min-h-full flex-col">
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-6">
          <Link to="/" className="text-sm font-semibold tracking-tight">
            Lumindex
          </Link>

          <div className="flex items-center gap-4">
            <nav className="flex items-center gap-1">
              {navItems
                .filter((item) => !item.protected || isAuthenticated)
                .map(({ to, label, end }) => (
                  <NavLink
                    key={to}
                    to={to}
                    end={end}
                    className={({ isActive }) =>
                      cn(
                        'rounded-md px-3 py-1.5 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground',
                        isActive && 'bg-muted text-foreground',
                      )
                    }
                  >
                    {label}
                  </NavLink>
                ))}
            </nav>

            <div className="flex items-center gap-2">
              {isAuthenticated ? (
                <>
                  <span className="hidden text-sm text-muted-foreground sm:inline">
                    {user?.displayName ?? user?.email}
                  </span>
                  <Button variant="outline" size="sm" onClick={handleLogout}>
                    Sign out
                  </Button>
                </>
              ) : (
                <>
                  <NavLink
                    to="/login"
                    className="rounded-md px-3 py-1.5 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                  >
                    Sign in
                  </NavLink>
                  <Link
                    to="/register"
                    className="inline-flex h-8 items-center justify-center rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground hover:bg-primary/90"
                  >
                    Sign up
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t">
        <div className="container mx-auto px-6 py-4 text-xs text-muted-foreground">
          Lumindex portfolio project — built with .NET 9, Azure OpenAI, and React.
        </div>
      </footer>
    </div>
  )
}
