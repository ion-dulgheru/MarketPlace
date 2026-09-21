import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useRef, useState, type FormEvent, type DragEvent } from "react";
import {
  ArrowLeft,
  Building2,
  Check,
  Home,
  Image as ImageIcon,
  KeyRound,
  MapPin,
  Star,
  Tag,
  Upload,
  X,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { createAdvert, addAdvertPhoto } from "@/api/adverts";
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

interface SelectedPhoto {
  id: string;
  file: File;
  previewUrl: string;
  isPrimary: boolean;
}

type Step = 1 | 2 | 3 | 4;

function CreateAdvertPage() {
  const navigate = useNavigate();

  // Current active step & highest unlocked step
  const [currentStep, setCurrentStep] = useState<Step>(1);
  const [unlockedStep, setUnlockedStep] = useState<Step>(1);

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

  const [photos, setPhotos] = useState<SelectedPhoto[]>([]);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const photosRef = useRef<SelectedPhoto[]>([]);
  photosRef.current = photos;

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

  // Cleanup object URLs on unmount
  useEffect(() => {
    return () => {
      photosRef.current.forEach((p) => URL.revokeObjectURL(p.previewUrl));
    };
  }, []);

  // Only logged in users can publish
  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/register" });
    }
  }, [navigate]);

  const handleAddFiles = (files: FileList | File[] | null) => {
    if (!files || files.length === 0) return;
    setPhotoError(null);

    const validFiles: File[] = [];
    const maxFileSize = 10 * 1024 * 1024; // 10MB
    const maxTotalPhotos = 20;

    if (photos.length + files.length > maxTotalPhotos) {
      setPhotoError(`You can upload up to ${maxTotalPhotos} photos per listing.`);
      return;
    }

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      if (!file) continue;

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

    setPhotos((current) => {
      const hasPrimary = current.some((p) => p.isPrimary);
      const newItems: SelectedPhoto[] = validFiles.map((file, idx) => ({
        id: `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`,
        file,
        previewUrl: URL.createObjectURL(file),
        isPrimary: !hasPrimary && idx === 0,
      }));
      return [...current, ...newItems];
    });

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleRemovePhoto = (id: string) => {
    setPhotos((current) => {
      const target = current.find((p) => p.id === id);
      if (target) {
        URL.revokeObjectURL(target.previewUrl);
      }
      const updated = current.filter((p) => p.id !== id);
      const first = updated[0];
      if (target?.isPrimary && first) {
        updated[0] = { ...first, isPrimary: true };
      }
      return updated;
    });
  };

  const handleSetPrimary = (id: string) => {
    setPhotos((current) =>
      current.map((p) => ({
        ...p,
        isPrimary: p.id === id,
      })),
    );
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
      handleAddFiles(e.dataTransfer.files);
    }
  };

  // Step 1 Validation & Completion
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
    setStatusMessage("Creating listing…");

    try {
      const computedFloor = buildingType === "Apartment" && apartmentFloor ? Number(apartmentFloor) : Number(levels);

      const created = await createAdvert({
        title: title.trim(),
        description: description,
        price: Number(price),
        surfaceArea: Number(surfaceArea),
        rooms: Number(rooms),
        floor: computedFloor,
        type: listingType,
        buildingType: buildingType,
        levels: Number(levels) || 1,
        apartmentFloor: apartmentFloor ? Number(apartmentFloor) : undefined,
        apartmentNumber: apartmentNumber.trim() || undefined,
        apartmentBlock: apartmentBlock.trim() || undefined,
        gardenSquareMeters: buildingType === "House" && gardenSquareMeters ? Number(gardenSquareMeters) : undefined,
        address: {
          country: country.trim(),
          city: city.trim(),
          region: region.trim(),
          streetAddress: streetAddress.trim(),
          streetNumber: streetNumber.trim(),
        },
      });

      const advertId = created.id || created.guid;

      if (advertId && photos.length > 0) {
        for (let i = 0; i < photos.length; i++) {
          const photo = photos[i];
          if (!photo) continue;
          setStatusMessage(`Uploading photo ${i + 1} of ${photos.length}…`);
          try {
            await addAdvertPhoto(advertId, photo.file, photo.isPrimary);
          } catch (photoErr) {
            console.error(`Failed to upload photo ${i + 1}:`, photoErr);
          }
        }
      }

      setStatusMessage("Listing published!");
      if (advertId) {
        void navigate({ to: `/listings/${advertId}` });
      } else {
        void navigate({ to: "/" });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setLoading(false);
      setStatusMessage(null);
    }
  };

  const stepsMeta = [
    { num: 1, title: "Advert Type", desc: "Type & building" },
    { num: 2, title: "Property Details", desc: buildingType === "Apartment" ? "Apartment details" : "House details" },
    { num: 3, title: "Address", desc: "Location & street" },
    { num: 4, title: "Price", desc: "Pricing & publish" },
  ];

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
            to="/"
            className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to homes
          </Link>
        </header>

        <section className="mx-auto w-full max-w-3xl border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <div>
            <p className="text-sm font-semibold text-primary">Step {currentStep} of 4</p>
            <h1 className="mt-1 font-display text-3xl font-bold sm:text-4xl">Publish a property</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              Complete each section to configure your listing.
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
                    Select the transaction type and the building category.
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

                {/* 2A. Details Apartment: Levels, Apt nr, Apt floor, Apt block */}
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

                {/* 2B. Details House: Levels, Garden square meters */}
                {buildingType === "House" && (
                  <div className="rounded-lg border border-border bg-muted/30 p-4 space-y-4">
                    <p className="text-xs font-bold uppercase tracking-wider text-primary">
                      House Specifics
                    </p>
                    <div className="grid gap-4 sm:grid-cols-2">
                      <label className="grid gap-1.5 text-sm font-medium">
                        Levels (instead of floors)
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
                        {photos.length} {photos.length === 1 ? "photo" : "photos"} selected
                      </span>
                    )}
                  </div>

                  {photos.length > 0 && (
                    <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                      {photos.map((photo) => (
                        <div
                          key={photo.id}
                          className="group relative aspect-[4/3] overflow-hidden rounded-md border border-border bg-muted shadow-sm transition-all hover:shadow-md"
                        >
                          <img
                            src={photo.previewUrl}
                            alt=""
                            className="size-full object-cover transition-transform duration-200 group-hover:scale-105"
                          />
                          {photo.isPrimary ? (
                            <span className="absolute left-1.5 top-1.5 inline-flex items-center gap-1 rounded-full bg-background/90 px-2 py-0.5 text-[11px] font-semibold text-primary shadow-sm backdrop-blur-sm">
                              <Star className="size-3 fill-primary text-primary" />
                              Cover
                            </span>
                          ) : (
                            <button
                              type="button"
                              onClick={() => handleSetPrimary(photo.id)}
                              title="Set as cover photo"
                              className="absolute left-1.5 top-1.5 inline-flex items-center gap-1 rounded-full bg-background/80 px-2 py-0.5 text-[11px] font-medium text-muted-foreground opacity-0 backdrop-blur-sm transition-opacity hover:bg-background hover:text-primary group-hover:opacity-100"
                            >
                              <Star className="size-3" />
                              Set cover
                            </button>
                          )}
                          <button
                            type="button"
                            onClick={() => handleRemovePhoto(photo.id)}
                            aria-label="Remove photo"
                            className="absolute right-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-background/90 text-destructive shadow-sm backdrop-blur-sm transition-colors hover:bg-destructive hover:text-destructive-foreground"
                          >
                            <X className="size-3.5" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}

                  <div
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                    onClick={() => fileInputRef.current?.click()}
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
                      onChange={(e) => handleAddFiles(e.target.files)}
                    />
                    <div className="grid size-10 place-items-center rounded-full bg-primary/10 text-primary">
                      {photos.length > 0 ? <ImageIcon className="size-5" /> : <Upload className="size-5" />}
                    </div>
                    <p className="mt-2 text-sm font-medium">
                      {photos.length > 0 ? "Add more photos" : "Choose photos or drag & drop"}
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
            {/* CATEGORY 3: Address (Address name MUST appear before street number)       */}
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

                {/* 3. Address name MUST appear before street number */}
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
            {/* CATEGORY 4: Price & Publish                                               */}
            {/* ========================================================================= */}
            {currentStep === 4 && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">4. Price & Publish</h2>
                  <p className="text-xs text-muted-foreground">
                    Set the listing price and review your configuration.
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
                        {surfaceArea} m² • {rooms} rooms • {levels} levels
                        {buildingType === "House" && gardenSquareMeters && ` • ${gardenSquareMeters} m² garden`}
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
                          {photos.length} photos selected
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
                    {loading ? (statusMessage ?? "Publishing…") : "Publish property"}
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
