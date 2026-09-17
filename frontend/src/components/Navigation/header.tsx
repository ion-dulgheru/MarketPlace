import { KeyRound, Menu, Plus, UserRound } from "lucide-react";
import { Button } from "../ui/button";
import { Link, useNavigate } from "@tanstack/react-router";
import { useAuth } from "@/hooks/useAuth";

export default function Header() {
  const navigate = useNavigate();
  const { loggedIn, logout } = useAuth();

  const handleLogout = () => {
    logout();
    void navigate({ to: "/login" });
  };

  return (
    <header className="sticky top-0 z-40  border-border bg-[#f5f9fa] backdrop-blur">
      <div className="mx-auto flex h-18 max-w-[1440px] items-center justify-between px-4 sm:px-7 lg:px-10">
        <a href="#top" className="flex items-center gap-2" aria-label="OpenKey home">
          <span className="grid size-9 place-items-center rounded-md  text-black">
            <KeyRound className="size-5" />
          </span>
          <span className="font-display text-2xl">OpenKey</span>
        </a>

        <nav
          className="hidden items-center gap-8 text-sm font-medium lg:flex"
          aria-label="Main navigation"
        >
          <a href="#listings" className="border-b-2 border-primary py-6">
            Browse homes
          </a>
          {loggedIn && (
            <Link to="/mylistings" className="py-6 text-muted-foreground hover:text-foreground">
              My listings
            </Link>
          )}
        </nav>

        <div className="flex items-center gap-2">
          {loggedIn ? (
            // Userul e logat → arată "Sign out" care apelează logout()
            <Button variant="ghost" className="hidden sm:inline-flex" onClick={handleLogout}>
              <UserRound /> Sign out
            </Button>
          ) : (
            // Userul nu e logat → arată "Sign in" care navighează la /login
            <Button
              variant="ghost"
              className="hidden sm:inline-flex"
              onClick={() => void navigate({ to: "/login" })}
            >
              <UserRound /> Sign in
            </Button>
          )}

          <Button onClick={() => void navigate({ to: loggedIn ? "/createadvert" : "/register" })}>
            <Plus /> Publish listing
          </Button>
          <Button variant="ghost" size="icon" className="lg:hidden" aria-label="Open menu">
            <Menu />
          </Button>
        </div>
      </div>
    </header>
  );
}
