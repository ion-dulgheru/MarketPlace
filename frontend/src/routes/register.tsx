import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { registerUser } from "@/api/auth";
import { getCaptchaToken } from "@/lib/recaptcha";

export const Route = createFileRoute("/register")({
  head: () => ({
    meta: [
      { title: "Create an account | OpenKey" },
      {
        name: "description",
        content: "Create an OpenKey account to contact property owners and manage your listings.",
      },
    ],
  }),
  component: RegisterPage,
});

function RegisterPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setLoading(true);

    const form = event.currentTarget;
    const email = (form.elements.namedItem("email") as HTMLInputElement).value;
    const password = (form.elements.namedItem("password") as HTMLInputElement).value;
    const firstName = (form.elements.namedItem("firstName") as HTMLInputElement).value;
    const lastName = (form.elements.namedItem("lastName") as HTMLInputElement).value;
    const dateOfBirth = (form.elements.namedItem("dateOfBirth") as HTMLInputElement).value;
    const phoneNumber = (form.elements.namedItem("phoneNumber") as HTMLInputElement).value;

    try {
      const captchaToken = await getCaptchaToken("register");
      await registerUser({
        email,
        password,
        firstName,
        lastName,
        dateOfBirth: dateOfBirth || null,
        phoneNumber: phoneNumber || null,
        captchaToken,
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
      <div className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-6xl flex-col justify-between gap-10">
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

        <section className="mx-auto w-full max-w-lg border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Join OpenKey</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Create your account</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Register to contact property owners and manage your listings.
          </p>

          <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
            <div className="grid grid-cols-2 gap-4">
              <label className="grid gap-1.5 text-sm font-medium">
                First name
                <input name="firstName" required type="text" placeholder="Ana" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Last name
                <input name="lastName" required type="text" placeholder="Popescu" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Email
              <input name="email" required type="email" placeholder="you@example.com" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <label className="grid gap-1.5 text-sm font-medium">
              Password
              <input name="password" required minLength={8} type="password" placeholder="••••••••" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <div className="grid grid-cols-2 gap-4">
              <label className="grid gap-1.5 text-sm font-medium">
                Date of birth
                <input name="dateOfBirth" type="date" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Phone number
                <input name="phoneNumber" type="tel" placeholder="+1 (555) 000-0000" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
            )}
            <Button type="submit" disabled={loading} className="mt-2 w-full">
              {loading ? "Creating account…" : "Create account"}
            </Button>
          </form>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
