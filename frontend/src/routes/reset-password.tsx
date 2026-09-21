import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { resetPassword } from "@/api/auth";

export const Route = createFileRoute("/reset-password")({
  head: () => ({
    meta: [
      { title: "Reset password | OpenKey" },
      {
        name: "description",
        content: "Choose a new password for your OpenKey account.",
      },
    ],
  }),
  component: ResetPasswordPage,
});

function ResetPasswordPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  const token = typeof window !== "undefined"
    ? new URLSearchParams(window.location.search).get("token") ?? ""
    : "";

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const form = event.currentTarget;
    const newPassword = (form.elements.namedItem("newPassword") as HTMLInputElement).value;
    const confirmPassword = (form.elements.namedItem("confirmPassword") as HTMLInputElement).value;

    if (newPassword !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    if (!token) {
      setError("Reset link is invalid or missing a token");
      return;
    }

    setLoading(true);
    try {
      await resetPassword(token, newPassword);
      setDone(true);
      setTimeout(() => void navigate({ to: "/login" }), 2000);
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
          <Link to="/login" className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground">
            <ArrowLeft className="size-4" /> Back to login
          </Link>
        </header>

        <section className="mx-auto w-full max-w-lg border border-border bg-card p-6 shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Almost there</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Choose a new password</h1>

          {!token && !done && (
            <p className="mt-3 text-sm leading-6 text-destructive">
              This reset link is missing its token. Please use the link from your email.
            </p>
          )}

          {done ? (
            <p className="mt-8 rounded-md bg-primary/10 px-3 py-3 text-sm text-primary">
              Your password has been reset. Redirecting you to login…
            </p>
          ) : (
            <form className="mt-8 grid gap-4" onSubmit={handleSubmit}>
              <label className="grid gap-1.5 text-sm font-medium">
                New password
                <input name="newPassword" required minLength={8} type="password" placeholder="••••••••" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Confirm new password
                <input name="confirmPassword" required minLength={8} type="password" placeholder="••••••••" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
              {error && (
                <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
              )}
              <Button type="submit" disabled={loading} className="mt-2 w-full">
                {loading ? "Saving…" : "Reset password"}
              </Button>
            </form>
          )}
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
