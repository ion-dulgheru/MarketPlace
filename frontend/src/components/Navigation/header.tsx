import { useEffect, useState } from "react";
import {
  Bell,
  BellOff,
  Heart,
  KeyRound,
  MessageSquare,
  Plus,
  Settings,
  Store,
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
                <PopoverContent align="end" className="w-80 sm:w-96 p-0 shadow-lg border border-border bg-card rounded-lg overflow-hidden">
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
                  ) : (
                    <div className="max-h-[380px] overflow-y-auto divide-y divide-border">
                      {inquiries.map((item) => (
                        <button
                          key={item.uuid}
                          type="button"
                          onClick={() => handleInquiryClick(item)}
                          className={`w-full text-left p-3.5 transition-colors hover:bg-muted/60 flex items-start gap-3 ${
                            item.status === "Unread" ? "bg-primary/[0.04]" : ""
                          }`}
                        >
                          <span className="mt-0.5 grid size-8 shrink-0 place-items-center rounded-full bg-primary/10 text-primary">
                            <MessageSquare className="size-4" />
                          </span>
                          <div className="min-w-0 flex-1">
                            <div className="flex items-center justify-between gap-1">
                              <span className="font-semibold text-xs text-foreground truncate">
                                {item.senderName || "Interested Buyer"}
                              </span>
                              <span className="shrink-0 text-[10px] text-muted-foreground">
                                {formatPostedDate(item.createdDate)}
                              </span>
                            </div>
                            {item.advertTitle && (
                              <p className="mt-0.5 text-[11px] font-medium text-primary line-clamp-1">
                                For: {item.advertTitle}
                              </p>
                            )}
                            <p className="mt-1 text-xs text-muted-foreground line-clamp-2">
                              {item.message}
                            </p>
                          </div>
                          {item.status === "Unread" && (
                            <span className="mt-1.5 size-2 shrink-0 rounded-full bg-red-500" />
                          )}
                        </button>
                      ))}
                    </div>
                  )}

                  {inquiries.length > 0 && (
                    <div className="border-t border-border p-2 bg-muted/20 text-center">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="w-full text-xs"
                        onClick={() => {
                          setPopoverOpen(false);
                          void navigate({ to: "/mylistings", search: { inquiries: undefined } });
                        }}
                      >
                        View all in My listings
                      </Button>
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
