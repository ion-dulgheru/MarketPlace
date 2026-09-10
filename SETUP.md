# Setup proiect – Real Estate Marketplace

Ghid pentru echipă: pași de la `git clone` / `git pull` până la aplicația care rulează
și se conectează la baza de date.

---

## 1. Ce trebuie instalat pe calculator (o singură dată)

| Tool | Versiune | Verificare |
|------|----------|------------|
| .NET SDK | **10.0** | `dotnet --version` |
| PostgreSQL | **16+** (merge și 14/15) | `psql --version` |
| EF Core CLI (`dotnet-ef`) | **10.x** | `dotnet ef --version` |

### .NET 10 SDK
https://dotnet.microsoft.com/download/dotnet/10.0 – instalați „SDK", nu doar „Runtime".

### PostgreSQL
https://www.postgresql.org/download/ – la instalare rețineți parola pusă pentru
userul `postgres`. Lăsați portul implicit **5432**.

### EF Core CLI
```bash
dotnet tool install --global dotnet-ef --version 10.*
```
Dacă mai era instalat de la un proiect vechi:
```bash
dotnet tool update --global dotnet-ef --version 10.*
```
> După instalare, **închideți și redeschideți terminalul** ca să se actualizeze PATH-ul.

---

## 2. Ia ultimele modificări

```bash
git pull
```

---

## 3. Configurează conexiunea la baza de date

Connection string-ul implicit este în `App.WebApi/appsettings.json`:

```
Host=localhost;Port=5432;Database=marketplace_db;Username=postgres;Password=Imobil1
```

**Dacă parola ta de `postgres` este alta**, NU modifica `appsettings.json` (se
comite în git). Suprascrie local cu user-secrets:

```bash
cd App.WebApi
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=marketplace_db;Username=postgres;Password=PAROLA_TA"
cd ..
```

Baza de date `marketplace_db` **nu trebuie creată manual** – o creează pasul 5
(migrarea) automat dacă nu există.

---

## 4. Restore + build

```bash
dotnet restore
dotnet build
```

Build-ul trebuie să treacă pe toate cele 6 proiecte.

---

## 5. Aplică migrările (creează tabelele în baza de date)

```bash
dotnet ef database update --project App.Persistence --startup-project App.WebApi
```

Asta creează baza `marketplace_db` (dacă lipsește) și tabelul `Users` din
migrarea `InitialCreate`.

Verificare rapidă în psql:
```bash
psql -U postgres -d marketplace_db -c "\dt"
```
Ar trebui să vezi tabelele `Users` și `__EFMigrationsHistory`.

---

## 6. Rulează API-ul

```bash
dotnet run --project App.WebApi
```

- Swagger: `https://localhost:<port>/swagger`
- Health check: `https://localhost:<port>/health` → `{ "status": "healthy" }`

Portul apare în consolă la pornire (`Now listening on: https://localhost:xxxx`).

---

## Comenzi utile pe parcurs

| Ce vrei | Comandă |
|---------|---------|
| Adaugi o migrare nouă | `dotnet ef migrations add NumeMigrare --project App.Persistence --startup-project App.WebApi` |
| Aplici migrările | `dotnet ef database update --project App.Persistence --startup-project App.WebApi` |
| Ștergi ultima migrare (neaplicată) | `dotnet ef migrations remove --project App.Persistence --startup-project App.WebApi` |
| Resetezi complet baza | `dotnet ef database drop --project App.Persistence --startup-project App.WebApi` apoi `database update` |
| Rulezi API-ul | `dotnet run --project App.WebApi` |
| Rulezi cu reload la salvare | `dotnet watch --project App.WebApi` |

---

## Probleme frecvente

**`dotnet ef` nu este recunoscut**
Nu ai instalat tool-ul sau nu ai restartat terminalul. Vezi pasul 1.

**`Npgsql.NpgsqlException: 28P01: password authentication failed`**
Parola din connection string nu e corectă. Vezi pasul 3 (user-secrets).

**`Npgsql.NpgsqlException: Connection refused` / `10061`**
Serviciul PostgreSQL nu rulează. Pornește-l din Services (Windows) sau
`pg_ctl start`.

**Build pică pe `App.Infrastructure` cu `IConfiguration could not be found`**
Lipsesc pachetele de configurare din `App.Infrastructure.csproj`. Adaugă:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
</ItemGroup>
```

**Migrarea zice `relation already exists`**
Baza are deja tabele dintr-o rulare veche. `dotnet ef database drop ...` apoi
`database update`.
