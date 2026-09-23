import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import {
  BedDouble,
  Heart,
  KeyRound,
  Layers,
  MapPin,
  MessageCircle,
  Plus,
  Search,
  SlidersHorizontal,
  Square,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { ContactOwnerDialog } from "@/components/dialogs/ContactOwnerDialog";
import { getCurrentUserUuid, isLoggedIn } from "@/lib/tokens";
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
type LocationScope = "all" | "chisinau" | (typeof MOLDOVA_DISTRICTS)[number];

const MOLDOVA_DISTRICTS = [
  "Anenii Noi",
  "Basarabeasca",
  "Briceni",
  "Cahul",
  "Cantemir",
  "Călărași",
  "Căușeni",
  "Cimișlia",
  "Criuleni",
  "Dondușeni",
  "Drochia",
  "Dubăsari",
  "Edineț",
  "Fălești",
  "Florești",
  "Glodeni",
  "Hîncești",
  "Ialoveni",
  "Leova",
  "Nisporeni",
  "Ocnița",
  "Orhei",
  "Rezina",
  "Rîșcani",
  "Sîngerei",
  "Soroca",
  "Strășeni",
  "Șoldănești",
  "Ștefan Vodă",
  "Taraclia",
  "Telenești",
  "Ungheni",
];

const CHISINAU_REGIONS = ["Botanica", "Buiucani", "Centru", "Ciocana", "Râșcani", "Telecentru"];

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
  const [propertyType, setPropertyType] = useState("Any");
  const [roomCount, setRoomCount] = useState("Any");
  const [priceRange, setPriceRange] = useState("");
  const [surfaceRange, setSurfaceRange] = useState("");
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [locationScope, setLocationScope] = useState<LocationScope>("all");
  const [chisinauDistrict, setChisinauDistrict] = useState("All Chișinău");
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

  const locationSearchTerm =
    locationScope === "all"
      ? ""
      : locationScope === "chisinau"
        ? chisinauDistrict === "All Chișinău"
          ? "Chișinău"
          : chisinauDistrict
        : locationScope;

  const effectiveSearchTerm = [debouncedQuery, locationSearchTerm].filter(Boolean).join(" ");

  const [minPrice, maxPrice] =
    priceRange === "under-50000"
      ? [undefined, 50000]
      : priceRange === "50000-100000"
        ? [50000, 100000]
        : priceRange === "100000-200000"
          ? [100000, 200000]
          : priceRange === "over-200000"
            ? [200000, undefined]
            : [undefined, undefined];

  const [minSurfaceArea, maxSurfaceArea] =
    surfaceRange === "under-50"
      ? [undefined, 50]
      : surfaceRange === "50-100"
        ? [50, 100]
        : surfaceRange === "100-200"
          ? [100, 200]
          : surfaceRange === "over-200"
            ? [200, undefined]
            : [undefined, undefined];

  useEffect(() => {
    setPage(1);
  }, [mode, effectiveSearchTerm, sort, priceRange, surfaceRange, roomCount, propertyType]);

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
      type: mode === "all" ? undefined : mode === "sale" ? "Sale" : "Rent",
      sortBy,
      sortDescending,
      searchTerm: effectiveSearchTerm || undefined,
      minPrice,
      maxPrice,
      minSurfaceArea,
      maxSurfaceArea,
      rooms: roomCount === "Any" ? undefined : Number(roomCount),
      buildingType: propertyType === "Any" ? undefined : (propertyType as "Apartment" | "House"),
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
  }, [
    mode,
    effectiveSearchTerm,
    sort,
    page,
    minPrice,
    maxPrice,
    minSurfaceArea,
    maxSurfaceArea,
    roomCount,
    propertyType,
  ]);

  useEffect(() => {
    if (isLoggedIn()) {
      void getFavoriteAdvertIds().then(setSavedIds);
    }
  }, []);

  const currentUserUuid = getCurrentUserUuid();

  const openContact = (advert: Advert) => {
    const isOwner = Boolean(
      currentUserUuid &&
      advert.userUuid &&
      currentUserUuid.toLowerCase() === advert.userUuid.toLowerCase(),
    );
    if (isOwner) return;
    setSelected(advert);
    setDialog("contact");
  };

  const toggleSaved = (guid: string) => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }
    const advert = adverts.find((a) => a.guid === guid);
    const isOwner = Boolean(
      currentUserUuid &&
      advert?.userUuid &&
      currentUserUuid.toLowerCase() === advert.userUuid.toLowerCase(),
    );
    if (isOwner) return;

    const nextSaved = !savedIds.includes(guid);
    setSavedIds((ids) => (nextSaved ? [...ids, guid] : ids.filter((id) => id !== guid)));
    void (nextSaved ? favoriteAdvert(guid) : unfavoriteAdvert(guid));
  };

  const hasMore = adverts.length < totalCount;

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />

      <main id="top">
        <section className="border-b border-border bg-[#f4f8f9]">
          <div className="mx-auto flex max-w-[1440px] flex-col items-center px-4 py-14 sm:px-7 sm:py-20 lg:px-10">
            <div className="w-full max-w-3xl text-center">
              <h1 className="font-sans text-4xl font-bold leading-[1.04] tracking-tight sm:text-5xl lg:text-[4.25rem]">
                Find a place you'll love to call home.
              </h1>
              <p className="mx-auto mt-4 max-w-xl text-sm leading-5 text-muted-foreground sm:text-base">
                Browse homes, apartments, land, and commercial properties for sale or rent, directly
                from owners and agencies.
              </p>
            </div>

            <div className="mt-9 w-full max-w-6xl rounded-lg border border-border bg-card p-3 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-4">
              <div className="grid gap-3 md:grid-flow-col md:grid-cols-[1.3fr_0.75fr_0.7fr_auto_auto] md:items-end">
                <div
                  className={
                    locationScope === "chisinau"
                      ? "grid min-w-0 grid-cols-[minmax(0,1fr)_7rem] items-end gap-2"
                      : "min-w-0"
                  }
                >
                  <label className="block min-w-0">
                    <span className="mb-1.5 block text-xs font-semibold text-foreground">
                      Location
                    </span>
                    <span className="flex h-11 min-w-0 items-center gap-2 rounded-md border border-input bg-background px-3">
                      <MapPin className="size-4 text-muted-foreground" />
                      <select
                        value={locationScope}
                        onChange={(event) => setLocationScope(event.target.value as LocationScope)}
                        className="min-w-0 flex-1 bg-transparent text-sm outline-none"
                      >
                        <option value="all">All of Moldova</option>
                        <option value="chisinau">Chișinău</option>
                        {MOLDOVA_DISTRICTS.map((district) => (
                          <option key={district} value={district}>
                            {district}
                          </option>
                        ))}
                      </select>
                    </span>
                  </label>

                  {locationScope === "chisinau" && (
                    <label className="block min-w-0">
                      <span className="mb-1.5 block text-xs font-semibold text-foreground">
                        Region
                      </span>
                      <select
                        value={chisinauDistrict}
                        onChange={(event) => setChisinauDistrict(event.target.value)}
                        className="h-11 w-full rounded-md border border-input bg-background px-2 text-sm outline-none focus:ring-1 focus:ring-ring"
                      >
                        {CHISINAU_REGIONS.map((region) => (
                          <option key={region} value={region}>
                            {region}
                          </option>
                        ))}
                      </select>
                    </label>
                  )}
                </div>
                <label className="block">
                  <span className="mb-1.5 block text-xs font-semibold text-foreground">
                    Property type
                  </span>
                  <select
                    value={propertyType}
                    onChange={(event) => setPropertyType(event.target.value)}
                    className="h-11 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus:ring-1 focus:ring-ring"
                  >
                    <option value="Any">Any type</option>
                    <option>Apartment</option>
                    <option>House</option>
                  </select>
                </label>
                <label className="block">
                  <span className="mb-1.5 block text-xs font-semibold text-foreground">
                    Looking for
                  </span>
                  <select
                    value={mode}
                    onChange={(event) => setMode(event.target.value as "all" | "sale" | "rent")}
                    className="h-11 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus:ring-1 focus:ring-ring"
                  >
                    <option value="all">All</option>
                    <option value="rent">Rent</option>
                    <option value="sale">Buy</option>
                  </select>
                </label>
                <Popover open={filtersOpen} onOpenChange={setFiltersOpen}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="h-11 gap-1 rounded-full px-2"
                      aria-label="Open filters"
                    >
                      <SlidersHorizontal className="size-4" />
                      <span>Filters</span>
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent align="end" className="w-[min(22rem,calc(100vw-2rem))] p-5">
                    <div className="space-y-4">
                      <div>
                        <h3 className="font-display text-xl">Refine your search</h3>
                        <p className="mt-1 text-xs text-muted-foreground">
                          Choose the details that matter most.
                        </p>
                      </div>
                      <label className="block">
                        <span className="mb-1.5 block text-xs font-semibold">Price</span>
                        <select
                          value={priceRange}
                          onChange={(event) => setPriceRange(event.target.value)}
                          className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus:ring-1 focus:ring-ring"
                        >
                          <option value="">Any price</option>
                          <option value="under-50000">Under 50,000</option>
                          <option value="50000-100000">50,000 - 100,000</option>
                          <option value="100000-200000">100,000 - 200,000</option>
                          <option value="over-200000">Over 200,000</option>
                        </select>
                      </label>
                      <div className="grid grid-cols-2 gap-3">
                        <label className="block">
                          <span className="mb-1.5 block text-xs font-semibold">Rooms</span>
                          <select
                            value={roomCount}
                            onChange={(event) => setRoomCount(event.target.value)}
                            className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus:ring-1 focus:ring-ring"
                          >
                            <option value="Any">Any</option>
                            <option value="1">1 room</option>
                            <option value="2">2 rooms</option>
                            <option value="3">3 rooms</option>
                            <option value="4">4 rooms</option>
                            <option value="5">5+ rooms</option>
                          </select>
                        </label>
                        <label className="block">
                          <span className="mb-1.5 block text-xs font-semibold">Surface area</span>
                          <select
                            value={surfaceRange}
                            onChange={(event) => setSurfaceRange(event.target.value)}
                            className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus:ring-1 focus:ring-ring"
                          >
                            <option value="">Any size</option>
                            <option value="under-50">Under 50 m²</option>
                            <option value="50-100">50 - 100 m²</option>
                            <option value="100-200">100 - 200 m²</option>
                            <option value="over-200">Over 200 m²</option>
                          </select>
                        </label>
                      </div>
                      <Button
                        variant="outline"
                        className="w-full"
                        onClick={() => {
                          setPropertyType("Any");
                          setPriceRange("");
                          setRoomCount("Any");
                          setSurfaceRange("");
                        }}
                      >
                        Clear filters
                      </Button>
                    </div>
                  </PopoverContent>
                </Popover>
                <Button
                  size="icon"
                  className="h-11 w-11"
                  aria-label="Search properties"
                  title="Search properties"
                  onClick={() =>
                    document.getElementById("listings")?.scrollIntoView({ behavior: "smooth" })
                  }
                >
                  <Search />
                </Button>
              </div>
            </div>
            <p className="mt-4 text-xs text-muted-foreground"></p>
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
                  const isOwner = Boolean(
                    currentUserUuid &&
                    advert.userUuid &&
                    currentUserUuid.toLowerCase() === advert.userUuid.toLowerCase(),
                  );
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
                          {advert.status === "Active" ? (
                            <span className="rounded-sm bg-background/95 px-2.5 py-1 text-xs font-bold uppercase">
                              {advert.type === "Sale" ? "For sale" : "For rent"}
                            </span>
                          ) : (
                            <span className="rounded-sm bg-status px-2.5 py-1 text-xs font-bold uppercase text-status-foreground">
                              {advert.status}
                            </span>
                          )}
                        </div>
                        {isOwner ? (
                          <span className="absolute right-3 top-3 rounded-sm bg-primary px-2.5 py-1 text-xs font-semibold text-primary-foreground shadow-sm">
                            Your listing
                          </span>
                        ) : (
                          <Button
                            size="icon"
                            variant="secondary"
                            className="absolute right-3 top-3 rounded-full"
                            onClick={() => toggleSaved(advert.guid)}
                            aria-label={saved ? "Remove from saved" : "Save listing"}
                          >
                            <Heart className={saved ? "fill-primary text-primary" : ""} />
                          </Button>
                        )}
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
                            Floors {advert.floor}
                          </span>
                        </div>
                        {!isOwner && (
                          <div className="mt-3 flex items-center justify-end gap-3">
                            <Button size="sm" variant="outline" onClick={() => openContact(advert)}>
                              Contact
                            </Button>
                          </div>
                        )}
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
        advertUuid={selected?.guid ?? ""}
        sellerName="the owner"
        listingTitle={selected?.title}
        onClose={() => setDialog(null)}
        onSignIn={() => void navigate({ to: "/login" })}
      />
    </div>
  );
}
