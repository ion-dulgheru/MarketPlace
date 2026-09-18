import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useRef, useState, type FormEvent, type DragEvent } from "react";
import {
  ArrowLeft,
  Building2,
  Check,
  Home,
  Image as ImageIcon,
  KeyRound,
  Loader2,
  MapPin,
  Star,
  Tag,
  Upload,
  X,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  getAdvertById,
  updateAdvert,
  addAdvertPhoto,
  deleteAdvertPhoto,
  getAdvertPhotoUrl,
  type Advert,
  type AdvertPhoto,
} from "@/api/adverts";
import { isLoggedIn } from "@/lib/tokens";
import { RichTextEditor } from "@/components/ui/RichTextEditor";

export const Route = createFileRoute("/editadvert/$listingId")({
  head: () => ({
    meta: [
      { title: "Edit listing | OpenKey" },
      {
        name: "description",
        content: "Edit your property listing on OpenKey.",
      },
    ],
  }),
  component: EditAdvertPage,
});

type Step = 1 | 2 | 3 | 4;

function EditAdvertPage() {
  const navigate = useNavigate();
  const { listingId } = Route.useParams();

  const [advert, setAdvert] = useState<Advert | null | undefined>(undefined);

  // Stepper state - all steps unlocked since existing data is already loaded
  const [currentStep, setCurrentStep] = useState<Step>(1);
  const [unlockedStep, setUnlockedStep] = useState<Step>(4);

  // Category 1: Advert Type
  const [listingType, setListingType] = useState<"Sale" | "Rent">("Sale");
  const [buildingType, setBuildingType] = useState<"Apartment" | "House">("Apartment");

  // Category 2: Details
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [surfaceArea, setSurfaceArea] = useState("");
  const [rooms, setRooms] = useState("");
  const [levels, setLevels] = useState("1");
  // 2A. Apartment Details
  const [apartmentNumber, setApartmentNumber] = useState("");
  const [apartmentFloor, setApartmentFloor] = useState("");
  const [apartmentBlock, setApartmentBlock] = useState("");
  // 2B. House Details
  const [gardenSquareMeters, setGardenSquareMeters] = useState("");

  // Photos
  const [photos, setPhotos] = useState<AdvertPhoto[]>([]);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [pendingPhotoId, setPendingPhotoId] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Category 3: Address
  const [country, setCountry] = useState("Moldova");
  const [city, setCity] = useState("Chișinău");
  const [region, setRegion] = useState("");
  const [streetAddress, setStreetAddress] = useState("");
  const [streetNumber, setStreetNumber] = useState("");

  // Category 4: Price
  const [price, setPrice] = useState("");

  // Submission state
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    let cancelled = false;
    getAdvertById(listingId)
      .then((result) => {
        if (cancelled) return;
        setAdvert(result);

        if (result) {
          setListingType(result.type ?? "Sale");
          setBuildingType(result.buildingType ?? "Apartment");
          setTitle(result.title ?? "");
          setDescription(result.description ?? "");
          setSurfaceArea(result.surfaceArea ? String(result.surfaceArea) : "");
          setRooms(result.rooms ? String(result.rooms) : "");
          setLevels(result.levels ? String(result.levels) : "1");

          setApartmentNumber(result.apartmentNumber ?? "");
          setApartmentFloor(
            result.apartmentFloor != null
              ? String(result.apartmentFloor)
              : result.buildingType === "Apartment" && result.floor
                ? String(result.floor)
                : "",
          );
          setApartmentBlock(result.apartmentBlock ?? "");
          setGardenSquareMeters(
            result.gardenSquareMeters != null ? String(result.gardenSquareMeters) : "",
          );

          setCountry(result.address?.country ?? "Moldova");
          setCity(result.address?.city ?? "Chișinău");
          setRegion(result.address?.region ?? "");
          setStreetAddress(result.address?.streetAddress ?? "");
          setStreetNumber(result.address?.streetNumber ?? "");

          setPrice(result.price ? String(result.price) : "");
          setPhotos(result.photos ?? []);
        }
      })
      .catch(() => {
        if (!cancelled) setAdvert(null);
      });

    return () => {
      cancelled = true;
    };
  }, [listingId, navigate]);

  // Photo handlers
  const handleUploadPhotos = async (files: FileList | File[] | null) => {
    if (!files || files.length === 0) return;
    setPhotoError(null);

    const maxFileSize = 10 * 1024 * 1024; // 10MB
    const maxTotalPhotos = 20;

    if (photos.length + files.length > maxTotalPhotos) {
      setPhotoError(`You can upload up to ${maxTotalPhotos} photos per listing.`);
      return;
    }

    const validFiles: File[] = [];
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      if (!file.type.startsWith("image/")) {
        setPhotoError("Only image files (JPEG, PNG, WebP, etc.) are supported.");
        continue;
      }
      if (file.size > maxFileSize) {
        setPhotoError(`"${file.name}" exceeds the 10MB file size limit.`);
        continue;
      }
      validFiles.push(file);
    }

    if (validFiles.length === 0) return;

    setUploading(true);
    try {
      for (const file of validFiles) {
        const isPrimary = photos.length === 0 && validFiles.indexOf(file) === 0;
        const uploaded = await addAdvertPhoto(listingId, file, isPrimary);
        setPhotos((current) => [...current, uploaded]);
      }
    } catch (err) {
      setPhotoError(err instanceof Error ? err.message : "Failed to upload photo");
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handleDeletePhoto = async (photoUuid: string) => {
    setPhotoError(null);
    setPendingPhotoId(photoUuid);
    try {
      await deleteAdvertPhoto(listingId, photoUuid);
      setPhotos((current) => current.filter((photo) => photo.uuid !== photoUuid));
    } catch (err) {
      setPhotoError(err instanceof Error ? err.message : "Failed to delete photo");
    } finally {
      setPendingPhotoId(null);
    }
  };

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
    if (e.dataTransfer.files) {
      void handleUploadPhotos(e.dataTransfer.files);
    }
  };

  // Step 1 Completion
  const completeStep1 = () => {
    setError(null);
    setUnlockedStep((prev) => (prev < 2 ? 2 : prev));
    setCurrentStep(2);
  };

  // Step 2 Validation & Completion
  const completeStep2 = () => {
    setError(null);
    if (!title.trim()) {
      setError("Please enter a title for your property.");
      return;
    }
    const stripped = description.replace(/<[^>]*>/g, "").trim();
    if (!stripped) {
      setError("Please enter a description for your property.");
      return;
    }
    if (!surfaceArea || Number(surfaceArea) <= 0) {
      setError("Please enter a valid surface area in m².");
      return;
    }
    if (!rooms || Number(rooms) <= 0) {
      setError("Please specify the number of rooms.");
      return;
    }
    if (!levels || Number(levels) <= 0) {
      setError("Please specify the number of levels.");
      return;
    }

    setUnlockedStep((prev) => (prev < 3 ? 3 : prev));
    setCurrentStep(3);
  };

  // Step 3 Validation & Completion
  const completeStep3 = () => {
    setError(null);
    if (!country.trim() || !city.trim() || !region.trim()) {
      setError("Please fill in country, city, and region.");
      return;
    }
    if (!streetAddress.trim()) {
      setError("Please enter the address / street name.");
      return;
    }
    if (!streetNumber.trim()) {
      setError("Please enter the street number.");
      return;
    }

    setUnlockedStep((prev) => (prev < 4 ? 4 : prev));
    setCurrentStep(4);
  };

  // Final Submit
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setPhotoError(null);

    if (!price || Number(price) <= 0) {
      setError("Please enter a valid price.");
      return;
    }

    setLoading(true);
    setStatusMessage("Saving changes…");

    try {
      const computedFloor =
        buildingType === "Apartment" && apartmentFloor ? Number(apartmentFloor) : Number(levels);

      await updateAdvert(listingId, {
        title: title.trim(),
        description,
        price: Number(price),
        surfaceArea: Number(surfaceArea),
        rooms: Number(rooms),
        floor: computedFloor,
        levels: Number(levels) || 1,
        apartmentFloor: apartmentFloor ? Number(apartmentFloor) : undefined,
        apartmentNumber: apartmentNumber.trim() || undefined,
        apartmentBlock: apartmentBlock.trim() || undefined,
        gardenSquareMeters:
          buildingType === "House" && gardenSquareMeters ? Number(gardenSquareMeters) : undefined,
        address: {
          country: country.trim(),
          city: city.trim(),
          region: region.trim(),
          streetAddress: streetAddress.trim(),
          streetNumber: streetNumber.trim(),
        },
      });

      setStatusMessage("Listing updated!");
      void navigate({ to: `/listings/${listingId}` });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong while saving changes");
    } finally {
      setLoading(false);
      setStatusMessage(null);
    }
  };

  const stepsMeta = [
    { num: 1, title: "Advert Type", desc: "Type & building" },
    {
      num: 2,
      title: "Property Details",
      desc: buildingType === "Apartment" ? "Apartment details" : "House details",
    },
    { num: 3, title: "Address", desc: "Location & street" },
    { num: 4, title: "Price", desc: "Pricing & save" },
  ];

  if (advert === undefined) {
    return (
      <main className="grid min-h-screen place-items-center bg-brand-soft/65 px-4 text-center">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="size-8 animate-spin text-primary" />
          <p className="text-sm font-medium text-muted-foreground">Loading listing details…</p>
        </div>
      </main>
    );
  }

  if (advert === null) {
    return (
      <main className="grid min-h-screen place-items-center bg-brand-soft/65 px-4 text-center">
        <div>
          <h1 className="font-display text-4xl">Listing not found</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            This advert may have been removed, or you don't have access to edit it.
          </p>
          <Button asChild className="mt-6">
            <Link to="/mylistings">Back to my listings</Link>
          </Button>
        </div>
      </main>
    );
  }

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
          <Link
            to="/mylistings"
            className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to my listings
          </Link>
        </header>

        <section className="mx-auto w-full max-w-3xl border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <div>
            <p className="text-sm font-semibold text-primary">Step {currentStep} of 4</p>
            <h1 className="mt-1 font-display text-3xl font-bold sm:text-4xl">Edit listing</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              Update property specifications, photos, address, or pricing.
            </p>
          </div>

          {/* Stepper Progress Bar */}
          <div className="mt-6 grid grid-cols-4 gap-2 border-b border-border pb-6">
            {stepsMeta.map((s) => {
              const isPassed = unlockedStep > s.num;
              const isCurrent = currentStep === s.num;
              const isClickable = unlockedStep >= s.num;
              return (
                <button
                  key={s.num}
                  type="button"
                  disabled={!isClickable}
                  onClick={() => isClickable && setCurrentStep(s.num as Step)}
                  className={`flex flex-col items-start rounded-md p-2 text-left transition-colors ${
                    isCurrent
                      ? "bg-primary/10 border-b-2 border-primary"
                      : isClickable
                        ? "hover:bg-muted cursor-pointer"
                        : "opacity-40 cursor-not-allowed"
                  }`}
                >
                  <div className="flex items-center gap-1.5">
                    <span
                      className={`grid size-5 place-items-center rounded-full text-[11px] font-bold ${
                        isPassed
                          ? "bg-primary text-primary-foreground"
                          : isCurrent
                            ? "bg-primary text-primary-foreground"
                            : "bg-muted text-muted-foreground"
                      }`}
                    >
                      {isPassed ? <Check className="size-3 stroke-[3]" /> : s.num}
                    </span>
                    <span className="text-xs font-semibold text-foreground">{s.title}</span>
                  </div>
                  <span className="hidden text-[11px] text-muted-foreground sm:inline mt-0.5 pl-6">
                    {s.desc}
                  </span>
                </button>
              );
            })}
          </div>

          {error && (
            <div className="mt-4 rounded-md bg-destructive/10 px-4 py-2.5 text-sm text-destructive font-medium">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="mt-6">
            {/* ========================================================================= */}
            {/* CATEGORY 1: Advert type (listing type / building type)                    */}
            {/* ========================================================================= */}
            {currentStep === 1 && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">1. Advert type</h2>
                  <p className="text-xs text-muted-foreground">
                    Review or adjust the transaction type and property category.
                  </p>
                </div>

                {/* Listing Type: Sale or Rent */}
                <div>
                  <span className="mb-2 block text-xs font-semibold text-foreground">Listing type</span>
                  <div className="grid grid-cols-2 gap-3">
                    <button
                      type="button"
                      onClick={() => setListingType("Sale")}
                      className={`flex items-center justify-center gap-2 rounded-lg border p-4 font-medium transition-all ${
                        listingType === "Sale"
                          ? "border-primary bg-primary/5 text-primary ring-2 ring-primary/20 shadow-sm"
                          : "border-input bg-background text-muted-foreground hover:border-primary/50"
                      }`}
                    >
                      <Tag className="size-4" />
                      <span>For sale</span>
                    </button>
                    <button
                      type="button"
                      onClick={() => setListingType("Rent")}
                      className={`flex items-center justify-center gap-2 rounded-lg border p-4 font-medium transition-all ${
                        listingType === "Rent"
                          ? "border-primary bg-primary/5 text-primary ring-2 ring-primary/20 shadow-sm"
                          : "border-input bg-background text-muted-foreground hover:border-primary/50"
                      }`}
                    >
                      <KeyRound className="size-4" />
                      <span>For rent</span>
                    </button>
                  </div>
                </div>

                {/* Building Type: Apartment or House */}
                <div>
                  <span className="mb-2 block text-xs font-semibold text-foreground">Building type</span>
                  <div className="grid grid-cols-2 gap-3">
                    <button
                      type="button"
                      onClick={() => setBuildingType("Apartment")}
                      className={`flex flex-col items-center justify-center gap-2 rounded-lg border p-5 font-medium transition-all ${
                        buildingType === "Apartment"
                          ? "border-primary bg-primary/5 text-primary ring-2 ring-primary/20 shadow-sm"
                          : "border-input bg-background text-muted-foreground hover:border-primary/50"
                      }`}
                    >
                      <Building2 className="size-7" />
                      <span className="text-sm font-semibold">Apartment</span>
                      <span className="text-xs text-muted-foreground">Flat in a residential block</span>
                    </button>
                    <button
                      type="button"
                      onClick={() => setBuildingType("House")}
                      className={`flex flex-col items-center justify-center gap-2 rounded-lg border p-5 font-medium transition-all ${
                        buildingType === "House"
                          ? "border-primary bg-primary/5 text-primary ring-2 ring-primary/20 shadow-sm"
                          : "border-input bg-background text-muted-foreground hover:border-primary/50"
                      }`}
                    >
                      <Home className="size-7" />
                      <span className="text-sm font-semibold">House</span>
                      <span className="text-xs text-muted-foreground">Individual house or villa</span>
                    </button>
                  </div>
                </div>

                <Button type="button" onClick={completeStep1} className="w-full mt-4">
                  Continue to Details
                </Button>
              </div>
            )}

            {/* ========================================================================= */}
            {/* CATEGORY 2: Details (2A. Details Apartment or 2B. Details House)          */}
            {/* ========================================================================= */}
            {currentStep === 2 && (
              <div className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-lg font-semibold text-foreground">
                      2. {buildingType === "Apartment" ? "Apartment Details" : "House Details"}
                    </h2>
                    <p className="text-xs text-muted-foreground">
                      {buildingType === "Apartment"
                        ? "Configure levels, apartment floor, block, and room metrics."
                        : "Configure levels, garden size, and house metrics."}
                    </p>
                  </div>
                  <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                    {buildingType}
                  </span>
                </div>

                {/* Common: Title */}
                <label className="grid gap-1.5 text-sm font-medium">
                  Title
                  <input
                    name="title"
                    required
                    maxLength={200}
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    placeholder={
                      buildingType === "Apartment"
                        ? "Modern 2-room apartment in Botanica"
                        : "Spacious modern house with garden"
                    }
                    className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                  />
                </label>

                {/* Common: Description */}
                <div className="grid gap-1.5 text-sm font-medium">
                  <span>Description</span>
                  <RichTextEditor value={description} onChange={setDescription} />
                </div>

                {/* Common Metrics: Surface Area & Rooms */}
                <div className="grid gap-4 sm:grid-cols-2">
                  <label className="grid gap-1.5 text-sm font-medium">
                    Surface Area (m²)
                    <input
                      name="surfaceArea"
                      required
                      type="number"
                      min={1}
                      value={surfaceArea}
                      onChange={(e) => setSurfaceArea(e.target.value)}
                      placeholder="84"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                  <label className="grid gap-1.5 text-sm font-medium">
                    Rooms
                    <input
                      name="rooms"
                      required
                      type="number"
                      min={1}
                      max={30}
                      value={rooms}
                      onChange={(e) => setRooms(e.target.value)}
                      placeholder="3"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                </div>

                {/* 2A. Details Apartment */}
                {buildingType === "Apartment" && (
                  <div className="rounded-lg border border-border bg-muted/30 p-4 space-y-4">
                    <p className="text-xs font-bold uppercase tracking-wider text-primary">
                      Apartment Specifics
                    </p>
                    <div className="grid gap-4 sm:grid-cols-2">
                      <label className="grid gap-1.5 text-sm font-medium">
                        Levels
                        <input
                          name="levels"
                          required
                          type="number"
                          min={1}
                          max={10}
                          value={levels}
                          onChange={(e) => setLevels(e.target.value)}
                          placeholder="1 (e.g. 1, or 2 for duplex)"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                      <label className="grid gap-1.5 text-sm font-medium">
                        Apartment Floor
                        <input
                          name="apartmentFloor"
                          type="number"
                          min={0}
                          max={100}
                          value={apartmentFloor}
                          onChange={(e) => setApartmentFloor(e.target.value)}
                          placeholder="e.g. 4"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2">
                      <label className="grid gap-1.5 text-sm font-medium">
                        Apartment Nr.
                        <input
                          name="apartmentNumber"
                          maxLength={50}
                          value={apartmentNumber}
                          onChange={(e) => setApartmentNumber(e.target.value)}
                          placeholder="e.g. 42B"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                      <label className="grid gap-1.5 text-sm font-medium">
                        Apartment Block
                        <input
                          name="apartmentBlock"
                          maxLength={50}
                          value={apartmentBlock}
                          onChange={(e) => setApartmentBlock(e.target.value)}
                          placeholder="e.g. Block A, Scara 2"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                    </div>
                  </div>
                )}

                {/* 2B. Details House */}
                {buildingType === "House" && (
                  <div className="rounded-lg border border-border bg-muted/30 p-4 space-y-4">
                    <p className="text-xs font-bold uppercase tracking-wider text-primary">
                      House Specifics
                    </p>
                    <div className="grid gap-4 sm:grid-cols-2">
                      <label className="grid gap-1.5 text-sm font-medium">
                        Levels
                        <input
                          name="levels"
                          required
                          type="number"
                          min={1}
                          max={10}
                          value={levels}
                          onChange={(e) => setLevels(e.target.value)}
                          placeholder="e.g. 2 levels"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                      <label className="grid gap-1.5 text-sm font-medium">
                        Garden square meters (m²)
                        <input
                          name="gardenSquareMeters"
                          type="number"
                          min={0}
                          value={gardenSquareMeters}
                          onChange={(e) => setGardenSquareMeters(e.target.value)}
                          placeholder="e.g. 150"
                          className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                        />
                      </label>
                    </div>
                  </div>
                )}

                {/* Photos Dropzone & List */}
                <div className="border-t border-border pt-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-semibold text-foreground">Photos</p>
                      <p className="text-xs text-muted-foreground">
                        The starred photo is used as the cover photo.
                      </p>
                    </div>
                    {photos.length > 0 && (
                      <span className="text-xs font-medium text-muted-foreground">
                        {photos.length} {photos.length === 1 ? "photo" : "photos"}
                      </span>
                    )}
                  </div>

                  {photos.length > 0 && (
                    <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                      {photos.map((photo) => (
                        <div
                          key={photo.uuid}
                          className="group relative aspect-[4/3] overflow-hidden rounded-md border border-border bg-muted shadow-sm transition-all hover:shadow-md"
                        >
                          <img
                            src={getAdvertPhotoUrl(photo.photoUrl)}
                            alt=""
                            className="size-full object-cover transition-transform duration-200 group-hover:scale-105"
                          />
                          {photo.isPrimary && (
                            <span className="absolute left-1.5 top-1.5 inline-flex items-center gap-1 rounded-full bg-background/90 px-2 py-0.5 text-[11px] font-semibold text-primary shadow-sm backdrop-blur-sm">
                              <Star className="size-3 fill-primary text-primary" />
                              Cover
                            </span>
                          )}
                          <button
                            type="button"
                            disabled={pendingPhotoId === photo.uuid}
                            onClick={() => handleDeletePhoto(photo.uuid)}
                            aria-label="Remove photo"
                            className="absolute right-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-background/90 text-destructive shadow-sm backdrop-blur-sm transition-colors hover:bg-destructive hover:text-destructive-foreground disabled:opacity-50"
                          >
                            {pendingPhotoId === photo.uuid ? (
                              <Loader2 className="size-3.5 animate-spin" />
                            ) : (
                              <X className="size-3.5" />
                            )}
                          </button>
                        </div>
                      ))}
                    </div>
                  )}

                  <div
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                    onClick={() => !uploading && fileInputRef.current?.click()}
                    className={`mt-4 flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-6 text-center transition-colors ${
                      isDragging
                        ? "border-primary bg-primary/10"
                        : "border-muted-foreground/25 hover:border-primary/50 hover:bg-muted/40"
                    }`}
                  >
                    <input
                      ref={fileInputRef}
                      type="file"
                      accept="image/*"
                      multiple
                      className="hidden"
                      onChange={(e) => void handleUploadPhotos(e.target.files)}
                    />
                    <div className="grid size-10 place-items-center rounded-full bg-primary/10 text-primary">
                      {uploading ? (
                        <Loader2 className="size-5 animate-spin" />
                      ) : photos.length > 0 ? (
                        <ImageIcon className="size-5" />
                      ) : (
                        <Upload className="size-5" />
                      )}
                    </div>
                    <p className="mt-2 text-sm font-medium">
                      {uploading
                        ? "Uploading photo…"
                        : photos.length > 0
                          ? "Add more photos"
                          : "Choose photos or drag & drop"}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      PNG, JPG, WebP up to 10MB each (max 20 photos)
                    </p>
                  </div>

                  {photoError && (
                    <p className="mt-2 rounded-md bg-destructive/10 px-3 py-2 text-xs text-destructive">
                      {photoError}
                    </p>
                  )}
                </div>

                <div className="flex gap-3 pt-2">
                  <Button type="button" variant="outline" onClick={() => setCurrentStep(1)}>
                    Back
                  </Button>
                  <Button type="button" onClick={completeStep2} className="flex-1">
                    Continue to Address
                  </Button>
                </div>
              </div>
            )}

            {/* ========================================================================= */}
            {/* CATEGORY 3: Address                                                       */}
            {/* ========================================================================= */}
            {currentStep === 3 && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">3. Address</h2>
                  <p className="text-xs text-muted-foreground">
                    Where is the property located? Address name appears before street number.
                  </p>
                </div>

                <div className="grid gap-4 sm:grid-cols-3">
                  <label className="grid gap-1.5 text-sm font-medium">
                    Country
                    <input
                      name="country"
                      required
                      maxLength={100}
                      value={country}
                      onChange={(e) => setCountry(e.target.value)}
                      placeholder="Moldova"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                  <label className="grid gap-1.5 text-sm font-medium">
                    City
                    <input
                      name="city"
                      required
                      maxLength={100}
                      value={city}
                      onChange={(e) => setCity(e.target.value)}
                      placeholder="Chișinău"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                  <label className="grid gap-1.5 text-sm font-medium">
                    Region / Neighbourhood
                    <input
                      name="region"
                      required
                      maxLength={100}
                      value={region}
                      onChange={(e) => setRegion(e.target.value)}
                      placeholder="Centru, Rîșcani, etc."
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                </div>

                <div className="grid gap-4 sm:grid-cols-[2fr_1fr]">
                  <label className="grid gap-1.5 text-sm font-medium">
                    <span className="flex items-center gap-1.5">
                      <MapPin className="size-4 text-primary" />
                      Address name (Street address)
                    </span>
                    <input
                      name="streetAddress"
                      required
                      maxLength={200}
                      value={streetAddress}
                      onChange={(e) => setStreetAddress(e.target.value)}
                      placeholder="Strada Ștefan cel Mare"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                  <label className="grid gap-1.5 text-sm font-medium">
                    Street number
                    <input
                      name="streetNumber"
                      required
                      maxLength={20}
                      value={streetNumber}
                      onChange={(e) => setStreetNumber(e.target.value)}
                      placeholder="12A"
                      className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                    />
                  </label>
                </div>

                <div className="flex gap-3 pt-2">
                  <Button type="button" variant="outline" onClick={() => setCurrentStep(2)}>
                    Back
                  </Button>
                  <Button type="button" onClick={completeStep3} className="flex-1">
                    Continue to Price
                  </Button>
                </div>
              </div>
            )}

            {/* ========================================================================= */}
            {/* CATEGORY 4: Price & Save                                                  */}
            {/* ========================================================================= */}
            {currentStep === 4 && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">4. Price & Save</h2>
                  <p className="text-xs text-muted-foreground">
                    Update your listing price and review changes before saving.
                  </p>
                </div>

                <label className="grid gap-1.5 text-sm font-medium">
                  Price in MDL (Moldovan Leu)
                  <div className="relative">
                    <input
                      name="price"
                      required
                      type="number"
                      min={1}
                      value={price}
                      onChange={(e) => setPrice(e.target.value)}
                      onWheel={(e) => e.currentTarget.blur()}
                      placeholder="485000"
                      className="h-12 w-full rounded-md border border-input bg-background pl-4 pr-16 text-lg font-bold outline-none focus:ring-2 focus:ring-ring no-spin [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
                    />
                    <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-muted-foreground">
                      MDL
                    </span>
                  </div>
                </label>

                {/* Summary Box */}
                <div className="rounded-lg border border-border bg-muted/40 p-5 space-y-3">
                  <p className="text-xs font-bold uppercase tracking-wider text-foreground">
                    Listing Summary
                  </p>
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    <div>
                      <span className="text-xs text-muted-foreground block">Listing & Building</span>
                      <span className="font-medium text-foreground">
                        {listingType === "Sale" ? "For Sale" : "For Rent"} • {buildingType}
                      </span>
                    </div>
                    <div>
                      <span className="text-xs text-muted-foreground block">Dimensions</span>
                      <span className="font-medium text-foreground">
                        {surfaceArea || "0"} m² • {rooms || "0"} rooms • {levels || "1"} levels
                        {buildingType === "House" &&
                          gardenSquareMeters &&
                          ` • ${gardenSquareMeters} m² garden`}
                      </span>
                    </div>
                    <div className="col-span-2">
                      <span className="text-xs text-muted-foreground block">Location</span>
                      <span className="font-medium text-foreground">
                        {streetAddress} {streetNumber}, {region}, {city}, {country}
                      </span>
                    </div>
                    {photos.length > 0 && (
                      <div className="col-span-2">
                        <span className="text-xs text-muted-foreground block">Photos</span>
                        <span className="font-medium text-foreground">
                          {photos.length} {photos.length === 1 ? "photo" : "photos"}
                        </span>
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex gap-3 pt-2">
                  <Button type="button" variant="outline" onClick={() => setCurrentStep(3)}>
                    Back
                  </Button>
                  <Button type="submit" disabled={loading} className="flex-1 h-11 text-base">
                    {loading ? (statusMessage ?? "Saving changes…") : "Save changes"}
                  </Button>
                </div>
              </div>
            )}
          </form>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
