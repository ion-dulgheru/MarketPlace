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

export interface AdvertAddress {
  country: string;
  city: string;
  region: string;
  streetAddress: string;
  streetNumber: string;
}

export interface AdvertPhoto {
  guid: string;
  photoUrl: string;
  isPrimary: boolean;
}

export interface Advert {
  guid: string;
  title: string;
  description: string;
  price: number;
  surfaceArea: number;
  rooms: number;
  floor: number;
  status: string;
  type: string;
  createdDate: string;
  address: AdvertAddress;
  photos: AdvertPhoto[];
}

export interface GetAdvertsResponse {
  items: Advert[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GetAdvertsQuery {
  page?: number;
  pageSize?: number;
  mine?: boolean;
  searchTerm?: string;
  type?: string;
  city?: string;
  minPrice?: number;
  maxPrice?: number;
  minSurfaceArea?: number;
  maxSurfaceArea?: number;
  rooms?: number;
  sortBy?: string;
  sortDescending?: boolean;
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

export async function getActiveAdverts(query?: GetAdvertsQuery): Promise<GetAdvertsResponse> {
  const params = new URLSearchParams();
  if (query) {
    if (query.page) params.set("page", String(query.page));
    if (query.pageSize) params.set("pageSize", String(query.pageSize));
    if (query.mine) params.set("mine", "true");
    if (query.searchTerm) params.set("searchTerm", query.searchTerm);
    if (query.type) params.set("type", query.type);
    if (query.city) params.set("city", query.city);
    if (query.minPrice !== undefined) params.set("minPrice", String(query.minPrice));
    if (query.maxPrice !== undefined) params.set("maxPrice", String(query.maxPrice));
    if (query.minSurfaceArea !== undefined) params.set("minSurfaceArea", String(query.minSurfaceArea));
    if (query.maxSurfaceArea !== undefined) params.set("maxSurfaceArea", String(query.maxSurfaceArea));
    if (query.rooms !== undefined) params.set("rooms", String(query.rooms));
    if (query.sortBy) params.set("sortBy", query.sortBy);
    if (query.sortDescending !== undefined) params.set("sortDescending", String(query.sortDescending));
  }

  const queryString = params.toString();
  const url = queryString ? `/api/adverts?${queryString}` : "/api/adverts";
  const response = await apiFetch(url);

  if (!response.ok) {
    throw new Error("Failed to fetch adverts");
  }

  return (await response.json()) as GetAdvertsResponse;
}

export async function getAdvertById(uuid: string): Promise<Advert | null> {
  const response = await apiFetch(`/api/adverts/${uuid}`);
  if (!response.ok) {
    if (response.status === 404) return null;
    throw new Error("Failed to fetch advert details");
  }
  return (await response.json()) as Advert;
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

export async function getFavoriteAdverts(page = 1, pageSize = 20): Promise<GetAdvertsResponse> {
  const response = await apiFetch(`/api/adverts/favorites?page=${page}&pageSize=${pageSize}`);
  if (!response.ok) {
    return { items: [], page, pageSize, totalCount: 0 };
  }
  return (await response.json()) as GetAdvertsResponse;
}

export async function getFavoriteAdvertIds(): Promise<string[]> {
  const data = await getFavoriteAdverts(1, 100);
  return data.items.map((item) => item.guid);
}
