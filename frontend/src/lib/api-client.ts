import { getAccessToken, getRefreshToken, saveTokens, clearTokens } from "@/lib/tokens";

export const API_URL = import.meta.env.VITE_API_URL as string;

async function tryRefresh(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  try {
    const response = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) return false;

    const data = (await response.json()) as { accessToken: string; refreshToken: string };
    saveTokens(data.accessToken, data.refreshToken);
    return true;
  } catch {
    return false;
  }
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
