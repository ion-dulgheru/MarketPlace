import { useState, useEffect } from "react";
import { isLoggedIn, isAdmin as checkIsAdmin, getUserRole } from "@/lib/tokens";
import { logoutUser } from "@/api/auth";

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
    void logoutUser();
    setLoggedIn(false);
    setIsAdmin(false);
    setRole(null);
  };

  return { loggedIn, isAdmin, role, logout };
}
