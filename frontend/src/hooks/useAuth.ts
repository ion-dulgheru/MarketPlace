import { useState, useEffect } from "react";
import { isLoggedIn, clearTokens, isAdmin as checkIsAdmin, getUserRole } from "@/lib/tokens";

export function useAuth() {
  // Starea locală: este userul logat?
  // Pornim cu false (localStorage nu există la randarea pe server) și
  // citim valoarea reală după montare, doar în browser.
  const [loggedIn, setLoggedIn] = useState<boolean>(false);
  const [isAdmin, setIsAdmin] = useState<boolean>(false);
  const [role, setRole] = useState<string | null>(null);

  useEffect(() => {
    setLoggedIn(isLoggedIn());
    setIsAdmin(checkIsAdmin());
    setRole(getUserRole());

    // Sincronizăm starea dacă localStorage se schimbă în alt tab
    const handleStorage = () => {
      setLoggedIn(isLoggedIn());
      setIsAdmin(checkIsAdmin());
      setRole(getUserRole());
    };
    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  // Funcția de logout: șterge token-urile și actualizează starea
  const logout = () => {
    clearTokens();
    setLoggedIn(false);
    setIsAdmin(false);
    setRole(null);
  };

  return { loggedIn, isAdmin, role, logout };
}
