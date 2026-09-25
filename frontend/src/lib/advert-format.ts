import type { Advert } from "@/api/adverts";

export function formatAdvertPrice(advert: Pick<Advert, "price" | "type" | "currency">): string {
  const amount = new Intl.NumberFormat("ro-MD").format(advert.price);
  const currencyLabel = advert.currency === "Eur" ? "€" : "MDL";
  return advert.type === "Rent"
    ? `${currencyLabel} ${amount} / month`
    : `${currencyLabel} ${amount}`;
}

export function formatPostedDate(createdDate: string): string {
  const created = new Date(createdDate);
  const diffMinutes = Math.floor((Date.now() - created.getTime()) / 60000);

  if (diffMinutes < 1) return "Just now";
  if (diffMinutes < 60) return `${diffMinutes} min ago`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} ${diffHours === 1 ? "hour" : "hours"} ago`;

  const diffDays = Math.floor(diffHours / 24);
  if (diffDays === 1) return "Yesterday";
  if (diffDays < 7) return `${diffDays} days ago`;

  return created.toLocaleDateString();
}
