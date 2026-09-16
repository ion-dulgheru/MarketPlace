import apartment from "@/assets/openkey-apartment.jpg";
import townhouse from "@/assets/openkey-townhouse.jpg";
import loft from "@/assets/openkey-loft.jpg";

export type ListingDetail = {
  id: number;
  title: string;
  location: string;
  price: string;
  kind: "sale" | "rent";
  type: string;
  beds: number;
  baths: number;
  area: number;
  images: string[];
  imageAlt: string;
  seller: string;
  sellerRole: string;
  posted: string;
  description: string;
  amenities: string[];
  status?: "sold" | "rented";
};

export const listingDetails: ListingDetail[] = [
  {
    id: 1,
    title: "Sunny apartment near the park",
    location: "Centru, Chișinău",
    price: "MDL 1,850,000",
    kind: "sale",
    type: "Apartment",
    beds: 2,
    baths: 1,
    area: 84,
    images: [apartment, loft, townhouse],
    imageAlt: "Bright apartment with large windows",
    seller: "Ana Popescu",
    sellerRole: "Owner",
    posted: "18 min ago",
    description:
      "A bright, comfortable apartment close to the park, local shops, and public transport. The open living area receives plenty of natural light and is ready for its next owner.",
    amenities: ["Large windows", "Central heating", "Furnished kitchen", "Near public transport"],
  },
  {
    id: 2,
    title: "Family house with garden",
    location: "Telecentru, Chișinău",
    price: "MDL 3,450,000",
    kind: "sale",
    type: "House",
    beds: 4,
    baths: 2,
    area: 142,
    images: [townhouse, apartment, loft],
    imageAlt: "Family house with a garden",
    seller: "Moldova Home",
    sellerRole: "Agency",
    posted: "1 hour ago",
    description:
      "A spacious family home with a private garden and comfortable rooms for everyday living. The property offers a quiet setting while remaining close to the city.",
    amenities: ["Private garden", "Parking", "Storage room", "Quiet neighbourhood"],
  },
  {
    id: 3,
    title: "Modern loft in the city centre",
    location: "Centru, Chișinău",
    price: "MDL 18,500 / month",
    kind: "rent",
    type: "Loft",
    beds: 2,
    baths: 1,
    area: 96,
    images: [loft, apartment, townhouse],
    imageAlt: "Renovated loft with a modern interior",
    seller: "Victor Rusu",
    sellerRole: "Owner",
    posted: "2 hours ago",
    description:
      "A renovated loft in the city centre with a modern interior, generous living space, and easy access to restaurants, offices, and cultural spaces.",
    amenities: ["Modern kitchen", "Air conditioning", "High ceilings", "City centre location"],
  },
  {
    id: 4,
    title: "Quiet family home near the lake",
    location: "Bălți",
    price: "MDL 2,250,000",
    kind: "sale",
    type: "House",
    beds: 3,
    baths: 2,
    area: 128,
    images: [townhouse, loft, apartment],
    imageAlt: "Family home with a private yard",
    seller: "Nord Imobil",
    sellerRole: "Agency",
    posted: "Yesterday",
    description:
      "A calm family home near the lake with a private yard, generous bedrooms, and space for relaxing with family and friends.",
    amenities: ["Private yard", "Near the lake", "Parking", "Two bathrooms"],
    status: "sold",
  },
  {
    id: 5,
    title: "Bright studio near the university",
    location: "Rîșcani, Chișinău",
    price: "MDL 8,500 / month",
    kind: "rent",
    type: "Studio",
    beds: 1,
    baths: 1,
    area: 42,
    images: [apartment, loft, townhouse],
    imageAlt: "Bright modern studio apartment",
    seller: "Elena Ceban",
    sellerRole: "Owner",
    posted: "Yesterday",
    description:
      "A bright and practical studio in a convenient location near the university, shops, and public transport.",
    amenities: ["Furnished", "Near university", "Internet ready", "Public transport nearby"],
  },
  {
    id: 6,
    title: "Spacious apartment with terrace",
    location: "Botanica, Chișinău",
    price: "MDL 14,000 / month",
    kind: "rent",
    type: "Apartment",
    beds: 2,
    baths: 2,
    area: 110,
    images: [loft, townhouse, apartment],
    imageAlt: "Spacious apartment with a modern kitchen",
    seller: "Capital Living",
    sellerRole: "Agency",
    posted: "2 days ago",
    description:
      "A spacious apartment with a terrace, modern kitchen, and two bathrooms in a well-connected part of Botanica.",
    amenities: ["Private terrace", "Modern kitchen", "Two bathrooms", "Elevator"],
    status: "rented",
  },
];
