const ACCESS_TOKEN_KEY = "openkey_access_token";
const REFRESH_TOKEN_KEY = "openkey_refresh_token";

const hasLocalStorage = typeof window !== "undefined" && typeof window.localStorage !== "undefined";

export function saveTokens(accessToken: string, refreshToken: string) {
  if (!hasLocalStorage) return;
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export function getAccessToken(): string | null {
  if (!hasLocalStorage) return null;
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  if (!hasLocalStorage) return null;
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function clearTokens() {
  if (!hasLocalStorage) return;
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function isLoggedIn(): boolean {
  return getAccessToken() !== null;
}

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

// Extrage rolul utilizatorului logat din JWT access token
export function getUserRole(): string | null {
  const token = getAccessToken();
  if (!token) return null;
  try {
    const parts = token.split(".");
    const payloadPart = parts[1];
    if (parts.length !== 3 || !payloadPart) return null;
    const payload = JSON.parse(atob(payloadPart));
    const role =
      payload.role ??
      payload.roles ??
      payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      null;
    if (Array.isArray(role)) {
      return role.includes("Admin") ? "Admin" : ((role[0] as string) ?? null);
    }
    return typeof role === "string" ? role : null;
  } catch {
    return null;
  }
}

// Returnează true dacă utilizatorul curent are rolul de Admin
export function isAdmin(): boolean {
  return getUserRole() === "Admin";
}
