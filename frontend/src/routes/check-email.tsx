import { Link, createFileRoute } from "@tanstack/react-router";
import { KeyRound, MailCheck } from "lucide-react";

export const Route = createFileRoute("/check-email")({
  head: () => ({
    meta: [
      { title: "Verify your email | OpenKey" },
      {
        name: "description",
        content: "Confirm your email address to finish creating your OpenKey account.",
      },
    ],
  }),
  component: CheckEmailPage,
});

function CheckEmailPage() {
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
          <div className="mx-auto grid size-14 place-items-center rounded-full bg-primary/10 text-primary">
            <MailCheck className="size-7" />
          </div>
          <p className="mt-4 text-sm font-semibold text-primary">Almost there</p>
          <h1 className="mt-2 font-display text-4xl leading-tight">Check your email</h1>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            We&apos;ve sent you a verification link. Click it to activate your account, then come
            back and log in.
          </p>

          <Link
            to="/login"
            className="mt-8 inline-flex h-11 items-center justify-center rounded-md bg-primary px-6 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
          >
            Go to login
          </Link>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
