import { useAuthStore } from '@/features/auth/auth-store'

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

/** Error thrown for any non-2xx API response, carrying parsed RFC 7807 problem details. */
export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }

  /** Field-level validation messages, keyed by property name, when present. */
  get fieldErrors(): Record<string, string[]> | undefined {
    return this.problem?.errors
  }
}

interface ApiRequestOptions extends Omit<RequestInit, 'body'> {
  /** Attach the bearer token from the auth store. Defaults to true. */
  auth?: boolean
  /** Object serialized to a JSON request body. */
  json?: unknown
}

const API_BASE = '/api'

export async function apiFetch<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const { auth = true, json, headers, ...rest } = options

  const finalHeaders = new Headers(headers)
  let body: BodyInit | undefined
  if (json !== undefined) {
    finalHeaders.set('Content-Type', 'application/json')
    body = JSON.stringify(json)
  }

  if (auth) {
    const token = useAuthStore.getState().token
    if (token) {
      finalHeaders.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(`${API_BASE}${path}`, { ...rest, headers: finalHeaders, body })

  if (response.status === 401 && auth) {
    // The stored token is missing/expired/invalid — drop the stale session.
    useAuthStore.getState().clear()
  }

  if (!response.ok) {
    const problem = await safeParseProblem(response)
    const message =
      problem?.detail ?? problem?.title ?? `Request failed with status ${response.status}`
    throw new ApiError(response.status, message, problem)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function safeParseProblem(response: Response): Promise<ProblemDetails | undefined> {
  try {
    const contentType = response.headers.get('Content-Type') ?? ''
    if (contentType.includes('json')) {
      return (await response.json()) as ProblemDetails
    }
  } catch {
    // Ignore parse failures; fall back to a generic message.
  }
  return undefined
}
