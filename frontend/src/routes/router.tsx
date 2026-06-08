import { createBrowserRouter } from 'react-router-dom'

import { RequireAuth } from '@/features/auth/RequireAuth'

import { AppLayout } from './AppLayout'
import { ChatPage } from './ChatPage'
import { DocumentsPage } from './DocumentsPage'
import { HomePage } from './HomePage'
import { LoginPage } from './LoginPage'
import { RegisterPage } from './RegisterPage'

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <HomePage /> },
      {
        element: <RequireAuth />,
        children: [
          { path: 'documents', element: <DocumentsPage /> },
          { path: 'chat', element: <ChatPage /> },
        ],
      },
    ],
  },
])
