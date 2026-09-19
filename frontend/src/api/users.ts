import { apiFetch } from "@/lib/api-client";

export interface CurrentUser {
  uuid: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  dateOfBirth: string | null;
  phoneNumber: string | null;
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await apiFetch("/api/users/me");
  if (!response.ok) {
    throw new Error("Failed to load account details");
  }

  return (await response.json()) as CurrentUser;
}