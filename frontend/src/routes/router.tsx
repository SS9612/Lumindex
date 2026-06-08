import { createBrowserRouter } from 'react-router-dom'

import { AppLayout } from './AppLayout'
import { ChatPage } from './ChatPage'
import { DocumentsPage } from './DocumentsPage'
import { HomePage } from './HomePage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'documents', element: <DocumentsPage /> },
      { path: 'chat', element: <ChatPage /> },
    ],
  },
])
