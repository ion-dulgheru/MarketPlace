import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { loginUser } from "@/api/auth";

export const Route = createFileRoute("/login")({
  head: () => ({
    meta: [
      { title: "Log in | OpenKey" },
      {
        name: "description",
        content: "Log in to contact property owners and manage your OpenKey listings.",
      },
    ],
  }),
  component: LoginPage,
});

function LoginPage() {
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

    try {
      await loginUser({ email, password });
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
          <p className="text-sm font-semibold text-primary">Welcome back</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Log in to OpenKey</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Log in to contact property owners and manage your listings.
          </p>

          <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
            <label className="grid gap-1.5 text-sm font-medium">
              Email
              <input name="email" required type="email" placeholder="you@example.com" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <label className="grid gap-1.5 text-sm font-medium">
              Password
              <input name="password" required type="password" placeholder="••••••••" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <Link to="/forgot-password" className="-mt-1 justify-self-end text-sm font-medium text-primary hover:underline">
              Forgot password?
            </Link>
            {error && (
              <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
            )}
            <Button type="submit" disabled={loading} className="mt-2 w-full">
              {loading ? "Logging in…" : "Log in"}
            </Button>
          </form>

          <p className="mt-6 border-t border-border pt-5 text-center text-sm text-muted-foreground">
            Don&apos;t have an account?{" "}
            <Link to="/register" className="font-semibold text-primary hover:underline">
              Create account
            </Link>
          </p>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
