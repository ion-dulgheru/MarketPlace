const SITE_KEY = import.meta.env["VITE_RECAPTCHA_SITE_KEY"] as string;

declare global {
  interface Window {
    grecaptcha?: {
      ready: (cb: () => void) => void;
      execute: (siteKey: string, options: { action: string }) => Promise<string>;
    };
  }
}

let scriptPromise: Promise<void> | null = null;

function loadScript(): Promise<void> {
  if (scriptPromise) return scriptPromise;

  scriptPromise = new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = `https://www.google.com/recaptcha/api.js?render=${SITE_KEY}`;
    script.async = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("Failed to load reCAPTCHA"));
    document.head.appendChild(script);
  });

  return scriptPromise;
}

// Rulează doar în browser; obține un token reCAPTCHA v3 pentru acțiunea dată.
// Timeout explicit — dacă scriptul Google nu se încarcă/inițializează (site key
// lipsă sau greșită, rețea, Google indisponibil), promisiunea nu trebuie să
// rămână blocată la infinit și să lase butonul de submit "agățat".
export async function getCaptchaToken(action: string, timeoutMs = 10000): Promise<string> {
  const tokenPromise = (async () => {
    await loadScript();

    return new Promise<string>((resolve, reject) => {
      window.grecaptcha!.ready(() => {
        window.grecaptcha!.execute(SITE_KEY, { action }).then(resolve).catch(reject);
      });
    });
  })();

  const timeoutPromise = new Promise<string>((_, reject) => {
    setTimeout(() => reject(new Error("reCAPTCHA timed out — please try again.")), timeoutMs);
  });

  return Promise.race([tokenPromise, timeoutPromise]);
}
