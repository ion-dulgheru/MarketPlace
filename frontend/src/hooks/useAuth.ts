import { useState, useEffect } from "react";
import { isLoggedIn, clearTokens, isAdmin as checkIsAdmin, getUserRole } from "@/lib/tokens";

export function useAuth() {
  const [loggedIn, setLoggedIn] = useState<boolean>(false);
  const [isAdmin, setIsAdmin] = useState<boolean>(false);
  const [role, setRole] = useState<string | null>(null);

  useEffect(() => {
    setLoggedIn(isLoggedIn());
    setIsAdmin(checkIsAdmin());
    setRole(getUserRole());

    const handleStorage = () => {
      setLoggedIn(isLoggedIn());
      setIsAdmin(checkIsAdmin());
      setRole(getUserRole());
    };
    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  const logout = () => {
    clearTokens();
    setLoggedIn(false);
    setIsAdmin(false);
    setRole(null);
  };

  return { loggedIn, isAdmin, role, logout };
}
