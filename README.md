# Real Estate Marketplace

A web platform where property owners and agencies publish sale or rental listings that
are visible publicly and immediately, with no moderation. Visitors search and filter
listings freely; creating an account is only required to contact an owner. The owner
manually marks a listing as sold or rented.

This document is the **use case catalogue** for the MVP: the scope, the data model it
implies, the design decisions applied throughout, and every user-facing scenario with
its flow, error paths, data and endpoint.

---

## Scope

The platform supports a **single account type** (no multi-agent roles or permissions).
An account is a `User`; agencies are just a `User` with an `isAgency` flag rather than a
separate entity.

### Core features

| # | Feature |
|---|---------|
| 1 | Publish a listing (sale or rent), edit it, and manually mark it sold / rented |
| 2 | Public search and filtering (type, price, location, rooms, m²) — no account needed |
| 3 | Send a contact request to the owner / agency — requires an account |

Sign-up and login are infrastructure that feature 3 depends on; they are not counted as
a separate feature.

### Out of scope for this MVP

| Excluded | Reason |
|----------|--------|
| Rental management (payments, payment history) | Deferred to a later phase |
| AI price estimation | Later stage, gated by a separate checklist |
| Real online payments | A contact is not an in-platform transaction |
| In-app messaging | Contact form / phone number only |
| Mobile app | Web only |
| Reviews / ratings | Not planned |
| Interactive map | Text / dropdown filtering only |
| Identity / ownership verification (KYC) | Listings are accepted without verification |
| Admin moderation / approval queue | Listings go live immediately |
| Multi-agent agency accounts (roles, permissions) | A single `User` with an `isAgency` flag |

### Known limitation, stated not hidden

Without moderation, any registered account can publish instantly — including duplicates
or spam. This is an acceptable simplification for a student project, not for a real
public marketplace. Fake or duplicate listings are out of scope for this MVP, in the
same spirit as the disclaimer used for simulated payments.

---

## Data model (preview)

| Entity | Notes |
|--------|-------|
| `Listing` | `type` (Sale / Rent); rent-only fields nullable (e.g. price per month); `status` (Active / Sold / Rented) — no `PendingApproval`, since new listings start as `Active` |
| `User` | `isAgency` (bool) instead of a separate `Agency` entity |
| `ContactRequest` | `listingId`, `fromUserId`, `message`, `createdAt` — a single record, not a conversation thread |
| `Photo` | Only the URL and metadata; the file itself lives in object storage |
| `Favorite` | `(userId, listingId)` pair |
| `ListingReport` | `listingId`, `reason` from a fixed category list |
| `CaptchaVerification` | Optional audit log only (see UC-20) |

### Decisions applied throughout

- **Identifiers are `uuid`**, never a numeric `id`.
- **Photos are stored as files** in file / object storage, never as a database blob —
  only the URL, size and content type live in the `Photo` table.
- **Price** (`amount`, `currency`, `priceType`) and **address**
  (`postCode`, `street`, `number`, `city`, `sector`) are each modelled as one grouped
  field set, not scattered individually.
- **Ownership checks happen inside the query**, not after loading the record. A record
  owned by someone else is reported as `404 Not Found`, never `403 Forbidden`.
- **The owner identity comes from the access token**, never from a client-supplied id.
- **Soft delete**: deleting a listing sets `deletedAt`; the row is never physically
  removed. `status` and `deletedAt` are independent.

---

## Use case catalogue

| # | Use case | Actor | Endpoint |
|---|----------|-------|----------|
| 1 | Register | Visitor | `POST /api/auth/register` |
| 2 | Sign in | Registered user | `POST /api/auth/login` |
| 3 | Publish a new listing | Owner / agency | `POST /api/listings` |
| 4 | Edit / delete a listing | Listing author | `PUT /api/listings/{uuid}` |
| 5 | Mark listing sold / rented | Owner | `PUT /api/listings/{uuid}/status` |
| 6 | List my own listings | Owner / agency | `GET /api/listings?mine=true` |
| 7 | Search listings with filters | Visitor | `GET /api/listings` |
| 8 | View listing detail | Visitor | `GET /api/listings/{uuid}` |
| 9 | Manage listing photos | Owner | `POST` / `DELETE /api/listings/{uuid}/photos` |
| 10 | Send a contact request | Signed-in user | `POST /api/listings/{uuid}/contact-requests` |
| 11 | View received contact requests | Listing owner | `GET /api/listings/{uuid}/contact-requests` |
| 12 | Mark a contact request as read | Listing owner | `PUT /api/contact-requests/{uuid}/status` |
| 13 | Password recovery | Any user | `POST /api/auth/password-reset-requests` · `POST /api/auth/password-resets` |
| 14 | Change password | Signed-in user | `PATCH /api/users/me/password` |
| 15 | Update my profile | Signed-in user | `PUT /api/users/me` |
| 16 | Favorite / unfavorite a listing | Signed-in user | `PUT /api/listings/{uuid}/favorites` |
| 17 | View my favorite listings | Signed-in user | `GET /api/users/me/favorites` |
| 18 | Report a listing | Signed-in user | `POST /api/listings/{uuid}/reports` |
| 19 | View public agency profile | Visitor | `GET /api/user/{uuid}` |
| 20 | Verify CAPTCHA (anti-bot check) | System | Embedded in Register and Report |
| 21 | Verify email address | Newly registered user | `POST /api/auth/email-verifications` |

---

### UC-1 · Register

| | |
|---|---|
| **Actor** | Any visitor (unauthenticated) |
| **Precondition** | None |

**Main flow**

1. The user opens the sign-up form.
2. Enters email, password and display name.
3. Submits the form.
4. The system checks that the email is not already in use.
5. The system hashes the password.
6. The system creates the user account.
7. The system confirms that the account was created.

**Alternatives and errors** — Email already registered → `409 Conflict`; invalid email
format; password below the minimum length; missing required field.

**Result** — A new `User` record is created.

**Data** — `string email`, `string passwordHash` (derived server-side, never sent by the
client), `string displayName`.

**Endpoint** — `POST /api/auth/register` → `201`

---

### UC-2 · Sign in

| | |
|---|---|
| **Actor** | Registered user |
| **Precondition** | Has an account |

**Main flow**

1. Enters email and password.
2. Submits.
3. The system looks up the user by email.
4. The system compares the password hash.
5. The system issues an access token.
6. The frontend stores the token.
7. The user is redirected to the main page.

**Alternatives and errors** — Wrong email or password → `401 Unauthorized`. The response
is identical for a wrong password and for a non-existent email, so the two cannot be
told apart.

**Result** — The client holds a valid access token for subsequent requests.

**Data** — `string email`, `string password` (verified against the hash, not persisted).

**Endpoint** — `POST /api/auth/login` → `200`

---

### UC-3 · Publish a new listing

| | |
|---|---|
| **Actor** | Property owner or agency (authenticated) |
| **Precondition** | Signed in |

**Main flow**

1. Selects "Create a new listing".
2. Fills the form: `type` (Sale / Rent), price (`amount`, `currency`), address
   (`postCode`, `street`, `number`, `city`, `sector`), rooms, area, description.
3. Submits.
4. The system validates required fields and that price > 0.
5. The system creates the listing with `status = Active` and `owner = current user`
   (taken from the token).
6. The system confirms creation and shows the detail page.

**Alternatives and errors** — Missing required field; price ≤ 0; not signed in → `401`.

**Result** — A new `Listing` is created with status `Active`, visible immediately in
public search (no moderation).

**Data** — `enum type` (Sale / Rent); price group — `decimal amount`, `string currency`,
`enum priceType`; address group — `string postCode`, `string street`, `string number`,
`string city`, `string sector`; `int rooms`, `decimal area`, `string description`;
`guid ownerId` (from the token, never from the request body).

**Endpoint** — `POST /api/listings` → `201`

---

### UC-4 · Edit / delete a listing

| | |
|---|---|
| **Actor** | Owner (author) of the listing |
| **Precondition** | Signed in; `ownerId = current user`, checked inside the query — not after loading the record |

**Main flow — edit**

1. The owner opens their listing.
2. Changes one or more fields.
3. Submits.
4. The system verifies ownership.
5. The system updates the record.

**Main flow — delete (soft)**

1. The owner selects "Delete listing".
2. The system verifies ownership.
3. The system sets `deletedAt` — the row is never physically removed.
4. The listing disappears from search and from "My listings" but remains in the database.

**Alternatives and errors** — Not the owner → `404`, never `403` (someone else's record
is simply "not found"); listing already deleted → `404`; invalid field values on edit.

**Result** — Either the listing's fields are updated, or `deletedAt` is set. `status` and
`deletedAt` are independent — a deleted listing keeps whatever status it had.

**Data** — Edit flow: any subset of typed fields from the `Listing` schema above.
Delete flow: `guid uuid` only.

**Endpoint** — `PUT /api/listings/{uuid}` → `200` (edit) · `DELETE /api/listings/{uuid}`
→ `204` (soft delete)

---

### UC-5 · Mark listing sold / rented

| | |
|---|---|
| **Actor** | Owner |
| **Precondition** | Signed in, owner of the listing, listing is `Active` |

**Main flow**

1. The owner opens their listing.
2. Selects "Mark as sold / rented".
3. The system verifies ownership.
4. The system updates `status`.

**Alternatives and errors** — Not the owner → `404`; listing already `Sold` / `Rented` →
`400` (no-op transition rejected); listing deleted → `404`.

**Result** — `status` changes to `Sold` or `Rented`; the listing stays visible on its
detail page but is excluded from default search results.

**Data** — `guid uuid`, `enum status` (Sold / Rented).

**Endpoint** — `PUT /api/listings/{uuid}/status` → `204`

---

### UC-6 · List my own listings

| | |
|---|---|
| **Actor** | Owner or agency, signed in |
| **Precondition** | Signed in |

**Main flow**

1. Opens "My listings".
2. The system resolves the owner identity from the access token, never from a
   client-supplied id.
3. The system returns all of the caller's listings, including `Sold` / `Rented`, and
   excluding deleted ones.

**Alternatives and errors** — Not signed in → `401`.

**Result** — A paged list of the caller's own listings, any status except deleted.

**Data** — `int page`, `int pageSize`.

**Endpoint** — `GET /api/listings?mine=true` → `200`

---

### UC-7 · Search listings with filters

| | |
|---|---|
| **Actor** | Any visitor, no account |
| **Precondition** | None |

**Main flow**

1. The visitor sets filters (type, price range, city / sector, rooms).
2. The system returns matching `Active`, non-deleted listings, paged.

**Alternatives and errors** — No results → empty state, not an error.

**Result** — A paged list of public listings matching the filters.

**Data** — `enum type`, `decimal priceMin`, `decimal priceMax`, `string city`,
`int rooms`, `int page`.

**Endpoint** — `GET /api/listings` → `200`

---

### UC-8 · View listing detail

| | |
|---|---|
| **Actor** | Any visitor, no account |
| **Precondition** | Listing exists, not deleted |

**Main flow**

1. The visitor opens a listing from the search results.
2. The system returns the full details and photos.

**Alternatives and errors** — Listing not found or deleted → `404`.

**Result** — The full listing detail is shown, including all photos.

**Data** — `guid uuid`.

**Endpoint** — `GET /api/listings/{uuid}` → `200`

---

### UC-9 · Manage listing photos

| | |
|---|---|
| **Actor** | Owner |
| **Precondition** | Signed in, owner of the listing |

**Main flow — add photo**

1. The owner selects "Add photo".
2. Uploads an image file.
3. The system validates the real file content type (not just the extension) and the
   size limit.
4. The system stores the file in file / object storage — cloud object storage if the
   project continues past the defence — and saves only the URL, size and content type
   in the database.

**Main flow — remove photo**

1. The owner selects a photo.
2. The system verifies ownership via the parent listing.
3. The system deletes the database row and the underlying file.

**Alternatives and errors** — Unsupported file type; file too large; not the owner →
`404`; more than 10 photos on one listing → rejected.

**Result** — A `Photo` row is created or removed; the listing's photo count is updated.

**Data** — `guid listingId`, `file file` (validated), `long sizeBytes`,
`string contentType`, `int order`.

**Endpoint** — `POST /api/listings/{uuid}/photos` → `201` ·
`DELETE /api/listings/{uuid}/photos/{photoUuid}` → `204`

---

### UC-10 · Send a contact request

| | |
|---|---|
| **Actor** | Any signed-in user |
| **Precondition** | Signed in; listing exists and is `Active` |

**Main flow**

1. The user opens a listing.
2. Fills in a short message.
3. Submits.
4. The system checks that the listing exists and is `Active`.
5. The system saves the request with `fromUserId` taken from the token.

**Alternatives and errors** — Listing not found / not `Active` → `404`; not signed in →
`401`; empty message → `400`.

**Result** — A `ContactRequest` is saved with status `Unread`, visible to the listing's
owner.

**Data** — `guid listingUuid`, `string message`.

**Endpoint** — `POST /api/listings/{uuid}/contact-requests` → `201`

---

### UC-11 · View received contact requests

| | |
|---|---|
| **Actor** | Owner of the listing |
| **Precondition** | Signed in, owner |

**Main flow**

1. The owner opens "Requests" for a listing.
2. The system returns all requests where `listing.ownerId = current user`.

**Alternatives and errors** — Not the owner → `404`.

**Result** — A list of contact requests for that listing.

**Data** — `guid listingUuid`.

**Endpoint** — `GET /api/listings/{uuid}/contact-requests` → `200`

---

### UC-12 · Mark a contact request as read

| | |
|---|---|
| **Actor** | Owner of the referenced listing |
| **Precondition** | Signed in, owner |

**Main flow**

1. The owner opens a request.
2. The system marks it `Read`.

**Alternatives and errors** — Neither the sender nor the listing owner → `404`.

**Result** — `status` changes to `Read`.

**Data** — `guid contactRequestUuid`.

**Endpoint** — `PUT /api/contact-requests/{uuid}/status` → `204`

---

### UC-13 · Password recovery

| | |
|---|---|
| **Actor** | Any user who forgot their password |
| **Precondition** | The account may or may not exist — not revealed to the caller either way |

**Main flow**

1. The user requests recovery by email.
2. The system issues a time-limited reset token and emails it (a console stub in local
   development).
3. The user opens the link and sets a new password.
4. The system verifies the token and updates the password hash.

**Alternatives and errors** — Token expired or invalid → `400`; the response is identical
whether or not the email exists, for the same reason as sign-in.

**Result** — The password hash is updated.

**Data** — Request step: `string email`. Reset step: `string token`,
`string newPassword`.

**Endpoint** — `POST /api/auth/password-reset-requests` → `202` ·
`POST /api/auth/password-resets` → `200`

---

### UC-14 · Change password

| | |
|---|---|
| **Actor** | Signed-in user |
| **Precondition** | Signed in |

**Main flow**

1. The user enters the current and the new password.
2. The system verifies the current password.
3. The system updates the hash.

**Alternatives and errors** — Current password wrong → `400`.

**Result** — The password hash is updated.

**Data** — `string currentPassword`, `string newPassword`.

**Endpoint** — `PATCH /api/users/me/password` → `204`

---

### UC-15 · Update my profile

| | |
|---|---|
| **Actor** | Signed-in user |
| **Precondition** | Signed in |

**Main flow**

1. The user edits `displayName` (and other editable profile fields).
2. Submits.
3. The system updates the record.

**Alternatives and errors** — Invalid value.

**Result** — The `User` record is updated.

**Data** — `string displayName`.

**Endpoint** — `PUT /api/users/me` → `200`

---

### UC-16 · Favorite / unfavorite a listing

| | |
|---|---|
| **Actor** | Signed-in user |
| **Precondition** | Signed in; listing exists, not deleted |

**Main flow**

1. The user clicks the favorite icon on a listing.
2. The system creates (or removes) a `Favorite` row for `(userId, listingId)`.

**Alternatives and errors** — Listing not found / deleted → `404`; already favorited →
no-op, not an error.

**Result** — The favorite is added or removed.

**Data** — `guid listingUuid`.

**Endpoint** — `PUT /api/listings/{uuid}/favorites` → `201`

---

### UC-17 · View my favorite listings

| | |
|---|---|
| **Actor** | Signed-in user |
| **Precondition** | Signed in |

**Main flow**

1. The user opens "Favorites".
2. The system returns the listings joined from the user's `Favorite` rows, excluding
   deleted listings.

**Result** — A paged list of favorited listings.

**Data** — `int page`.

**Endpoint** — `GET /api/users/me/favorites` → `200`

---

### UC-18 · Report a listing

| | |
|---|---|
| **Actor** | Signed-in user |
| **Precondition** | Signed in; listing exists |

**Main flow**

1. The user selects "Report" on a listing.
2. Chooses a reason.
3. Submits.
4. The system saves the report.
5. The system sends a notification to the platform operator.

**Alternatives and errors** — Listing not found → `404`; duplicate report by the same
user → rejected or no-op.

**Result** — A `ListingReport` row is saved. **Honest limitation, stated not hidden:**
there is no moderation workflow at this MVP — the report is a database row, checked
manually if needed, in the same spirit as the disclaimer used for simulated payments.

**Data** — `guid listingUuid`, `string reason` (from a fixed category list — e.g. Spam /
Fraud / Duplicate).

**Endpoint** — `POST /api/listings/{uuid}/reports` → `201`

---

### UC-19 · View public agency profile

| | |
|---|---|
| **Actor** | Any visitor |
| **Precondition** | The target user has `isAgency = true` |

**Main flow**

1. The visitor opens an agency's public profile from a listing.
2. The system returns the public fields (`displayName`, `isAgency`, member since) and
   reuses the existing listings endpoint filtered by that owner.

**Alternatives and errors** — User not found or not an agency → `404`.

**Result** — The public profile is shown alongside that agency's active listings.

**Data** — `guid userUuid`.

**Endpoint** — `GET /api/user/{uuid}` → `200` (public fields only — no separate endpoint)

---

### UC-20 · Verify CAPTCHA (anti-bot check)

| | |
|---|---|
| **Actor** | System — triggered automatically inside **Register** and **Report a listing**, the two public write actions most exposed to bots |
| **Precondition** | The client has already obtained a token from the CAPTCHA provider's widget before submitting the form |

**Main flow**

1. The client renders the CAPTCHA widget from the provider (e.g. Google reCAPTCHA) on
   the form.
2. The user solves the challenge.
3. The client submits the form together with the captcha token.
4. The server calls the CAPTCHA provider's verification API with the token.
5. The provider returns a pass / fail result and a score.
6. If the check passes, the server proceeds with the original action (account creation,
   report submission); a verification attempt is logged either way.

**Alternatives and errors** — Token missing → `400`; token invalid or expired → `400`;
provider score below the threshold → `403` (treated as a bot); provider API unreachable
→ the request is rejected, never silently allowed through.

**Result** — A `CaptchaVerification` row is logged (passed or failed) and the underlying
action proceeds only if it passed.

> **Technical note.** A standard third-party CAPTCHA (reCAPTCHA / hCaptcha) needs no
> database table at all — the provider is stateless from the server's point of view, and
> one real-time verify call is enough. The `CaptchaVerification` table exists only if you
> want an audit log for rate-limiting or abuse analysis, not because CAPTCHA
> verification itself requires storage.

**Data** — `string captchaToken` (from the client); server-recorded — `string
ipAddress`, `bool passed`, `datetime verifiedAt`.

**Endpoint** — Not a dedicated endpoint. The check is embedded inside
`POST /api/auth/register` and `POST /api/listings/{uuid}/reports`, both of which require
a `captchaToken` field in the request body.

---

### UC-21 · Verify email address

| | |
|---|---|
| **Actor** | Newly registered user |
| **Precondition** | Account created via Register; email not yet verified |

**Main flow**

1. The system sends a verification email immediately after Register, containing a
   time-limited link with a token.
2. The user opens the link.
3. The server looks up the token.
4. The server checks that the token is valid and not expired or already used.
5. The server sets `User.emailVerifiedAt` to now and marks the token consumed.
6. The user is redirected to a confirmation page.

**Alternatives and errors** — Token invalid, expired, or already consumed → `400`; if the
user requests a new link, a fresh token is issued and the previous one is invalidated.

**Result** — `User.emailVerifiedAt` is set.

> **Open scope decision.** At this MVP, an unverified account can still publish listings
> and send contact requests — verification is informational only, not a gate. Blocking
> unverified accounts from those actions would change the precondition on UC-3 and UC-10
> and should be a deliberate decision, not a side effect of adding this use case.

**Data** — Request step: `string email`. Verify step: `string token`.

**Endpoint** — `POST /api/auth/email-verification-requests` → `202` (resend) ·
`POST /api/auth/email-verifications` → `200` (consume token)
