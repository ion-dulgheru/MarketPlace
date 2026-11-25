import {
  getAccessToken,
  saveAccessToken,
  clearTokens,
  isLoggedIn,
  isSignedOut,
} from "@/lib/tokens";

export const API_URL = import.meta.env.VITE_API_URL as string;

// Un singur refresh odata: token-ul se roteste, deci cereri paralele cu acelasi cookie ar esua.
let refreshPromise: Promise<boolean> | null = null;

function tryRefresh(): Promise<boolean> {
  refreshPromise ??= refreshAccessToken().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

// Fara body: refresh token-ul pleaca automat in cookie-ul HttpOnly (credentials: "include").
async function refreshAccessToken(): Promise<boolean> {
  try {
    const response = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });

    if (!response.ok) return false;

    const data = (await response.json()) as { accessToken: string };
    if (isSignedOut()) return false;
    saveAccessToken(data.accessToken);
    return true;
  } catch {
    return false;
  }
}

// Fara access token, incearca sa refaca sesiunea din cookie-ul de refresh.
export async function ensureSession(): Promise<boolean> {
  if (isLoggedIn()) return true;
  if (isSignedOut()) return false;
  return tryRefresh();
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = getAccessToken();

  const isFormData = init.body instanceof FormData;

  const makeRequest = (accessToken: string | null) =>
    fetch(`${API_URL}${path}`, {
      ...init,
      headers: {
        ...(isFormData ? {} : { "Content-Type": "application/json" }),
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        ...(init.headers ?? {}),
      },
    });

  let response = await makeRequest(token);

  if (response.status === 401) {
    const refreshed = await tryRefresh();
    if (refreshed) {
      response = await makeRequest(getAccessToken());
    } else {
      clearTokens();
      window.location.href = "/login";
    }
  }

  return response;
}
