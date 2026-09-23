import { useEffect, useState } from "react";
import {
  Bell,
  BellOff,
  Check,
  CheckCheck,
  Heart,
  KeyRound,
  MessageSquare,
  Plus,
  Settings,
  Store,
  Trash2,
  UserRound,
} from "lucide-react";
import { Button } from "../ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import { Link, useLocation, useNavigate } from "@tanstack/react-router";
import { useAuth } from "@/hooks/useAuth";
import {
  getReceivedContactRequests,
  markContactRequestAsRead,
  type ContactRequest,
} from "@/api/adverts";
import { formatPostedDate } from "@/lib/advert-format";

export default function Header() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { loggedIn } = useAuth();
  const isAccountPage = pathname === "/account";
  const [inquiries, setInquiries] = useState<ContactRequest[]>([]);
  const [popoverOpen, setPopoverOpen] = useState(false);
  const [loadingInquiries, setLoadingInquiries] = useState(false);
  const [showAllNotifications, setShowAllNotifications] = useState(false);
  const [updatingNotifications, setUpdatingNotifications] = useState(false);

  useEffect(() => {
    if (!loggedIn) {
      setInquiries([]);
      return;
    }

    let cancelled = false;
    setLoadingInquiries(true);

    getReceivedContactRequests()
      .then((data) => {
        if (!cancelled) setInquiries(data);
      })
      .catch((err) => {
        console.error("Failed to load notifications", err);
      })
      .finally(() => {
        if (!cancelled) setLoadingInquiries(false);
      });

    return () => {
      cancelled = true;
    };
  }, [loggedIn]);

  const unreadCount = inquiries.filter((r) => r.status === "Unread").length;
  const visibleInquiries = showAllNotifications
    ? inquiries
    : inquiries.filter((item) => item.status === "Unread");

  const handleInquiryClick = (item: ContactRequest) => {
    setPopoverOpen(false);
    if (item.status === "Unread") {
      void markContactRequestAsRead(item.uuid);
      setInquiries((prev) =>
        prev.map((i) => (i.uuid === item.uuid ? { ...i, status: "Read" } : i)),
      );
    }
    if (item.advertUuid) {
      void navigate({
        to: "/mylistings",
        search: { inquiries: item.advertUuid },
      });
    } else {
      void navigate({ to: "/mylistings", search: { inquiries: undefined } });
    }
  };

  const handleMarkAsRead = async (item: ContactRequest) => {
    if (item.status === "Read") return;

    try {
      await markContactRequestAsRead(item.uuid);
      setInquiries((prev) =>
        prev.map((inquiry) =>
          inquiry.uuid === item.uuid ? { ...inquiry, status: "Read" } : inquiry,
        ),
      );
    } catch (err) {
      console.error("Failed to mark notification as read", err);
    }
  };

  const handleMarkAllAsRead = async () => {
    const unreadInquiries = inquiries.filter((item) => item.status === "Unread");
    if (unreadInquiries.length === 0) return;

    setUpdatingNotifications(true);
    const results = await Promise.allSettled(
      unreadInquiries.map((item) => markContactRequestAsRead(item.uuid)),
    );
    const readUuids = new Set(
      unreadInquiries
        .filter((_, index) => results[index].status === "fulfilled")
        .map((item) => item.uuid),
    );
    setInquiries((prev) =>
      prev.map((item) => (readUuids.has(item.uuid) ? { ...item, status: "Read" } : item)),
    );
    results.forEach((result) => {
      if (result.status === "rejected") {
        console.error("Failed to mark notification as read", result.reason);
      }
    });
    setUpdatingNotifications(false);
  };

  const handleDeleteNotification = (uuid: string) => {
    setInquiries((prev) => prev.filter((item) => item.uuid !== uuid));
  };

  return (
    <header className="sticky top-0 z-40 border-border bg-[#f5f9fa] backdrop-blur">
      <div className="mx-auto flex h-18 max-w-[1440px] items-center justify-between px-4 sm:px-7 lg:px-10">
        <Link to="/" className="flex items-center gap-2" aria-label="OpenKey home">
          <span className="grid size-9 place-items-center rounded-md text-black">
            <KeyRound className="size-5" />
          </span>
          <span className="font-display text-2xl">OpenKey</span>
        </Link>

        <div className="flex items-center gap-2">
          {loggedIn ? (
            <div className="hidden items-center lg:flex">
              {isAccountPage ? (
                <Button variant="ghost" onClick={() => void navigate({ to: "/settings" })}>
                  <Settings className="size-4" /> Settings
                </Button>
              ) : (
                <div className="group relative flex items-center">
                  <Button variant="ghost" onClick={() => void navigate({ to: "/account" })}>
                    <UserRound className="size-4" /> Account
                  </Button>

                  <div className="flex max-w-0 items-center gap-1 overflow-hidden opacity-0 transition-all duration-200 group-hover:max-w-xs group-hover:opacity-100">
                    <Button
                      variant="ghost"
                      className="shrink-0"
                      onClick={() => void navigate({ to: "/savedhomes" })}
                    >
                      <Heart className="size-4 text-primary" /> Saved listings
                    </Button>
                    <Button
                      variant="ghost"
                      className="shrink-0"
                      onClick={() =>
                        void navigate({ to: "/mylistings", search: { inquiries: undefined } })
                      }
                    >
                      <Store className="size-4 text-primary" /> My listings
                    </Button>
                  </div>
                </div>
              )}
              <Popover open={popoverOpen} onOpenChange={setPopoverOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="relative"
                    aria-label="Notifications"
                    title="Notifications"
                  >
                    <Bell className="size-5" />
                    {unreadCount > 0 && (
                      <span className="absolute top-1.5 right-1.5 flex size-2.5">
                        <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-red-400 opacity-75"></span>
                        <span className="relative inline-flex size-2.5 rounded-full bg-red-500"></span>
                      </span>
                    )}
                  </Button>
                </PopoverTrigger>
                <PopoverContent
                  align="end"
                  className="w-80 sm:w-96 p-0 shadow-lg border border-border bg-card rounded-lg overflow-hidden"
                >
                  <div className="flex items-center justify-between border-b border-border px-4 py-3 bg-muted/40">
                    <div className="flex items-center gap-2">
                      <Bell className="size-4 text-primary" />
                      <span className="font-semibold text-sm">Inquiries</span>
                    </div>
                    {unreadCount > 0 && (
                      <span className="rounded-full bg-red-500/15 px-2 py-0.5 text-xs font-semibold text-red-600">
                        {unreadCount} unread
                      </span>
                    )}
                  </div>

                  {loadingInquiries ? (
                    <div className="p-8 text-center text-xs text-muted-foreground">
                      Loading inquiries…
                    </div>
                  ) : inquiries.length === 0 ? (
                    <div className="p-8 text-center">
                      <div className="mx-auto grid size-12 place-items-center rounded-full bg-muted text-muted-foreground">
                        <BellOff className="size-6" />
                      </div>
                      <p className="mt-3 text-sm font-medium">No new notifications</p>
                      <p className="mt-1 text-xs text-muted-foreground max-w-[240px] mx-auto">
                        When buyers send an inquiry for any of your listings, it will appear here.
                      </p>
                    </div>
                  ) : visibleInquiries.length === 0 ? (
                    <div className="p-8 text-center text-xs text-muted-foreground">
                      No unread notifications
                    </div>
                  ) : (
                    <div className="max-h-[380px] overflow-y-auto divide-y divide-border">
                      {visibleInquiries.map((item) => (
                        <div
                          key={item.uuid}
                          className={`w-full p-3.5 transition-colors hover:bg-muted/60 flex items-start gap-3 ${
                            item.status === "Unread" ? "bg-primary/[0.04]" : ""
                          }`}
                        >
                          <button
                            type="button"
                            onClick={() => handleInquiryClick(item)}
                            className="flex min-w-0 flex-1 items-start gap-3 text-left"
                          >
                            <span className="mt-0.5 grid size-8 shrink-0 place-items-center rounded-full bg-primary/10 text-primary">
                              <MessageSquare className="size-4" />
                            </span>
                            <span className="min-w-0 flex-1">
                              <span className="flex items-center justify-between gap-1">
                                <span className="truncate text-xs font-semibold text-foreground">
                                  {item.senderName || "Interested Buyer"}
                                </span>
                                <span className="shrink-0 text-[10px] text-muted-foreground">
                                  {formatPostedDate(item.createdDate)}
                                </span>
                              </span>
                              {item.advertTitle && (
                                <span className="mt-0.5 block line-clamp-1 text-[11px] font-medium text-primary">
                                  For: {item.advertTitle}
                                </span>
                              )}
                              <span className="mt-1 block line-clamp-2 text-xs text-muted-foreground">
                                {item.message}
                              </span>
                            </span>
                            {item.status === "Unread" && (
                              <span className="mt-1.5 size-2 shrink-0 rounded-full bg-red-500" />
                            )}
                          </button>
                          <div className="flex shrink-0 items-start gap-1">
                            {item.status === "Unread" && (
                              <Button
                                variant="ghost"
                                size="icon"
                                className="size-7"
                                aria-label="Mark notification as read"
                                title="Mark as read"
                                onClick={() => void handleMarkAsRead(item)}
                              >
                                <Check className="size-3.5" />
                              </Button>
                            )}
                            <Button
                              variant="ghost"
                              size="icon"
                              className="size-7 text-muted-foreground hover:text-destructive"
                              aria-label="Delete notification"
                              title="Delete notification"
                              onClick={() => handleDeleteNotification(item.uuid)}
                            >
                              <Trash2 className="size-3.5" />
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {inquiries.length > 0 && (
                    <div className="border-t border-border bg-muted/20 p-2">
                      <div className="flex gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="min-w-0 flex-1 text-xs"
                          disabled={unreadCount === 0 || updatingNotifications}
                          onClick={() => void handleMarkAllAsRead()}
                        >
                          <CheckCheck className="size-3.5" />
                          {updatingNotifications ? "Marking…" : "Mark all as read"}
                        </Button>
                        <Button
                          variant={showAllNotifications ? "secondary" : "ghost"}
                          size="sm"
                          className="min-w-0 flex-1 text-xs"
                          onClick={() => setShowAllNotifications((current) => !current)}
                        >
                          <Bell className="size-3.5" />
                          {showAllNotifications ? "Unread only" : "View all"}
                        </Button>
                      </div>
                    </div>
                  )}
                </PopoverContent>
              </Popover>
            </div>
          ) : (
            <Button
              variant="ghost"
              className="hidden lg:inline-flex"
              onClick={() => void navigate({ to: "/login" })}
            >
              <UserRound /> Sign in
            </Button>
          )}

          <Button onClick={() => void navigate({ to: loggedIn ? "/createadvert" : "/register" })}>
            <Plus /> Publish listing
          </Button>
        </div>
      </div>
    </header>
  );
}
