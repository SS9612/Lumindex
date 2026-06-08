import { Link, NavLink, Outlet } from 'react-router-dom'

import { cn } from '@/lib/utils'

const navItems = [
  { to: '/', label: 'Home', end: true },
  { to: '/documents', label: 'Documents' },
  { to: '/chat', label: 'Chat' },
]

export function AppLayout() {
  return (
    <div className="flex min-h-full flex-col">
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-6">
          <Link to="/" className="text-sm font-semibold tracking-tight">
            DocuMind
          </Link>
          <nav className="flex items-center gap-1">
            {navItems.map(({ to, label, end }) => (
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
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t">
        <div className="container mx-auto px-6 py-4 text-xs text-muted-foreground">
          DocuMind portfolio project — built with .NET 9, Azure OpenAI, and React.
        </div>
      </footer>
    </div>
  )
}
