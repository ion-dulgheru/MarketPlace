import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { ArrowLeft, KeyRound, LogOut, UserRound } from "lucide-react";
import Header from "@/components/Navigation/header";
import { Button } from "@/components/ui/button";
import { changePassword, getCurrentUser, updateCurrentUser, type CurrentUser } from "@/api/users";
import { isLoggedIn } from "@/lib/tokens";
import { useAuth } from "@/hooks/useAuth";

export const Route = createFileRoute("/settings")({
  head: () => ({ meta: [{ title: "Settings | OpenKey" }] }),
  component: SettingsPage,
});

function SettingsPage() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [user, setUser] = useState<CurrentUser | null>(null);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
      return;
    }

    void getCurrentUser()
      .then(setUser)
      .catch(() => void navigate({ to: "/login" }));
  }, [navigate]);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />
      <main className="mx-auto max-w-[1200px] px-4 py-10 sm:px-7 lg:px-10 lg:py-14">
        <Link
          to="/account"
          className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4" /> Back to account
        </Link>
        <ChangePasswordSection />
        {user && <EditAccountSection user={user} onSaved={setUser} />}
        <section className="mt-8 rounded-lg border border-border bg-card p-6 sm:p-10">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h2 className="font-display text-2xl">Sign out</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                Sign out of your OpenKey account on this device.
              </p>
            </div>
            <Button
              variant="outline"
              className="shrink-0 gap-1.5 text-destructive hover:text-destructive"
              onClick={() => {
                logout();
                void navigate({ to: "/login" });
              }}
            >
              <LogOut className="size-4" /> Sign out
            </Button>
          </div>
        </section>
      </main>
    </div>
  );
}

function EditAccountSection({
  user,
  onSaved,
}: {
  user: CurrentUser;
  onSaved: (user: CurrentUser) => void;
}) {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccess(false);
    const form = event.currentTarget;
    const data = {
      firstName: (form.elements.namedItem("firstName") as HTMLInputElement).value.trim(),
      lastName: (form.elements.namedItem("lastName") as HTMLInputElement).value.trim(),
      dateOfBirth: (form.elements.namedItem("dateOfBirth") as HTMLInputElement).value || null,
      phoneNumber:
        (form.elements.namedItem("phoneNumber") as HTMLInputElement).value.trim() || null,
    };

    setLoading(true);
    try {
      await updateCurrentUser(data);
      onSaved({ ...user, ...data });
      setSuccess(true);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Failed to update account");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="mt-8 rounded-lg border border-border bg-card p-6 sm:p-10">
      <div className="flex items-center gap-2">
        <UserRound className="size-5 text-primary" />
        <h2 className="font-display text-2xl">Edit account</h2>
      </div>
      <p className="mt-2 text-sm text-muted-foreground">Update your personal information.</p>
      <form className="mt-6 grid max-w-2xl gap-4 sm:grid-cols-2" onSubmit={handleSubmit}>
        <label className="grid gap-1.5 text-sm font-medium">
          First name
          <input
            name="firstName"
            required
            defaultValue={user.firstName ?? ""}
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          Last name
          <input
            name="lastName"
            required
            defaultValue={user.lastName ?? ""}
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          Date of birth
          <input
            name="dateOfBirth"
            type="date"
            defaultValue={user.dateOfBirth?.slice(0, 10) ?? ""}
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          Phone number
          <input
            name="phoneNumber"
            defaultValue={user.phoneNumber ?? ""}
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        {error && (
          <p className="sm:col-span-2 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {error}
          </p>
        )}
        {success && (
          <p className="sm:col-span-2 rounded-md bg-primary/10 px-3 py-2 text-sm text-primary">
            Account updated successfully.
          </p>
        )}
        <Button type="submit" disabled={loading} className="sm:col-span-2 w-full sm:w-auto">
          {loading ? "Saving…" : "Save account"}
        </Button>
      </form>
    </section>
  );
}

function ChangePasswordSection() {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccess(false);

    const form = event.currentTarget;
    const currentPassword = (form.elements.namedItem("currentPassword") as HTMLInputElement).value;
    const newPassword = (form.elements.namedItem("newPassword") as HTMLInputElement).value;
    const confirmPassword = (form.elements.namedItem("confirmPassword") as HTMLInputElement).value;

    if (newPassword !== confirmPassword) {
      setError("New passwords do not match");
      return;
    }

    setLoading(true);
    try {
      await changePassword({ currentPassword, newPassword });
      setSuccess(true);
      form.reset();
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Failed to change password");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="mt-8 rounded-lg border border-border bg-card p-6 sm:p-10">
      <div className="flex items-center gap-2">
        <KeyRound className="size-5 text-primary" />
        <h1 className="font-display text-2xl">Change password</h1>
      </div>
      <p className="mt-2 text-sm text-muted-foreground">
        Update the password you use to log in to OpenKey.
      </p>

      <form className="mt-6 grid max-w-md gap-4" onSubmit={handleSubmit}>
        <label className="grid gap-1.5 text-sm font-medium">
          Current password
          <input
            name="currentPassword"
            required
            type="password"
            placeholder="••••••••"
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          New password
          <input
            name="newPassword"
            required
            minLength={8}
            type="password"
            placeholder="••••••••"
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          Confirm new password
          <input
            name="confirmPassword"
            required
            minLength={8}
            type="password"
            placeholder="••••••••"
            className="h-11 rounded-md border border-input bg-background px-3 outline-none focus:ring-2 focus:ring-ring"
          />
        </label>
        {error && (
          <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</p>
        )}
        {success && (
          <p className="rounded-md bg-primary/10 px-3 py-2 text-sm text-primary">
            Password updated successfully.
          </p>
        )}
        <Button type="submit" disabled={loading} className="mt-2 w-full sm:w-auto">
          {loading ? "Saving…" : "Update password"}
        </Button>
      </form>
    </section>
  );
}
