import { useState } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { reportAdvert } from "@/api/adverts";

const REASONS = [
  { value: "Spam", label: "Spam" },
  { value: "Fraud", label: "Fraud" },
  { value: "Duplicate", label: "Duplicate listing" },
] as const;

type ReportAdvertDialogProps = {
  open: boolean;
  advertUuid: string;
  listingTitle?: string | undefined;
  onClose: () => void;
};

export function ReportAdvertDialog({
  open,
  advertUuid,
  listingTitle,
  onClose,
}: ReportAdvertDialogProps) {
  const [reason, setReason] = useState<string>("Spam");
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sent, setSent] = useState(false);

  const handleClose = () => {
    setReason("Spam");
    setError(null);
    setSent(false);
    onClose();
  };

  const handleSubmit = async () => {
    setSending(true);
    setError(null);

    try {
      await reportAdvert(advertUuid, reason);
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to submit report.");
    } finally {
      setSending(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="font-display text-2xl">Report listing</DialogTitle>
          <DialogDescription>
            {sent
              ? "Thanks — we've received your report."
              : `Let us know what's wrong with "${listingTitle}".`}
          </DialogDescription>
        </DialogHeader>

        {sent ? (
          <DialogFooter>
            <Button onClick={handleClose}>Close</Button>
          </DialogFooter>
        ) : (
          <>
            <div className="grid gap-3 py-2">
              <RadioGroup value={reason} onValueChange={setReason} disabled={sending}>
                {REASONS.map((r) => (
                  <div key={r.value} className="flex items-center gap-2">
                    <RadioGroupItem value={r.value} id={`reason-${r.value}`} />
                    <Label htmlFor={`reason-${r.value}`}>{r.label}</Label>
                  </div>
                ))}
              </RadioGroup>
              {error && <p className="text-sm text-destructive">{error}</p>}
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={handleClose} disabled={sending}>
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={() => void handleSubmit()}
                disabled={sending}
              >
                {sending && <Loader2 className="animate-spin" />}
                Submit report
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}