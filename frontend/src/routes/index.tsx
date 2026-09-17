import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import {
  BedDouble,
  Heart,
  KeyRound,
  Layers,
  MapPin,
  Plus,
  Search,
  Sparkles,
  Square,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { ContactOwnerDialog } from "@/components/dialogs/ContactOwnerDialog";
import { isLoggedIn } from "@/lib/tokens";
import {
  getAdverts,
  getAdvertPhotoUrl,
  getFavoriteAdvertIds,
  favoriteAdvert,
  unfavoriteAdvert,
  type Advert,
} from "@/api/adverts";
import { formatAdvertPrice, formatPostedDate } from "@/lib/advert-format";
import placeholderImage from "@/assets/openkey-apartment.jpg";
import Header from "@/components/Navigation/header";

const PAGE_SIZE = 9;

type SortOption = "date" | "price-asc" | "price-desc";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "OpenKey Moldova — Homes for Sale & Rent" },
      {
        name: "description",
        content:
          "Browse public property listings across Moldova or publish your own home for sale or rent instantly on OpenKey.",
      },
      { property: "og:title", content: "OpenKey Moldova — Homes for Sale & Rent" },
      {
        property: "og:description",
        content:
          "Open property listings across Moldova, published instantly by owners and agencies.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: Index,
});

function Index() {
  const navigate = useNavigate();
  const [mode, setMode] = useState<"all" | "sale" | "rent">("all");
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [sort, setSort] = useState<SortOption>("date");

  const [adverts, setAdverts] = useState<Advert[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState(false);

  const [savedIds, setSavedIds] = useState<string[]>([]);
  const [dialog, setDialog] = useState<"contact" | null>(null);
  const [selected, setSelected] = useState<Advert | null>(null);

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedQuery(query), 400);
    return () => clearTimeout(timeout);
  }, [query]);

  useEffect(() => {
    setPage(1);
  }, [mode, debouncedQuery, sort]);

  useEffect(() => {
    let cancelled = false;
    const isFirstPage = page === 1;
    (isFirstPage ? setLoading : setLoadingMore)(true);
    setError(false);

    const [sortBy, sortDescending] =
      sort === "price-asc"
        ? (["price", false] as const)
        : sort === "price-desc"
          ? (["price", true] as const)
          : (["date", true] as const);

    getAdverts({
      page,
      pageSize: PAGE_SIZE,
      searchTerm: debouncedQuery || undefined,
      type: mode === "all" ? undefined : mode === "sale" ? "Sale" : "Rent",
      sortBy,
      sortDescending,
    })
      .then((response) => {
        if (cancelled) return;
        setAdverts((previous) => (isFirstPage ? response.items : [...previous, ...response.items]));
        setTotalCount(response.totalCount);
      })
      .catch(() => {
        if (!cancelled) setError(true);
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
          setLoadingMore(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [mode, debouncedQuery, sort, page]);

  useEffect(() => {
    if (isLoggedIn()) {
      void getFavoriteAdvertIds().then(setSavedIds);
    }
  }, []);

  const openContact = (advert: Advert) => {
    setSelected(advert);
    setDialog("contact");
  };

  const toggleSaved = (guid: string) => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }
    const nextSaved = !savedIds.includes(guid);
    setSavedIds((ids) => (nextSaved ? [...ids, guid] : ids.filter((id) => id !== guid)));
    void (nextSaved ? favoriteAdvert(guid) : unfavoriteAdvert(guid));
  };

  const hasMore = adverts.length < totalCount;

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />

      <main id="top">
        <section className="border-b border-border bg-[#f5f9fa]">
          <div className="mx-auto max-w-[1440px] px-4 py-12 sm:px-7 sm:py-16 lg:px-10 flex flex-col items-center">
            <div className="max-w-4xl w-full text-center">
              <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-primary">
                <Sparkles className="size-4" /> New homes appear the moment they’re published
              </p>
              <h1 className="max-w-3xl font-display text-4xl leading-[1.04] sm:text-5xl lg:text-6xl">
                Find a place you'll love to call home.
              </h1>
              <p className="mt-4 max-w-2xl text-base leading-7 text-muted-foreground sm:text-lg">
                Open listings from owners and agencies. Browse freely, contact directly, and skip
                the waiting list.
              </p>
            </div>

            <div className="mt-9 w-full max-w-6xl border border-border bg-card p-3 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-4">
              <div className="mb-3 flex w-fit gap-1 rounded-md bg-muted p-1">
                {(["all", "sale", "rent"] as const).map((item) => (
                  <Button
                    key={item}
                    size="sm"
                    variant={mode === item ? "default" : "ghost"}
                    onClick={() => setMode(item)}
                  >
                    {item === "all" ? "All homes" : item === "sale" ? "For sale" : "For rent"}
                  </Button>
                ))}
              </div>
              <div className="grid gap-2 md:grid-cols-[1fr_auto]">
                <label className="flex h-12 items-center gap-3 rounded-md border border-input bg-background px-4">
                  <MapPin className="size-5 text-primary" />
                  <span className="sr-only">Search adverts</span>
                  <input
                    value={query}
                    onChange={(event) => setQuery(event.target.value)}
                    placeholder="Search by title or description"
                    className="min-w-0 flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground"
                  />
                </label>
                <Button className="h-12 px-6">
                  <Search /> Search homes
                </Button>
              </div>
            </div>
          </div>
        </section>

        <section
          id="listings"
          className="mx-auto max-w-[1440px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14"
        >
          <div className="mb-7 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div>
              <p className="text-xs font-bold uppercase text-primary">Recently published</p>
              <h2 className="mt-1 font-display text-3xl sm:text-4xl">Homes ready to discover</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                {loading ? "Loading listings…" : `${totalCount} listings match your search`}
              </p>
            </div>
            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              Sort by{" "}
              <select
                value={sort}
                onChange={(event) => setSort(event.target.value as SortOption)}
                className="rounded-md border border-input bg-background px-3 py-2 font-medium text-foreground outline-none"
              >
                <option value="date">Newest first</option>
                <option value="price-asc">Price: low to high</option>
                <option value="price-desc">Price: high to low</option>
              </select>
            </label>
          </div>

          {error ? (
            <div className="border-y border-border py-20 text-center">
              <h3 className="font-display text-2xl">Couldn't load listings</h3>
              <p className="mt-2 text-sm text-muted-foreground">Please try again in a moment.</p>
            </div>
          ) : !loading && adverts.length === 0 ? (
            <div className="border-y border-border py-20 text-center">
              <Search className="mx-auto size-8 text-muted-foreground" />
              <h3 className="mt-4 font-display text-2xl">No homes found</h3>
              <p className="mt-2 text-sm text-muted-foreground">
                Try another search term or category.
              </p>
              <Button
                variant="outline"
                className="mt-5"
                onClick={() => {
                  setQuery("");
                  setMode("all");
                }}
              >
                Clear filters
              </Button>
            </div>
          ) : (
            <>
              <div className="grid gap-x-5 gap-y-9 sm:grid-cols-2 lg:grid-cols-3">
                {adverts.map((advert) => {
                  const primaryPhoto = advert.photos[0];
                  const location = [advert.address.region, advert.address.city]
                    .filter(Boolean)
                    .join(", ");
                  const saved = savedIds.includes(advert.guid);
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
                              primaryPhoto
                                ? getAdvertPhotoUrl(primaryPhoto.photoUrl)
                                : placeholderImage
                            }
                            alt={advert.title}
                            loading="lazy"
                            className="listing-image size-full object-cover"
                          />
                        </Link>
                        <div className="absolute left-3 top-3 flex gap-2">
                          <span className="rounded-sm bg-background/95 px-2.5 py-1 text-xs font-bold uppercase">
                            {advert.type === "Sale" ? "For sale" : "For rent"}
                          </span>
                        </div>
                        <Button
                          size="icon"
                          variant="secondary"
                          className="absolute right-3 top-3 rounded-full"
                          onClick={() => toggleSaved(advert.guid)}
                          aria-label={saved ? "Remove from saved" : "Save listing"}
                        >
                          <Heart className={saved ? "fill-primary text-primary" : ""} />
                        </Button>
                      </div>
                      <div className="pt-4">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <p className="font-display text-2xl">{formatAdvertPrice(advert)}</p>
                            <h3 className="mt-1 text-base font-semibold">
                              <Link
                                to="/listings/$listingId"
                                params={{ listingId: advert.guid }}
                                className="hover:text-primary"
                              >
                                {advert.title}
                              </Link>
                            </h3>
                          </div>
                          <span className="shrink-0 text-xs text-muted-foreground">
                            {formatPostedDate(advert.createdDate)}
                          </span>
                        </div>
                        <p className="mt-1.5 flex items-center gap-1.5 text-sm text-muted-foreground">
                          <MapPin className="size-3.5" />
                          {location}
                        </p>
                        <div className="mt-4 flex items-center gap-4 border-y border-border py-3 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1.5">
                            <BedDouble className="size-4" />
                            {advert.rooms}
                          </span>
                          <span className="flex items-center gap-1.5">
                            <Square className="size-4" />
                            {advert.surfaceArea} m²
                          </span>
                          <span className="flex items-center gap-1.5">
                            <Layers className="size-4" />
                            Floor {advert.floor}
                          </span>
                        </div>
                        <div className="mt-3 flex items-center justify-end gap-3">
                          <Button size="sm" variant="outline" onClick={() => openContact(advert)}>
                            Contact
                          </Button>
                        </div>
                      </div>
                    </article>
                  );
                })}
              </div>

              {hasMore && (
                <div className="mt-10 flex justify-center">
                  <Button
                    variant="outline"
                    disabled={loadingMore}
                    onClick={() => setPage((current) => current + 1)}
                  >
                    {loadingMore ? "Loading…" : "Load more"}
                  </Button>
                </div>
              )}
            </>
          )}
        </section>

        <section className="border-y border-border bg-primary text-primary-foreground">
          <div className="mx-auto flex max-w-[1440px] flex-col justify-between gap-8 px-4 py-10 sm:px-7 md:flex-row md:items-center lg:px-10">
            <div>
              <p className="text-sm font-semibold opacity-80">For owners & agencies</p>
              <h2 className="mt-2 font-display text-3xl sm:text-4xl">
                Your listing. Live in minutes.
              </h2>
              <p className="mt-2 max-w-xl text-sm leading-6 opacity-80">
                Publish directly, update it anytime, and mark it sold or rented when the deal is
                done.
              </p>
            </div>
            <Button variant="secondary" size="lg" asChild>
              <Link to="/createadvert">
                <Plus /> Publish a property
              </Link>
            </Button>
          </div>
        </section>
      </main>

      <footer className="mx-auto flex max-w-[1440px] flex-col justify-between gap-5 px-4 py-8 text-sm text-muted-foreground sm:flex-row sm:items-center sm:px-7 lg:px-10">
        <div className="flex items-center gap-2 text-foreground">
          <KeyRound className="size-5 text-primary" />
          <span className="font-display text-lg">OpenKey</span>
        </div>
        <p>Open property listings, published directly.</p>
        <p>© 2026 OpenKey</p>
      </footer>

      <ContactOwnerDialog
        open={dialog === "contact"}
        sellerName="the owner"
        listingTitle={selected?.title}
        onClose={() => setDialog(null)}
        onSignIn={() => void navigate({ to: "/login" })}
      />
    </div>
  );
}
