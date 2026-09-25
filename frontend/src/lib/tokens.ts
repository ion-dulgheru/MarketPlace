// Cheile sub care salvăm token-urile în localStorage (browser)
const ACCESS_TOKEN_KEY = "openkey_access_token";
const REFRESH_TOKEN_KEY = "openkey_refresh_token";

// localStorage nu există la randarea pe server (SSR) — evităm crash-ul acolo
const hasLocalStorage = typeof window !== "undefined" && typeof window.localStorage !== "undefined";

// Salvează ambele token-uri după login / register
export function saveTokens(accessToken: string, refreshToken: string) {
  if (!hasLocalStorage) return;
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

// Citește access token-ul (folosit la fiecare cerere autentificată)
export function getAccessToken(): string | null {
  if (!hasLocalStorage) return null;
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

// Citește refresh token-ul (folosit când access token-ul a expirat)
export function getRefreshToken(): string | null {
  if (!hasLocalStorage) return null;
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

// Șterge token-urile la logout
export function clearTokens() {
  if (!hasLocalStorage) return;
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

// Returnează true dacă userul este logat (are un access token salvat)
export function isLoggedIn(): boolean {
  return getAccessToken() !== null;
}

// Extrage UUID-ul utilizatorului logat din JWT access token
export function getCurrentUserUuid(): string | null {
  const token = getAccessToken();
  if (!token) return null;
  try {
    const parts = token.split(".");
    const payloadPart = parts[1];
    if (parts.length !== 3 || !payloadPart) return null;
    const payload = JSON.parse(atob(payloadPart));
    return (payload.sub ?? payload.nameid ?? payload.userId ?? null) as string | null;
  } catch {
    return null;
  }
}
