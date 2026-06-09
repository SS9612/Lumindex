import type { DocumentStatus } from './types'

export const ALLOWED_EXTENSIONS = ['.pdf', '.docx', '.txt'] as const

/** Mirrors the API's accept attribute and request size limit (25 MB). */
export const ACCEPT_ATTRIBUTE = ALLOWED_EXTENSIONS.join(',')
export const MAX_FILE_SIZE_BYTES = 25 * 1024 * 1024

/** Statuses that mean the backend is still processing, so the list should keep polling. */
export const ACTIVE_STATUSES: DocumentStatus[] = ['Pending', 'Processing']

export function hasAllowedExtension(fileName: string): boolean {
  const lower = fileName.toLowerCase()
  return ALLOWED_EXTENSIONS.some((ext) => lower.endsWith(ext))
}
