# Security TODO — DAS (Dezvoltarea Aplicațiilor Securizate)

Bazat pe `project_requirements.md` (cerințele PBL/DAS) comparat cu starea actuală a
codului (`backend/`, `frontend/`) la data de 2026-09-25. Conține doar ce **nu** e
făcut încă / e făcut parțial. Pentru context, la final e un rezumat scurt al ce
există deja.

---

## ✅ Rezolvate (2026-09-25)

- [x] **Certificat JWT / parolă în appsettings** — verificare mai atentă: `.pfx`-ul
  **nu a fost niciodată comis în git** (era deja acoperit de `backend/.gitignore`).
  Doar parola placeholder din `appsettings.json` era în clar — am regenerat
  certificatul local cu o parolă nouă random, am scos parola din `appsettings.json`
  (acum `""`, la fel ca `ConnectionStrings:Default`) și am mutat-o în user-secrets.
  Am documentat în `SETUP.md` cum își generează fiecare coleg propriul certificat
  local. Producția folosește deja Key Vault via pipeline-ul de deploy — de
  verificat manual (nu am acces) că secretul `JWT_SIGNING_CERTIFICATE_BASE64` din
  GitHub Actions nu a fost generat din acest fișier local.
- [x] **Security headers** — adăugate în `Program.cs`: `X-Content-Type-Options`,
  `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` (toate mediile) și
  `Content-Security-Policy` + `UseHsts()` (doar non-Development; CSP e sărit în
  dev fiindcă Swagger UI are un `<script>` inline care ar fi blocat).
- [x] **Rate limiting** — `AddRateLimiter`/`UseRateLimiter` (built-in .NET, fără
  pachet extern), policy `"auth"` (10 request-uri/minut per IP) aplicat pe tot
  `AuthController` (login, register, refresh, verify-email, reset parolă).

---

## 1. Criptare și autentificare

- [ ] **MFA/2FA** — nu există deloc (nici TOTP, nici email/SMS code). Cerința o
  menționează explicit ("Use multi-factor authentication").
- [ ] **Criptare la rest pentru date sensibile** — nu e configurată explicit
  criptare pe coloane sensibile în DB (se bazează implicit pe criptarea default a
  Postgres/Azure, dar asta nu e documentată/verificată nicăieri).
- [x] Hashing parole (BCrypt) — făcut, nu mai e nevoie de lucru aici.

## 2. API-uri securizate

- [ ] **OAuth 2.0** — cerința îl menționează explicit; proiectul folosește JWT
  custom (cert-signed) în loc de OAuth 2.0. E o alegere validă, dar trebuie
  documentată explicit ca decizie (de ce JWT propriu și nu OAuth 2.0/OpenID Connect).
- [ ] **Global exception handling / ProblemDetails** — nu există middleware de
  excepții neprins; risc de scurgere de stack trace pe erori neașteptate în
  producție.

## 3. Securitate frontend

- [ ] **Tokenurile stau în `localStorage`** (`frontend/src/lib/tokens.ts`), nu în
  cookie `httpOnly`/`Secure`/`SameSite` → vulnerabile la furt prin XSS, dacă vreodată
  apare o breșă XSS. De discutat: migrare refresh token către cookie httpOnly.
- [x] Sanitizare HTML pe input utilizator (`HtmlSanitizerService`) — există.
- [~] **CSRF** — nefolosite cookie-uri pentru auth (bearer token în header), deci
  riscul clasic CSRF e redus structural; nu există totuși token anti-CSRF explicit
  și nici documentat *de ce* nu e nevoie — merită un paragraf în documentația de
  securitate.

## 4. Securitate backend

- [x] SQL Injection — mitigat structural prin EF Core (query-uri parametrizate),
  nu se folosește SQL raw nicăieri găsit.
- [ ] **RBAC real** — nu există rol/permisiune pe `User` (entitatea `User` n-are
  câmp `Role`). Singurul control de tip admin e o listă de email-uri hardcodată în
  `appsettings.json` (`Admin:Emails`) verificată în `AdminAccessService`, folosită
  doar pe 2 handler-e (`ImportAdverts`, `AttachAdvertImages`). E un RBAC minimal,
  nu unul general (policy-based) — de discutat dacă rămâne așa (README zice
  "single account type, no roles" ca decizie MVP) sau se documentează explicit ca
  limitare cunoscută.
- [ ] **Security logging / audit trail** — nu există logging dedicat pentru
  evenimente de securitate (login eșuat, cont blocat, acces admin, etc.), doar
  logging-ul default ASP.NET Core. Nu există Application Insights / Serilog
  configurat.
- [ ] **Plan de update/patch periodic pentru dependențe** — nu există proces
  documentat (ex. Dependabot, `dotnet list package --vulnerable` în CI).

## 5. Gestionarea bazelor de date

- [ ] **RBAC la nivel de date** — vezi punctul de mai sus (secțiunea 4).
- [ ] **Backup & recovery** — niciun proces documentat sau automatizat găsit în
  `.infra/`.
- [ ] **Criptare explicită date sensibile** — vezi secțiunea 1.

## 6. Documentație cerută de curs (livrabile, nu cod)

- [ ] **Security Documentation** — document cu măsurile de securitate, threat
  model, risk assessment. Nu există încă un fișier dedicat.
- [ ] **Incident Response Plan** — plan de răspuns la incidente. Nu există.
- [ ] **Explicația per-măsură** (Problem / Relevance / Implementation / Testing /
  Limitations) pentru fiecare control de securitate implementat — cerută explicit
  pentru prezentarea finală, nescrisă încă pentru niciuna din măsurile de mai jos:
  BCrypt, JWT+refresh tokens, lockout brute-force, verificare email, reset parolă,
  reCAPTCHA, validare upload fișiere, sanitizare HTML, protecție IDOR pe adverturi.

---

## Rezumat — ce e deja făcut (pentru context, nu mai e pe listă)

- Autentificare JWT (cert-signed) + refresh token cu sesiuni hash-uite în DB,
  access token cu expirare scurtă (15 min).
- Hashing parole cu BCrypt.
- Lockout cont după 5 login-uri eșuate (protecție brute-force parțială).
- Verificare email la înregistrare (token + expirare).
- Reset parolă prin token cu expirare.
- reCAPTCHA (Google) la înregistrare.
- Validare input pe toate comenzile principale (FluentValidation, ~22 validatoare).
- Validare upload imagini: whitelist extensii (`.jpg/.jpeg/.png/.webp`) + limită 5MB.
- Sanitizare HTML pe câmpuri rich-text (protecție XSS pe conținut generat de user).
- Protecție IDOR: operațiile pe adverturi verifică ownership (`GetByUuidForOwnerAsync`),
  nu doar existența resursei.
- CORS restricționat pe origini explicite (localhost, `*.azurewebsites.net`,
  frontend URL configurat) — nu wildcard.
- `UseHttpsRedirection()` + `UseHsts()` activ în non-dev.
- Fără cookie-uri pentru auth (bearer token), deci suprafață CSRF redusă structural.
- Security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`,
  `Permissions-Policy`, `Content-Security-Policy`) setate global în `Program.cs`.
- Rate limiting pe `AuthController` (10 req/min/IP) — completează lockout-ul de cont.
- Certificat JWT local regenerat, fără parolă în clar în `appsettings.json`.
