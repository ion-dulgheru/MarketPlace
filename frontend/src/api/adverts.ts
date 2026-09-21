import { apiFetch, API_URL } from "@/lib/api-client";

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
  floor?: number;
  type: "Sale" | "Rent";
  buildingType: "Apartment" | "House";
  levels: number;
  apartmentFloor?: number | undefined;
  apartmentNumber?: string | undefined;
  apartmentBlock?: string | undefined;
  gardenSquareMeters?: number | undefined;
  address: CreateAdvertAddress;
}

export interface CreateAdvertResponse {
  id: string;
  guid: string;
}

export async function createAdvert(data: CreateAdvertRequest): Promise<CreateAdvertResponse> {
  const response = await apiFetch("/api/adverts", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to create advert" }));
    throw new Error((error as { message?: string }).message ?? "Failed to create advert");
  }

  if (response.status === 204) {
    return { id: "", guid: "" };
  }

  const result = (await response.json()) as { id?: string; guid?: string };
  const id = result.guid ?? result.id ?? "";
  return { id, guid: id };
}

export async function setAdvertFavorite(uuid: string, isFavorite: boolean): Promise<void> {
  const response = await apiFetch(`/api/adverts/${uuid}/favorite`, {
    method: "POST",
    body: JSON.stringify({ isFavorite }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to update favorite status" }));
    throw new Error((error as { message?: string }).message ?? "Failed to update favorite status");
  }
}

export const favoriteAdvert = (uuid: string) => setAdvertFavorite(uuid, true);
export const unfavoriteAdvert = (uuid: string) => setAdvertFavorite(uuid, false);

export interface GetFavoritesResponse {
  items: Advert[];
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

export async function sendContactRequest(uuid: string, message: string): Promise<void> {
  const response = await apiFetch(`/api/adverts/${uuid}/contact-requests`, {
    method: "POST",
    body: JSON.stringify({ message }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to send contact request" }));
    throw new Error((error as { message?: string }).message ?? "Failed to send contact request");
  }
}

export interface ContactRequest {
  uuid: string;
  fromUserUuid: string;
  message: string;
  status: "Unread" | "Read";
  createdDate: string;
  senderName?: string | null;
  senderEmail?: string | null;
  senderPhone?: string | null;
}

export async function getAdvertContactRequests(uuid: string): Promise<ContactRequest[]> {
  const response = await apiFetch(`/api/adverts/${uuid}/contact-requests`);
  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to load contact requests" }));
    throw new Error((error as { message?: string }).message ?? "Failed to load contact requests");
  }
  return (await response.json()) as ContactRequest[];
}

export async function markContactRequestAsRead(uuid: string): Promise<void> {
  const response = await apiFetch(`/api/contact-requests/${uuid}/status`, {
    method: "PUT",
  });
  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to mark contact request as read" }));
    throw new Error((error as { message?: string }).message ?? "Failed to mark contact request as read");
  }
}

export interface AdvertAddress {
  country: string;
  city: string;
  region: string;
  streetAddress: string;
  streetNumber: string;
}

export interface AdvertPhoto {
  uuid: string;
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
  status: "Active" | "Sold" | "Rented";
  type: "Sale" | "Rent";
  buildingType?: "Apartment" | "House";
  levels?: number;
  apartmentFloor?: number | null;
  apartmentNumber?: string | null;
  apartmentBlock?: string | null;
  gardenSquareMeters?: number | null;
  createdDate: string;
  address: AdvertAddress;
  photos: AdvertPhoto[];
  userUuid?: string;
}

export interface GetAdvertsResponse {
  items: Advert[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GetAdvertsParams {
  page?: number;
  pageSize?: number;
  mine?: boolean | undefined;
  searchTerm?: string | undefined;
  type?: "Sale" | "Rent" | undefined;
  city?: string | undefined;
  minPrice?: number | undefined;
  maxPrice?: number | undefined;
  minSurfaceArea?: number | undefined;
  maxSurfaceArea?: number | undefined;
  rooms?: number | undefined;
  sortBy?: "date" | "price" | "surfacearea";
  sortDescending?: boolean;
}

export async function getAdverts(params: GetAdvertsParams = {}): Promise<GetAdvertsResponse> {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.set(key.charAt(0).toUpperCase() + key.slice(1), String(value));
    }
  }

  const response = await apiFetch(`/api/adverts?${searchParams.toString()}`);
  if (!response.ok) {
    throw new Error("Failed to load adverts");
  }

  return (await response.json()) as GetAdvertsResponse;
}

export function getAdvertPhotoUrl(photoUrl: string): string {
  const normalized = photoUrl.replace("/uploads/adverts/", "/uploads/listings/");
  return normalized.startsWith("http") ? normalized : `${API_URL}${normalized}`;
}

export async function addAdvertPhoto(
  advertGuid: string,
  file: File,
  isPrimary = false,
): Promise<AdvertPhoto> {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("isPrimary", String(isPrimary));

  const response = await apiFetch(`/api/adverts/${advertGuid}/photos`, {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to upload photo" }));
    throw new Error((error as { message?: string }).message ?? "Failed to upload photo");
  }

  return (await response.json()) as AdvertPhoto;
}

export async function deleteAdvertPhoto(advertGuid: string, photoUuid: string): Promise<void> {
  const response = await apiFetch(`/api/adverts/${advertGuid}/photos/${photoUuid}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to delete photo" }));
    throw new Error((error as { message?: string }).message ?? "Failed to delete photo");
  }
}

export interface UpdateAdvertRequest {
  title: string;
  description: string;
  price: number;
  surfaceArea: number;
  rooms: number;
  floor: number;
  address: CreateAdvertAddress;
  levels?: number;
  apartmentFloor?: number | undefined;
  apartmentNumber?: string | undefined;
  apartmentBlock?: string | undefined;
  gardenSquareMeters?: number | undefined;
}

export async function updateAdvert(guid: string, data: UpdateAdvertRequest): Promise<void> {
  const response = await apiFetch(`/api/adverts/${guid}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to update advert" }));
    throw new Error((error as { message?: string }).message ?? "Failed to update advert");
  }
}

export async function updateAdvertStatus(
  guid: string,
  status: "Active" | "Sold" | "Rented",
): Promise<void> {
  const response = await apiFetch(`/api/adverts/${guid}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to update status" }));
    throw new Error((error as { message?: string }).message ?? "Failed to update status");
  }
}

export async function deleteAdvert(guid: string): Promise<void> {
  const response = await apiFetch(`/api/adverts/${guid}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to delete advert" }));
    throw new Error((error as { message?: string }).message ?? "Failed to delete advert");
  }
}

export async function getAdvertById(guid: string): Promise<Advert | null> {
  const response = await apiFetch(`/api/adverts/${guid}`);
  if (response.status === 404) {
    return null;
  }
  if (!response.ok) {
    throw new Error("Failed to load advert");
  }

  return (await response.json()) as Advert;
}
