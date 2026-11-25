import { useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type ReactNode } from "react";
import { ensureSession } from "@/lib/api-client";
import { getUserRole } from "@/lib/tokens";

// Guard doar pentru UX: securitatea reala e pe server (policy AdminOnly).
export function RequireRole({ role, children }: { role: string; children: ReactNode }) {
  const navigate = useNavigate();
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    let active = true;
    void ensureSession().then((signedIn) => {
      if (!active) return;
      if (!signedIn) {
        void navigate({ to: "/login" });
      } else if (getUserRole() !== role) {
        void navigate({ to: "/" });
      } else {
        setAllowed(true);
      }
    });
    return () => {
      active = false;
    };
  }, [navigate, role]);

  return allowed ? <>{children}</> : null;
}
