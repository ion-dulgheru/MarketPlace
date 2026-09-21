import { Bell, Heart, KeyRound, LogOut, Menu, Plus, UserRound } from "lucide-react";
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
        <Link to="/" className="flex items-center gap-2" aria-label="OpenKey home">
          <span className="grid size-9 place-items-center rounded-md  text-black">
            <KeyRound className="size-5" />
          </span>
          <span className="font-display text-2xl">OpenKey</span>
        </Link>

        <div className="flex items-center gap-2">
          {loggedIn ? (
            <div className="hidden items-center gap-1 sm:flex">
              <Button
                variant="ghost"
                size="icon"
                aria-label="Notifications"
                title="Notifications"
              >
                <Bell />
              </Button>
              <Button variant="ghost" onClick={() => void navigate({ to: "/savedhomes" })}>
                <Heart className="size-4 text-primary" /> Saved listings
              </Button>
              <Button variant="ghost" onClick={() => void navigate({ to: "/account" })}>
                <UserRound /> Account
              </Button>
              <Button variant="ghost" onClick={handleLogout}>
                <LogOut /> Sign out
              </Button>
            </div>
          ) : (
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
