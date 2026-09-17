import { saveTokens } from "@/lib/tokens";

const API_URL = (import.meta.env.VITE_API_URL as string) || "http://localhost:5136";

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string | null;
  phoneNumber?: string | null;
}

export interface AuthTokensResponse {
  accessToken: string;
  refreshToken: string;
}

export async function registerUser(data: RegisterRequest): Promise<AuthTokensResponse> {
  const response = await fetch(`${API_URL}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Registration failed" }));
    throw new Error((error as { message?: string }).message ?? "Registration failed");
  }

  const tokens = await response.json() as AuthTokensResponse;
  // Salvează token-urile în localStorage imediat după înregistrare
  saveTokens(tokens.accessToken, tokens.refreshToken);
  return tokens;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export async function loginUser(data: LoginRequest): Promise<AuthTokensResponse> {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Invalid email or password" }));
    throw new Error((error as { message?: string }).message ?? "Invalid email or password");
  }

  const tokens = await response.json() as AuthTokensResponse;
  // Salvează token-urile în localStorage imediat după login
  saveTokens(tokens.accessToken, tokens.refreshToken);
  return tokens;
}
