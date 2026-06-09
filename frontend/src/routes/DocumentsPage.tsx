import { useRef, useState } from 'react'
import { AlertCircle, FileText, Loader2, Upload } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  ACCEPT_ATTRIBUTE,
  ALLOWED_EXTENSIONS,
  MAX_FILE_SIZE_BYTES,
  hasAllowedExtension,
} from '@/features/documents/constants'
import { useDocuments, useUploadDocument } from '@/features/documents/hooks'
import type { DocumentStatus, DocumentSummary } from '@/features/documents/types'
import { ApiError } from '@/lib/api'
import { cn } from '@/lib/utils'

export function DocumentsPage() {
  const { data: documents, isPending, isError, error } = useDocuments()
  const upload = useUploadDocument()

  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragging, setIsDragging] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)

  const handleFiles = (files: FileList | null) => {
    setValidationError(null)
    upload.reset()

    const file = files?.[0]
    if (!file) {
      return
    }

    if (!hasAllowedExtension(file.name)) {
      setValidationError(`Unsupported file type. Allowed: ${ALLOWED_EXTENSIONS.join(', ')}.`)
      return
    }

    if (file.size > MAX_FILE_SIZE_BYTES) {
      setValidationError(`File is too large. Maximum size is ${formatBytes(MAX_FILE_SIZE_BYTES)}.`)
      return
    }

    upload.mutate(file)
  }

  const onDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setIsDragging(false)
    handleFiles(event.dataTransfer.files)
  }

  const uploadError =
    upload.error instanceof ApiError
      ? upload.error.message
      : upload.error
        ? 'Upload failed. Please try again.'
        : null

  return (
    <div className="container mx-auto max-w-4xl px-6 py-12">
      <header className="mb-8">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Documents</h1>
        <p className="text-muted-foreground">
          Upload PDF, DOCX, or TXT files. Each document is stored securely and queued for processing.
        </p>
      </header>

      <Card className="mb-8">
        <CardHeader>
          <CardTitle>Upload a document</CardTitle>
          <CardDescription>
            Drag and drop a file here, or browse. Up to {formatBytes(MAX_FILE_SIZE_BYTES)} per file.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div
            onDragOver={(event) => {
              event.preventDefault()
              setIsDragging(true)
            }}
            onDragLeave={() => setIsDragging(false)}
            onDrop={onDrop}
            className={cn(
              'flex flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed border-input px-6 py-10 text-center transition-colors',
              isDragging && 'border-primary bg-accent/40',
            )}
          >
            <Upload className="h-8 w-8 text-muted-foreground" aria-hidden />
            <div className="text-sm text-muted-foreground">
              <span className="font-medium text-foreground">Drag &amp; drop</span> your file here
            </div>
            <input
              ref={inputRef}
              type="file"
              accept={ACCEPT_ATTRIBUTE}
              className="hidden"
              onChange={(event) => {
                handleFiles(event.target.files)
                // Allow re-selecting the same file after an error.
                event.target.value = ''
              }}
            />
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={upload.isPending}
              onClick={() => inputRef.current?.click()}
            >
              {upload.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                  Uploading…
                </>
              ) : (
                'Browse files'
              )}
            </Button>
          </div>

          {(validationError || uploadError) && (
            <p className="mt-3 flex items-center gap-1.5 text-sm text-destructive" role="alert">
              <AlertCircle className="h-4 w-4" aria-hidden />
              {validationError ?? uploadError}
            </p>
          )}

          {upload.isSuccess && !validationError && (
            <p className="mt-3 text-sm text-muted-foreground" role="status">
              “{upload.data.fileName}” uploaded and queued for processing.
            </p>
          )}
        </CardContent>
      </Card>

      <section>
        <h2 className="mb-3 text-lg font-semibold tracking-tight">Your documents</h2>

        {isPending ? (
          <div className="flex items-center gap-2 py-10 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
            Loading documents…
          </div>
        ) : isError ? (
          <p className="py-10 text-sm text-destructive" role="alert">
            {error instanceof ApiError ? error.message : 'Failed to load documents.'}
          </p>
        ) : documents.length === 0 ? (
          <Card>
            <CardContent className="flex flex-col items-center gap-2 py-12 text-center">
              <FileText className="h-8 w-8 text-muted-foreground" aria-hidden />
              <p className="text-sm text-muted-foreground">
                No documents yet. Upload your first file to get started.
              </p>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <ul className="divide-y">
              {documents.map((document) => (
                <DocumentRow key={document.id} document={document} />
              ))}
            </ul>
          </Card>
        )}
      </section>
    </div>
  )
}

function DocumentRow({ document }: { document: DocumentSummary }) {
  return (
    <li className="flex items-center gap-4 px-6 py-4">
      <FileText className="h-5 w-5 shrink-0 text-muted-foreground" aria-hidden />
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">{document.fileName}</p>
        <p className="text-xs text-muted-foreground">
          {formatBytes(document.sizeBytes)} · uploaded {formatDate(document.createdAt)}
          {document.status === 'Ready' && document.chunkCount > 0
            ? ` · ${document.chunkCount} chunks`
            : ''}
        </p>
        {document.status === 'Failed' && document.statusDetail && (
          <p className="mt-0.5 truncate text-xs text-destructive">{document.statusDetail}</p>
        )}
      </div>
      <StatusBadge status={document.status} />
    </li>
  )
}

const STATUS_STYLES: Record<DocumentStatus, string> = {
  Pending: 'bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300',
  Processing: 'bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-300',
  Ready: 'bg-green-100 text-green-800 dark:bg-green-950 dark:text-green-300',
  Failed: 'bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-300',
}

function StatusBadge({ status }: { status: DocumentStatus }) {
  const isActive = status === 'Pending' || status === 'Processing'
  return (
    <span
      className={cn(
        'inline-flex shrink-0 items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium',
        STATUS_STYLES[status],
      )}
    >
      {isActive && <Loader2 className="h-3 w-3 animate-spin" aria-hidden />}
      {status}
    </span>
  )
}

function formatBytes(bytes: number): string {
  if (bytes === 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const exponent = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1)
  const value = bytes / 1024 ** exponent
  return `${value.toFixed(exponent === 0 ? 0 : 1)} ${units[exponent]}`
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}
