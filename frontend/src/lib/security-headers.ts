// Sent on every response: static files (nitro route rules) and pages, errors included (server.ts).
// frame-ancestors cannot be set from the CSP <meta> tag.
export const SECURITY_HEADERS = {
  "X-Frame-Options": "DENY",
  "Content-Security-Policy": "frame-ancestors 'none'",
  "X-Content-Type-Options": "nosniff",
  "Referrer-Policy": "strict-origin-when-cross-origin",
  "Permissions-Policy": "geolocation=(), camera=(), microphone=()",
  "Strict-Transport-Security": "max-age=2592000",
};
