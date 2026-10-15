# Vehicle Manager (CreditWorks Software Engineer Assignment)

A web application for managing vehicles and weight categories. A vehicle's category is calculated automatically from its weight, and category ranges are validated so there are never gaps or overlaps.

- **Backend:** C#, ASP.NET Core (controllers), Entity Framework Core, Microsoft SQL Server
- **Frontend:** React + TypeScript + Tailwind CSS (Vite, pnpm)

## Setup

### Required software

| Software | Notes |
|---|---|
| .NET SDK 10 | `dotnet --version` |
| SQL Server 2019 or later | Express is fine. Any reachable instance works. |
| Node.js 20.19+ (or 22.12+) | For the React client |
| pnpm | `npm install -g pnpm` |
| EF Core CLI | `dotnet tool install --global dotnet-ef` |

**SQL Server on macOS/Linux:** SQL Server does not run natively on macOS. One option is a Docker container:

```bash
docker run --platform linux/amd64 \
  -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=YourStrong!Passw0rd' \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

On Windows, SQL Server Express or LocalDB works without Docker.

### 1. Configure the connection string

No credentials are stored in the repository. Set the connection string with .NET user-secrets (run from the repo root, adjusting server and password to your instance):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=localhost,1433;Database=VehiclesDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True' --project src/Vehicles.Api
```

`TrustServerCertificate=True` is for local development only.

### 2. Create the database

```bash
dotnet ef database update --project src/Vehicles.Infrastructure --startup-project src/Vehicles.Api
```

This creates `VehiclesDb`, the tables, and the seed data (5 manufacturers, 3 categories).

### 3. Build

```bash
dotnet build
cd client && pnpm install && cd ..
```

### 4. Run

API (from the repo root, listens on http://localhost:5221):

```bash
dotnet run --project src/Vehicles.Api --launch-profile http
```

Client (second terminal):

```bash
cd client
pnpm dev
```

Open http://localhost:5173. The client calls the API at `http://localhost:5221` by default. To change that, set `VITE_API_URL` when starting the client.

### 5. Run the tests

```bash
dotnet test tests/Vehicles.Tests.Unit          # no database needed
dotnet test tests/Vehicles.Tests.Integration   # needs SQL Server and the user-secret above
dotnet test                                    # both
```

There are 59 tests (33 unit, 26 integration). The integration tests use the same user-secret but a separate database, `VehiclesDb_Test`, which they create from the migrations. Development data is not touched.

## Design notes

### Architecture

Four projects under `src/`, with dependencies pointing inward:

| Project | Responsibility |
|---|---|
| `Vehicles.Api` | Controllers, exception handling, CORS (presentation) |
| `Vehicles.Application` | `VehicleService`, `CategoryService`, `ManufacturerService`, DTOs (use cases) |
| `Vehicles.Infrastructure` | `VehiclesDbContext`, EF Core migrations (data access) |
| `Vehicles.Domain` | Entities, `CategoryResolver`, `CategoryConfigurationValidator`, `VehicleValidator` (business rules, no dependencies) |

Flow: `React -> Controller -> Service -> Domain rules + DbContext -> SQL Server`.

Services use `VehiclesDbContext` directly rather than through repository interfaces. EF Core's DbContext already acts as a unit of work, so an extra layer would add code without benefit at this size. The trade-off is that service behaviour is tested with integration tests against a real database rather than unit tests with mocks.

### Database design

| Table | Notes |
|---|---|
| `Manufacturers` | Seeded with Mazda, Mercedes, Honda, Ferrari, Toyota. Vehicles reference it by foreign key, so manufacturers are not hard-coded anywhere. Unique name. |
| `Categories` | `Name`, `Icon` (a key such as `truck`), `MinWeight decimal(10,2)`. Unique name and unique `MinWeight`. |
| `Vehicles` | `OwnerName`, `ManufacturerId` (FK, restrict delete), `Year`, `Weight decimal(10,2)`. |

- **Category is calculated, not stored.** A vehicle has no category column. Changing category definitions therefore updates every vehicle immediately, and the category can never be inconsistent with the configured ranges.
- **Icons** are stored as a key. The frontend maps each key to an inline SVG, so adding a new icon is a frontend change only.
- **Check constraints** (`Weight > 0`, `MinWeight >= 0`) and unique indexes act as a database-level safety net behind the validators.

### Category calculation

`CategoryResolver.Resolve(weight, categories)` returns the category with the highest `MinWeight` that is less than or equal to the weight. The vehicle list loads all categories once and resolves each vehicle in memory.

### Category boundary rules

- Only each category's **lower bound** is stored. A category's upper bound is the next category's lower bound, and the highest category has no upper bound.
- Ranges are **half-open**: a category covers its `MinWeight` up to but **not including** the next category's `MinWeight`. A vehicle of exactly 500.00 kg is therefore Medium only, never Light.
- With this model, gaps and overlaps cannot exist for a valid configuration. The validator enforces the remaining rules:
  - At least one category exists.
  - The lowest category starts at 0 kg.
  - No two categories share a start weight.
  - Names are unique (case-insensitive), and name and icon are required.
  - Start weights are not negative and have at most two decimal places.
- Every create, update or delete loads all categories, applies the change in memory, validates the **resulting set**, and saves only if it is valid. Invalid changes (for example deleting the lowest category) return a 400 and leave the data unchanged.

Example: with Light 0, Medium 500, Heavy 2500, a 2200 kg vehicle is Medium. After moving Heavy's start to 2000 it shows as Heavy, with no vehicle data changed.

### Assumptions

- Weights are in kilograms with at most two decimal places and must be positive.
- A "sensible" year is 1886 (first petrol car) up to the current year.
- Maximum weight is 99,999,999.99 kg (the `decimal(10,2)` limit).
- Users edit a category's **start weight** only. The upper bound is derived. This keeps ranges gap-free and overlap-free by construction.
- The frontend is a single-page app with three views (vehicle list, add vehicle, categories).

### Known limitations and deliberate omissions

- No authentication or authorisation.
- No concurrency control: two people editing categories at the same moment could overwrite each other. The unique indexes still prevent duplicate start weights.
- No paging on the vehicle list.
- No edit or delete for vehicles (not required by the assignment).
- The API runs over plain HTTP in development (HTTPS redirection removed). Production should use HTTPS.
- `TrustServerCertificate=True` and the `sa` account are used for local development only.
- Vehicle sorting happens in the database. Category resolution happens in memory, which is fine at this scale.
- Browser-side validation is minimal. The server is the source of truth for validation.

### What I would improve for production

- Authentication and role-based access to category administration.
- Optimistic concurrency (row version) on categories.
- Paging, searching and filtering on the vehicle list.
- A dedicated least-privilege database login and proper certificates.
- Structured logging, health checks and a CI pipeline running both test suites.
- Frontend component tests, and a shared API contract (for example OpenAPI) to generate client types.
