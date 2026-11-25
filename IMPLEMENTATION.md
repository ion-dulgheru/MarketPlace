# Ce este implementat în acest proiect

Document tehnic „as-built": ce există concret în cod la data de 2026-11-25, nu ce
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
| Logging | logging-ul default ASP.NET Core + audit de securitate în tabela `SecurityAuditLogs` |

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
- Refresh token în cookie HttpOnly, sesiuni hash-uite în DB (`POST /api/auth/refresh`)
- Logout cu revocarea sesiunii (`POST /api/auth/logout`)
- Verificare în doi pași TOTP: pasul 2 la login, activare, dezactivare
  (`POST /api/auth/login/mfa`, `POST /api/auth/mfa/setup|enable|disable`)
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
- Upload / ștergere fotografii, validate și re-encodate pe server
  (`POST/DELETE /api/adverts/{uuid}/photos`)
- Favorite (`POST /api/adverts/{uuid}/favorite`, `GET /api/adverts/favorites`)
- Raportare advert, doar autentificat (`POST /api/adverts/{uuid}/reports`)
- Cereri de contact către proprietar (`POST /api/adverts/{uuid}/contact-requests`,
  `GET /api/adverts/{uuid}/contact-requests`)

### Cereri de contact (`api/contact-requests`)
- Listare cereri primite (`GET /api/contact-requests`)
- Marcare ca citită (`PUT /api/contact-requests/{uuid}/status`)

### Admin (`api/admin`) — policy `AdminOnly` (rolul `Admin`)
- Listare și închidere raportări (`GET /api/admin/reports`, `DELETE /api/admin/reports/{uuid}`)
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
- **Testare**: fără test dedicat pentru hash la înregistrare; loginul cu parolă greșită
  e verificat în `SecurityAuditAndRbacTests`.
- **Limitări**: work factor fix, nu configurabil per mediu; nu există pepper
  (secret suplimentar la nivel de aplicație), doar salt-ul intern BCrypt.

### 4.2. Autentificare JWT + refresh token
- **Problemă**: sesiunile trebuie gestionate fără a ține state pe server la fiecare
  request, dar cu posibilitate de revocare.
- **Relevanță**: API-ul e consumat de un frontend separat; access token-ul merge în header,
  iar refresh token-ul doar în cookie-ul HttpOnly `openkey_rt`.
- **Implementare**: access token JWT semnat cu certificat X.509 (nu secret simetric),
  expirare scurtă (15 min, `Jwt:ExpiryMinutes`). Refresh token opac, generat random,
  stocat hash-uit în tabela `UserSession` (nu în clar), valabil 30 zile. La refresh,
  sesiunea veche e marcată `Redeemed` (nu mai poate fi refolosită) și se emite un
  refresh token nou — rotație cu detectare de reuse. Resetarea parolei revocă toate sesiunile,
  iar schimbarea parolei revocă toate celelalte sesiuni.
- **Testare**: `LogoutCommandHandlerTests`, `SecurityAuditAndRbacTests`,
  `PasswordSessionRevocationTests`.
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
- **Testare**: `MfaTests` (blocare după 5 coduri greșite); pentru parolă, fără test dedicat.
- **Limitări**: lockout e per cont, nu per IP — nu previne enumerarea de conturi sau
  spam-ul distribuit pe conturi diferite. Completat parțial de rate limiting (4.6).

### 4.4. Verificare email
- **Problemă**: prevenirea înregistrării cu adrese de email false/inexistente.
- **Relevanță**: reduce spam-ul de conturi și asigură un canal de contact valid
  pentru recuperare parolă.
- **Implementare**: token random cu expirare, hash-uit în DB (`EmailVerificationTokenHash`);
  contul rămâne creat, dar **login-ul e blocat** până la verificare
  (`SignInCommandHandler` verifică `EmailVerification`).
- **Testare**: fără test automat dedicat.
- **Limitări**: nu există rate limiting explicit pe retrimiterea emailului de
  verificare (dincolo de policy-ul general `"auth"`).

### 4.5. CAPTCHA (Google reCAPTCHA)
- **Problemă**: bot-uri care fac înregistrări în masă.
- **Relevanță**: înregistrarea e publică, deci atractivă pentru automatizare. Raportarea
  cere autentificare și nu are CAPTCHA.
- **Implementare**: `ICaptchaVerifier`/`GoogleRecaptchaVerifier` verifică server-side
  tokenul primit din widget-ul reCAPTCHA înainte de a procesa comanda.
- **Testare**: fără test automat dedicat.
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
- **Limitări**: IP-ul clientului vine din ultimul `X-Forwarded-For`, adăugat de App Service
  (`ForwardedHeaders`, un singur hop); nu există limitare globală pe restul API-ului, doar pe auth.

### 4.7. Validare input (FluentValidation)
- **Problemă**: date malformate sau intenționat malițioase ajungând în business logic.
- **Relevanță**: fiecare endpoint care acceptă input de la client e o suprafață de
  atac (injection, buffer/size abuse, valori invalide).
- **Implementare**: câte un `Validator` per comandă (~22 total), rulat automat printr-un
  MediatR pipeline behavior înainte de handler.
- **Testare**: teste unitare pentru o parte din validatoarele de adverturi
  (ex. `CreateAdvertCommandValidatorTests`); restul nu au teste dedicate.
- **Limitări**: validarea e la nivel de aplicație (business rules), nu există un
  strat separat de rate/size limiting la nivel de request HTTP brut (ex. body prea mare).

### 4.8. Upload fișiere — validare și re-encodare
- **Problemă**: upload de fișiere malițioase (executabile deghizate în imagini,
  fișiere uriașe → DoS) și scurgere de date personale prin EXIF/GPS.
- **Relevanță**: singura formă de upload din aplicație (fotografii de advert).
- **Implementare**: whitelist extensii, limită 5MB per fișier, apoi `SkiaImageProcessor`
  detectează formatul real (JPEG/PNG/WebP), respinge peste 6000×6000 px înainte de decodare
  și re-encodează imaginea, deci metadatele și datele ascunse dispar.
- **Testare**: `AddAdvertPhotoCommandValidatorTests`, `SkiaImageProcessorTests`.
- **Limitări**: fără scanare antivirus.

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
- **Testare**: `DeleteAdvertCommandHandlerTests`, `GetAdvertContactRequestsQueryHandlerTests`.
- **Limitări**: pattern aplicat consecvent pe adverturi; nu a fost auditat exhaustiv
  pentru fiecare handler din aplicație.

### 4.11. CORS restricționat
- **Problemă**: alte site-uri fac request-uri cross-origin către API cu credențiale
  utilizatorului (dacă politica ar fi wildcard + `AllowCredentials`).
- **Relevanță**: API-ul acceptă `AllowCredentials`, deci un wildcard ar fi periculos.
- **Implementare**: `SetIsOriginAllowed` acceptă doar originea exactă din `Frontend:BaseUrl`
  (plus `localhost` în Development).
- **Testare**: manual, pe site-ul live.
- **Limitări**: fără test automat.

### 4.12. Security headers + HSTS
- **Problemă**: clickjacking (lipsă `X-Frame-Options`), MIME-sniffing (`X-Content-Type-Options`),
  downgrade HTTP→HTTPS (lipsă HSTS), scurgere de `Referer` cross-origin.
- **Relevanță**: măsuri de întărire ieftine, recomandate ca bază pentru orice API web.
- **Implementare**: middleware în `Program.cs` — `X-Content-Type-Options: nosniff`,
  `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`,
  `Permissions-Policy` restrictiv, `Content-Security-Policy` (doar non-Development —
  Swagger UI folosește un `<script>` inline pe care CSP l-ar bloca), `UseHsts()`
  (doar non-Development). Headerele se pun la trimiterea răspunsului, deci apar și pe erorile 500.
- **Testare**: manual, verificat header-ele în răspuns via curl/browser devtools.
- **Limitări**: CSP e dezactivat complet în Development; nu există `Strict-Transport-Security`
  în dev (normal, dev rulează pe HTTP local).

### 4.13. Refresh token în cookie HttpOnly și CSRF (SR-07)
- **Problemă**: un refresh token citibil din JS poate fi furat prin XSS; un cookie poate
  aduce CSRF.
- **Relevanță**: refresh token-ul e valabil 30 de zile.
- **Implementare**: cookie `openkey_rt` (`HttpOnly; Secure; SameSite=None; Path=/api/auth`),
  rotit la refresh și revocat la logout. API-ul autorizează prin `Authorization: Bearer`, pe
  care un site străin nu îl poate trimite; răspunsul la refresh e protejat de CORS.
- **Testare**: `LogoutCommandHandlerTests` + verificare manuală în browser.
- **Limitări**: access token-ul (15 min) e în `localStorage`; un site străin poate cel mult
  declanșa un logout.

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

### 4.15. Verificare în doi pași — TOTP (SR-12)
- **Problemă**: o parolă furată ajunge pentru a intra în cont.
- **Relevanță**: conturile conțin date personale și cereri de contact.
- **Implementare**: cod de 6 cifre, secret criptat cu Data Protection, token intermediar de
  5 minute, cod nereutilizabil, blocarea contului după 5 coduri greșite.
- **Testare**: `MfaTests`.
- **Limitări**: opțional pentru utilizator; fără coduri de recuperare.

### 4.16. RBAC și audit de securitate (SR-13)
- **Problemă**: acces neautorizat la funcțiile de admin și lipsa urmelor după un incident.
- **Relevanță**: importul și raportările sunt funcții privilegiate.
- **Implementare**: policy `AdminOnly` pe `AdminController`, plus verificarea rolului din DB la
  import. `DbSecurityAuditLogger` scrie în `SecurityAuditLogs` loginurile, blocările, resetările
  de parolă, evenimentele MFA și accesul interzis (cu IP și user agent).
- **Testare**: `SecurityAuditAndRbacTests`.
- **Limitări**: rolul se atribuie manual în DB; rolul din JWT rămâne valabil până la expirare
  (15 min); fără retenție sau alertare pe audit.

### 4.17. Tratarea globală a erorilor (SR-14)
- **Problemă**: mesajele de excepție pot dezvălui detalii interne.
- **Relevanță**: API-ul e public.
- **Implementare**: `GlobalExceptionHandler` întoarce `ProblemDetails` generic cu `traceId`;
  `InvalidBodyFilter` răspunde 400 la JSON lipsă sau malformat.
- **Testare**: `GlobalExceptionHandlerTests`, `ExceptionStatusCodeMapperTests`,
  `SafeProblemDetailsFactoryTests`, `InvalidBodyFilterTests`.
- **Limitări**: `traceId`-ul se caută manual în loguri.

### 4.18. Dependențe și imagini Docker
- **Problemă**: biblioteci cu vulnerabilități cunoscute.
- **Relevanță**: aplicația depinde de sute de pachete npm și NuGet.
- **Implementare**: Dependabot săptămânal + alerte; `npm audit`, verificare NuGet și Trivy
  blocante pe HIGH/CRITICAL în CI; Trivy blocant pe imagini la deploy; imaginea Web nu conține npm.
- **Testare**: la scanarea din 2026-11-25 ambele imagini aveau 0 vulnerabilități HIGH/CRITICAL
  reparabile.
- **Limitări**: Trivy ignoră vulnerabilitățile fără fix disponibil (`ignore-unfixed`).

---

## 5. Ce nu e implementat încă

Vezi `SECURITY_TODO.md` pentru lista completă.
