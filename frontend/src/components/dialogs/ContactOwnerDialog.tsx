import { useState } from "react";
import { Loader2, UserRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { isLoggedIn } from "@/lib/tokens";
import { sendContactRequest } from "@/api/adverts";

type ContactOwnerDialogProps = {
  open: boolean;
  advertUuid: string;
  sellerName?: string | undefined;
  listingTitle?: string | undefined;
  onClose: () => void;
  onSignIn: () => void;
};

export function ContactOwnerDialog({
  open,
  advertUuid,
  sellerName,
  listingTitle,
  onClose,
  onSignIn,
}: ContactOwnerDialogProps) {
  const [message, setMessage] = useState("");
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sent, setSent] = useState(false);

  const handleClose = () => {
    setMessage("");
    setError(null);
    setSent(false);
    onClose();
  };

  const handleSend = async () => {
    if (!message.trim()) {
      setError("Please write a message before sending.");
      return;
    }

    setSending(true);
    setError(null);

    try {
      await sendContactRequest(advertUuid, message.trim());
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to send message.");
    } finally {
      setSending(false);
    }
  };

  if (!isLoggedIn()) {
    return (
      <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="font-display text-2xl">
              Contact {sellerName}
            </DialogTitle>
            <DialogDescription>
              Create an account or sign in to contact the owner of “{listingTitle}”.
              Browsing always stays open.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-3 py-2">
            <Button onClick={onSignIn}>
              <UserRound /> Continue to sign in
            </Button>
            <Button variant="outline" onClick={handleClose}>
              Keep browsing
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="font-display text-2xl">
            Contact {sellerName}
          </DialogTitle>
          <DialogDescription>
            {sent
              ? "Your message has been sent."
              : `Send a message about "${listingTitle}".`}
          </DialogDescription>
        </DialogHeader>

        {sent ? (
          <DialogFooter>
            <Button onClick={handleClose}>Close</Button>
          </DialogFooter>
        ) : (
          <>
            <div className="grid gap-2 py-2">
              <Textarea
                placeholder="Hi, is this still available?"
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                rows={4}
                disabled={sending}
              />
              {error && <p className="text-sm text-destructive">{error}</p>}
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={handleClose} disabled={sending}>
                Cancel
              </Button>
              <Button onClick={() => void handleSend()} disabled={sending}>
                {sending && <Loader2 className="animate-spin" />}
                Send message
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}