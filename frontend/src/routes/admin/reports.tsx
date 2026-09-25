import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, useMemo } from "react";
import {
  AlertTriangle,
  ArrowLeft,
  Building2,
  Check,
  CheckCheck,
  Copy,
  ExternalLink,
  Filter,
  Flag,
  Loader2,
  Lock,
  Pencil,
  RefreshCw,
  Search,
  Shield,
  ShieldAlert,
  ShieldCheck,
  ShieldX,
  Trash2,
  X,
} from "lucide-react";
import Header from "@/components/Navigation/header";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useAuth } from "@/hooks/useAuth";
import { getCurrentUser, type CurrentUser } from "@/api/users";
import { getAdvertReports, dismissAdvertReport, type AdvertReport } from "@/api/admin";
import { deleteAdvert } from "@/api/adverts";
import { formatPostedDate } from "@/lib/advert-format";
import { QuickEditAdvertDialog } from "@/components/admin/QuickEditAdvertDialog";
import { toast } from "sonner";
import { Toaster } from "@/components/ui/sonner";

export const Route = createFileRoute("/admin/reports")({
  head: () => ({
    meta: [
      { title: "Listing Reports | OpenKey Admin" },
      { name: "description", content: "Moderate and manage property listing reports" },
    ],
  }),
  component: AdminReportsPage,
});

type ReasonFilter = "All" | "Spam" | "Fraud" | "Duplicate" | "Other";
type SortOrder = "newest" | "oldest" | "title";

const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

function AdminReportsPage() {
  const navigate = useNavigate();
  const { loggedIn, isAdmin: tokenIsAdmin, logout } = useAuth();

  // Authentication & authorization states
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [authChecking, setAuthChecking] = useState(true);

  // Reports data state
  const [reports, setReports] = useState<AdvertReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filters and search
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedReason, setSelectedReason] = useState<ReasonFilter>("All");
  const [sortBy, setSortBy] = useState<SortOrder>("newest");

  // Dialog actions
  const [quickEditAdvertUuid, setQuickEditAdvertUuid] = useState<string | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<{
    reportUuid: string;
    advertUuid: string;
    advertTitle: string;
  } | null>(null);
  const [alsoDismissOnDelete, setAlsoDismissOnDelete] = useState(true);
  const [deletingListing, setDeletingListing] = useState(false);

  const [dismissTarget, setDismissTarget] = useState<{
    reportUuid: string;
    advertTitle: string;
  } | null>(null);
  const [dismissingReport, setDismissingReport] = useState(false);

  // Copied feedback helper
  const [copiedId, setCopiedId] = useState<string | null>(null);

  // 1. Check authentication and admin permissions
  useEffect(() => {
    let cancelled = false;

    // Small delay to allow localStorage/auth hook hydration
    const timer = setTimeout(() => {
      if (!loggedIn) {
        setAuthChecking(false);
        return;
      }

      getCurrentUser()
        .then((userData) => {
          if (!cancelled) {
            setCurrentUser(userData);
          }
        })
        .catch(() => {
          // If profile fetch fails, tokenIsAdmin can still grant access
        })
        .finally(() => {
          if (!cancelled) {
            setAuthChecking(false);
          }
        });
    }, 100);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [loggedIn]);

  const isUserAdmin = Boolean(tokenIsAdmin || currentUser?.role === "Admin");

  // 2. Fetch reports when admin access confirmed
  const loadReports = async (isManualRefresh = false) => {
    if (isManualRefresh) {
      setRefreshing(true);
    } else {
      setLoading(true);
    }
    setError(null);

    try {
      const data = await getAdvertReports(1, 100);
      setReports(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load reports");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    if (!authChecking && loggedIn && isUserAdmin) {
      void loadReports();
    }
  }, [authChecking, loggedIn, isUserAdmin]);

  // Handle copy helper
  const handleCopy = (text: string, id: string) => {
    void navigator.clipboard.writeText(text);
    setCopiedId(id);
    toast.success("Copied to clipboard");
    setTimeout(() => {
      setCopiedId((current) => (current === id ? null : current));
    }, 2000);
  };

  // 3. Delete advert handler
  const handleConfirmDeleteListing = async () => {
    if (!deleteTarget) return;

    setDeletingListing(true);
    try {
      // Soft-delete the advert
      await deleteAdvert(deleteTarget.advertUuid);

      // If requested, also dismiss the report
      if (alsoDismissOnDelete) {
        await dismissAdvertReport(deleteTarget.reportUuid);
        setReports((prev) => prev.filter((r) => r.uuid !== deleteTarget.reportUuid));
        toast.success(`Deleted listing "${deleteTarget.advertTitle}" and dismissed report.`);
      } else {
        // Mark title as deleted in list
        setReports((prev) =>
          prev.map((r) =>
            r.advertUuid === deleteTarget.advertUuid
              ? { ...r, advertTitle: "(deleted listing)" }
              : r,
          ),
        );
        toast.success(`Deleted listing "${deleteTarget.advertTitle}".`);
      }

      setDeleteTarget(null);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to delete listing.");
    } finally {
      setDeletingListing(false);
    }
  };

  // 4. Dismiss report handler
  const handleConfirmDismissReport = async () => {
    if (!dismissTarget) return;

    setDismissingReport(true);
    try {
      await dismissAdvertReport(dismissTarget.reportUuid);
      setReports((prev) => prev.filter((r) => r.uuid !== dismissTarget.reportUuid));
      toast.success("Report dismissed successfully.");
      setDismissTarget(null);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to dismiss report.");
    } finally {
      setDismissingReport(false);
    }
  };

  // 5. Updated title from quick edit
  const handleListingSaved = (updatedTitle: string) => {
    if (!quickEditAdvertUuid) return;
    setReports((prev) =>
      prev.map((r) =>
        r.advertUuid === quickEditAdvertUuid ? { ...r, advertTitle: updatedTitle } : r,
      ),
    );
  };

  // Filtered and sorted reports
  const filteredReports = useMemo(() => {
    return reports
      .filter((report) => {
        // Reason filter
        if (selectedReason !== "All") {
          if (selectedReason === "Other") {
            const known = ["spam", "fraud", "duplicate"];
            if (known.includes(report.reason.toLowerCase())) return false;
          } else if (report.reason.toLowerCase() !== selectedReason.toLowerCase()) {
            return false;
          }
        }

        // Search query
        if (searchQuery.trim()) {
          const q = searchQuery.toLowerCase();
          const titleMatches = report.advertTitle?.toLowerCase().includes(q);
          const reasonMatches = report.reason?.toLowerCase().includes(q);
          const descMatches = report.description?.toLowerCase().includes(q);
          const reporterMatches = report.reporterUuid?.toLowerCase().includes(q);
          const advertMatches = report.advertUuid?.toLowerCase().includes(q);
          if (
            !titleMatches &&
            !reasonMatches &&
            !descMatches &&
            !reporterMatches &&
            !advertMatches
          ) {
            return false;
          }
        }

        return true;
      })
      .sort((a, b) => {
        if (sortBy === "newest") {
          return new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime();
        }
        if (sortBy === "oldest") {
          return new Date(a.createdDate).getTime() - new Date(b.createdDate).getTime();
        }
        if (sortBy === "title") {
          return (a.advertTitle || "").localeCompare(b.advertTitle || "");
        }
        return 0;
      });
  }, [reports, selectedReason, searchQuery, sortBy]);

  // Statistics calculation
  const stats = useMemo(() => {
    const total = reports.length;
    let spam = 0;
    let fraud = 0;
    let duplicate = 0;
    let other = 0;

    reports.forEach((r) => {
      const reasonLower = r.reason.toLowerCase();
      if (reasonLower === "spam") spam++;
      else if (reasonLower === "fraud") fraud++;
      else if (reasonLower === "duplicate") duplicate++;
      else other++;
    });

    return { total, spam, fraud, duplicate, other };
  }, [reports]);

  // Helper for badge colors
  const getReasonBadge = (reason: string) => {
    const reasonLower = reason.toLowerCase();
    if (reasonLower === "fraud") {
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-red-500/30 bg-red-500/10 px-2.5 py-0.5 text-xs font-semibold text-red-700 dark:text-red-400">
          <ShieldAlert className="size-3 text-red-600" />
          <span>Fraud</span>
        </span>
      );
    }
    if (reasonLower === "spam") {
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-amber-500/30 bg-amber-500/10 px-2.5 py-0.5 text-xs font-semibold text-amber-700 dark:text-amber-400">
          <AlertTriangle className="size-3 text-amber-600" />
          <span>Spam</span>
        </span>
      );
    }
    if (reasonLower === "duplicate") {
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-blue-500/30 bg-blue-500/10 px-2.5 py-0.5 text-xs font-semibold text-blue-700 dark:text-blue-400">
          <Copy className="size-3 text-blue-600" />
          <span>Duplicate</span>
        </span>
      );
    }
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full border border-muted-foreground/30 bg-muted px-2.5 py-0.5 text-xs font-semibold text-muted-foreground">
        <Flag className="size-3" />
        <span>{reason}</span>
      </span>
    );
  };

  // Loading state while verifying user role
  if (authChecking) {
    return (
      <div className="min-h-screen bg-background text-foreground">
        <Header />
        <main className="mx-auto max-w-[1200px] px-4 py-16 sm:px-7 lg:px-10">
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <Loader2 className="size-10 animate-spin text-primary" />
            <p className="mt-4 text-sm font-medium text-muted-foreground">
              Verifying admin credentials…
            </p>
          </div>
        </main>
      </div>
    );
  }

  // Not logged in: Redirect prompt / login card
  if (!loggedIn) {
    return (
      <div className="min-h-screen bg-background text-foreground">
        <Header />
        <main className="mx-auto max-w-[540px] px-4 py-20">
          <div className="overflow-hidden rounded-2xl border border-border bg-card p-8 text-center shadow-lg">
            <div className="mx-auto grid size-14 place-items-center rounded-2xl bg-amber-500/10 text-amber-600">
              <Lock className="size-7" />
            </div>
            <h1 className="mt-5 font-display text-2xl font-bold">Authentication Required</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              You must be logged in as an administrator to view and manage listing reports.
            </p>
            <div className="mt-6 flex flex-col gap-3">
              <Button onClick={() => void navigate({ to: "/login" })} className="w-full">
                Sign in
              </Button>
              <Button variant="ghost" onClick={() => void navigate({ to: "/" })} className="w-full">
                Back to marketplace
              </Button>
            </div>
          </div>
        </main>
      </div>
    );
  }

  // Logged in but not admin: Access Denied 403
  if (!isUserAdmin) {
    return (
      <div className="min-h-screen bg-background text-foreground">
        <Header />
        <main className="mx-auto max-w-[620px] px-4 py-20">
          <div className="overflow-hidden rounded-2xl border border-destructive/30 bg-card p-8 text-center shadow-xl">
            <div className="mx-auto grid size-16 place-items-center rounded-2xl bg-destructive/10 text-destructive">
              <ShieldX className="size-9" />
            </div>
            <div className="mt-4 inline-flex items-center gap-1.5 rounded-full bg-destructive/10 px-3 py-1 text-xs font-semibold text-destructive">
              <span>Error 403 • Access Restricted</span>
            </div>
            <h1 className="mt-4 font-display text-3xl font-bold">Admin Privileges Required</h1>
            <p className="mt-3 text-sm text-muted-foreground leading-relaxed">
              Your account does not have administrator permissions to access the reports moderation
              portal. Only designated platform administrators can manage reported adverts.
            </p>
            <div className="mt-8 flex flex-col sm:flex-row items-center justify-center gap-3">
              <Button
                variant="outline"
                onClick={() => void navigate({ to: "/" })}
                className="w-full sm:w-auto"
              >
                <ArrowLeft className="size-4" /> Return to Marketplace
              </Button>
              <Button
                variant="secondary"
                onClick={() => {
                  logout();
                  void navigate({ to: "/login" });
                }}
                className="w-full sm:w-auto"
              >
                Sign in with another account
              </Button>
            </div>
          </div>
        </main>
      </div>
    );
  }

  // Admin view
  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />
      <Toaster />

      <main className="mx-auto max-w-[1280px] px-4 py-8 sm:px-7 lg:px-10">
        {/* Navigation Breadcrumb */}
        <div className="flex items-center justify-between">
          <Link
            to="/"
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="size-4" /> Back to browse
          </Link>

          <div className="flex items-center gap-2">
            <span className="inline-flex items-center gap-1.5 rounded-full border border-amber-500/30 bg-amber-500/10 px-3 py-1 text-xs font-semibold text-amber-700 dark:text-amber-400">
              <ShieldCheck className="size-3.5" />
              <span>Admin Access Active</span>
            </span>
          </div>
        </div>

        {/* Hero Section */}
        <section className="mt-6 overflow-hidden rounded-2xl border border-border bg-gradient-to-br from-[#e8f3f0] to-[#f4faf9] p-6 sm:p-8 dark:from-[#132220] dark:to-[#0f1716] shadow-xs">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6">
            <div className="max-w-2xl">
              <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-[0.14em] text-primary">
                <ShieldAlert className="size-4" />
                <span>Moderation & Quality Control</span>
              </div>
              <h1 className="mt-2 font-display text-3xl sm:text-4xl font-bold tracking-tight text-foreground">
                Listing Reports Management
              </h1>
              <p className="mt-2 text-sm sm:text-base text-muted-foreground">
                Review user-submitted reports, manage flagged listings, edit listing content, or
                remove non-compliant adverts to keep the marketplace safe.
              </p>
            </div>

            <div className="flex shrink-0 items-center gap-2 self-start md:self-center">
              <Button
                variant="outline"
                size="sm"
                onClick={() => void loadReports(true)}
                disabled={loading || refreshing}
                className="gap-2 bg-card hover:bg-muted shadow-xs"
              >
                <RefreshCw className={`size-4 ${refreshing ? "animate-spin" : ""}`} />
                <span>{refreshing ? "Refreshing…" : "Refresh reports"}</span>
              </Button>
            </div>
          </div>
        </section>

        {/* Summary Metric Cards */}
        <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-4">
          <div className="rounded-xl border border-border bg-card p-4 shadow-xs">
            <div className="flex items-center justify-between text-muted-foreground">
              <span className="text-xs font-medium uppercase tracking-wider">Total Reports</span>
              <Flag className="size-4 text-primary" />
            </div>
            <p className="mt-2 font-display text-2xl sm:text-3xl font-bold text-foreground">
              {stats.total}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">Active in queue</p>
          </div>

          <div className="rounded-xl border border-red-500/20 bg-card p-4 shadow-xs">
            <div className="flex items-center justify-between text-muted-foreground">
              <span className="text-xs font-medium uppercase tracking-wider text-red-600 dark:text-red-400">
                Fraud Flags
              </span>
              <ShieldAlert className="size-4 text-red-600 dark:text-red-400" />
            </div>
            <p className="mt-2 font-display text-2xl sm:text-3xl font-bold text-red-600 dark:text-red-400">
              {stats.fraud}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">High priority attention</p>
          </div>

          <div className="rounded-xl border border-amber-500/20 bg-card p-4 shadow-xs">
            <div className="flex items-center justify-between text-muted-foreground">
              <span className="text-xs font-medium uppercase tracking-wider text-amber-600 dark:text-amber-400">
                Spam Reports
              </span>
              <AlertTriangle className="size-4 text-amber-600 dark:text-amber-400" />
            </div>
            <p className="mt-2 font-display text-2xl sm:text-3xl font-bold text-amber-600 dark:text-amber-400">
              {stats.spam}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">Irrelevant or abusive</p>
          </div>

          <div className="rounded-xl border border-blue-500/20 bg-card p-4 shadow-xs">
            <div className="flex items-center justify-between text-muted-foreground">
              <span className="text-xs font-medium uppercase tracking-wider text-blue-600 dark:text-blue-400">
                Duplicates
              </span>
              <Copy className="size-4 text-blue-600 dark:text-blue-400" />
            </div>
            <p className="mt-2 font-display text-2xl sm:text-3xl font-bold text-blue-600 dark:text-blue-400">
              {stats.duplicate}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">Redundant listings</p>
          </div>
        </div>

        {/* Filters and Controls Toolbar */}
        <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-border pb-4">
          <div className="flex flex-1 items-center gap-2 max-w-md">
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Search by title, reason, or reporter ID…"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 pr-8"
              />
              {searchQuery && (
                <button
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  <X className="size-4" />
                </button>
              )}
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            {/* Filter Pills */}
            <div className="flex items-center rounded-lg border border-border bg-card p-1">
              {(["All", "Fraud", "Spam", "Duplicate"] as ReasonFilter[]).map((reason) => {
                const count =
                  reason === "All"
                    ? stats.total
                    : reason === "Fraud"
                      ? stats.fraud
                      : reason === "Spam"
                        ? stats.spam
                        : stats.duplicate;

                const isSelected = selectedReason === reason;
                return (
                  <button
                    key={reason}
                    type="button"
                    onClick={() => setSelectedReason(reason)}
                    className={`inline-flex items-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-medium transition-colors ${
                      isSelected
                        ? "bg-primary text-primary-foreground shadow-xs"
                        : "text-muted-foreground hover:text-foreground"
                    }`}
                  >
                    <span>{reason}</span>
                    <span
                      className={`rounded-full px-1.5 py-0.2 text-[10px] ${
                        isSelected
                          ? "bg-primary-foreground/20 text-primary-foreground"
                          : "bg-muted text-muted-foreground"
                      }`}
                    >
                      {count}
                    </span>
                  </button>
                );
              })}
            </div>

            {/* Sort selector */}
            <Select value={sortBy} onValueChange={(val: SortOrder) => setSortBy(val)}>
              <SelectTrigger className="w-[140px] text-xs">
                <SelectValue placeholder="Sort by" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="newest">Newest first</SelectItem>
                <SelectItem value="oldest">Oldest first</SelectItem>
                <SelectItem value="title">Title (A-Z)</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Reports Content List */}
        <section className="mt-6">
          {loading ? (
            <div className="grid gap-4 py-8">
              {[1, 2, 3].map((i) => (
                <div
                  key={i}
                  className="rounded-xl border border-border bg-card p-6 animate-pulse space-y-4"
                >
                  <div className="flex justify-between items-center">
                    <div className="h-6 w-1/3 bg-muted rounded"></div>
                    <div className="h-5 w-24 bg-muted rounded-full"></div>
                  </div>
                  <div className="h-4 w-2/3 bg-muted rounded"></div>
                  <div className="h-10 w-full bg-muted rounded"></div>
                </div>
              ))}
            </div>
          ) : error ? (
            <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-8 text-center">
              <AlertTriangle className="mx-auto size-8 text-destructive" />
              <h3 className="mt-3 text-lg font-semibold text-destructive">
                Failed to load reports
              </h3>
              <p className="mt-1 text-sm text-destructive/80">{error}</p>
              <Button
                variant="outline"
                size="sm"
                className="mt-4 border-destructive/40"
                onClick={() => void loadReports()}
              >
                Try again
              </Button>
            </div>
          ) : filteredReports.length === 0 ? (
            <div className="rounded-2xl border border-border bg-card p-12 text-center shadow-xs">
              <div className="mx-auto grid size-16 place-items-center rounded-2xl bg-primary/10 text-primary">
                <CheckCheck className="size-8" />
              </div>
              <h2 className="mt-4 font-display text-2xl font-bold">
                {reports.length === 0 ? "All caught up!" : "No matching reports found"}
              </h2>
              <p className="mt-2 text-sm text-muted-foreground max-w-md mx-auto">
                {reports.length === 0
                  ? "There are currently no open reports for property listings. The marketplace is running smoothly."
                  : "Try clearing your search query or switching filters to view other reports."}
              </p>
              {reports.length > 0 && (
                <Button
                  variant="outline"
                  size="sm"
                  className="mt-4"
                  onClick={() => {
                    setSearchQuery("");
                    setSelectedReason("All");
                  }}
                >
                  Clear filters
                </Button>
              )}
            </div>
          ) : (
            <div className="grid gap-4">
              {filteredReports.map((report) => {
                const isAdvertDeleted =
                  !report.advertUuid ||
                  report.advertUuid === EMPTY_GUID ||
                  report.advertTitle === "(deleted listing)";

                return (
                  <article
                    key={report.uuid}
                    className="overflow-hidden rounded-xl border border-border bg-card shadow-xs transition-shadow hover:shadow-md"
                  >
                    <div className="p-5 sm:p-6">
                      {/* Card Header */}
                      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3 border-b border-border pb-4">
                        <div className="space-y-1.5 flex-1 min-w-0">
                          <div className="flex flex-wrap items-center gap-2">
                            {getReasonBadge(report.reason)}
                            <span className="text-xs text-muted-foreground">
                              Reported {formatPostedDate(report.createdDate)}
                            </span>
                          </div>

                          <div className="flex items-center gap-2">
                            {isAdvertDeleted ? (
                              <h3 className="font-display text-xl font-bold text-muted-foreground line-through">
                                {report.advertTitle}
                              </h3>
                            ) : (
                              <Link
                                to="/listings/$listingId"
                                params={{ listingId: report.advertUuid }}
                                target="_blank"
                                rel="noreferrer"
                                className="group inline-flex items-center gap-1.5 font-display text-xl font-bold text-foreground hover:text-primary transition-colors truncate"
                              >
                                <span>{report.advertTitle}</span>
                                <ExternalLink className="size-4 shrink-0 text-muted-foreground opacity-70 group-hover:text-primary group-hover:opacity-100 transition-opacity" />
                              </Link>
                            )}

                            {isAdvertDeleted && (
                              <Badge
                                variant="destructive"
                                className="text-[10px] uppercase font-bold"
                              >
                                Advert Deleted
                              </Badge>
                            )}
                          </div>
                        </div>

                        {/* Top quick dismiss button */}
                        <div className="shrink-0 flex items-center gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-xs gap-1.5 text-muted-foreground hover:text-foreground"
                            onClick={() =>
                              setDismissTarget({
                                reportUuid: report.uuid,
                                advertTitle: report.advertTitle,
                              })
                            }
                          >
                            <Check className="size-3.5" />
                            <span>Dismiss report</span>
                          </Button>
                        </div>
                      </div>

                      {/* Card Body: Reporter explanation & IDs */}
                      <div className="py-4 space-y-3">
                        {report.description ? (
                          <div className="rounded-lg border border-border bg-muted/30 p-3.5 text-sm">
                            <span className="block text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-1">
                              Reporter's comments:
                            </span>
                            <p className="text-foreground leading-relaxed italic">
                              "{report.description}"
                            </p>
                          </div>
                        ) : (
                          <p className="text-xs text-muted-foreground italic">
                            No additional description provided by the reporter.
                          </p>
                        )}

                        {/* Metadata IDs pill row */}
                        <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                          <div className="flex items-center gap-1.5">
                            <span className="font-medium text-foreground">Report ID:</span>
                            <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-[11px]">
                              {report.uuid.slice(0, 8)}…
                            </code>
                            <button
                              type="button"
                              onClick={() => handleCopy(report.uuid, `rep-${report.uuid}`)}
                              className="text-muted-foreground hover:text-foreground"
                              title="Copy Report UUID"
                            >
                              {copiedId === `rep-${report.uuid}` ? (
                                <Check className="size-3 text-green-600" />
                              ) : (
                                <Copy className="size-3" />
                              )}
                            </button>
                          </div>

                          <span className="text-border">•</span>

                          <div className="flex items-center gap-1.5">
                            <span className="font-medium text-foreground">Listing UUID:</span>
                            <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-[11px]">
                              {report.advertUuid.slice(0, 8)}…
                            </code>
                            <button
                              type="button"
                              onClick={() =>
                                handleCopy(report.advertUuid, `adv-${report.advertUuid}`)
                              }
                              className="text-muted-foreground hover:text-foreground"
                              title="Copy Advert UUID"
                            >
                              {copiedId === `adv-${report.advertUuid}` ? (
                                <Check className="size-3 text-green-600" />
                              ) : (
                                <Copy className="size-3" />
                              )}
                            </button>
                          </div>

                          <span className="text-border">•</span>

                          <div className="flex items-center gap-1.5">
                            <span className="font-medium text-foreground">Reporter UUID:</span>
                            <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-[11px]">
                              {report.reporterUuid.slice(0, 8)}…
                            </code>
                            <button
                              type="button"
                              onClick={() =>
                                handleCopy(report.reporterUuid, `user-${report.reporterUuid}`)
                              }
                              className="text-muted-foreground hover:text-foreground"
                              title="Copy Reporter UUID"
                            >
                              {copiedId === `user-${report.reporterUuid}` ? (
                                <Check className="size-3 text-green-600" />
                              ) : (
                                <Copy className="size-3" />
                              )}
                            </button>
                          </div>
                        </div>
                      </div>

                      {/* Card Footer: Action Bar to Manage the Advert and Report */}
                      <div className="flex flex-wrap items-center justify-between gap-3 border-t border-border pt-4 bg-muted/10 -mx-5 -mb-5 px-5 py-3 sm:-mx-6 sm:-mb-6 sm:px-6">
                        <div className="flex flex-wrap items-center gap-2">
                          {/* View Listing Button */}
                          {!isAdvertDeleted && (
                            <Button
                              variant="outline"
                              size="sm"
                              className="gap-1.5 text-xs bg-card"
                              asChild
                            >
                              <a
                                href={`/listings/${report.advertUuid}`}
                                target="_blank"
                                rel="noreferrer"
                              >
                                <ExternalLink className="size-3.5" />
                                <span>View listing</span>
                              </a>
                            </Button>
                          )}

                          {/* Quick Edit Advert Button */}
                          {!isAdvertDeleted && (
                            <Button
                              variant="outline"
                              size="sm"
                              className="gap-1.5 text-xs bg-card"
                              onClick={() => setQuickEditAdvertUuid(report.advertUuid)}
                            >
                              <Pencil className="size-3.5 text-primary" />
                              <span>Quick edit</span>
                            </Button>
                          )}

                          {/* Full Edit Advert Link */}
                          {!isAdvertDeleted && (
                            <Button
                              variant="ghost"
                              size="sm"
                              className="gap-1 text-xs text-muted-foreground hover:text-foreground"
                              onClick={() =>
                                void navigate({ to: `/editadvert/${report.advertUuid}` })
                              }
                            >
                              <span>Full editor</span>
                            </Button>
                          )}
                        </div>

                        <div className="flex items-center gap-2">
                          {/* Delete Advert Button */}
                          {!isAdvertDeleted && (
                            <Button
                              variant="destructive"
                              size="sm"
                              className="gap-1.5 text-xs"
                              onClick={() =>
                                setDeleteTarget({
                                  reportUuid: report.uuid,
                                  advertUuid: report.advertUuid,
                                  advertTitle: report.advertTitle,
                                })
                              }
                            >
                              <Trash2 className="size-3.5" />
                              <span>Delete advert</span>
                            </Button>
                          )}

                          {/* Dismiss Report (if advert is already deleted) */}
                          {isAdvertDeleted && (
                            <Button
                              variant="secondary"
                              size="sm"
                              className="gap-1.5 text-xs"
                              onClick={() =>
                                setDismissTarget({
                                  reportUuid: report.uuid,
                                  advertTitle: report.advertTitle,
                                })
                              }
                            >
                              <Check className="size-3.5" />
                              <span>Remove report</span>
                            </Button>
                          )}
                        </div>
                      </div>
                    </div>
                  </article>
                );
              })}
            </div>
          )}
        </section>
      </main>

      {/* Quick Edit Dialog */}
      <QuickEditAdvertDialog
        advertUuid={quickEditAdvertUuid}
        isOpen={Boolean(quickEditAdvertUuid)}
        onClose={() => setQuickEditAdvertUuid(null)}
        onSaved={handleListingSaved}
      />

      {/* Delete Advert Confirmation Dialog */}
      <Dialog open={Boolean(deleteTarget)} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <div className="flex items-center gap-3 text-destructive">
              <div className="grid size-10 place-items-center rounded-full bg-destructive/10 text-destructive">
                <Trash2 className="size-5" />
              </div>
              <DialogTitle className="font-display text-xl">Delete Property Listing?</DialogTitle>
            </div>
            <DialogDescription className="pt-2 text-sm leading-relaxed">
              Are you sure you want to delete the advert{" "}
              <strong className="text-foreground font-semibold">
                "{deleteTarget?.advertTitle}"
              </strong>
              ? This will remove the listing from active search results and mark it as deleted.
            </DialogDescription>
          </DialogHeader>

          <div className="rounded-lg border border-border bg-muted/40 p-3 my-2">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="dismiss-on-delete"
                checked={alsoDismissOnDelete}
                onCheckedChange={(checked) => setAlsoDismissOnDelete(Boolean(checked))}
              />
              <label
                htmlFor="dismiss-on-delete"
                className="text-xs font-medium text-foreground cursor-pointer select-none"
              >
                Also dismiss this report automatically after deletion
              </label>
            </div>
          </div>

          <DialogFooter className="gap-2 sm:gap-0 pt-2">
            <Button
              variant="outline"
              onClick={() => setDeleteTarget(null)}
              disabled={deletingListing}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deletingListing}
              onClick={() => void handleConfirmDeleteListing()}
              className="gap-1.5"
            >
              {deletingListing ? (
                <>
                  <Loader2 className="size-4 animate-spin" />
                  <span>Deleting…</span>
                </>
              ) : (
                <>
                  <Trash2 className="size-4" />
                  <span>Delete listing</span>
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Dismiss Report Confirmation Dialog */}
      <Dialog
        open={Boolean(dismissTarget)}
        onOpenChange={(open) => !open && setDismissTarget(null)}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <div className="flex items-center gap-3 text-primary">
              <div className="grid size-10 place-items-center rounded-full bg-primary/10 text-primary">
                <Check className="size-5" />
              </div>
              <DialogTitle className="font-display text-xl">Dismiss Report</DialogTitle>
            </div>
            <DialogDescription className="pt-2 text-sm leading-relaxed">
              Dismiss report for{" "}
              <strong className="text-foreground font-semibold">
                "{dismissTarget?.advertTitle}"
              </strong>
              ? This marks the report as reviewed and resolves it without modifying the listing.
            </DialogDescription>
          </DialogHeader>

          <DialogFooter className="gap-2 sm:gap-0 pt-3">
            <Button
              variant="outline"
              onClick={() => setDismissTarget(null)}
              disabled={dismissingReport}
            >
              Cancel
            </Button>
            <Button
              disabled={dismissingReport}
              onClick={() => void handleConfirmDismissReport()}
              className="gap-1.5"
            >
              {dismissingReport ? (
                <>
                  <Loader2 className="size-4 animate-spin" />
                  <span>Dismissing…</span>
                </>
              ) : (
                <>
                  <CheckCheck className="size-4" />
                  <span>Confirm Dismiss</span>
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
