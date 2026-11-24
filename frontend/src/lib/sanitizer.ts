import DOMPurify from "dompurify";

// Adapter peste DOMPurify: restul aplicatiei nu depinde direct de biblioteca.
export const sanitizer = {
  html: (dirty: string) => DOMPurify.sanitize(dirty, { USE_PROFILES: { html: true } }),
};
