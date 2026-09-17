import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import {
  Bath,
  BedDouble,
  ChevronDown,
  Heart,
  House,
  KeyRound,
  MapPin,
  Menu,
  Plus,
  Search,
  SlidersHorizontal,
  Sparkles,
  Square,
  UserRound,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { AddPropertyDialog } from "@/components/dialogs/AddPropertyDialog";
import { ContactOwnerDialog } from "@/components/dialogs/ContactOwnerDialog";
import apartment from "@/assets/openkey-apartment.jpg";
import townhouse from "@/assets/openkey-townhouse.jpg";
import loft from "@/assets/openkey-loft.jpg";
import Header from "@/components/Navigation/header";

type Listing = {
  id: number;
  title: string;
  location: string;
  price: string;
  kind: "sale" | "rent";
  type: string;
  beds: number;
  baths: number;
  area: number;
  image: string;
  imageAlt: string;
  seller: string;
  posted: string;
  status?: "sold" | "rented";
  featured?: boolean;
};

const listings: Listing[] = [
  { id: 1, title: "Sunny apartment near the park", location: "Centru, Chișinău", price: "MDL 1,850,000", kind: "sale", type: "Apartment", beds: 2, baths: 1, area: 84, image: apartment, imageAlt: "Bright apartment with large windows", seller: "Ana Popescu · Owner", posted: "18 min ago", featured: true },
  { id: 2, title: "Family house with garden", location: "Telecentru, Chișinău", price: "MDL 3,450,000", kind: "sale", type: "House", beds: 4, baths: 2, area: 142, image: townhouse, imageAlt: "Family house with a garden", seller: "Moldova Home · Agency", posted: "1 hour ago" },
  { id: 3, title: "Modern loft in the city centre", location: "Centru, Chișinău", price: "MDL 18,500 / month", kind: "rent", type: "Loft", beds: 2, baths: 1, area: 96, image: loft, imageAlt: "Renovated loft with a modern interior", seller: "Victor Rusu · Owner", posted: "2 hours ago" },
  { id: 4, title: "Quiet family home near the lake", location: "Bălți", price: "MDL 2,250,000", kind: "sale", type: "House", beds: 3, baths: 2, area: 128, image: townhouse, imageAlt: "Family home with a private yard", seller: "Nord Imobil · Agency", posted: "Yesterday", status: "sold" },
  { id: 5, title: "Bright studio near the university", location: "Rîșcani, Chișinău", price: "MDL 8,500 / month", kind: "rent", type: "Studio", beds: 1, baths: 1, area: 42, image: apartment, imageAlt: "Bright modern studio apartment", seller: "Elena Ceban · Owner", posted: "Yesterday" },
  { id: 6, title: "Spacious apartment with terrace", location: "Botanica, Chișinău", price: "MDL 14,000 / month", kind: "rent", type: "Apartment", beds: 2, baths: 2, area: 110, image: loft, imageAlt: "Spacious apartment with a modern kitchen", seller: "Capital Living · Agency", posted: "2 days ago", status: "rented" },
];
export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "OpenKey Moldova — Homes for Sale & Rent" },
      { name: "description", content: "Browse public property listings across Moldova or publish your own home for sale or rent instantly on OpenKey." },
      { property: "og:title", content: "OpenKey Moldova — Homes for Sale & Rent" },
      { property: "og:description", content: "Open property listings across Moldova, published instantly by owners and agencies." },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ]
  }),
  component: Index,
});

function Index() {
  const navigate = useNavigate();
  const [mode, setMode] = useState<"all" | "sale" | "rent">("all");
  const [query, setQuery] = useState("");
  const [propertyType, setPropertyType] = useState("All types");
  const [saved, setSaved] = useState<number[]>([]);
  const [dialog, setDialog] = useState<"contact" | "publish" | null>(null);
  const [selected, setSelected] = useState<Listing | null>(null);

  const visible = useMemo(() => listings.filter((listing) => {
    const matchesMode = mode === "all" || listing.kind === mode;
    const matchesQuery = `${listing.title} ${listing.location}`.toLowerCase().includes(query.toLowerCase());
    const matchesType = propertyType === "All types" || listing.type === propertyType;
    return matchesMode && matchesQuery && matchesType;
  }), [mode, query, propertyType]);

  const openContact = (listing: Listing) => { setSelected(listing); setDialog("contact"); };
  const toggleSaved = (id: number) => setSaved((items) => items.includes(id) ? items.filter((item) => item !== id) : [...items, id]);

  return (
    <div className="min-h-screen bg-background text-foreground"> 

     <Header/>

      <main id="top">
        <section className="border-b border-border bg-brand-soft/65">
          <div className="mx-auto max-w-[1440px] px-4 py-12 sm:px-7 sm:py-16 lg:px-10">
            <div className="max-w-4xl">
              <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-primary"><Sparkles className="size-4" /> New homes appear the moment they’re published</p>
              <h1 className="max-w-3xl font-display text-4xl leading-[1.04] sm:text-5xl lg:text-6xl">A clearer way to find your next place.</h1>
              <p className="mt-4 max-w-2xl text-base leading-7 text-muted-foreground sm:text-lg">Open listings from owners and agencies. Browse freely, contact directly, and skip the waiting list.</p>
            </div>

            <div className="mt-9 max-w-6xl border border-border bg-card p-3 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-4">
              <div className="mb-3 flex w-fit gap-1 rounded-md bg-muted p-1">
                {(["all", "sale", "rent"] as const).map((item) => <Button key={item} size="sm" variant={mode === item ? "default" : "ghost"} onClick={() => setMode(item)}>{item === "all" ? "All homes" : item === "sale" ? "For sale" : "For rent"}</Button>)}
              </div>
              <div className="grid gap-2 md:grid-cols-[1fr_190px_160px_auto]">
                <label className="flex h-12 items-center gap-3 rounded-md border border-input bg-background px-4">
                  <MapPin className="size-5 text-primary" /><span className="sr-only">Search location</span>
                  <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="City, neighbourhood, or postcode" className="min-w-0 flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground" />
                </label>
                <label className="flex h-12 items-center gap-3 rounded-md border border-input bg-background px-4">
                  <House className="size-5 text-primary" /><span className="sr-only">Property type</span>
                  <select value={propertyType} onChange={(event) => setPropertyType(event.target.value)} className="min-w-0 flex-1 appearance-none bg-transparent text-sm outline-none"><option>All types</option><option>Apartment</option><option>House</option><option>Loft</option><option>Studio</option></select><ChevronDown className="size-4 text-muted-foreground" />
                </label>
                <Button variant="outline" className="h-12 justify-between"><span className="flex items-center gap-2"><SlidersHorizontal /> Price & details</span></Button>
                <Button className="h-12 px-6"><Search /> Search homes</Button>
              </div>
            </div>
          </div>
        </section>

        <section id="listings" className="mx-auto max-w-[1440px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14">
          <div className="mb-7 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div><p className="text-xs font-bold uppercase text-primary">Recently published</p><h2 className="mt-1 font-display text-3xl sm:text-4xl">Homes ready to discover</h2><p className="mt-2 text-sm text-muted-foreground">{visible.length} listings match your search</p></div>
            <label className="flex items-center gap-2 text-sm text-muted-foreground">Sort by <select className="rounded-md border border-input bg-background px-3 py-2 font-medium text-foreground outline-none"><option>Newest first</option><option>Price: low to high</option><option>Price: high to low</option></select></label>
          </div>

          {visible.length ? <div className="grid gap-x-5 gap-y-9 sm:grid-cols-2 lg:grid-cols-3">
            {visible.map((listing) => <article key={listing.id} className="group min-w-0">
              <div className="relative aspect-[4/3] overflow-hidden rounded-md bg-muted">
                <Link to="/listings/$listingId" params={{ listingId: String(listing.id) }} aria-label={`View details for ${listing.title}`} className="block size-full"><img src={listing.image} alt={listing.imageAlt} width={1024} height={768} loading={listing.id === 1 ? "eager" : "lazy"} className="listing-image size-full object-cover" /></Link>
                <div className="absolute left-3 top-3 flex gap-2"><span className="rounded-sm bg-background/95 px-2.5 py-1 text-xs font-bold uppercase">{listing.kind === "sale" ? "For sale" : "For rent"}</span>{listing.featured && <span className="rounded-sm bg-accent px-2.5 py-1 text-xs font-bold text-accent-foreground">Fresh</span>}</div>
                <Button size="icon" variant="secondary" className="absolute right-3 top-3 rounded-full" onClick={() => toggleSaved(listing.id)} aria-label={saved.includes(listing.id) ? "Remove from saved" : "Save listing"}><Heart className={saved.includes(listing.id) ? "fill-primary text-primary" : ""} /></Button>
                {listing.status && <div className="absolute inset-0 grid place-items-center bg-foreground/45"><span className="-rotate-3 border-2 border-status-foreground bg-status px-5 py-2 text-lg font-bold uppercase text-status-foreground">{listing.status}</span></div>}
              </div>
              <div className="pt-4">
                <div className="flex items-start justify-between gap-3"><div><p className="font-display text-2xl">{listing.price}</p><h3 className="mt-1 text-base font-semibold"><Link to="/listings/$listingId" params={{ listingId: String(listing.id) }} className="hover:text-primary">{listing.title}</Link></h3></div><span className="shrink-0 text-xs text-muted-foreground">{listing.posted}</span></div>
                <p className="mt-1.5 flex items-center gap-1.5 text-sm text-muted-foreground"><MapPin className="size-3.5" />{listing.location}</p>
                <div className="mt-4 flex items-center gap-4 border-y border-border py-3 text-sm text-muted-foreground"><span className="flex items-center gap-1.5"><BedDouble className="size-4" />{listing.beds}</span><span className="flex items-center gap-1.5"><Bath className="size-4" />{listing.baths}</span><span className="flex items-center gap-1.5"><Square className="size-4" />{listing.area} m²</span></div>
                <div className="mt-3 flex items-center justify-between gap-3"><p className="truncate text-xs text-muted-foreground">{listing.seller}</p><Button size="sm" variant={listing.status ? "secondary" : "outline"} disabled={Boolean(listing.status)} onClick={() => openContact(listing)}>{listing.status ? "Unavailable" : "Contact"}</Button></div>
              </div>
            </article>)}
          </div> : <div className="border-y border-border py-20 text-center"><Search className="mx-auto size-8 text-muted-foreground" /><h3 className="mt-4 font-display text-2xl">No homes found</h3><p className="mt-2 text-sm text-muted-foreground">Try another location or property type.</p><Button variant="outline" className="mt-5" onClick={() => { setQuery(""); setPropertyType("All types"); setMode("all"); }}>Clear filters</Button></div>}
        </section>

        <section className="border-y border-border bg-primary text-primary-foreground">
          <div className="mx-auto flex max-w-[1440px] flex-col justify-between gap-8 px-4 py-10 sm:px-7 md:flex-row md:items-center lg:px-10">
            <div><p className="text-sm font-semibold opacity-80">For owners & agencies</p><h2 className="mt-2 font-display text-3xl sm:text-4xl">Your listing. Live in minutes.</h2><p className="mt-2 max-w-xl text-sm leading-6 opacity-80">Publish directly, update it anytime, and mark it sold or rented when the deal is done.</p></div>
            <Button variant="secondary" size="lg" onClick={() => setDialog("publish")}><Plus /> Publish a property</Button>
          </div>
        </section>
      </main>

      <footer className="mx-auto flex max-w-[1440px] flex-col justify-between gap-5 px-4 py-8 text-sm text-muted-foreground sm:flex-row sm:items-center sm:px-7 lg:px-10"><div className="flex items-center gap-2 text-foreground"><KeyRound className="size-5 text-primary" /><span className="font-display text-lg">OpenKey</span></div><p>Open property listings, published directly.</p><p>© 2026 OpenKey</p></footer>

      <ContactOwnerDialog
        open={dialog === "contact"}
        sellerName={selected?.seller.split(" · ")[0]}
        listingTitle={selected?.title}
        onClose={() => setDialog(null)}
        onSignIn={() => void navigate({ to: "/login" })}
      />
      <AddPropertyDialog
        open={dialog === "publish"}
        onClose={() => setDialog(null)}
      />
    </div>
  );
}