import { useState, type FormEvent } from "react";
import { Building2, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { createAdvert } from "@/api/adverts";

type AddPropertyDialogProps = {
  open: boolean;
  onClose: () => void;
};

export function AddPropertyDialog({ open, onClose }: AddPropertyDialogProps) {
  const [published, setPublished] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = () => {
    setPublished(false);
    setError(null);
    onClose();
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setLoading(true);

    const form = event.currentTarget;
    const get = (name: string) =>
      (form.elements.namedItem(name) as HTMLInputElement | HTMLSelectElement).value;

    try {
      await createAdvert({
        title: get("title"),
        description: get("description"),
        price: Number(get("price")),
        surfaceArea: Number(get("surfaceArea")),
        rooms: Number(get("rooms")),
        floor: Number(get("floor")),
        type: get("type") as "Sale" | "Rent",
        address: {
          country: get("country"),
          city: get("city"),
          region: get("region"),
          streetAddress: get("streetAddress"),
          streetNumber: get("streetNumber"),
        },
      });
      setPublished(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="font-display text-2xl">Publish a property</DialogTitle>
          <DialogDescription>
            Your listing becomes public immediately. You can mark it sold or rented later.
          </DialogDescription>
        </DialogHeader>

        {published ? (
          <div className="py-8 text-center">
            <span className="mx-auto grid size-14 place-items-center rounded-full bg-brand-soft text-primary">
              <Check className="size-7" />
            </span>
            <h3 className="mt-4 font-display text-2xl">Your listing is live</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              It is now visible to everyone browsing OpenKey.
            </p>
            <Button className="mt-6" onClick={handleClose}>Close</Button>
          </div>
        ) : (
          <form className="grid gap-4" onSubmit={handleSubmit}>
            {/* Listing type & Property type */}
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Listing type
                <select name="type" required className="h-11 rounded-md border border-input bg-background px-3">
                  <option value="Sale">For sale</option>
                  <option value="Rent">For rent</option>
                </select>
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Rooms
                <input name="rooms" required type="number" min={1} max={20} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>

            {/* Title */}
            <label className="grid gap-1.5 text-sm font-medium">
              Title
              <input name="title" required placeholder="Bright two-bedroom apartment" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {/* Description */}
            <label className="grid gap-1.5 text-sm font-medium">
              Description
              <textarea name="description" required rows={3} placeholder="Describe the property..." className="rounded-md border border-input bg-background px-3 py-2 outline-none focus:ring-2 focus:ring-ring resize-none" />
            </label>

            {/* Price & Surface */}
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Price (MDL)
                <input name="price" required type="number" min={1} placeholder="485000" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Size in m²
                <input name="surfaceArea" required type="number" min={1} placeholder="84" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>

            {/* Floor */}
            <label className="grid gap-1.5 text-sm font-medium">
              Floor
              <input name="floor" required type="number" min={0} max={100} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {/* Address */}
            <p className="text-sm font-semibold text-foreground border-t border-border pt-3">Address</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Country
                <input name="country" required placeholder="Moldova" defaultValue="Moldova" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                City
                <input name="city" required placeholder="Chișinău" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Region / Neighbourhood
                <input name="region" required placeholder="Centru" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Street number
                <input name="streetNumber" required placeholder="12A" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Street address
              <input name="streetAddress" required placeholder="Strada Ștefan cel Mare" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {/* Photo upload (UI only for now) */}
            <label className="grid gap-1.5 text-sm font-medium">
              Photos <span className="font-normal text-muted-foreground">(optional)</span>
              <span className="grid h-24 cursor-pointer place-items-center rounded-md border border-dashed border-input bg-muted text-xs text-muted-foreground">
                <Building2 className="mb-1 size-5" />Choose property photos
                <input type="file" accept="image/*" multiple className="sr-only" />
              </span>
            </label>

            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
            )}

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleClose}>Cancel</Button>
              <Button type="submit" disabled={loading}>
                {loading ? "Publishing…" : "Publish now"}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
