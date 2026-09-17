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
import { RichTextEditor } from "@/components/ui/RichTextEditor";

type AddPropertyDialogProps = {
  open: boolean;
  published: boolean;
  onClose: () => void;
  onPublish: (event: FormEvent<HTMLFormElement>) => void;
};

export function AddPropertyDialog({
  open,
  published,
  onClose,
  onPublish,
}: AddPropertyDialogProps) {
  const [description, setDescription] = useState("");
  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
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
            <Button className="mt-6" onClick={onClose}>View listing</Button>
          </div>
        ) : (
          <form className="grid gap-4" onSubmit={onPublish}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Listing type
                <select className="h-11 rounded-md border border-input bg-background px-3">
                  <option>For sale</option><option>For rent</option>
                </select>
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Property type
                <select className="h-11 rounded-md border border-input bg-background px-3">
                  <option>Apartment</option><option>House</option><option>Studio</option><option>Loft</option>
                </select>
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Title
              <input required placeholder="Bright two-bedroom apartment" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <label className="grid gap-1.5 text-sm font-medium">
              Location
              <input required placeholder="Neighbourhood, city" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <div className="grid gap-1.5 text-sm font-medium">
              <span>Description</span>
              <RichTextEditor value={description} onChange={setDescription} />
              <input type="hidden" name="description" value={description} />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Price
                <input required type="number" placeholder="485000" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Size in m²
                <input required type="number" placeholder="84" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Photos
              <span className="grid h-24 cursor-pointer place-items-center rounded-md border border-dashed border-input bg-muted text-xs text-muted-foreground">
                <Building2 className="mb-1 size-5" />Choose property photos
                <input type="file" accept="image/*" className="sr-only" />
              </span>
            </label>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
              <Button type="submit">Publish now</Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
