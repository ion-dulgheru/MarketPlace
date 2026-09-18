import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ArrowLeft, BedDouble, Heart, Layers, MapPin, MessageSquare, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  getAdvertById,
  getAdvertPhotoUrl,
  favoriteAdvert,
  unfavoriteAdvert,
  getFavoriteAdvertIds,
  type Advert,
} from "@/api/adverts";
import { formatAdvertPrice, formatPostedDate } from "@/lib/advert-format";
import { getCurrentUserUuid, isLoggedIn } from "@/lib/tokens";
import { ContactOwnerDialog } from "@/components/dialogs/ContactOwnerDialog";
import { AdvertContactRequestsDialog } from "@/components/dialogs/AdvertContactRequestsDialog";
import placeholderImage from "@/assets/openkey-apartment.jpg";
import DOMPurify from "dompurify";

export const Route = createFileRoute("/listings/$listingId")({
  head: () => ({
    meta: [{ title: "Listing details | OpenKey Moldova" }],
  }),
  component: ListingDetailsPage,
});

function ListingDetailsPage() {
  const navigate = useNavigate();
  const { listingId } = Route.useParams();

  const [advert, setAdvert] = useState<Advert | null | undefined>(undefined);
  const [selectedImage, setSelectedImage] = useState(0);
  const [saved, setSaved] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [inquiriesOpen, setInquiriesOpen] = useState(false);

  const currentUserUuid = getCurrentUserUuid();
  const isOwner = Boolean(
    currentUserUuid &&
      advert?.userUuid &&
      currentUserUuid.toLowerCase() === advert.userUuid.toLowerCase(),
  );

  useEffect(() => {
    let cancelled = false;
    setAdvert(undefined);
    setSelectedImage(0);

    getAdvertById(listingId)
      .then((result) => {
        if (!cancelled) setAdvert(result);
      })
      .catch(() => {
        if (!cancelled) setAdvert(null);
      });

    return () => {
      cancelled = true;
    };
  }, [listingId]);

  useEffect(() => {
    if (isLoggedIn()) {
      void getFavoriteAdvertIds().then((ids) => {
        if (ids.includes(listingId)) {
          setSaved(true);
        }
      });
    }
  }, [listingId]);

  const handleToggleFavorite = async () => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }
    const nextSaved = !saved;
    setSaved(nextSaved);
    try {
      if (nextSaved) {
        await favoriteAdvert(listingId);
      } else {
        await unfavoriteAdvert(listingId);
      }
    } catch {
      setSaved(!nextSaved);
    }
  };

  const handleContact = () => {
    if (isOwner) return;
    setDialogOpen(true);
  };

  if (advert === undefined) {
    return (
      <main className="grid min-h-screen place-items-center bg-background px-4 text-center">
        <p className="text-sm text-muted-foreground">Loading listing…</p>
      </main>
    );
  }

  if (advert === null) {
    return (
      <main className="grid min-h-screen place-items-center bg-background px-4 text-center">
        <div>
          <h1 className="font-display text-4xl">Listing not found</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            This advert may have been removed or the link is incorrect.
          </p>
          <Button asChild className="mt-6">
            <Link to="/">Back to listings</Link>
          </Button>
        </div>
      </main>
    );
  }

  const images =
    advert.photos.length > 0
      ? advert.photos.map((photo) => getAdvertPhotoUrl(photo.photoUrl))
      : [placeholderImage];

  const location = [
    [advert.address.streetAddress, advert.address.streetNumber].filter(Boolean).join(" "),
    advert.address.city,
    advert.address.region,
  ]
    .filter(Boolean)
    .join(", ");

  const unavailable = advert.status !== "Active";

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-background">
        <div className="mx-auto flex h-18 max-w-[1440px] items-center px-4 sm:px-7 lg:px-10">
          <Link
            to="/"
            className="flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to listings
          </Link>
        </div>
      </header>

      <main className="mx-auto max-w-[1440px] px-4 py-8 sm:px-7 sm:py-12 lg:px-10">
        <div className="grid gap-10 lg:grid-cols-[minmax(0,1.35fr)_minmax(320px,0.65fr)] lg:items-start">
          <section aria-label="Listing photos">
            <div className="relative aspect-[4/3] overflow-hidden rounded-md bg-muted">
              <img
                src={images[selectedImage]}
                alt={advert.title}
                className="size-full object-cover"
              />
              {unavailable && (
                <div className="absolute inset-0 grid place-items-center bg-foreground/35">
                  <span className="-rotate-3 border-2 border-status-foreground bg-status px-5 py-2 text-lg font-bold uppercase text-status-foreground">
                    {advert.status}
                  </span>
                </div>
              )}
            </div>
            {images.length > 1 && (
              <div className="mt-3 grid grid-cols-3 gap-3">
                {images.map((image, index) => (
                  <button
                    key={image}
                    type="button"
                    onClick={() => setSelectedImage(index)}
                    className={`aspect-[4/3] overflow-hidden rounded-md border-2 bg-muted ${selectedImage === index ? "border-primary" : "border-transparent"}`}
                    aria-label={`View photo ${index + 1}`}
                  >
                    <img src={image} alt="" className="size-full object-cover" />
                  </button>
                ))}
              </div>
            )}
          </section>

          <section>
            <div className="flex items-start justify-between gap-4">
              <div>
                {advert.status !== "Active" ? (
                  <span className="rounded-sm bg-status px-2.5 py-1 text-xs font-bold uppercase text-status-foreground">
                    {advert.status}
                  </span>
                ) : (
                  <span className="text-xs font-bold uppercase text-primary">
                    {advert.type === "Sale" ? "For sale" : "For rent"}
                  </span>
                )}
                <h1 className="mt-2 font-display text-4xl leading-tight sm:text-5xl">
                  {advert.title}
                </h1>
              </div>
              <Button
                size="icon"
                variant="outline"
                className="shrink-0 rounded-full"
                onClick={handleToggleFavorite}
                aria-label={saved ? "Remove from saved" : "Save listing"}
              >
                <Heart className={saved ? "fill-primary text-primary" : ""} />
              </Button>
            </div>
            {location && (
              <p className="mt-5 flex items-center gap-2 text-muted-foreground">
                <MapPin className="size-4 text-primary" />
                {location}
              </p>
            )}
            <p className="mt-6 font-display text-3xl">{formatAdvertPrice(advert)}</p>

            <div className="mt-6 grid grid-cols-3 border-y border-border py-4 text-sm text-muted-foreground">
              <span className="flex items-center gap-2">
                <BedDouble className="size-4" />
                {advert.rooms} rooms
              </span>
              <span className="flex items-center gap-2">
                <Square className="size-4" />
                {advert.surfaceArea} m²
              </span>
              <span className="flex items-center gap-2">
                <Layers className="size-4" />
                Floor {advert.floor}
              </span>
            </div>

            <div className="mt-7 flex items-center justify-between gap-4 border-b border-border pb-6">
              <p className="text-sm text-muted-foreground">
                Posted {formatPostedDate(advert.createdDate)}
              </p>
              {isOwner ? (
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    className="gap-1.5"
                    onClick={() => setInquiriesOpen(true)}
                  >
                    <MessageSquare className="size-4 text-primary" />
                    Inquiries
                  </Button>
                  <Button variant="outline" asChild>
                    <Link to="/editadvert/$listingId" params={{ listingId: advert.guid }}>
                      Edit listing
                    </Link>
                  </Button>
                </div>
              ) : (
                <Button disabled={unavailable} onClick={handleContact}>
                  {unavailable ? "Unavailable" : "Contact seller"}
                </Button>
              )}
            </div>

            <div className="mt-7">
              <h2 className="font-display text-2xl">About this property</h2>
              <div
                className="mt-3 text-sm leading-7 text-muted-foreground [&_p]:mb-3 [&_strong]:font-semibold [&_strong]:text-foreground [&_em]:italic [&_h2]:text-lg [&_h2]:font-bold [&_h2]:text-foreground [&_h2]:mt-4 [&_h2]:mb-2 [&_ul]:list-disc [&_ul]:ml-5 [&_ul]:mb-3 [&_ol]:list-decimal [&_ol]:ml-5 [&_ol]:mb-3"
                dangerouslySetInnerHTML={{
                  __html: DOMPurify.sanitize(advert.description),
                }}
              />
            </div>
          </section>
        </div>
      </main>

      <ContactOwnerDialog
        open={dialogOpen}
        advertUuid={advert.guid}
        sellerName="the owner"
        listingTitle={advert.title}
        onClose={() => setDialogOpen(false)}
        onSignIn={() => void navigate({ to: "/login" })}
      />

      {inquiriesOpen && advert && (
        <AdvertContactRequestsDialog
          open={inquiriesOpen}
          advertUuid={advert.guid}
          advertTitle={advert.title}
          onClose={() => setInquiriesOpen(false)}
        />
      )}
    </div>
  );
}
