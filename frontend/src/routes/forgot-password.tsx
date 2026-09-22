import { Link, createFileRoute } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { requestPasswordReset } from "@/api/auth";

export const Route = createFileRoute("/forgot-password")({
  head: () => ({
    meta: [
      { title: "Forgot password | OpenKey" },
      {
        name: "description",
        content: "Request a password reset link for your OpenKey account.",
      },
    ],
  }),
  component: ForgotPasswordPage,
});

function ForgotPasswordPage() {
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setLoading(true);

    const form = event.currentTarget;
    const email = (form.elements.namedItem("email") as HTMLInputElement).value;

    try {
      await requestPasswordReset(email);
      setSubmitted(true);
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
          <Link
            to="/login"
            className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" /> Back to login
          </Link>
        </header>

        <section className="mx-auto w-full max-w-lg border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Reset your password</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Forgot password</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Enter your email and we&apos;ll send you a link to reset your password.
          </p>

          {submitted ? (
            <p className="mt-8 rounded-md bg-primary/10 px-3 py-3 text-sm text-primary">
              If an account exists for that email, a reset link is on its way. Check your inbox.
            </p>
          ) : (
            <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
              <label className="grid gap-1.5 text-sm font-medium">
                Email
                <input
                  name="email"
                  required
                  type="email"
                  placeholder="you@example.com"
                  className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
                />
              </label>
              {error && (
                <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {error}
                </p>
              )}
              <Button type="submit" disabled={loading} className="mt-2 w-full">
                {loading ? "Sending…" : "Send reset link"}
              </Button>
            </form>
          )}
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
