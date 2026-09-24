import { useState, useEffect } from "react";
import { Loader2, ExternalLink, Save, X } from "lucide-react";
import { useNavigate } from "@tanstack/react-router";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { getAdvertById, updateAdvert, updateAdvertStatus, type Advert } from "@/api/adverts";
import { toast } from "sonner";

interface QuickEditAdvertDialogProps {
  advertUuid: string | null;
  isOpen: boolean;
  onClose: () => void;
  onSaved: (updatedTitle: string) => void;
}

export function QuickEditAdvertDialog({
  advertUuid,
  isOpen,
  onClose,
  onSaved,
}: QuickEditAdvertDialogProps) {
  const navigate = useNavigate();
  const [advert, setAdvert] = useState<Advert | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Form fields
  const [title, setTitle] = useState("");
  const [price, setPrice] = useState("");
  const [surfaceArea, setSurfaceArea] = useState("");
  const [rooms, setRooms] = useState("");
  const [status, setStatus] = useState<"Active" | "Sold" | "Rented">("Active");
  const [description, setDescription] = useState("");

  useEffect(() => {
    if (!isOpen || !advertUuid) {
      setAdvert(null);
      setError(null);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    getAdvertById(advertUuid)
      .then((data) => {
        if (cancelled) return;
        if (!data) {
          setError("Listing could not be found or has been deleted.");
          return;
        }
        setAdvert(data);
        setTitle(data.title || "");
        setPrice(String(data.price || ""));
        setSurfaceArea(String(data.surfaceArea || ""));
        setRooms(String(data.rooms || ""));
        setStatus(data.status || "Active");
        // Strip basic HTML tags for plain editing if present
        const plainDesc = (data.description || "")
          .replace(/<[^>]+>/g, " ")
          .replace(/\s+/g, " ")
          .trim();
        setDescription(plainDesc);
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Failed to load listing");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [isOpen, advertUuid]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!advert || !advertUuid) return;

    if (!title.trim()) {
      setError("Title is required.");
      return;
    }

    const numPrice = Number(price);
    if (!price || isNaN(numPrice) || numPrice <= 0) {
      setError("Please enter a valid price greater than 0.");
      return;
    }

    const numArea = Number(surfaceArea);
    if (!surfaceArea || isNaN(numArea) || numArea <= 0) {
      setError("Please enter a valid surface area greater than 0.");
      return;
    }

    const numRooms = Number(rooms);
    if (!rooms || isNaN(numRooms) || numRooms <= 0) {
      setError("Please enter a valid number of rooms.");
      return;
    }

    setSaving(true);
    setError(null);

    try {
      // 1. Update status if changed
      if (status !== advert.status) {
        await updateAdvertStatus(advertUuid, status);
      }

      // 2. Update advert details
      await updateAdvert(advertUuid, {
        title: title.trim(),
        description: description.trim() || advert.description,
        price: numPrice,
        surfaceArea: numArea,
        rooms: numRooms,
        floor: advert.floor ?? 1,
        levels: advert.levels ?? 1,
        apartmentFloor: advert.apartmentFloor,
        apartmentNumber: advert.apartmentNumber,
        apartmentBlock: advert.apartmentBlock,
        gardenSquareMeters: advert.gardenSquareMeters,
        address: advert.address || {
          country: "Moldova",
          city: "Chișinău",
          region: "",
          streetAddress: "",
          streetNumber: "",
        },
      });

      toast.success("Listing updated successfully!");
      onSaved(title.trim());
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update listing.");
    } finally {
      setSaving(false);
    }
  };

  const handleOpenFullEditor = () => {
    if (!advertUuid) return;
    onClose();
    void navigate({ to: `/editadvert/${advertUuid}` });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-[620px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center justify-between pr-4">
            <DialogTitle className="font-display text-2xl">Quick Edit Listing</DialogTitle>
          </div>
          <DialogDescription>
            Modify key listing attributes directly from the admin reports panel.
          </DialogDescription>
        </DialogHeader>

        {loading ? (
          <div className="flex flex-col items-center justify-center py-12 gap-3">
            <Loader2 className="size-8 animate-spin text-primary" />
            <p className="text-sm text-muted-foreground">Loading listing details…</p>
          </div>
        ) : error && !advert ? (
          <div className="rounded-lg border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">
            {error}
          </div>
        ) : advert ? (
          <form onSubmit={handleSave} className="space-y-4 py-2">
            {error && (
              <div className="rounded-lg border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                {error}
              </div>
            )}

            <div className="space-y-1.5">
              <Label htmlFor="edit-title">Listing Title</Label>
              <Input
                id="edit-title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Listing title"
                disabled={saving}
                required
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="edit-price">Price (MDL)</Label>
                <Input
                  id="edit-price"
                  type="number"
                  min="1"
                  step="any"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  placeholder="e.g. 85000"
                  disabled={saving}
                  required
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="edit-status">Listing Status</Label>
                <Select
                  value={status}
                  onValueChange={(val: "Active" | "Sold" | "Rented") => setStatus(val)}
                  disabled={saving}
                >
                  <SelectTrigger id="edit-status">
                    <SelectValue placeholder="Select status" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Active">Active</SelectItem>
                    <SelectItem value="Sold">Sold</SelectItem>
                    <SelectItem value="Rented">Rented</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="edit-surface">Surface Area (m²)</Label>
                <Input
                  id="edit-surface"
                  type="number"
                  min="1"
                  step="any"
                  value={surfaceArea}
                  onChange={(e) => setSurfaceArea(e.target.value)}
                  placeholder="e.g. 65"
                  disabled={saving}
                  required
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="edit-rooms">Rooms</Label>
                <Input
                  id="edit-rooms"
                  type="number"
                  min="1"
                  value={rooms}
                  onChange={(e) => setRooms(e.target.value)}
                  placeholder="e.g. 2"
                  disabled={saving}
                  required
                />
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="edit-desc">Description</Label>
              <Textarea
                id="edit-desc"
                rows={4}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Listing description"
                disabled={saving}
              />
            </div>

            <div className="rounded-lg border border-border bg-muted/40 p-3 flex items-center justify-between text-xs text-muted-foreground">
              <span>Need to change photos, location, or advanced house/apartment features?</span>
              <Button
                type="button"
                variant="link"
                size="sm"
                className="h-auto p-0 font-medium text-primary gap-1"
                onClick={handleOpenFullEditor}
              >
                <span>Full Editor</span>
                <ExternalLink className="size-3" />
              </Button>
            </div>

            <DialogFooter className="gap-2 sm:gap-0 pt-2">
              <Button type="button" variant="outline" onClick={onClose} disabled={saving}>
                Cancel
              </Button>
              <Button type="submit" disabled={saving} className="gap-1.5">
                {saving ? (
                  <>
                    <Loader2 className="size-4 animate-spin" />
                    <span>Saving…</span>
                  </>
                ) : (
                  <>
                    <Save className="size-4" />
                    <span>Save Changes</span>
                  </>
                )}
              </Button>
            </DialogFooter>
          </form>
        ) : null}
      </DialogContent>
    </Dialog>
  );
}
