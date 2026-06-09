import { apiFetch } from '@/lib/api'

import type { DocumentSummary } from './types'

export const listDocuments = () => apiFetch<DocumentSummary[]>('/documents')

export const uploadDocument = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return apiFetch<DocumentSummary>('/documents', { method: 'POST', formData })
}
