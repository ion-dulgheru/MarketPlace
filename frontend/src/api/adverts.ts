import { apiFetch } from "@/lib/api-client";

export interface CreateAdvertAddress {
  country: string;
  city: string;
  region: string;
  streetAddress: string;
  streetNumber: string;
}

export interface CreateAdvertRequest {
  title: string;
  description: string;
  price: number;
  surfaceArea: number;
  rooms: number;
  floor: number;
  type: "Sale" | "Rent";
  address: CreateAdvertAddress;
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

export async function setAdvertFavorite(uuid: string, isFavorite: boolean): Promise<void> {
  const response = await apiFetch(`/api/adverts/${uuid}/favorite`, {
    method: "POST",
    body: JSON.stringify({ isFavorite }),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to update favorite status" }));
    throw new Error((error as { message?: string }).message ?? "Failed to update favorite status");
  }
}

export const favoriteAdvert = (uuid: string) => setAdvertFavorite(uuid, true);
export const unfavoriteAdvert = (uuid: string) => setAdvertFavorite(uuid, false);

export interface GetFavoritesResponse {
  items: Array<{ guid: string; [key: string]: unknown }>;
  page: number;
  pageSize: number;
  totalCount: number;
}

export async function getFavoriteAdverts(page = 1, pageSize = 20): Promise<GetFavoritesResponse> {
  const response = await apiFetch(`/api/adverts/favorites?page=${page}&pageSize=${pageSize}`);
  if (!response.ok) {
    return { items: [], page, pageSize, totalCount: 0 };
  }
  return (await response.json()) as GetFavoritesResponse;
}

export async function getFavoriteAdvertIds(): Promise<string[]> {
  const data = await getFavoriteAdverts(1, 100);
  return data.items.map((item) => item.guid);
}
