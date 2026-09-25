import { useState, useEffect } from "react";
import { isLoggedIn, clearTokens } from "@/lib/tokens";

export function useAuth() {
  const [loggedIn, setLoggedIn] = useState<boolean>(false);

  useEffect(() => {
    setLoggedIn(isLoggedIn());

    const handleStorage = () => setLoggedIn(isLoggedIn());
    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  const logout = () => {
    clearTokens();
    setLoggedIn(false);
  };

  return { loggedIn, logout };
}
