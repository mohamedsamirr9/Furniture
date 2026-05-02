/** Extract human-readable message from HttpClient error responses (.NET ProblemDetails, strings, etc.). */
export function getApiErrorMessage(err: unknown, fallback = 'Request failed'): string {
  if (err === null || err === undefined) return fallback;
  const e = err as any;
  const body = e.error;

  if (typeof body === 'string' && body.trim()) {
    return body.trim();
  }

  if (body && typeof body === 'object') {
    const d = body.message ?? body.detail ?? body.title;
    if (typeof d === 'string' && d.trim()) return d.trim();

    const errs = body.errors;
    if (errs && typeof errs === 'object') {
      const first = Object.values(errs)[0];
      if (Array.isArray(first) && first.length) return String(first[0]);
    }
  }

  if (typeof e.message === 'string' && e.message) return e.message;
  return fallback;
}

export function isPendingSellerRequestErrorMessage(msg: string): boolean {
  return /pending.*application|already have a pending/i.test(msg);
}
