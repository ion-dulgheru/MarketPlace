import { useState, useEffect } from "react";
import { isLoggedIn, clearTokens } from "@/lib/tokens";

export function useAuth() {
  // Starea locală: este userul logat?
  // Pornim cu false (localStorage nu există la randarea pe server) și
  // citim valoarea reală după montare, doar în browser.
  const [loggedIn, setLoggedIn] = useState<boolean>(false);

  useEffect(() => {
    setLoggedIn(isLoggedIn());

    // Sincronizăm starea dacă localStorage se schimbă în alt tab
    const handleStorage = () => setLoggedIn(isLoggedIn());
    window.addEventListener("storage", handleStorage);
    return () => window.removeEventListener("storage", handleStorage);
  }, []);

  // Funcția de logout: șterge token-urile și actualizează starea
  const logout = () => {
    clearTokens();
    setLoggedIn(false);
  };

  return { loggedIn, logout };
}
