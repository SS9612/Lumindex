import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { listDocuments, uploadDocument } from './api'
import { ACTIVE_STATUSES } from './constants'

export const documentKeys = {
  all: ['documents'] as const,
}

export function useDocuments() {
  return useQuery({
    queryKey: documentKeys.all,
    queryFn: listDocuments,
    // Poll while any document is still being processed, then stop.
    refetchInterval: (query) => {
      const hasActive = query.state.data?.some((doc) => ACTIVE_STATUSES.includes(doc.status))
      return hasActive ? 4000 : false
    },
  })
}

export function useUploadDocument() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: uploadDocument,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all })
    },
  })
}
