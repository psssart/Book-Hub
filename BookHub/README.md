# BookHub

An online bookstore built with .NET 8.0 and ASP.NET Core MVC — featuring forums, ratings, warehouse maps with geospatial data, and a versioned REST API. Clean layered architecture with 18 projects.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Docker Deployment](#docker-deployment)
- [Testing](#testing)
- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Database](#database)
- [Authentication & Authorization](#authentication--authorization)
- [REST API](#rest-api)
- [Search System](#search-system)
- [Real-Time Communication](#real-time-communication)
- [Email Services](#email-services)
- [Frontend](#frontend)
- [Useful Commands](#useful-commands)
- [Key Dependencies](#key-dependencies)

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Entity Framework Core Tools](https://docs.microsoft.com/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)
- [Docker](https://www.docker.com/) & Docker Compose
- PostgreSQL 16+ with PostGIS 3 (provided via Docker, or install locally)

## Getting Started

### Running with Docker (recommended)

```bash
docker-compose up -d
```

The app will be available at **https://localhost:443**. The database, migrations, and seed data are handled automatically.

### Running Locally

1. **Start a PostgreSQL instance** (or use the Docker one on port 7890):
   ```bash
   docker-compose up -d sql
   ```

2. **Apply migrations:**
   ```bash
   dotnet ef database update --project App.DAL.EF --startup-project WebApp
   ```

3. **Generate an HTTPS certificate** (if not already present):
   ```bash
   openssl req -x509 -newkey rsa:4096 -sha256 -nodes \
       -keyout key.pem -out cert.pem -days 365 -subj "/CN=localhost" && \
   openssl pkcs12 -export -out https/bookhub.pfx \
       -inkey key.pem -in cert.pem -passout pass:MyPassword && \
   rm key.pem cert.pem
   ```

4. **Build and run:**
   ```bash
   dotnet build
   dotnet run --project WebApp/WebApp.csproj
   ```

5. Navigate to **https://localhost:5001**. Swagger UI is at `/swagger` (development only).

### Database Connection

Configured in `appsettings.json`, overridable via environment variables:

| Environment | Host | Port | Database |
|-------------|------|------|----------|
| Local | `localhost` | `7890` | `bookhub` |
| Docker | `bookhub-sql-dev` | `5432` | `bookhub` |

Default credentials: `postgres` / `postgres`

## Docker Deployment

### Development (`docker-compose.yml`)

| Service | Description |
|---------|-------------|
| `sql` | PostgreSQL + PostGIS (`db.Dockerfile`), port 7890, with health checks |
| `app` | `dotnet watch` with hot-reload, volume-mounted source, HTTPS cert |

### Production (`docker-compose.prod.yml`)

| Service | Description |
|---------|-------------|
| `sql` | PostgreSQL + PostGIS, localhost-only binding (`127.0.0.1:5432`) |
| `migrator` | EF Core bundle for migrations (`./artifacts/efbundle`) |
| `app` | GHCR image, read-only filesystem, tmpfs for `/tmp` |

Production hardening: `no-new-privileges`, `cap_drop: ALL`, file descriptor limits, data protection keys on a persistent volume.

### Dockerfiles

- `Dockerfile` — Multi-stage production build (SDK -> Alpine chiseled runtime)
- `dev.Dockerfile` — Development with `dotnet watch`
- `db.Dockerfile` — PostgreSQL 16.4 with PostGIS 3

## Testing

```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test ./App.Test/App.Test.csproj
dotnet test ./Base.Tests/Base.Tests.csproj
```

| Project | What it covers |
|---------|---------------|
| `App.Test` | Integration tests with `CustomWebApplicationFactory` (in-memory DB) — API happy flow, MVC happy flow, repository/service tests, controller tests |
| `Base.Tests` | Base class unit tests and shared test helpers (`HtmlClientExtension`, `HtmlHelpers`) |

---

## Architecture

### Solution Structure (18 projects)

The solution is organized into three tiers:

#### Base Layer — Generic/Reusable (`Base.*`)

| Project | Purpose |
|---------|---------|
| `Base.Domain` | Abstract base classes: `BaseEntityId`, `BaseEntityIdMetadata`, `BaseRefreshToken` |
| `Base.Contracts.Domain` | Domain interfaces: `IDomainEntityId`, `IDomainEntityMetadata`, `IDomainAppUser` |
| `Base.Contracts.DAL` | Generic repository interfaces: `IEntityRepository<T>`, `IUnitOfWork`, `IDalMapper` |
| `Base.Contracts.BLL` | Generic service interfaces: `IEntityService<T>`, `IBLL`, `IBLLMapper` |
| `Base.DAL.EF` | Generic EF Core implementations: `BaseEntityRepository`, `BaseUnitOfWork` |
| `Base.BLL` | Generic BLL implementations: `BaseBLL`, `BaseEntityService` |
| `Base.Tests` | Test infrastructure (test entities, test DbContext, HTML helpers) |

#### App Layer — Application-Specific (`App.*`)

| Project | Purpose |
|---------|---------|
| `App.Domain` | Domain entities and identity models |
| `App.Contracts.DAL` | `IAppUnitOfWork` + custom repository interfaces |
| `App.DAL.EF` | EF Core: `AppDbContext`, `AppUOW`, repositories, migrations |
| `App.Contracts.BLL` | `IAppBLL` + custom service interfaces |
| `App.BLL` | `AppBLL` + service implementations |
| `App.DTO` | API DTOs (versioned under `v1_0/`) |
| `App.BLL.DTO` | BLL-layer DTOs |
| `App.DAL.DTO` | DAL-layer DTOs |
| `App.Test` | Integration and unit tests |
| `Helpers` | JWT generation/validation utilities, JSON helpers |

#### WebApp — Presentation

| Folder | Purpose |
|--------|---------|
| `Controllers/` | 16 MVC controllers |
| `ApiControllers/` | 4 REST API controllers (versioned) |
| `Areas/` | Identity (Razor Pages) and Admin (placeholder) |
| `Hubs/` | SignalR hub for real-time discussions |
| `Infrastructure/` | Data seeding, email, extensions |
| `Helpers/` | AutoMapper profiles, file validation attributes |
| `Models/` & `ViewModels/` | View-specific models |
| `Views/` | Razor views for all entities |

### Key Patterns

**Unit of Work + Repository** — `IAppUnitOfWork` exposes Topics, Messages, Ratings, and Users repositories. Custom repositories extend `IEntityRepository<T>` with domain-specific queries. Other entities are accessed directly via `AppDbContext` in MVC controllers.

**Service Layer (BLL)** — `IAppBLL` exposes Messages, Ratings, Topics, and Users services. Services wrap repositories with BLL-layer mapping. API controllers inject `IAppBLL`; MVC controllers inject `AppDbContext` directly for entities without custom repositories.

**AutoMapper** — Three profiles handle mapping between layers:
- `App.DAL.EF.AutoMapperProfile` — Domain entities <-> DAL DTOs
- `App.BLL.AutoMapperProfile` — DAL DTOs <-> BLL DTOs
- `WebApp.Helpers.AutoMapperProfile` — BLL DTOs <-> API DTOs

**DI flow in `Program.cs`:**
```
AppDbContext -> IAppUnitOfWork (AppUOW) -> IAppBLL (AppBLL)
```

### Controllers

**16 MVC Controllers** (`WebApp/Controllers/`):

| Controller | Responsibility |
|------------|---------------|
| `HomeController` | Landing page with full-text search, filtering, sorting |
| `BooksController` | CRUD + book availability view with warehouse maps |
| `AuthorsController`, `PublishersController`, `GenresController`, `WarehousesController` | Entity CRUD |
| `BooksAuthorsController`, `BooksGenresController`, `BooksWarehousesController` | Junction table management |
| `DiscussionsController`, `TopicsController`, `MessagesController` | Forum with SignalR broadcasting |
| `RatingsController` | Book ratings and reviews |
| `PurchasesController`, `PurchasedBooksController` | Purchase/cart management |
| `UsersSubscriptionsController` | User book subscriptions |

---

## Domain Model

### Main Entities (`App.Domain.Entities/`)

| Entity | Key fields |
|--------|-----------|
| `Book` | Title, Price, Description, ReleaseYear, imageData, PublisherId, SearchVector |
| `Author` | Name, Age, Biography, imageData, SearchVector |
| `Publisher` | Name, Description |
| `Genre` | Name, Description, IsMainGenre |
| `Warehouse` | Name, GpsX, GpsY, Country, Location (NetTopologySuite Point) |
| `Discussion` | Title, Description, CreationTime, imageData, BookId?, GenreId?, AuthorId? |
| `Topic` | Title, Content, CreationTime, AppUserId, DiscussionId |
| `Message` | Content, CreationTime, AppUserId, TopicId |
| `Rating` | Value (float), Comment, AppUserId, BookId |
| `Purchase` | Value, Discount, CreationTime, AppUserId |

### Junction Tables (`App.Domain.Address_Tables/`)

| Table | Keys | Extra data |
|-------|------|------------|
| `BookAuthor` | BookId + AuthorId | — |
| `BookGenre` | BookId + GenreId | — |
| `BookWarehouses` | BookId + WarehouseId | Count, LastSupply (inventory) |
| `PurchasedBook` | BookId + PurchaseId | BookHasRead |
| `UserSubscription` | AppUserId + BookId | CreationTime |

### Identity (`App.Domain.Identity/`)

| Class | Base | Extra fields |
|-------|------|-------------|
| `AppUser` | `IdentityUser<Guid>` | FirstName, LastName, AvatarImageData |
| `AppRole` | `IdentityRole<Guid>` | — |
| `AppUserRole` | `IdentityUserRole<Guid>` | — |
| `AppRefreshToken` | `BaseRefreshToken` | RefreshToken, ExpirationDT, PreviousRefreshToken |

All entities inherit from `BaseEntityId` (Guid-based IDs). Many-to-many relationships use explicit junction table entities with their own Guid IDs. Image data (book covers, author portraits) is stored as `byte[]`.

---

## Database

**PostgreSQL with PostGIS** — uses Npgsql with `UseNetTopologySuite()` for geospatial warehouse data.

### Full-Text Search

- `tsvector` columns on `Book` and `Author` with GIN indexes
- Trigram indexes (`gin_trgm_ops`) for fuzzy text matching

### Migrations (`App.DAL.EF/Migrations/`)

| Migration | Description |
|-----------|-------------|
| `20240407172441_FOOBAR` | Initial schema |
| `20251024112416_AddFullTextSearch` | tsvector columns and GIN indexes |
| `20260211100741_AddWarehouseLocationAndBookWarehouseSupply` | PostGIS location + inventory tracking |
| `20260211143827_AddWarehouseCountry` | Warehouse country field |

DateTime values are automatically converted to UTC in `SaveChangesAsync`.

### Data Seeding

Seed data lives in `WebApp/Infrastructure/Data/SeedData/` as JSON files: `authors.json`, `books.json`, `genres.json`, `publishers.json`, `warehouses.json`. Configuration is in `appsettings.json` under the `SeedData` section. Seeds include users with roles, publishers, authors (with images), genres, warehouses (with GPS coordinates), and books (with author/genre/warehouse links). Executed automatically via `app.SeedAsync()` in `Program.cs`.

---

## Authentication & Authorization

The application uses a **dual authentication system**:

| Channel | Method | Details |
|---------|--------|---------|
| MVC UI | Cookie-based | ASP.NET Core Identity, cookie `.AspNetCore.Identity.Application` |
| REST API | JWT Bearer | HmacSha256 signing, ClockSkew=0, default 120s expiration |

Configured in `AuthenticationExtensions.AddAppAuthentication()`.

**Roles:** `Admin` and `User` (seeded via `appsettings.json`).

**Identity pages** (under `Areas/Identity/`): login, register, 2FA, password reset, account management (full ASP.NET Core Identity Razor Pages).

---

## REST API

4 API controllers under `WebApp/ApiControllers/`, route pattern: `/api/v{version}/[controller]/[action]`

| Controller | Endpoints |
|------------|-----------|
| `Identity/AccountController` | Register, Login, RefreshToken, Logout |
| `MessagesController` | CRUD for messages |
| `TopicsController` | CRUD for topics |
| `RatingsController` | CRUD for ratings |

### Versioning

- Library: `Asp.Versioning.Mvc` 8.1.0
- Default version: **v1.0**, deprecated: v0.9
- Format: `v{Major}.{Minor}`
- DTOs in `App.DTO/v1_0/` with `CreateInfo`/`UpdateInfo` patterns for write operations

Swagger UI with Bearer security definition is available at `/swagger` in development.

---

## Search System

The `HomeController` implements a multi-strategy search with a fallback chain:

1. **PostgreSQL full-text search** — `tsvector` + `to_tsquery`
2. **Trigram similarity** — fuzzy matching for typos
3. **LIKE fallback** — case-insensitive with 3-character prefix matching

Additional features:
- Filter by authors, genres, publishers, warehouses
- Sort by price, year, rating
- AJAX partial view support (`_SearchResults.cshtml`)

---

## Real-Time Communication

SignalR hub at `/hubs/discussion` (requires authentication).

**Group-based broadcasting:**
- `discussion_{id}` groups receive new topic notifications
- `topic_{id}` groups receive new message notifications

Controllers broadcast after successful database saves. The client (`wwwroot/js/discussion-hub.js`) uses auto-reconnect with exponential backoff (0, 2, 10, 30 seconds).

---

## Email Services

- **MailKit** for SMTP delivery (`WebApp/Infrastructure/Email/MailKitEmailSender.cs`)
- HTML templates for `ConfirmEmail` and `ResetPassword` (`EmailTemplateService.cs`)
- SMTP configured in `appsettings.json` under `Smtp` (Gmail, StartTls)

---

## Frontend

| Category | Details |
|----------|---------|
| CSS framework | Bootstrap 5.3 |
| JavaScript | jQuery 3.6, jQuery Validation, QRCode.js |
| Fonts | Gilroy (Light, Medium, SemiBold) |
| Theme | Cookie-based dark/light toggle via `data-theme` attribute and CSS custom properties |

**Health check:** `/health` endpoint returns HTTP 200 (used by Docker health checks).

**Session:** Distributed in-memory cache, 20-minute idle timeout, HttpOnly/Secure/SameSite=Lax cookies (shopping cart and user state).

---

## Useful Commands

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project App.DAL.EF --startup-project WebApp

# Apply migrations
dotnet ef database update --project App.DAL.EF --startup-project WebApp

# Remove last migration (if not yet applied)
dotnet ef migrations remove --project App.DAL.EF --startup-project WebApp
```

### Code Generation

```bash
# Scaffold an MVC controller with views
dotnet aspnet-codegenerator controller -name <EntityName>Controller \
    -actions \
    -m App.Domain.Entities.<EntityName> \
    -dc AppDbContext \
    -outDir Controllers \
    --useDefaultLayout \
    --useAsyncActions \
    --referenceScriptLibraries \
    -f

# Scaffold an API controller
dotnet aspnet-codegenerator controller -name <EntityName>Controller \
    -m App.Domain.<EntityName> \
    -dc AppDbContext \
    -outDir ApiControllers \
    -api \
    --useAsyncActions \
    -f
```

### Build Configuration (`Directory.Build.props`)

- Latest C# language version with nullable reference types enabled
- Select nullability warnings treated as errors (CS8600, CS8602, CS8603, CS8613, CS8618, CS8625)
- Separate `bin`/`obj` paths for host (`obj/local/`) vs Docker (`obj/container/`)

---

## Key Dependencies

| Package | Version |
|---------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` (+ NetTopologySuite) | 8.0.2 |
| `AutoMapper` | 13.0.1 |
| `Asp.Versioning.Mvc` | 8.1.0 |
| `MailKit` | 4.13.0 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.3 |
| `Swashbuckle.AspNetCore` | 6.5.0 |
| `System.IdentityModel.Tokens.Jwt` | 7.1.2 |
