import { useState, useEffect } from "react";
import { isLoggedIn, clearTokens } from "@/lib/tokens";

export function useAuth() {
  // Starea locală: este userul logat?
  // Citim direct din localStorage la inițializare
  const [loggedIn, setLoggedIn] = useState<boolean>(isLoggedIn);

  // Sincronizăm starea dacă localStorage se schimbă în alt tab
  useEffect(() => {
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
