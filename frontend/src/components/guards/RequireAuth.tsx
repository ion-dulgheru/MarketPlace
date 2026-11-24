import { useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type ReactNode } from "react";
import { isLoggedIn } from "@/lib/tokens";

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
    if (isLoggedIn()) {
      setAllowed(true);
    } else {
      void navigate({ to: redirectTo });
    }
  }, [navigate, redirectTo]);

  return allowed ? <>{children}</> : null;
}
