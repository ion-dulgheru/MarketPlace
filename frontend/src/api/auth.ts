import { clearTokens, markSignedOut, saveAccessToken } from "@/lib/tokens";

const API_URL = import.meta.env.VITE_API_URL as string;

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string | null;
  phoneNumber?: string | null;
  captchaToken: string;
}

export async function registerUser(data: RegisterRequest): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Registration failed" }));
    throw new Error((error as { message?: string }).message ?? "Registration failed");
  }
}

export async function verifyEmail(token: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/verify-email`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Verification link is invalid or expired." }));
    throw new Error(
      (error as { message?: string }).message ?? "Verification link is invalid or expired.",
    );
  }
}

export async function resendVerificationEmail(email: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/verification-emails`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  });

  if (!response.ok) {
    throw new Error(
      response.status === 429
        ? "Too many attempts. Wait a minute and try again."
        : "Failed to send the verification email",
    );
  }
}

export interface LoginRequest {
  email: string;
  password: string;
}

export type LoginResult = { mfaRequired: false } | { mfaRequired: true; mfaToken: string };

// Refresh token-ul nu vine in body, ci in cookie-ul HttpOnly openkey_rt.
interface SignInResponse {
  accessToken: string | null;
  mfaRequired: boolean;
  mfaToken: string | null;
}

/** The mfaToken expired or was invalidated: the user has to enter the password again. */
export class MfaSessionExpiredError extends Error {}

export async function loginUser(data: LoginRequest): Promise<LoginResult> {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Invalid email or password" }));
    throw new Error((error as { message?: string }).message ?? "Invalid email or password");
  }

  const result = (await response.json()) as SignInResponse;
  if (result.mfaRequired && result.mfaToken) {
    return { mfaRequired: true, mfaToken: result.mfaToken };
  }

  saveSignIn(result);
  return { mfaRequired: false };
}

export async function verifyMfaLogin(mfaToken: string, code: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/login/mfa`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ mfaToken, code }),
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => ({}))) as { code?: string; message?: string };
    const fallback =
      response.status === 429
        ? "Too many attempts. Wait a minute and try again."
        : "Could not verify the code";
    const message = error.message ?? fallback;
    throw error.code === "Auth.InvalidMfaToken" || error.code === "Auth.AccountLocked"
      ? new MfaSessionExpiredError(message)
      : new Error(message);
  }

  saveSignIn((await response.json()) as SignInResponse);
}

function saveSignIn(result: SignInResponse) {
  if (!result.accessToken) {
    throw new Error("Unexpected response from the server");
  }
  saveAccessToken(result.accessToken);
}

// Sterge token-ul local imediat, apoi revoca sesiunea pe server (care sterge si cookie-ul).
export async function logoutUser(): Promise<void> {
  markSignedOut();
  clearTokens();
  try {
    await fetch(`${API_URL}/api/auth/logout`, { method: "POST", credentials: "include" });
  } catch {
    // fara retea: utilizatorul a iesit oricum local
  }
}

export async function requestPasswordReset(email: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/password-reset-requests`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to request password reset" }));
    throw new Error((error as { message?: string }).message ?? "Failed to request password reset");
  }
}

export async function resetPassword(token: string, newPassword: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/auth/password-resets`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token, newPassword }),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to reset password" }));
    throw new Error((error as { message?: string }).message ?? "Failed to reset password");
  }
}
