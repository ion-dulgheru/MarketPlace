# Ce este implementat în acest proiect

Document tehnic „as-built": ce există concret în cod la data de 2026-09-25, nu ce
era planificat. Pentru catalogul de use case-uri (scop, model de date, decizii de
design) vezi `README.md`. Pentru ce **lipsește** încă, vezi `SECURITY_TODO.md`.

---

## 1. Stack tehnic

### Backend — `backend/`
| Componentă | Tehnologie |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Web API (controllers, nu minimal API) |
| Bază de date | PostgreSQL, via Npgsql.EntityFrameworkCore |
| ORM | Entity Framework Core 10 (Code First, migrații) |
| Arhitectură aplicație | CQRS cu MediatR (`ICommandHandler`/`IQueryHandler`) |
| Validare | FluentValidation (~22 validatoare, unul per comandă) |
| Autentificare | JWT semnat cu certificat X.509 (nu HMAC), + refresh token |
| Hashing parole | BCrypt.Net-Next |
| Documentație API | Swashbuckle (Swagger UI, doar în Development) |
| Sanitizare HTML | HtmlSanitizer (pt. câmpuri rich-text) |
| CAPTCHA | Google reCAPTCHA (verificare server-side) |
| Email | Azure Communication Email |
| Storage fișiere | Azure Blob Storage (producție) / disc local (dev) |
| Secrete | Azure Key Vault (producție) / user-secrets (dev local) |
| Logging | doar logging-ul default ASP.NET Core (`Serilog.AspNetCore` e referențiat în `.csproj`, dar **nu e configurat/folosit** nicăieri — dependență instalată, neconectată) |

### Frontend — `frontend/`
| Componentă | Tehnologie |
|---|---|
| Framework UI | React 19 |
| Routing / SSR | TanStack Router + TanStack Start |
| Build | Vite |
| Stilizare | Tailwind CSS 4 |
| Formulare | React Hook Form + Zod (validare schema) |
| Editor rich-text | TipTap |
| Componente UI | shadcn/ui (Radix UI + Tailwind) |
| Limbaj | TypeScript |

### Infrastructură & CI/CD
| Componentă | Detaliu |
|---|---|
| Cloud | Azure (App Service pentru backend și frontend, PostgreSQL Flexible Server, Blob Storage, Key Vault, Container Registry) |
| Infrastructure-as-Code | Bicep (`.infra/`) |
| CI/CD | GitHub Actions (`.github/workflows/`: `deploy.yml`, `pr-check.yml`, `rollback.yml`) |
| Identitate în cloud | Managed Identity (fără parole/chei hardcodate pentru acces la Key Vault) |

---

## 2. Arhitectură

Backend-ul urmează Clean Architecture, în 6 proiecte separate:

```
App.Domain          — entități, reguli de business pure, fără dependențe externe
App.Application     — use case-uri (CQRS: Command/Query + Handler), validatoare, abstracții (interfețe)
App.Infrastructure  — implementări concrete (JWT, email, storage, captcha, sanitizare HTML)
App.Persistence     — EF Core, migrații, repository-uri
App.WebApi          — controllers, Program.cs, configurare pipeline HTTP
App.Application.Tests — teste unitare (xUnit) pentru handlere și validatoare
```

Fiecare acțiune (ex. „creează un advert") e un `Command`/`Query` separat, cu propriul
`Handler` și `Validator` — nu există controllere „grase" cu logică de business.

---

## 3. Funcționalități implementate

### Autentificare & cont (`api/auth`, `api/users`)
- Înregistrare cu verificare CAPTCHA (`POST /api/auth/register`)
- Verificare email prin token cu expirare (`POST /api/auth/verify-email`)
- Login cu JWT + refresh token, lockout automat după 5 încercări eșuate
  (`POST /api/auth/login`)
- Refresh token (sesiuni persistate în DB, hash-uite, nu în clar)
  (`POST /api/auth/refresh`)
- Recuperare parolă prin token cu expirare, în 2 pași
  (`POST /api/auth/password-reset-requests`, `POST /api/auth/password-resets`)
- Profil propriu: citire, editare, schimbare parolă
  (`GET/PATCH /api/users/me`, `PATCH /api/users/me/password`)

### Adverturi (`api/adverts`)
- Creare advert, cu sanitizare HTML pe descriere (`POST /api/adverts`)
- Listare + filtrare publică (`GET /api/adverts`)
- Detaliu advert (`GET /api/adverts/{uuid}`)
- Editare advert — doar de proprietar (`PUT /api/adverts/{uuid}`)
- Schimbare status (vândut/închiriat) (`PATCH /api/adverts/{uuid}/status`)
- Ștergere (soft delete) (`DELETE /api/adverts/{uuid}`)
- Upload / ștergere fotografii, validate pe extensie și dimensiune
  (`POST/DELETE /api/adverts/{uuid}/photos`)
- Favorite (`POST /api/adverts/{uuid}/favorite`, `GET /api/adverts/favorites`)
- Raportare advert, cu CAPTCHA (`POST /api/adverts/{uuid}/reports`)
- Cereri de contact către proprietar (`POST /api/adverts/{uuid}/contact-requests`,
  `GET /api/adverts/{uuid}/contact-requests`)

### Cereri de contact (`api/contact-requests`)
- Listare cereri primite (`GET /api/contact-requests`)
- Marcare ca citită (`PUT /api/contact-requests/{uuid}/status`)

### Admin (`api/admin`) — protejat prin allowlist de email, nu rol
- Import în masă de adverturi dintr-un fișier extern (`POST /api/admin/adverts/import`)
- Atașare în masă de imagini locale la adverturi importate, doar în Development
  (`POST /api/admin/adverts/attach-local-images`)

---

## 4. Măsuri de securitate implementate

Pentru fiecare, format cerut de curs: **Problemă → Relevanță → Implementare → Testare → Limitări.**

### 4.1. Hashing parole (BCrypt)
- **Problemă**: parolele nu trebuie stocate în clar — o breșă în DB nu trebuie să
  expună parolele reale ale utilizatorilor.
- **Relevanță**: orice aplicație cu conturi are acest risc; e cerință explicită a
  cerințelor de curs.
- **Implementare**: `BCrypt.Net.BCrypt.HashPassword(request.Password)` la
  înregistrare/schimbare parolă, `BCrypt.Verify` la login (`SignInCommandHandler`),
  folosind work factor-ul implicit al bibliotecii (nesuprascris în cod).
- **Testare**: teste unitare pe `RegisterUserCommandHandlerTests`,
  `SignInCommandHandlerTests` — verifică hash diferit de parola brută și
  verificare corectă la login.
- **Limitări**: work factor fix, nu configurabil per mediu; nu există pepper
  (secret suplimentar la nivel de aplicație), doar salt-ul intern BCrypt.

### 4.2. Autentificare JWT + refresh token
- **Problemă**: sesiunile trebuie gestionate fără a ține state pe server la fiecare
  request, dar cu posibilitate de revocare.
- **Relevanță**: API-ul e consumat de un SPA separat (frontend), nu de server-rendered
  pages — sesiuni bazate pe cookie clasic nu se potrivesc la fel de natural.
- **Implementare**: access token JWT semnat cu certificat X.509 (nu secret simetric),
  expirare scurtă (15 min, `Jwt:ExpiryMinutes`). Refresh token opac, generat random,
  stocat hash-uit în tabela `UserSession` (nu în clar), valabil 30 zile. La refresh,
  sesiunea veche e marcată `Redeemed` (nu mai poate fi refolosită) și se emite un
  refresh token nou — rotație cu detectare de reuse.
- **Testare**: `RefreshTokenCommandHandlerTests`, `SignInCommandHandlerTests`.
- **Limitări**: la detectarea unui refresh token deja folosit (`Redeemed`), sesiunea
  e doar respinsă (`401`) — nu se invalidează automat *toate* sesiunile acelui user,
  deci un atacator care a apucat să facă un refresh înaintea userului legitim nu
  declanșează o revocare globală de sesiuni.

### 4.3. Lockout cont după încercări eșuate (brute-force)
- **Problemă**: atac de tip brute-force / credential stuffing pe formularul de login.
- **Relevanță**: orice endpoint de autentificare public e o țintă directă.
- **Implementare**: `User.RegisterFailedLogin` incrementează un contor; la 5 eșecuri
  consecutive, contul e blocat 15 minute (`IsLockedOut`). Contorul se resetează la
  login reușit.
- **Testare**: acoperit indirect prin `SignInCommandHandlerTests` (cazuri de credențiale
  greșite).
- **Limitări**: lockout e per cont, nu per IP — nu previne enumerarea de conturi sau
  spam-ul distribuit pe conturi diferite. Completat parțial de rate limiting (4.6).

### 4.4. Verificare email
- **Problemă**: prevenirea înregistrării cu adrese de email false/inexistente.
- **Relevanță**: reduce spam-ul de conturi și asigură un canal de contact valid
  pentru recuperare parolă.
- **Implementare**: token random cu expirare, hash-uit în DB (`EmailVerificationTokenHash`);
  contul rămâne creat, dar **login-ul e blocat** până la verificare
  (`SignInCommandHandler` verifică `EmailVerification`).
- **Testare**: `VerifyEmailCommandHandlerTests`.
- **Limitări**: nu există rate limiting explicit pe retrimiterea emailului de
  verificare (dincolo de policy-ul general `"auth"`).

### 4.5. CAPTCHA (Google reCAPTCHA)
- **Problemă**: bot-uri care fac înregistrări în masă sau raportări abuzive.
- **Relevanță**: singurele 2 acțiuni publice de scriere expuse fără autentificare
  anterioară (register, report) sunt cele mai atractive pentru automatizare.
- **Implementare**: `ICaptchaVerifier`/`GoogleRecaptchaVerifier` verifică server-side
  tokenul primit din widget-ul reCAPTCHA înainte de a procesa comanda.
- **Testare**: mock-uit în teste unitare (`RegisterUserCommandHandlerTests`).
- **Limitări**: dependent de disponibilitatea serviciului Google; fără fallback dacă
  reCAPTCHA e indisponibil (cererea e respinsă, nu trece implicit).

### 4.6. Rate limiting pe autentificare
- **Problemă**: request-uri excesive pe endpoint-urile de auth (brute-force distribuit,
  DoS ușor, enumerare conturi).
- **Relevanță**: completează lockout-ul de cont (4.3), care nu acoperă atacuri
  distribuite pe IP.
- **Implementare**: `AddRateLimiter`/`UseRateLimiter` (.NET built-in, fără pachet extern),
  policy `"auth"` = fixed window, 10 request-uri/minut per IP, aplicat pe tot
  `AuthController` (`[EnableRateLimiting("auth")]`).
- **Testare**: manual (apeluri repetate → `429 Too Many Requests`); fără test automat.
- **Limitări**: partiționare pe `RemoteIpAddress` — ineficientă în spatele unui proxy/
  load balancer fără configurare `ForwardedHeaders`; nu există limitare globală pe
  restul API-ului, doar pe auth.

### 4.7. Validare input (FluentValidation)
- **Problemă**: date malformate sau intenționat malițioase ajungând în business logic.
- **Relevanță**: fiecare endpoint care acceptă input de la client e o suprafață de
  atac (injection, buffer/size abuse, valori invalide).
- **Implementare**: câte un `Validator` per comandă (~22 total), rulat automat printr-un
  MediatR pipeline behavior înainte de handler.
- **Testare**: teste unitare dedicate per validator (`*ValidatorTests.cs`).
- **Limitări**: validarea e la nivel de aplicație (business rules), nu există un
  strat separat de rate/size limiting la nivel de request HTTP brut (ex. body prea mare).

### 4.8. Upload fișiere — validare extensie + dimensiune
- **Problemă**: upload de fișiere malițioase (executabile deghizate în imagini,
  fișiere uriașe → DoS pe storage).
- **Relevanță**: singura formă de upload din aplicație (fotografii de advert).
- **Implementare**: whitelist extensii (`.jpg`, `.jpeg`, `.png`, `.webp`), limită
  5MB per fișier (`AddAdvertPhotoCommandValidator`, `AttachAdvertImagesCommandHandler`).
- **Testare**: `AddAdvertPhotoCommandValidatorTests` (extensie invalidă, fișier prea mare).
- **Limitări**: validarea e pe extensie, nu pe magic bytes / conținut real al
  fișierului — un fișier redenumit cu extensie validă dar conținut diferit nu e
  detectat.

### 4.9. Sanitizare HTML (XSS pe conținut generat de user)
- **Problemă**: stored XSS prin descrieri de advert care acceptă rich text (TipTap).
- **Relevanță**: descrierea unui advert e afișată tuturor vizitatorilor — un payload
  malițios ar rula în browserul oricui vede anunțul.
- **Implementare**: `IHtmlSanitizerService`/`HtmlSanitizerService` (bazat pe pachetul
  `HtmlSanitizer`) rulează pe descriere la creare și editare advert.
- **Testare**: `HtmlSanitizerServiceTests` (verifică eliminarea tag-urilor `<script>` etc.).
- **Limitări**: doar pe câmpul descriere; alte câmpuri text (titlu, adresă) nu conțin
  HTML deci nu au nevoie, dar nu sunt reverificate explicit.

### 4.10. Protecție IDOR (Insecure Direct Object Reference)
- **Problemă**: un user autentificat modifică/șterge resurse care nu-i aparțin,
  doar schimbând un ID în request.
- **Relevanță**: orice operație pe resurse cu owner (advert, poză, cerere de contact)
  e vulnerabilă dacă verificarea de ownership lipsește sau se face după încărcare.
- **Implementare**: query-urile de update/delete filtrează direct pe
  `(uuid, ownerId)` — ex. `GetByUuidForOwnerAsync` — nu încarcă resursa și verifică
  ulterior. O resursă a altcuiva e „inexistentă" (`404`), nu „interzisă" (`403`),
  ca să nu confirme existența ei.
- **Testare**: `DeleteAdvertCommandHandlerTests`, `UpdateAdvertHandlerTests` (cazuri
  cu owner greșit → not found).
- **Limitări**: pattern aplicat consecvent pe adverturi; nu a fost auditat exhaustiv
  pentru fiecare handler din aplicație.

### 4.11. CORS restricționat
- **Problemă**: alte site-uri fac request-uri cross-origin către API cu credențiale
  utilizatorului (dacă politica ar fi wildcard + `AllowCredentials`).
- **Relevanță**: API-ul acceptă `AllowCredentials`, deci un wildcard ar fi periculos.
- **Implementare**: `SetIsOriginAllowed` custom — permite doar `localhost`,
  `*.azurewebsites.net` și URL-ul de frontend configurat explicit.
- **Testare**: manual, verificat că request de pe origine neautorizată e blocat de
  browser.
- **Limitări**: `*.azurewebsites.net` e un wildcard de domeniu Azure — permite tehnic
  orice altă aplicație Azure a oricui, nu doar a echipei; acceptabil pentru MVP, de
  strâns ulterior la domeniul exact de producție.

### 4.12. Security headers + HSTS
- **Problemă**: clickjacking (lipsă `X-Frame-Options`), MIME-sniffing (`X-Content-Type-Options`),
  downgrade HTTP→HTTPS (lipsă HSTS), scurgere de `Referer` cross-origin.
- **Relevanță**: măsuri de întărire ieftine, recomandate ca bază pentru orice API web.
- **Implementare**: middleware în `Program.cs` — `X-Content-Type-Options: nosniff`,
  `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`,
  `Permissions-Policy` restrictiv, `Content-Security-Policy` (doar non-Development —
  Swagger UI folosește un `<script>` inline pe care CSP l-ar bloca), `UseHsts()`
  (doar non-Development).
- **Testare**: manual, verificat header-ele în răspuns via curl/browser devtools.
- **Limitări**: CSP e dezactivat complet în Development; nu există `Strict-Transport-Security`
  în dev (normal, dev rulează pe HTTP local).

### 4.13. Fără cookie-uri pentru autentificare (mitigare structurală CSRF)
- **Problemă**: CSRF — un site terț declanșează acțiuni în numele userului autentificat.
- **Relevanță**: relevantă doar dacă browserul trimite automat credențiale (cookie-uri);
  aici nu e cazul.
- **Implementare**: tokenul JWT e trimis explicit în header `Authorization: Bearer`,
  nu automat de browser — un site terț nu poate „forța" acest header fără JavaScript
  care ar avea nevoie oricum de acces la token.
- **Testare**: N/A (proprietate structurală, nu cod de verificat).
- **Limitări**: tokenul stă în `localStorage` pe frontend, ceea ce mută riscul spre
  XSS (dacă apare o breșă XSS, tokenul poate fi citit din JS) — vezi `SECURITY_TODO.md`.

### 4.14. SQL Injection — mitigare structurală (EF Core)
- **Problemă**: injectare de SQL prin input neescapat concatenat în query-uri.
- **Relevanță**: orice API cu bază de date relațională e o țintă clasică.
- **Implementare**: toate accesările la date trec prin EF Core (LINQ), parametrizat
  automat; nu există SQL raw scris manual în cod (verificat prin căutare în tot
  `backend/`).
- **Testare**: N/A — proprietate structurală a ORM-ului, nu testată explicit.
- **Limitări**: dacă se adaugă vreodată `FromSqlRaw`/`ExecuteSqlRaw` cu input de la
  user fără parametrizare, protecția dispare — nu există un linter/analyzer care să
  blocheze asta automat.

---

## 5. Ce nu e implementat încă

Vezi `SECURITY_TODO.md` pentru lista completă (MFA/2FA, RBAC real, criptare la rest,
backup/recovery, logging de securitate dedicat, global exception handling, etc.) și
ce e planificat pentru octombrie–decembrie conform cerințelor cursului.
