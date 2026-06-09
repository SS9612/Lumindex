export type DocumentStatus = 'Pending' | 'Processing' | 'Ready' | 'Failed'

export interface DocumentSummary {
  id: string
  fileName: string
  contentType: string
  sizeBytes: number
  status: DocumentStatus
  statusDetail: string | null
  chunkCount: number
  createdAt: string
  processedAt: string | null
}
