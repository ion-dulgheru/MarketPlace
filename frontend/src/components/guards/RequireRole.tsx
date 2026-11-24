import { useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type ReactNode } from "react";
import { getUserRole, isLoggedIn } from "@/lib/tokens";

// Guard doar pentru UX: securitatea reala e pe server (policy AdminOnly).
export function RequireRole({ role, children }: { role: string; children: ReactNode }) {
  const navigate = useNavigate();
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    if (!isLoggedIn()) {
      void navigate({ to: "/login" });
    } else if (getUserRole() !== role) {
      void navigate({ to: "/" });
    } else {
      setAllowed(true);
    }
  }, [navigate, role]);

  return allowed ? <>{children}</> : null;
}
