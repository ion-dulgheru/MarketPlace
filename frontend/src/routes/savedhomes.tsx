import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ArrowLeft, BedDouble, Heart, Layers, MapPin, Search, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import Header from "@/components/Navigation/header";
import {
  getFavoriteAdverts,
  unfavoriteAdvert,
  getAdvertPhotoUrl,
  type Advert,
} from "@/api/adverts";
import { formatAdvertPrice, formatPostedDate } from "@/lib/advert-format";
import { isLoggedIn } from "@/lib/tokens";
import placeholderImage from "@/assets/openkey-apartment.jpg";

export const Route = createFileRoute("/savedhomes")({
  head: () => ({
    meta: [{ title: "Saved listings | OpenKey" }],
  }),
  component: SavedHomesPage,
});

function SavedHomesPage() {
  const navigate = useNavigate();
  const [adverts, setAdverts] = useState<Advert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [unfavoritingId, setUnfavoritingId] = useState<string | null>(null);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    let cancelled = false;
    getFavoriteAdverts(1, 100)
      .then((response) => {
        if (!cancelled) setAdverts(response.items);
      })
      .catch(() => {
        if (!cancelled) setError(true);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [navigate]);

  const handleRemoveFavorite = async (guid: string) => {
    setUnfavoritingId(guid);
    const previous = [...adverts];
    setAdverts((current) => current.filter((item) => item.guid !== guid));

    try {
      await unfavoriteAdvert(guid);
    } catch {
      setAdverts(previous);
    } finally {
      setUnfavoritingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />
      <main className="mx-auto max-w-[1440px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14">
        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          <Link
            to="/account"
            className="inline-flex items-center gap-1.5 hover:text-foreground transition-colors"
          >
            <ArrowLeft className="size-4" /> My account
          </Link>
          <span>/</span>
          <Link to="/" className="hover:text-foreground transition-colors">
            Browse homes
          </Link>
        </div>

        <div className="mt-6 mb-8 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <p className="text-xs font-bold uppercase tracking-wider text-primary">
              Your collection
            </p>
            <h1 className="mt-1 font-display text-3xl sm:text-4xl">Saved listings</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              {loading
                ? "Loading saved listings…"
                : `${adverts.length} ${adverts.length === 1 ? "listing" : "listings"} saved for quick access`}
            </p>
          </div>
          <Button variant="outline" asChild>
            <Link to="/">
              <Search className="size-4" /> Explore more homes
            </Link>
          </Button>
        </div>

        {error ? (
          <div className="border-y border-border py-20 text-center">
            <h2 className="font-display text-2xl">Couldn't load your saved listings</h2>
            <p className="mt-2 text-sm text-muted-foreground">Please try again in a moment.</p>
            <Button
              variant="outline"
              className="mt-4"
              onClick={() => {
                setError(false);
                setLoading(true);
                getFavoriteAdverts(1, 100)
                  .then((res) => setAdverts(res.items))
                  .catch(() => setError(true))
                  .finally(() => setLoading(false));
              }}
            >
              Retry
            </Button>
          </div>
        ) : loading ? (
          <div className="border-y border-border py-20 text-center">
            <p className="text-sm text-muted-foreground">Loading your saved listings…</p>
          </div>
        ) : adverts.length === 0 ? (
          <div className="rounded-lg border border-border bg-card p-12 text-center">
            <div className="mx-auto grid size-16 place-items-center rounded-full bg-primary/10 text-primary">
              <Heart className="size-8" />
            </div>
            <h2 className="mt-5 font-display text-2xl">No saved listings yet</h2>
            <p className="mx-auto mt-2 max-w-md text-sm text-muted-foreground">
              Save properties you are interested in by clicking the heart icon on any listing. They
              will appear here for easy comparison.
            </p>
            <Button asChild className="mt-6">
              <Link to="/">Browse available homes</Link>
            </Button>
          </div>
        ) : (
          <div className="grid gap-x-5 gap-y-9 sm:grid-cols-2 lg:grid-cols-3">
            {adverts.map((advert) => {
              const primaryPhoto = advert.photos[0];
              const location = [advert.address.region, advert.address.city]
                .filter(Boolean)
                .join(", ");

              return (
                <article key={advert.guid} className="group min-w-0">
                  <div className="relative aspect-[4/3] overflow-hidden rounded-md bg-muted">
                    <Link
                      to="/listings/$listingId"
                      params={{ listingId: advert.guid }}
                      aria-label={`View details for ${advert.title}`}
                      className="block size-full"
                    >
                      <img
                        src={
                          primaryPhoto ? getAdvertPhotoUrl(primaryPhoto.photoUrl) : placeholderImage
                        }
                        alt={advert.title}
                        loading="lazy"
                        className="size-full object-cover transition-transform duration-300 group-hover:scale-105"
                      />
                    </Link>
                    <div className="absolute left-3 top-3 flex gap-2">
                      {advert.status === "Active" ? (
                        <span
                          className={`rounded-sm px-2.5 py-1 text-xs font-bold uppercase shadow-sm ${
                            advert.type === "Sale"
                              ? "bg-rose-100 text-rose-700"
                              : "bg-sky-100 text-sky-700"
                          }`}
                        >
                          {advert.type === "Sale" ? "For sale" : "For rent"}
                        </span>
                      ) : (
                        <span className="rounded-sm bg-status px-2.5 py-1 text-xs font-bold uppercase text-status-foreground shadow-sm">
                          {advert.status}
                        </span>
                      )}
                    </div>
                    <Button
                      size="icon"
                      variant="secondary"
                      className="absolute right-3 top-3 rounded-full shadow-md hover:bg-destructive/10 hover:text-destructive transition-colors"
                      disabled={unfavoritingId === advert.guid}
                      onClick={() => handleRemoveFavorite(advert.guid)}
                      aria-label="Remove from saved"
                      title="Remove from saved"
                    >
                      <Heart className="fill-primary text-primary group-hover/btn:fill-destructive" />
                    </Button>
                  </div>

                  <div className="pt-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <span className="font-display text-2xl font-bold">
                          {formatAdvertPrice(advert)}
                        </span>
                        <Link
                          to="/listings/$listingId"
                          params={{ listingId: advert.guid }}
                          className="mt-1 block font-display text-lg hover:text-primary transition-colors line-clamp-1"
                        >
                          {advert.title}
                        </Link>
                      </div>
                    </div>

                    <p className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
                      <MapPin className="size-3.5 shrink-0" />
                      <span className="truncate">{location || "Moldova"}</span>
                    </p>

                    <div className="mt-3 flex items-center gap-4 text-xs text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <BedDouble className="size-4 text-foreground" />
                        {advert.rooms} {advert.rooms === 1 ? "room" : "rooms"}
                      </span>
                      <span className="flex items-center gap-1">
                        <Square className="size-4 text-foreground" />
                        {advert.surfaceArea} m²
                      </span>
                      {advert.floor ? (
                        <span className="flex items-center gap-1">
                          <Layers className="size-4 text-foreground" />
                          Floors {advert.floor}
                        </span>
                      ) : null}
                    </div>

                    <p className="mt-3 text-[11px] text-muted-foreground">
                      Saved listing · Posted {formatPostedDate(advert.createdDate)}
                    </p>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </main>
    </div>
  );
}
