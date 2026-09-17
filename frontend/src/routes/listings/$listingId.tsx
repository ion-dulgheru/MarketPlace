import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ArrowLeft, Bath, BedDouble, Check, Heart, MapPin, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import { listingDetails } from "@/data/listing-details";
import { favoriteAdvert, unfavoriteAdvert, getFavoriteAdvertIds } from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
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
  const listing = listingDetails.find((item) => item.id === Number(listingId));
  const [selectedImage, setSelectedImage] = useState(0);
  const [saved, setSaved] = useState(false);

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
      // Keep optimistic state or revert on real server error if desired
    }
  };

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
                src={listing.images[selectedImage]}
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
                className="mt-3 text-sm leading-7 text-muted-foreground [&_p]:mb-3 [&_strong]:font-semibold [&_strong]:text-foreground [&_em]:italic [&_h2]:text-lg [&_h2]:font-bold [&_h2]:text-foreground [&_h2]:mt-4 [&_h2]:mb-2 [&_ul]:list-disc [&_ul]:ml-5 [&_ul]:mb-3 [&_ol]:list-decimal [&_ol]:ml-5 [&_ol]:mb-3"
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
