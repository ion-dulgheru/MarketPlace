import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound, Star, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  getAdvertById,
  updateAdvert,
  addAdvertPhoto,
  deleteAdvertPhoto,
  getAdvertPhotoUrl,
  type Advert,
  type AdvertPhoto,
} from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
import { RichTextEditor } from "@/components/ui/RichTextEditor";

export const Route = createFileRoute("/editadvert/$listingId")({
  head: () => ({
    meta: [{ title: "Edit listing | OpenKey" }],
  }),
  component: EditAdvertPage,
});

function EditAdvertPage() {
  const navigate = useNavigate();
  const { listingId } = Route.useParams();

  const [advert, setAdvert] = useState<Advert | null | undefined>(undefined);
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const [photos, setPhotos] = useState<AdvertPhoto[]>([]);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [pendingPhotoId, setPendingPhotoId] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    let cancelled = false;
    getAdvertById(listingId)
      .then((result) => {
        if (cancelled) return;
        setAdvert(result);
        setDescription(result?.description ?? "");
        setPhotos(result?.photos ?? []);
      })
      .catch(() => {
        if (!cancelled) setAdvert(null);
      });

    return () => {
      cancelled = true;
    };
  }, [listingId, navigate]);

  const handleUploadPhoto = async (file: File) => {
    setPhotoError(null);
    setUploading(true);
    try {
      const photo = await addAdvertPhoto(listingId, file, photos.length === 0);
      setPhotos((current) => [...current, photo]);
    } catch (err) {
      setPhotoError(err instanceof Error ? err.message : "Failed to upload photo");
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handleDeletePhoto = async (photoUuid: string) => {
    setPhotoError(null);
    setPendingPhotoId(photoUuid);
    try {
      await deleteAdvertPhoto(listingId, photoUuid);
      setPhotos((current) => current.filter((photo) => photo.uuid !== photoUuid));
    } catch (err) {
      setPhotoError(err instanceof Error ? err.message : "Failed to delete photo");
    } finally {
      setPendingPhotoId(null);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setLoading(true);

    const form = event.currentTarget;
    const get = (name: string) =>
      (form.elements.namedItem(name) as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement)
        .value;

    const strippedText = description.replace(/<[^>]*>/g, "").trim();
    if (!strippedText) {
      setError("Description is required.");
      setLoading(false);
      return;
    }

    try {
      await updateAdvert(listingId, {
        title: get("title"),
        description,
        price: Number(get("price")),
        surfaceArea: Number(get("surfaceArea")),
        rooms: Number(get("rooms")),
        floor: Number(get("floor")),
        address: {
          country: get("country"),
          city: get("city"),
          region: get("region"),
          streetAddress: get("streetAddress"),
          streetNumber: get("streetNumber"),
        },
      });
      void navigate({ to: "/mylistings" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  if (advert === undefined) {
    return (
      <main className="grid min-h-screen place-items-center bg-brand-soft/65 px-4 text-center">
        <p className="text-sm text-muted-foreground">Loading listing…</p>
      </main>
    );
  }

  if (advert === null) {
    return (
      <main className="grid min-h-screen place-items-center bg-brand-soft/65 px-4 text-center">
        <div>
          <h1 className="font-display text-4xl">Listing not found</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            This advert may have been removed, or you don't have access to edit it.
          </p>
          <Button asChild className="mt-6">
            <Link to="/mylistings">Back to my listings</Link>
          </Button>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-brand-soft/65 px-4 py-8 text-foreground sm:px-6 lg:px-10">
      <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl flex-col justify-between gap-10">
        <header className="flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2" aria-label="OpenKey home">
            <span className="grid size-9 place-items-center rounded-md bg-primary text-primary-foreground">
              <KeyRound className="size-5" />
            </span>
            <span className="font-display text-2xl">OpenKey</span>
          </Link>
          <Link
            to="/mylistings"
            className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to my listings
          </Link>
        </header>

        <section className="mx-auto w-full max-w-3xl border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">
            {advert.type === "Sale" ? "For sale" : "For rent"}
          </p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Edit listing</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Changes are saved immediately once you submit.
          </p>

          <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Rooms
                <input
                  name="rooms"
                  required
                  type="number"
                  min={1}
                  max={20}
                  defaultValue={advert.rooms}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Floor
                <input
                  name="floor"
                  required
                  type="number"
                  min={0}
                  max={100}
                  defaultValue={advert.floor}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
            </div>

            <label className="grid gap-1.5 text-sm font-medium">
              Title
              <input
                name="title"
                required
                maxLength={200}
                defaultValue={advert.title}
                className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
              />
            </label>

            <div className="grid gap-1.5 text-sm font-medium">
              <span>Description</span>
              <RichTextEditor value={description} onChange={setDescription} />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Price (MDL)
                <input
                  name="price"
                  required
                  type="number"
                  min={1}
                  defaultValue={advert.price}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Size in m²
                <input
                  name="surfaceArea"
                  required
                  type="number"
                  min={1}
                  defaultValue={advert.surfaceArea}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
            </div>

            <p className="border-t border-border pt-3 text-sm font-semibold text-foreground">
              Address
            </p>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Country
                <input
                  name="country"
                  required
                  maxLength={100}
                  defaultValue={advert.address.country}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                City
                <input
                  name="city"
                  required
                  maxLength={100}
                  defaultValue={advert.address.city}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Region / Neighbourhood
                <input
                  name="region"
                  required
                  maxLength={100}
                  defaultValue={advert.address.region}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Street number
                <input
                  name="streetNumber"
                  required
                  maxLength={20}
                  defaultValue={advert.address.streetNumber}
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Street address
              <input
                name="streetAddress"
                required
                maxLength={200}
                defaultValue={advert.address.streetAddress}
                className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
              />
            </label>

            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {error}
              </p>
            )}

            <Button type="submit" disabled={loading} className="mt-2 w-full">
              {loading ? "Saving…" : "Save changes"}
            </Button>
          </form>

          <div className="mt-8 border-t border-border pt-6">
            <p className="text-sm font-semibold text-foreground">Photos</p>

            {photos.length > 0 && (
              <div className="mt-3 grid grid-cols-3 gap-3 sm:grid-cols-4">
                {photos.map((photo) => (
                  <div
                    key={photo.uuid}
                    className="group relative aspect-[4/3] overflow-hidden rounded-md bg-muted"
                  >
                    <img
                      src={getAdvertPhotoUrl(photo.photoUrl)}
                      alt=""
                      className="size-full object-cover"
                    />
                    {photo.isPrimary && (
                      <span className="absolute left-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-background/90 text-primary">
                        <Star className="size-3.5 fill-primary" />
                      </span>
                    )}
                    <button
                      type="button"
                      disabled={pendingPhotoId === photo.uuid}
                      onClick={() => void handleDeletePhoto(photo.uuid)}
                      aria-label="Delete photo"
                      className="absolute right-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-background/90 text-destructive hover:bg-background"
                    >
                      <X className="size-3.5" />
                    </button>
                  </div>
                ))}
              </div>
            )}

            <label className="mt-4 flex h-11 w-fit cursor-pointer items-center gap-2 rounded-md border border-input bg-background px-4 text-sm font-medium hover:bg-muted">
              {uploading ? "Uploading…" : "Add a photo"}
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                disabled={uploading}
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (file) void handleUploadPhoto(file);
                }}
              />
            </label>

            {photoError && (
              <p className="mt-3 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {photoError}
              </p>
            )}
          </div>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
