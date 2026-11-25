import { useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type ReactNode } from "react";
import { ensureSession } from "@/lib/api-client";

// Guard doar pentru UX: securitatea reala e pe server ([Authorize]).
// Verificarea e in useEffect, fiindca la SSR nu exista localStorage.
export function RequireAuth({
  children,
  redirectTo = "/login",
}: {
  children: ReactNode;
  redirectTo?: "/login" | "/register";
}) {
  const navigate = useNavigate();
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    let active = true;
    void ensureSession().then((signedIn) => {
      if (!active) return;
      if (signedIn) {
        setAllowed(true);
      } else {
        void navigate({ to: redirectTo });
      }
    });
    return () => {
      active = false;
    };
  }, [navigate, redirectTo]);

  return allowed ? <>{children}</> : null;
}
