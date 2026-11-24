import { apiFetch } from "@/lib/api-client";

export interface MfaSetup {
  otpAuthUri: string;
  secret: string;
}

async function post(path: string, body?: unknown): Promise<Response> {
  const response = await apiFetch(path, {
    method: "POST",
    ...(body === undefined ? {} : { body: JSON.stringify(body) }),
  });

  if (!response.ok) {
    const fallback =
      response.status === 429
        ? "Too many attempts. Wait a minute and try again."
        : "Failed to update two-step verification";
    const error = (await response.json().catch(() => ({}))) as { message?: string };
    throw new Error(error.message ?? fallback);
  }

  return response;
}

export async function setupMfa(): Promise<MfaSetup> {
  return (await (await post("/api/auth/mfa/setup")).json()) as MfaSetup;
}

export async function enableMfa(code: string): Promise<void> {
  await post("/api/auth/mfa/enable", { code });
}

export async function disableMfa(code: string): Promise<void> {
  await post("/api/auth/mfa/disable", { code });
}
