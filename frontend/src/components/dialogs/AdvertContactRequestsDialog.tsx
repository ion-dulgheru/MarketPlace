import { useEffect, useState } from "react";
import { Check, Clock, Inbox, Loader2, Mail, MessageSquare, Phone, UserRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  getAdvertContactRequests,
  markContactRequestAsRead,
  type ContactRequest,
} from "@/api/adverts";
import { formatPostedDate } from "@/lib/advert-format";

type AdvertContactRequestsDialogProps = {
  open: boolean;
  advertUuid: string;
  advertTitle: string;
  onClose: () => void;
};

export function AdvertContactRequestsDialog({
  open,
  advertUuid,
  advertTitle,
  onClose,
}: AdvertContactRequestsDialogProps) {
  const [requests, setRequests] = useState<ContactRequest[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [markingId, setMarkingId] = useState<string | null>(null);

  useEffect(() => {
    if (!open || !advertUuid) return;

    let cancelled = false;
    setLoading(true);
    setError(null);

    getAdvertContactRequests(advertUuid)
      .then((data) => {
        if (!cancelled) {
          setRequests(data);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Failed to load contact requests");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [open, advertUuid]);

  const handleMarkAsRead = async (uuid: string) => {
    setMarkingId(uuid);
    try {
      await markContactRequestAsRead(uuid);
      setRequests((current) =>
        current.map((req) => (req.uuid === uuid ? { ...req, status: "Read" } : req)),
      );
    } catch (err) {
      console.error("Failed to mark contact request as read", err);
    } finally {
      setMarkingId(null);
    }
  };

  const unreadCount = requests.filter((r) => r.status === "Unread").length;

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <span className="grid size-8 place-items-center rounded-md bg-primary/10 text-primary">
              <MessageSquare className="size-4" />
            </span>
            <div>
              <DialogTitle className="text-xl">Inquiries & Contact Requests</DialogTitle>
              <DialogDescription className="mt-0.5 line-clamp-1">
                For: <span className="font-medium text-foreground">{advertTitle}</span>
              </DialogDescription>
            </div>
          </div>
          {requests.length > 0 && (
            <div className="mt-2 flex items-center gap-2 text-xs">
              <span className="rounded-full bg-muted px-2.5 py-0.5 font-medium text-muted-foreground">
                {requests.length} total
              </span>
              {unreadCount > 0 ? (
                <span className="rounded-full bg-primary/15 px-2.5 py-0.5 font-semibold text-primary">
                  {unreadCount} unread
                </span>
              ) : (
                <span className="rounded-full bg-status/15 px-2.5 py-0.5 font-semibold text-status">
                  All read
                </span>
              )}
            </div>
          )}
        </DialogHeader>

        <div className="mt-4">
          {loading ? (
            <div className="flex min-h-[220px] flex-col items-center justify-center gap-2 text-center text-muted-foreground">
              <Loader2 className="size-7 animate-spin text-primary" />
              <p className="text-sm">Loading inquiries…</p>
            </div>
          ) : error ? (
            <div className="rounded-md bg-destructive/10 p-4 text-center text-sm text-destructive">
              <p>{error}</p>
              <Button
                variant="outline"
                size="sm"
                className="mt-3"
                onClick={() => {
                  setLoading(true);
                  setError(null);
                  getAdvertContactRequests(advertUuid)
                    .then(setRequests)
                    .catch((e) => setError(e instanceof Error ? e.message : "Failed to load"))
                    .finally(() => setLoading(false));
                }}
              >
                Try again
              </Button>
            </div>
          ) : requests.length === 0 ? (
            <div className="flex min-h-[220px] flex-col items-center justify-center gap-2 rounded-lg border border-dashed border-border p-6 text-center">
              <span className="grid size-12 place-items-center rounded-full bg-muted text-muted-foreground">
                <Inbox className="size-6" />
              </span>
              <p className="mt-1 text-sm font-semibold text-foreground">No inquiries yet</p>
              <p className="max-w-xs text-xs text-muted-foreground">
                When interested buyers or tenants send a contact request for this listing, their
                messages and contact details will appear here.
              </p>
            </div>
          ) : (
            <div className="max-h-[60vh] space-y-3 overflow-y-auto pr-1">
              {requests.map((request) => {
                const isUnread = request.status === "Unread";
                return (
                  <div
                    key={request.uuid}
                    className={`rounded-lg border p-4 transition-colors ${
                      isUnread
                        ? "border-primary/40 bg-primary/[0.03] shadow-xs"
                        : "border-border bg-card"
                    }`}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="flex items-center gap-1.5 font-semibold text-foreground text-sm">
                            <UserRound className="size-4 text-primary" />
                            {request.senderName || "Interested Buyer"}
                          </span>
                          {isUnread ? (
                            <span className="rounded-full bg-primary/15 px-2 py-0.5 text-[11px] font-bold text-primary">
                              Unread
                            </span>
                          ) : (
                            <span className="rounded-full bg-muted px-2 py-0.5 text-[11px] font-medium text-muted-foreground">
                              Read
                            </span>
                          )}
                        </div>

                        <div className="mt-1.5 flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                          {request.senderEmail && (
                            <a
                              href={`mailto:${request.senderEmail}`}
                              className="inline-flex items-center gap-1 hover:text-primary hover:underline"
                            >
                              <Mail className="size-3.5" />
                              {request.senderEmail}
                            </a>
                          )}
                          {request.senderPhone && (
                            <a
                              href={`tel:${request.senderPhone}`}
                              className="inline-flex items-center gap-1 hover:text-primary hover:underline"
                            >
                              <Phone className="size-3.5" />
                              {request.senderPhone}
                            </a>
                          )}
                          <span className="inline-flex items-center gap-1">
                            <Clock className="size-3" />
                            {formatPostedDate(request.createdDate)}
                          </span>
                        </div>
                      </div>

                      {isUnread && (
                        <Button
                          size="sm"
                          variant="ghost"
                          className="h-8 gap-1 text-xs"
                          disabled={markingId === request.uuid}
                          onClick={() => void handleMarkAsRead(request.uuid)}
                        >
                          {markingId === request.uuid ? (
                            <Loader2 className="size-3.5 animate-spin" />
                          ) : (
                            <Check className="size-3.5" />
                          )}
                          Mark read
                        </Button>
                      )}
                    </div>

                    <div className="mt-3 rounded-md bg-muted/40 p-3 text-sm leading-relaxed text-foreground whitespace-pre-wrap">
                      {request.message}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <DialogFooter className="mt-2 sm:justify-end">
          <Button variant="outline" onClick={onClose}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
