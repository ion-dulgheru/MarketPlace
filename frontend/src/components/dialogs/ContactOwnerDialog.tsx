import { UserRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

type ContactOwnerDialogProps = {
  open: boolean;
  sellerName?: string | undefined;
  listingTitle?: string | undefined;
  onClose: () => void;
  onSignIn: () => void;
};

export function ContactOwnerDialog({
  open,
  sellerName,
  listingTitle,
  onClose,
  onSignIn,
}: ContactOwnerDialogProps) {
  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
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
          <Button variant="outline" onClick={onClose}>
            Keep browsing
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
