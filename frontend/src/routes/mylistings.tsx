import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ArrowLeft, BedDouble, KeyRound, Layers, MessageSquare, Plus, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  getAdverts,
  getAdvertPhotoUrl,
  updateAdvertStatus,
  deleteAdvert,
  type Advert,
} from "@/api/adverts";
import { formatAdvertPrice, formatPostedDate } from "@/lib/advert-format";
import { isLoggedIn } from "@/lib/tokens";
import { AdvertContactRequestsDialog } from "@/components/dialogs/AdvertContactRequestsDialog";
import placeholderImage from "@/assets/openkey-apartment.jpg";

export const Route = createFileRoute("/mylistings")({
  validateSearch: (search: Record<string, unknown>) => ({
    inquiries: typeof search.inquiries === "string" ? search.inquiries : undefined,
  }),
  head: () => ({
    meta: [{ title: "My listings | OpenKey" }],
  }),
  component: MyListingsPage,
});

const statusLabel: Record<Advert["status"], string> = {
  Active: "Active",
  Sold: "Sold",
  Rented: "Rented",
};

function MyListingsPage() {
  const navigate = useNavigate();
  const search = Route.useSearch();
  const [adverts, setAdverts] = useState<Advert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [pendingId, setPendingId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [activeInquiryAdvert, setActiveInquiryAdvert] = useState<{
    guid: string;
    title: string;
  } | null>(null);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    let cancelled = false;
    getAdverts({ mine: true, pageSize: 100, sortBy: "date", sortDescending: true })
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

  useEffect(() => {
    if (search.inquiries && adverts.length > 0) {
      const match = adverts.find(
        (a) => a.guid.toLowerCase() === (search.inquiries as string).toLowerCase(),
      );
      if (match) {
        setActiveInquiryAdvert({ guid: match.guid, title: match.title });
      } else {
        setActiveInquiryAdvert({ guid: search.inquiries, title: "Your listing" });
      }
    }
  }, [search.inquiries, adverts]);

  const handleMarkStatus = async (advert: Advert, status: "Active" | "Sold" | "Rented") => {
    setPendingId(advert.guid);
    setActionError(null);
    try {
      await updateAdvertStatus(advert.guid, status);
      setAdverts((current) =>
        current.map((item) => (item.guid === advert.guid ? { ...item, status } : item)),
      );
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Failed to update status");
    } finally {
      setPendingId(null);
    }
  };

  const handleDelete = async (advert: Advert) => {
    if (!window.confirm(`Delete "${advert.title}"? This can't be undone.`)) {
      return;
    }
    setPendingId(advert.guid);
    setActionError(null);
    try {
      await deleteAdvert(advert.guid);
      setAdverts((current) => current.filter((item) => item.guid !== advert.guid));
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Failed to delete listing");
      setPendingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-background">
        <div className="mx-auto flex h-18 max-w-[1440px] items-center justify-between px-4 sm:px-7 lg:px-10">
          <Link to="/" className="flex items-center gap-2" aria-label="OpenKey home">
            <span className="grid size-9 place-items-center rounded-md text-black">
              <KeyRound className="size-5" />
            </span>
            <span className="font-display text-2xl">OpenKey</span>
          </Link>
          <Link
            to="/"
            className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to homes
          </Link>
        </div>
      </header>

      <main className="mx-auto max-w-[1440px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14">
        <div className="mb-7 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <p className="text-xs font-bold uppercase text-primary">Your properties</p>
            <h1 className="mt-1 font-display text-3xl sm:text-4xl">My listings</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              {loading ? "Loading your listings…" : `${adverts.length} listings published`}
            </p>
          </div>
          <Button asChild>
            <Link to="/createadvert">
              <Plus /> Publish a property
            </Link>
          </Button>
        </div>

        {actionError && (
          <p className="mb-5 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {actionError}
          </p>
        )}

        {error ? (
          <div className="border-y border-border py-20 text-center">
            <h2 className="font-display text-2xl">Couldn't load your listings</h2>
            <p className="mt-2 text-sm text-muted-foreground">Please try again in a moment.</p>
          </div>
        ) : !loading && adverts.length === 0 ? (
          <div className="border-y border-border py-20 text-center">
            <h2 className="font-display text-2xl">You haven't published anything yet</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Your listings will show up here once you publish one.
            </p>
            <Button asChild className="mt-5">
              <Link to="/createadvert">
                <Plus /> Publish a property
              </Link>
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
                  <Link
                    to="/listings/$listingId"
                    params={{ listingId: advert.guid }}
                    aria-label={`View details for ${advert.title}`}
                    className="relative block aspect-[4/3] overflow-hidden rounded-md bg-muted"
                  >
                    <img
                      src={
                        primaryPhoto ? getAdvertPhotoUrl(primaryPhoto.photoUrl) : placeholderImage
                      }
                      alt={advert.title}
                      loading="lazy"
                      className="listing-image size-full object-cover"
                    />
                    <div className="absolute left-3 top-3 flex gap-2">
                      {advert.status === "Active" ? (
                        <span className="rounded-sm bg-background/95 px-2.5 py-1 text-xs font-bold uppercase">
                          {advert.type === "Sale" ? "For sale" : "For rent"}
                        </span>
                      ) : (
                        <span className="rounded-sm bg-status px-2.5 py-1 text-xs font-bold uppercase text-status-foreground">
                          {statusLabel[advert.status]}
                        </span>
                      )}
                    </div>
                  </Link>
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
                    <p className="mt-1.5 text-sm text-muted-foreground">{location}</p>
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
                    <div className="mt-3 flex flex-wrap items-center justify-end gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        className="gap-1.5"
                        onClick={() =>
                          setActiveInquiryAdvert({ guid: advert.guid, title: advert.title })
                        }
                      >
                        <MessageSquare className="size-3.5 text-primary" />
                        Inquiries
                      </Button>
                      <Button size="sm" variant="outline" asChild>
                        <Link to="/editadvert/$listingId" params={{ listingId: advert.guid }}>
                          Edit
                        </Link>
                      </Button>
                      {advert.status === "Active" ? (
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={pendingId === advert.guid}
                          onClick={() =>
                            void handleMarkStatus(
                              advert,
                              advert.type === "Sale" ? "Sold" : "Rented",
                            )
                          }
                        >
                          Mark as {advert.type === "Sale" ? "sold" : "rented"}
                        </Button>
                      ) : (
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={pendingId === advert.guid}
                          onClick={() => void handleMarkStatus(advert, "Active")}
                        >
                          Reactivate
                        </Button>
                      )}
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-destructive hover:text-destructive"
                        disabled={pendingId === advert.guid}
                        onClick={() => void handleDelete(advert)}
                      >
                        Delete
                      </Button>
                    </div>
                  </div>
                </article>
              );
            })}
          </div>
        )}

        {activeInquiryAdvert && (
          <AdvertContactRequestsDialog
            open={!!activeInquiryAdvert}
            advertUuid={activeInquiryAdvert.guid}
            advertTitle={activeInquiryAdvert.title}
            onClose={() => setActiveInquiryAdvert(null)}
          />
        )}
      </main>
    </div>
  );
}
