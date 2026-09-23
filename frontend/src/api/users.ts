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

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export async function changePassword(data: ChangePasswordRequest): Promise<void> {
  const response = await apiFetch("/api/users/me/password", {
    method: "PATCH",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to change password" }));
    throw new Error((error as { message?: string })?.message ?? "Failed to change password");
  }
}


export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string | null;
  phoneNumber: string | null;
}

export async function updateCurrentUser(data: UpdateUserRequest): Promise<void> {
  const response = await apiFetch("/api/users/me", {
    method: "PATCH",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to update account" }));
    throw new Error((error as { message?: string })?.message ?? "Failed to update account");
  }
}

