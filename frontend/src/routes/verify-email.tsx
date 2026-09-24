import { Link, createFileRoute } from "@tanstack/react-router";
import { useEffect, useRef, useState } from "react";
import { KeyRound } from "lucide-react";
import { verifyEmail } from "@/api/auth";

export const Route = createFileRoute("/verify-email")({
  head: () => ({
    meta: [
      { title: "Verify your email | OpenKey" },
      {
        name: "description",
        content: "Confirming your OpenKey account email address.",
      },
    ],
  }),
  component: VerifyEmailPage,
});

type Status = "verifying" | "success" | "error";

function VerifyEmailPage() {
  const [status, setStatus] = useState<Status>("verifying");
  const [error, setError] = useState<string | null>(null);
  const hasRequested = useRef(false);

  useEffect(() => {
    if (hasRequested.current) return;
    hasRequested.current = true;

    const token =
      typeof window !== "undefined"
        ? (new URLSearchParams(window.location.search).get("token") ?? "")
        : "";

    if (!token) {
      setStatus("error");
      setError("Verification link is missing its token.");
      return;
    }

    verifyEmail(token)
      .then(() => setStatus("success"))
      .catch((err: unknown) => {
        setStatus("error");
        setError(err instanceof Error ? err.message : "Something went wrong");
      });
  }, []);

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
        </header>

        <section className="mx-auto w-full max-w-lg border border-border bg-card p-6 text-center shadow-[0_18px_50px_-32px_oklch(0.22_0.025_155/0.35)] sm:p-10">
          <p className="text-sm font-semibold text-primary">Almost there</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Email verification</h1>

          {status === "verifying" && (
            <p className="mt-6 text-sm leading-6 text-muted-foreground">
              Verifying your email address…
            </p>
          )}

          {status === "success" && (
            <>
              <p className="mt-6 rounded-md bg-primary/10 px-3 py-3 text-sm text-primary">
                Your email has been verified. You can now log in.
              </p>
              <Link
                to="/login"
                className="mt-8 inline-flex h-11 items-center justify-center rounded-md bg-primary px-6 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
              >
                Go to login
              </Link>
            </>
          )}

          {status === "error" && (
            <p className="mt-6 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {error}
            </p>
          )}
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
