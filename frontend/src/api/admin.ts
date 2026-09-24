import { apiFetch } from "@/lib/api-client";

export interface AdvertReport {
  uuid: string;
  advertUuid: string;
  advertTitle: string;
  reporterUuid: string;
  reason: string;
  description: string | null;
  createdDate: string;
}

export async function getAdvertReports(page = 1, pageSize = 50): Promise<AdvertReport[]> {
  const response = await apiFetch(`/api/admin/reports?page=${page}&pageSize=${pageSize}`);
  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to load reports" }));
    throw new Error((error as { message?: string }).message ?? "Failed to load reports");
  }

  return (await response.json()) as AdvertReport[];
}

export async function dismissAdvertReport(uuid: string): Promise<void> {
  const response = await apiFetch(`/api/admin/reports/${uuid}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to dismiss report" }));
    throw new Error((error as { message?: string }).message ?? "Failed to dismiss report");
  }
}
