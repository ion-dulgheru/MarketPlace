# Security TODO — DAS (Dezvoltarea Aplicațiilor Securizate)

Bazat pe `project_requirements.md` (cerințele PBL/DAS) comparat cu starea actuală a
codului (`backend/`, `frontend/`) actualizat la 2026-11-25. Punctele bifate sunt făcute,
cu limitările lor; la final e un rezumat scurt al ce există deja.

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

- [x] **MFA/2FA** (SR-12) — TOTP opțional, cu secret criptat și blocare după 5 coduri greșite.
  Limitare: fără coduri de recuperare.
- [ ] **Criptare la rest pentru date sensibile** — secretul TOTP e criptat cu Data Protection;
  restul coloanelor se bazează pe criptarea implicită Azure, nedocumentată/neverificată.
- [x] Hashing parole (BCrypt) — făcut, nu mai e nevoie de lucru aici.

## 2. API-uri securizate

- [ ] **OAuth 2.0** — cerința îl menționează explicit; proiectul folosește JWT
  custom (cert-signed) în loc de OAuth 2.0. E o alegere validă, dar trebuie
  documentată explicit ca decizie (de ce JWT propriu și nu OAuth 2.0/OpenID Connect).
- [x] **Global exception handling / ProblemDetails** (SR-14) — `GlobalExceptionHandler`: răspuns
  generic cu `traceId`, fără mesajul excepției și fără stack trace.

## 3. Securitate frontend

- [x] **Refresh token în cookie HttpOnly** (SR-07) — refresh token-ul nu mai e în
  `localStorage` și nici în body-ul răspunsurilor: stă doar în cookie-ul `openkey_rt`
  (`HttpOnly; Secure; SameSite=None; Path=/api/auth`), rotit la fiecare refresh și
  revocat la `POST /api/auth/logout`. CORS acceptă doar originea exactă a frontend-ului
  (plus localhost în Development). Limitări: access token-ul (15 min) rămâne în
  `localStorage` (plan: păstrare în memorie); `SameSite=None` în loc de `Strict` din cauza
  subdomeniilor `azurewebsites.net` (plan: domeniu propriu).
- [x] Sanitizare HTML pe input utilizator (`HtmlSanitizerService`) — există; în frontend
  DOMPurify e folosit doar prin `src/lib/sanitizer.ts`.
- [x] **CSP în frontend** — `<meta http-equiv="Content-Security-Policy">` în `__root.tsx`.
  Limitare: `'unsafe-inline'` la scripturi (hidratare SSR); plan: nonce.
- [x] **CSRF** — API-ul autorizează cererile prin bearer token în header, pe care un site
  străin nu îl poate trimite. Singurul cookie de auth (`openkey_rt`) e folosit doar de
  `/api/auth/refresh` și `/api/auth/logout`: un site străin poate declanșa cererea, dar nu
  poate citi răspunsul (CORS strict, fără `*.azurewebsites.net`), deci nu obține access
  token-ul; cel mult forțează un logout. De aceea nu e nevoie de token anti-CSRF.

## 4. Securitate backend

- [x] SQL Injection — mitigat structural prin EF Core (query-uri parametrizate),
  nu se folosește SQL raw nicăieri găsit.
- [x] **RBAC** — rol pe `User` (`User`/`Admin`) și policy `AdminOnly` pe `AdminController`.
  Limitări: rolul se atribuie manual în DB; rolul din JWT e valabil până la expirare (15 min).
- [x] **Security logging / audit trail** (SR-13) — tabela `SecurityAuditLogs`: login, blocare,
  refresh, resetare parolă, MFA, acces admin interzis. Limitare: fără retenție și alertare.
- [x] **Update/patch pentru dependențe** — Dependabot săptămânal + alerte, `npm audit`, verificare
  NuGet și Trivy blocante în CI și la deploy.

## 5. Gestionarea bazelor de date

- [ ] **RBAC la nivel de date** — rolurile există doar în aplicație (secțiunea 4).
- [ ] **Backup & recovery** — backup automat 7 zile configurat în `.infra/modules/postgres.bicep`;
  restaurarea nu e documentată.
- [ ] **Criptare explicită date sensibile** — vezi secțiunea 1.

## 6. Documentație cerută de curs (livrabile, nu cod)

- [ ] **Security Documentation** — document cu măsurile de securitate, threat
  model, risk assessment. Nu există încă un fișier dedicat.
- [ ] **Incident Response Plan** — plan de răspuns la incidente. Nu există.
- [x] **Explicația per-măsură** (Problem / Relevance / Implementation / Testing /
  Limitations) — scrisă în `IMPLEMENTATION.md`, secțiunea 4.

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
- Upload imagini: whitelist extensii, limită 5MB, verificarea formatului real și re-encodare.
- Sanitizare HTML pe câmpuri rich-text (protecție XSS pe conținut generat de user).
- Protecție IDOR: operațiile pe adverturi verifică ownership (`GetByUuidForOwnerAsync`),
  nu doar existența resursei.
- CORS restricționat la originea exactă a frontend-ului (plus localhost în Development).
- `UseHttpsRedirection()` + `UseHsts()` activ în non-dev.
- Access token trimis ca bearer; singurul cookie de auth e `openkey_rt` (HttpOnly).
- Security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`,
  `Permissions-Policy`, `Content-Security-Policy`) setate global în `Program.cs`,
  inclusiv pe erorile 500.
- Rate limiting pe `AuthController` (10 req/min/IP) — completează lockout-ul de cont.
- Certificat JWT local regenerat, fără parolă în clar în `appsettings.json`.
