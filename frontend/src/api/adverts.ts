import { apiFetch } from "@/lib/api-client";

export interface CreateAdvertRequest {
  title: string;
  description: string;
  price: number;
  surfaceArea: number;
  rooms: number;
  floor: number;
  type: "Sale" | "Rent";
  address: {
    country: string;
    city: string;
    region: string;
    streetAddress: string;
    streetNumber: string;
  };
}

export async function createAdvert(data: CreateAdvertRequest): Promise<void> {
  const response = await apiFetch("/api/adverts", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to create advert" }));
    throw new Error((error as { message?: string }).message ?? "Failed to create advert");
  }
}
