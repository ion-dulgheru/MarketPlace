import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useRef, useState, type FormEvent, type DragEvent } from "react";
import { ArrowLeft, Image as ImageIcon, KeyRound, Star, Upload, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { createAdvert, addAdvertPhoto } from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
import { RichTextEditor } from "@/components/ui/RichTextEditor";

export const Route = createFileRoute("/createadvert")({
  head: () => ({
    meta: [
      { title: "Publish a listing | OpenKey" },
      {
        name: "description",
        content: "Publish your property for sale or rent on OpenKey.",
      },
    ],
  }),
  component: CreateAdvertPage,
});

interface SelectedPhoto {
  id: string;
  file: File;
  previewUrl: string;
  isPrimary: boolean;
}

function CreateAdvertPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [description, setDescription] = useState("");

  const [photos, setPhotos] = useState<SelectedPhoto[]>([]);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const photosRef = useRef<SelectedPhoto[]>([]);
  photosRef.current = photos;

  // Cleanup object URLs on unmount
  useEffect(() => {
    return () => {
      photosRef.current.forEach((p) => URL.revokeObjectURL(p.previewUrl));
    };
  }, []);

  // Doar userii logați pot publica un anunț
  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/register" });
    }
  }, [navigate]);

  const handleAddFiles = (files: FileList | File[] | null) => {
    if (!files || files.length === 0) return;
    setPhotoError(null);

    const validFiles: File[] = [];
    const maxFileSize = 10 * 1024 * 1024; // 10MB
    const maxTotalPhotos = 20;

    if (photos.length + files.length > maxTotalPhotos) {
      setPhotoError(`You can upload up to ${maxTotalPhotos} photos per listing.`);
      return;
    }

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      if (!file.type.startsWith("image/")) {
        setPhotoError("Only image files (JPEG, PNG, WebP, etc.) are supported.");
        continue;
      }
      if (file.size > maxFileSize) {
        setPhotoError(`"${file.name}" exceeds the 10MB file size limit.`);
        continue;
      }
      validFiles.push(file);
    }

    if (validFiles.length === 0) return;

    setPhotos((current) => {
      const hasPrimary = current.some((p) => p.isPrimary);
      const newItems: SelectedPhoto[] = validFiles.map((file, idx) => ({
        id: `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`,
        file,
        previewUrl: URL.createObjectURL(file),
        isPrimary: !hasPrimary && idx === 0,
      }));
      return [...current, ...newItems];
    });

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleRemovePhoto = (id: string) => {
    setPhotos((current) => {
      const target = current.find((p) => p.id === id);
      if (target) {
        URL.revokeObjectURL(target.previewUrl);
      }
      const updated = current.filter((p) => p.id !== id);
      // If we removed the primary photo and have remaining photos, make the first one primary
      if (target?.isPrimary && updated.length > 0) {
        updated[0] = { ...updated[0], isPrimary: true };
      }
      return updated;
    });
  };

  const handleSetPrimary = (id: string) => {
    setPhotos((current) =>
      current.map((p) => ({
        ...p,
        isPrimary: p.id === id,
      })),
    );
  };

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
    if (e.dataTransfer.files) {
      handleAddFiles(e.dataTransfer.files);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setPhotoError(null);
    setLoading(true);
    setStatusMessage("Creating listing…");

    const form = event.currentTarget;
    const get = (name: string) =>
      (form.elements.namedItem(name) as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement).value;

    const strippedText = description.replace(/<[^>]*>/g, "").trim();
    if (!strippedText) {
      setError("Description is required.");
      setLoading(false);
      setStatusMessage(null);
      return;
    }

    try {
      const created = await createAdvert({
        title: get("title"),
        description: description,
        price: Number(get("price")),
        surfaceArea: Number(get("surfaceArea")),
        rooms: Number(get("rooms")),
        floor: Number(get("floor")),
        type: get("type") as "Sale" | "Rent",
        address: {
          country: get("country"),
          city: get("city"),
          region: get("region"),
          streetAddress: get("streetAddress"),
          streetNumber: get("streetNumber"),
        },
      });

      const advertId = created.id || created.guid;

      if (advertId && photos.length > 0) {
        for (let i = 0; i < photos.length; i++) {
          setStatusMessage(`Uploading photo ${i + 1} of ${photos.length}…`);
          try {
            await addAdvertPhoto(advertId, photos[i].file, photos[i].isPrimary);
          } catch (photoErr) {
            console.error(`Failed to upload photo ${i + 1}:`, photoErr);
            // Continue uploading the remaining photos even if one fails
          }
        }
      }

      setStatusMessage("Listing published!");
      if (advertId) {
        void navigate({ to: `/listings/${advertId}` });
      } else {
        void navigate({ to: "/" });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setLoading(false);
      setStatusMessage(null);
    }
  };

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
          <Link to="/" className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground">
            <ArrowLeft className="size-4" /> Back to homes
          </Link>
        </header>

        <section className="mx-auto w-full max-w-3xl border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Publish your property</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Create a listing</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Your listing becomes public immediately. You can mark it sold or rented later.
          </p>

          <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Listing type
                <select name="type" required defaultValue="Sale" className="h-11 rounded-md border border-input bg-background px-3">
                  <option value="Sale">For sale</option>
                  <option value="Rent">For rent</option>
                </select>
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Rooms
                <input name="rooms" required type="number" min={1} max={20} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>

            <label className="grid gap-1.5 text-sm font-medium">
              Title
              <input name="title" required maxLength={200} placeholder="Bright two-bedroom apartment" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            <div className="grid gap-1.5 text-sm font-medium">
              <span>Description</span>
              <RichTextEditor value={description} onChange={setDescription} />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Price (MDL)
                <input name="price" required type="number" min={1} placeholder="485000" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Size in m²
                <input name="surfaceArea" required type="number" min={1} placeholder="84" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>

            <label className="grid gap-1.5 text-sm font-medium">
              Floor
              <input name="floor" required type="number" min={0} max={100} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {/* Photos Section */}
            <div className="border-t border-border pt-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-semibold text-foreground">Photos</p>
                  <p className="text-xs text-muted-foreground">
                    Upload photos of your property. The photo with the star will be the cover image.
                  </p>
                </div>
                {photos.length > 0 && (
                  <span className="text-xs font-medium text-muted-foreground">
                    {photos.length} {photos.length === 1 ? "photo" : "photos"} selected
                  </span>
                )}
              </div>

              {/* Photos Preview Grid */}
              {photos.length > 0 && (
                <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                  {photos.map((photo) => (
                    <div
                      key={photo.id}
                      className="group relative aspect-[4/3] overflow-hidden rounded-md border border-border bg-muted shadow-sm transition-all hover:shadow-md"
                    >
                      <img
                        src={photo.previewUrl}
                        alt=""
                        className="size-full object-cover transition-transform duration-200 group-hover:scale-105"
                      />

                      {/* Primary Badge or Make Primary Button */}
                      {photo.isPrimary ? (
                        <span className="absolute left-1.5 top-1.5 inline-flex items-center gap-1 rounded-full bg-background/90 px-2 py-0.5 text-[11px] font-semibold text-primary shadow-sm backdrop-blur-sm">
                          <Star className="size-3 fill-primary text-primary" />
                          Cover
                        </span>
                      ) : (
                        <button
                          type="button"
                          onClick={() => handleSetPrimary(photo.id)}
                          title="Set as cover photo"
                          className="absolute left-1.5 top-1.5 inline-flex items-center gap-1 rounded-full bg-background/80 px-2 py-0.5 text-[11px] font-medium text-muted-foreground opacity-0 backdrop-blur-sm transition-opacity hover:bg-background hover:text-primary group-hover:opacity-100"
                        >
                          <Star className="size-3" />
                          Set cover
                        </button>
                      )}

                      {/* Remove Button */}
                      <button
                        type="button"
                        onClick={() => handleRemovePhoto(photo.id)}
                        aria-label="Remove photo"
                        className="absolute right-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-background/90 text-destructive shadow-sm backdrop-blur-sm transition-colors hover:bg-destructive hover:text-destructive-foreground"
                      >
                        <X className="size-3.5" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* Upload Dropzone */}
              <div
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
                className={`mt-4 flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-6 text-center transition-colors ${
                  isDragging
                    ? "border-primary bg-primary/10"
                    : "border-muted-foreground/25 hover:border-primary/50 hover:bg-muted/40"
                }`}
              >
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  multiple
                  className="hidden"
                  onChange={(e) => handleAddFiles(e.target.files)}
                />
                <div className="grid size-10 place-items-center rounded-full bg-primary/10 text-primary">
                  {photos.length > 0 ? (
                    <ImageIcon className="size-5" />
                  ) : (
                    <Upload className="size-5" />
                  )}
                </div>
                <p className="mt-2 text-sm font-medium">
                  {photos.length > 0 ? "Add more photos" : "Choose photos or drag & drop"}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  PNG, JPG, WebP up to 10MB each (max 20 photos)
                </p>
              </div>

              {photoError && (
                <p className="mt-2 rounded-md bg-destructive/10 px-3 py-2 text-xs text-destructive">
                  {photoError}
                </p>
              )}
            </div>

            <p className="border-t border-border pt-3 text-sm font-semibold text-foreground">Address</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Country
                <input name="country" required maxLength={100} placeholder="Moldova" defaultValue="Moldova" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                City
                <input name="city" required maxLength={100} placeholder="Chișinău" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Region / Neighbourhood
                <input name="region" required maxLength={100} placeholder="Centru" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Street number
                <input name="streetNumber" required maxLength={20} placeholder="12A" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Street address
              <input name="streetAddress" required maxLength={200} placeholder="Strada Ștefan cel Mare" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
            )}

            <Button type="submit" disabled={loading} className="mt-2 w-full">
              {loading ? (statusMessage ?? "Publishing…") : "Publish now"}
            </Button>
          </form>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}

