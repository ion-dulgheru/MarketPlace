import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ArrowLeft, Bath, BedDouble, Check, Heart, Loader2, MapPin, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import { listingDetails, type ListingDetail } from "@/data/listing-details";
import { getAdvertById, favoriteAdvert, unfavoriteAdvert, getFavoriteAdvertIds } from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
import DOMPurify from "dompurify";
import apartment from "@/assets/openkey-apartment.jpg";

export const Route = createFileRoute("/listings/$listingId")({
  head: () => ({
    meta: [{ title: "Listing details | OpenKey Moldova" }],
  }),
  component: ListingDetailsPage,
});

function ListingDetailsPage() {
  const navigate = useNavigate();
  const { listingId } = Route.useParams();
  const [listing, setListing] = useState<ListingDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedImage, setSelectedImage] = useState(0);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    let isCancelled = false;
    setLoading(true);

    async function loadData() {
      if (isLoggedIn()) {
        try {
          const ids = await getFavoriteAdvertIds();
          if (!isCancelled && ids.includes(listingId)) {
            setSaved(true);
          }
        } catch {
          // Ignore
        }
      }

      if (listingId.includes("-")) {
        try {
          const advert = await getAdvertById(listingId);
          if (!isCancelled) {
            if (advert) {
              const fullAddress = [
                advert.address.streetAddress,
                advert.address.streetNumber,
                advert.address.region,
                advert.address.city,
                advert.address.country,
              ]
                .filter(Boolean)
                .join(", ");

              const mapped: ListingDetail = {
                id: advert.guid,
                title: advert.title,
                location: fullAddress || advert.address.city,
                price:
                  advert.type.toLowerCase() === "rent"
                    ? `MDL ${advert.price.toLocaleString()} / month`
                    : `MDL ${advert.price.toLocaleString()}`,
                kind: advert.type.toLowerCase() === "rent" ? "rent" : "sale",
                type: advert.type,
                beds: advert.rooms,
                baths: 1,
                area: advert.surfaceArea,
                images:
                  advert.photos && advert.photos.length > 0
                    ? advert.photos.map((p) => p.photoUrl)
                    : [apartment],
                imageAlt: advert.title,
                seller: "Owner",
                sellerRole: "Verified Seller",
                posted: new Date(advert.createdDate).toLocaleDateString("en-US", {
                  year: "numeric",
                  month: "short",
                  day: "numeric",
                }),
                description: advert.description,
                amenities: [
                  `Floor: ${advert.floor}`,
                  `Rooms: ${advert.rooms}`,
                  `Area: ${advert.surfaceArea} m²`,
                  `Type: ${advert.type}`,
                  `City: ${advert.address.city}`,
                ],
                status:
                  advert.status.toLowerCase() === "active"
                    ? undefined
                    : (advert.status.toLowerCase() as "sold" | "rented"),
              };
              setListing(mapped);
            } else {
              setListing(null);
            }
          }
        } catch {
          if (!isCancelled) setListing(null);
        }
      } else {
        const mockListing = listingDetails.find((item) => item.id === Number(listingId));
        if (!isCancelled) {
          setListing(mockListing ?? null);
        }
      }

      if (!isCancelled) {
        setLoading(false);
      }
    }

    void loadData();

    return () => {
      isCancelled = true;
    };
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

  if (loading) {
    return (
      <main className="grid min-h-screen place-items-center bg-background px-4 text-center">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="size-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">Loading property details…</p>
        </div>
      </main>
    );
  }

  if (!listing) {
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
                src={listing.images[selectedImage] || apartment}
                alt={listing.imageAlt}
                className="size-full object-cover"
              />
              {listing.status && (
                <div className="absolute inset-0 grid place-items-center bg-foreground/35">
                  <span className="-rotate-3 border-2 border-status-foreground bg-status px-5 py-2 text-lg font-bold uppercase text-status-foreground">
                    {listing.status}
                  </span>
                </div>
              )}
            </div>
            {listing.images.length > 1 && (
              <div className="mt-3 grid grid-cols-3 gap-3">
                {listing.images.map((image, index) => (
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
                <span className="text-xs font-bold uppercase text-primary">
                  {listing.kind === "sale" ? "For sale" : "For rent"}
                </span>
                <h1 className="mt-2 font-display text-4xl leading-tight sm:text-5xl">
                  {listing.title}
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
            <p className="mt-5 flex items-center gap-2 text-muted-foreground">
              <MapPin className="size-4 text-primary" />
              {listing.location}
            </p>
            <p className="mt-6 font-display text-3xl">{listing.price}</p>

            <div className="mt-6 grid grid-cols-3 border-y border-border py-4 text-sm text-muted-foreground">
              <span className="flex items-center gap-2">
                <BedDouble className="size-4" />
                {listing.beds} beds
              </span>
              <span className="flex items-center gap-2">
                <Bath className="size-4" />
                {listing.baths} baths
              </span>
              <span className="flex items-center gap-2">
                <Square className="size-4" />
                {listing.area} m²
              </span>
            </div>

            <div className="mt-7 flex items-center justify-between gap-4 border-b border-border pb-6">
              <div>
                <p className="font-semibold">{listing.seller}</p>
                <p className="text-sm text-muted-foreground">
                  {listing.sellerRole} · Posted {listing.posted}
                </p>
              </div>
              <Button disabled={Boolean(listing.status)}>
                {listing.status ? "Unavailable" : "Contact seller"}
              </Button>
            </div>

            <div className="mt-7">
              <h2 className="font-display text-2xl">About this property</h2>
              <div
                className="mt-3 text-sm leading-7 text-muted-foreground [&_p]:mb-3 [&_strong]:font-semibold [&_strong]:text-foreground [&_em]:italic [&_u]:underline [&_s]:line-through [&_h1]:text-2xl [&_h1]:font-bold [&_h1]:text-foreground [&_h1]:mt-4 [&_h1]:mb-2 [&_h2]:text-xl [&_h2]:font-bold [&_h2]:text-foreground [&_h2]:mt-4 [&_h2]:mb-2 [&_h3]:text-lg [&_h3]:font-bold [&_h3]:text-foreground [&_h3]:mt-3 [&_h3]:mb-1 [&_ul]:list-disc [&_ul]:ml-5 [&_ul]:mb-3 [&_ol]:list-decimal [&_ol]:ml-5 [&_ol]:mb-3 [&_li]:mb-1 [&_blockquote]:border-l-4 [&_blockquote]:border-primary/40 [&_blockquote]:pl-4 [&_blockquote]:italic [&_code]:rounded [&_code]:bg-muted [&_code]:px-1.5 [&_code]:py-0.5 [&_code]:text-xs [&_a]:text-primary [&_a]:underline"
                dangerouslySetInnerHTML={{
                  __html: DOMPurify.sanitize(listing.description),
                }}
              />
            </div>

            <div className="mt-7">
              <h2 className="font-display text-2xl">Features</h2>
              <ul className="mt-3 grid gap-3 sm:grid-cols-2">
                {listing.amenities.map((amenity) => (
                  <li
                    key={amenity}
                    className="flex items-center gap-2 text-sm text-muted-foreground"
                  >
                    <Check className="size-4 text-primary" />
                    {amenity}
                  </li>
                ))}
              </ul>
            </div>
          </section>
        </div>
      </main>
    </div>
  );
}

