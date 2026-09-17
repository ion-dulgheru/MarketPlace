import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { createAdvert } from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
import { RichTextEditor } from "@/components/ui/RichTextEditor";

export const Route = createFileRoute("/createadvert")({
  head: () => ({
    meta: [
      { title: "Publish a listing | OpenKey" },
      {
        name: "description",
        content: "Publish your property for sale or rent on OpenKey.",
      },
    ],
  }),
  component: CreateAdvertPage,
});

function CreateAdvertPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [description, setDescription] = useState("");

  // Doar userii logați pot publica un anunț
  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/register" });
    }
  }, [navigate]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setLoading(true);

    const form = event.currentTarget;
    const get = (name: string) =>
      (form.elements.namedItem(name) as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement).value;

    const strippedText = description.replace(/<[^>]*>/g, "").trim();
    if (!strippedText) {
      setError("Description is required.");
      setLoading(false);
      return;
    }

    try {
      await createAdvert({
        title: get("title"),
        description: description,
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
      void navigate({ to: "/" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen bg-brand-soft/65 px-4 py-8 text-foreground sm:px-6 lg:px-10">
      <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-3xl flex-col justify-between gap-10">
        <header className="flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2" aria-label="OpenKey home">
            <span className="grid size-9 place-items-center rounded-md bg-primary text-primary-foreground">
              <KeyRound className="size-5" />
            </span>
            <span className="font-display text-2xl">OpenKey</span>
          </Link>
          <Link to="/" className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground">
            <ArrowLeft className="size-4" /> Back to homes
          </Link>
        </header>

        <section className="mx-auto w-full max-w-3xl border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Publish your property</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Create a listing</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Your listing becomes public immediately. You can mark it sold or rented later.
          </p>

          <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Listing type
                <select name="type" required defaultValue="Sale" className="h-11 rounded-md border border-input bg-background px-3">
                  <option value="Sale">For sale</option>
                  <option value="Rent">For rent</option>
                </select>
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Rooms
                <input name="rooms" required type="number" min={1} max={20} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>

            <label className="grid gap-1.5 text-sm font-medium">
              Title
              <input name="title" required maxLength={200} placeholder="Bright two-bedroom apartment" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            <div className="grid gap-1.5 text-sm font-medium">
              <span>Description</span>
              <RichTextEditor value={description} onChange={setDescription} />
            </div>

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

            <label className="grid gap-1.5 text-sm font-medium">
              Floor
              <input name="floor" required type="number" min={0} max={100} placeholder="3" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            <p className="border-t border-border pt-3 text-sm font-semibold text-foreground">Address</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="grid gap-1.5 text-sm font-medium">
                Country
                <input name="country" required maxLength={100} placeholder="Moldova" defaultValue="Moldova" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                City
                <input name="city" required maxLength={100} placeholder="Chișinău" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Region / Neighbourhood
                <input name="region" required maxLength={100} placeholder="Centru" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Street number
                <input name="streetNumber" required maxLength={20} placeholder="12A" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Street address
              <input name="streetAddress" required maxLength={200} placeholder="Strada Ștefan cel Mare" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>

            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
            )}

            <Button type="submit" disabled={loading} className="mt-2 w-full">
              {loading ? "Publishing…" : "Publish now"}
            </Button>
          </form>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
