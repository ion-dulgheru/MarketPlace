import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { type FormEvent } from "react";
import { ArrowLeft, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";

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

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void navigate({ to: "/" });
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
            <label className="grid gap-1.5 text-sm font-medium">
              Name
              <input required type="text" placeholder="Your name" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <label className="grid gap-1.5 text-sm font-medium">
              Email
              <input required type="email" placeholder="you@example.com" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <label className="grid gap-1.5 text-sm font-medium">
              Password
              <input required type="password" placeholder="••••••••" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <div className="grid grid-cols-2 gap-4">
              <label className="grid gap-1.5 text-sm font-medium">
                Gender
                <select required defaultValue="" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring">
                  <option value="" disabled>Select</option>
                  <option value="male">Male</option>
                  <option value="female">Female</option>
                  <option value="other">Other</option>
                </select>
              </label>
              <label className="grid gap-1.5 text-sm font-medium">
                Age
                <input required type="number" min={1} max={120} placeholder="e.g. 28" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
              </label>
            </div>
            <label className="grid gap-1.5 text-sm font-medium">
              Phone number
              <input required type="tel" placeholder="+1 (555) 000-0000" className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring" />
            </label>
            <Button type="submit" className="mt-2 w-full">Create account</Button>
          </form>
        </section>

        <p className="text-center text-xs text-muted-foreground">© 2026 OpenKey</p>
      </div>
    </main>
  );
}
