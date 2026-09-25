import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import {
  ArrowLeft,
  ArrowRight,
  CalendarDays,
  CheckCircle2,
  Heart,
  Mail,
  Phone,
  ShieldCheck,
  Store,
  UserRound,
} from "lucide-react";
import Header from "@/components/Navigation/header";
import { Button } from "@/components/ui/button";
import { getCurrentUser, type CurrentUser } from "@/api/users";
import { getAdverts, getFavoriteAdverts } from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";

export const Route = createFileRoute("/account")({
  head: () => ({ meta: [{ title: "My account | OpenKey" }] }),
  component: AccountPage,
});

function AccountPage() {
  const navigate = useNavigate();
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [listingCount, setListingCount] = useState(0);
  const [savedCount, setSavedCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    let cancelled = false;
    Promise.all([
      getCurrentUser(),
      getAdverts({ mine: true, pageSize: 1 }),
      getFavoriteAdverts(1, 1),
    ])
      .then(([currentUser, listings, favorites]) => {
        if (cancelled) return;
        setUser(currentUser);
        setListingCount(listings.totalCount);
        setSavedCount(favorites.totalCount);
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

  const fullName = user ? [user.firstName, user.lastName].filter(Boolean).join(" ") : "";
  const initials = fullName
    ? fullName
        .split(" ")
        .map((part) => part[0])
        .join("")
        .slice(0, 2)
        .toUpperCase()
    : "OK";

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />
      <main className="mx-auto max-w-[1200px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14">
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4" /> Back to browse
        </Link>

        {loading ? (
          <div className="mt-12 text-sm text-muted-foreground">Loading your account...</div>
        ) : error || !user ? (
          <div className="mt-12 rounded-lg border border-border bg-card p-8 text-center">
            <h1 className="font-display text-3xl">Account unavailable</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              We could not load your account details.
            </p>
          </div>
        ) : (
          <section className="mt-8 overflow-hidden rounded-lg border border-border bg-card">
            <div className="bg-accent px-6 py-8 sm:px-10 sm:py-10">
              <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex items-center gap-4">
                  <div className="grid size-20 shrink-0 place-items-center rounded-full bg-primary text-2xl font-bold text-primary-foreground">
                    {initials}
                  </div>
                  <div>
                    <p className="text-xs font-bold uppercase tracking-[0.14em] text-primary">
                      OpenKey member
                    </p>
                    <h1 className="mt-1 font-display text-4xl">{fullName || "Your account"}</h1>
                    <p className="mt-1 text-sm text-muted-foreground">Buyer and seller profile</p>
                  </div>
                </div>
                <div className="flex flex-wrap items-center gap-3"></div>
              </div>
            </div>

            <div className="grid gap-8 px-6 py-8 sm:px-10 lg:grid-cols-[1.1fr_0.9fr]">
              <div>
                <h2 className="font-display text-2xl">Contact information</h2>
                <div className="mt-5 grid gap-4 sm:grid-cols-2">
                  <InfoItem icon={Mail} label="Email" value={user.email} />
                  <InfoItem icon={Phone} label="Phone" value={user.phoneNumber || "Not provided"} />
                  <InfoItem
                    icon={CalendarDays}
                    label="Date of birth"
                    value={formatDate(user.dateOfBirth)}
                  />
                  <InfoItem icon={UserRound} label="Account ID" value={user.uuid} compact />
                </div>
              </div>

              <div className="border-t border-border pt-8 lg:border-l lg:border-t-0 lg:pl-8 lg:pt-0">
                <h2 className="font-display text-2xl">Your activity</h2>
                <div className="mt-5 grid grid-cols-2 gap-3">
                  <ActivityStat
                    icon={Store}
                    value={listingCount}
                    label="Published listings"
                    to="/mylistings"
                  />
                  <ActivityStat
                    icon={Heart}
                    value={savedCount}
                    label="Saved listings"
                    to="/savedhomes"
                  />
                </div>
                <div className="mt-5 flex flex-col sm:flex-row gap-3">
                  <Link
                    to="/mylistings"
                    search={{ inquiries: undefined }}
                    className="inline-flex h-11 flex-1 items-center justify-center gap-2 rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
                  >
                    <Store className="size-4" />
                    View my listings
                  </Link>
                  <Link
                    to="/savedhomes"
                    className="inline-flex h-11 flex-1 items-center justify-center gap-2 rounded-md border border-input bg-background px-4 text-sm font-medium text-foreground shadow-sm transition-colors hover:bg-muted hover:text-foreground"
                  >
                    <Heart className="size-4 text-primary" />
                    View saved listings
                  </Link>
                </div>
                <div className="mt-5 flex items-start gap-3 rounded-md bg-muted p-4 text-sm">
                  <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-primary" />
                  <p className="text-muted-foreground">
                    Use this account to publish homes as a seller and save or contact listings as a
                    buyer.
                  </p>
                </div>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}

function InfoItem({
  icon: Icon,
  label,
  value,
  compact = false,
}: {
  icon: typeof Mail;
  label: string;
  value: string;
  compact?: boolean;
}) {
  return (
    <div className="flex min-w-0 gap-3 rounded-md border border-border p-4">
      <Icon className="mt-0.5 size-4 shrink-0 text-primary" />
      <div className="min-w-0">
        <p className="text-xs text-muted-foreground">{label}</p>
        <p
          className={
            compact ? "mt-1 break-all text-xs font-medium" : "mt-1 break-words text-sm font-medium"
          }
        >
          {value}
        </p>
      </div>
    </div>
  );
}

function ActivityStat({
  icon: Icon,
  value,
  label,
  to,
}: {
  icon: typeof Store;
  value: number;
  label: string;
  to?: string;
}) {
  const content = (
    <div
      className={`group rounded-md border border-border p-4 transition-all duration-200 ${
        to ? "cursor-pointer hover:border-primary/50 hover:bg-muted/40 hover:shadow-sm" : ""
      }`}
    >
      <div className="flex items-center justify-between">
        <Icon className="size-5 text-primary" />
        {to && (
          <ArrowRight className="size-4 text-muted-foreground transition-transform group-hover:translate-x-0.5 group-hover:text-primary" />
        )}
      </div>
      <p className="mt-3 text-3xl font-bold">{value}</p>
      <p className="mt-1 text-xs text-muted-foreground">{label}</p>
    </div>
  );

  if (to) {
    return (
      <Link to={to as never} className="block">
        {content}
      </Link>
    );
  }

  return content;
}

function formatDate(value: string | null) {
  if (!value) return "Not provided";
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(value));
}
