# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Build and Run
```bash
# Build the entire solution
dotnet build

# Run the web application (from solution root)
dotnet run --project WebApp/WebApp.csproj

# Run with Docker (development mode)
docker-compose up -d

# Access application at https://localhost:443 (Docker) or https://localhost:5001 (local)
# Swagger UI available at /swagger (Development only)
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test ./App.Test/App.Test.csproj
dotnet test ./Base.Tests/Base.Tests.csproj
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project App.DAL.EF --startup-project WebApp

# Apply migrations to database
dotnet ef database update --project App.DAL.EF --startup-project WebApp

# Remove last migration (if not applied)
dotnet ef migrations remove --project App.DAL.EF --startup-project WebApp
```

### Code Generation
```bash
# Scaffold MVC controller with views
dotnet aspnet-codegenerator controller -name <EntityName>Controller \
    -actions \
    -m App.Domain.Entities.<EntityName> \
    -dc AppDbContext \
    -outDir Controllers \
    --useDefaultLayout \
    --useAsyncActions \
    --referenceScriptLibraries \
    -f

# Scaffold API controller
dotnet aspnet-codegenerator controller -name <EntityName>Controller \
    -m App.Domain.<EntityName> \
    -dc AppDbContext \
    -outDir ApiControllers \
    -api \
    --useAsyncActions \
    -f
```

## Architecture Overview

BookHub is a .NET 8.0 ASP.NET Core MVC application — an online bookstore with forums, ratings, warehouse maps, and a REST API. It uses a clean, layered architecture.

### Solution Structure (18 projects)

The solution (`BookHub.sln`) is organized into three tiers:

1. **Base Layer** (Generic/Reusable — `Base.*`):
   - `Base.Domain` — Abstract base classes: `BaseEntityId`, `BaseEntityIdMetadata`, `BaseRefreshToken`
   - `Base.Contracts.Domain` — Domain interfaces: `IDomainEntityId`, `IDomainEntityMetadata`, `IDomainAppUser`, `IDomainAppUserId`
   - `Base.Contracts.DAL` — Generic repository interfaces: `IEntityRepository<T>`, `IUnitOfWork`, `IDalMapper`
   - `Base.Contracts.BLL` — Generic service interfaces: `IEntityService<T>`, `IBLL`, `IBLLMapper`
   - `Base.DAL.EF` — Generic EF Core implementations: `BaseEntityRepository`, `BaseUnitOfWork`, `BaseDalDomainMapper`
   - `Base.BLL` — Generic BLL implementations: `BaseBLL`, `BaseEntityService`
   - `Base.Tests` — Test infrastructure (test entities, test DbContext, HTML helpers)

2. **App Layer** (Application-Specific — `App.*`):
   - `App.Domain` — Domain entities and identity models
   - `App.Contracts.DAL` — `IAppUnitOfWork` + custom repository interfaces
   - `App.DAL.EF` — EF Core: `AppDbContext`, `AppUOW`, repositories, migrations
   - `App.Contracts.BLL` — `IAppBLL` + custom service interfaces
   - `App.BLL` — `AppBLL` + service implementations
   - `App.DTO` — API DTOs (versioned: `v1_0/`)
   - `App.BLL.DTO` — BLL-layer DTOs
   - `App.DAL.DTO` — DAL-layer DTOs
   - `App.Test` — Integration and unit tests
   - `Helpers` — JWT generation/validation utilities, JSON helpers

3. **WebApp** (Presentation):
   - `Controllers/` — 16 MVC controllers
   - `ApiControllers/` — 4 REST API controllers (versioned)
   - `Areas/` — Identity (Razor Pages) and Admin (placeholder)
   - `Hubs/` — SignalR hub for real-time discussions
   - `Infrastructure/` — Data seeding, email, extensions
   - `Helpers/` — AutoMapper profiles, file validation attributes
   - `Models/` & `ViewModels/` — View-specific models
   - `Views/` — Razor views for all entities

### Domain Entities

**Main Entities** (`App.Domain.Entities/`):
- `Book` — Tittle, Price, Description, ReleaseYear, imageData, PublisherId, SearchVector (NpgsqlTsVector)
- `Author` — Name, Age, Biography, imageData, SearchVector
- `Publisher` — Name, Description
- `Genre` — Name, Description, IsMainGenre
- `Warehouse` — Name, GpsX, GpsY, Country, Location (NetTopologySuite Point)
- `Discussion` — Tittle, Description, CreationTime, imageData, BookId?, GenreId?, AuthorId?, AppUserId
- `Topic` — Tittle, Content, CreationTime, AppUserId, DiscussionId
- `Message` — Content, CreationTime, AppUserId, TopicId
- `Rating` — Value (float), Comment, AppUserId, BookId
- `Purchase` — Value, Discount, CreationTime, AppUserId

**Junction Tables** (`App.Domain.Address_Tables/`):
- `BookAuthor` — BookId + AuthorId
- `BookGenre` — BookId + GenreId
- `BookWarehouses` — BookId + WarehouseId + Count + LastSupply (inventory tracking)
- `PurchasedBook` — BookId + PurchaseId + BookHasRead
- `UserSubscription` — AppUserId + BookId + CreationTime

**Identity** (`App.Domain.Identity/`):
- `AppUser` (extends IdentityUser<Guid>) — FirstName, LastName, AvatarImageData
- `AppRole` (extends IdentityRole<Guid>)
- `AppUserRole` (extends IdentityUserRole<Guid>)
- `AppRefreshToken` — RefreshToken, ExpirationDT, PreviousRefreshToken, PreviousExpirationDT

All entities inherit from `BaseEntityId` which implements `IDomainEntityId` with Guid-based IDs.

### Key Architectural Patterns

**Unit of Work + Repository:**
- `IAppUnitOfWork` exposes: Topics, Messages, Ratings, Users repositories
- Custom repositories (`ITopicRepository`, `IMessageRepository`, `IRatingRepository`) extend `IEntityRepository<T>` with domain-specific queries
- Other entities (Book, Author, etc.) are accessed directly via `AppDbContext` in MVC controllers

**Service Layer (BLL):**
- `IAppBLL` exposes: Messages, Ratings, Topics, Users services
- Services wrap repositories with BLL-layer mapping via `BaseEntityService<TDalDto, TBllDto, TRepository>`
- API controllers inject `IAppBLL`; MVC controllers inject `AppDbContext` directly for entities without custom repositories

**AutoMapper (3 profiles):**
- `App.DAL.EF.AutoMapperProfile` — Domain entities ↔ DAL DTOs
- `App.BLL.AutoMapperProfile` — DAL DTOs ↔ BLL DTOs
- `WebApp.Helpers.AutoMapperProfile` — BLL DTOs ↔ API DTOs

**Dependency Injection Flow (Program.cs):**
```
AppDbContext → IAppUnitOfWork (AppUOW) → IAppBLL (AppBLL)
```

### Controllers

**16 MVC Controllers** (`WebApp/Controllers/`):
- `HomeController` — Landing page with advanced full-text search, filtering, sorting
- `BooksController` — CRUD + availability view with warehouse maps
- `AuthorsController`, `PublishersController`, `GenresController`, `WarehousesController` — Entity CRUD
- `BooksAuthorsController`, `BooksGenresController`, `BooksWarehousesController` — Junction table management
- `DiscussionsController`, `TopicsController`, `MessagesController` — Forum with SignalR broadcasting
- `RatingsController` — Book ratings and reviews
- `PurchasesController`, `PurchasedBooksController` — Purchase/cart management
- `UsersSubscriptionsController` — User book subscriptions

**4 API Controllers** (`WebApp/ApiControllers/`):
- `Identity/AccountController` — Register, Login, RefreshToken, Logout (JWT-based)
- `MessagesController`, `TopicsController`, `RatingsController` — CRUD APIs
- Route pattern: `/api/v{version}/[controller]/[action]`

### Database Configuration

**PostgreSQL with PostGIS and Advanced Search:**
- Npgsql provider with `UseNetTopologySuite()` for geospatial data
- Full-text search: `tsvector` columns on Book and Author with GIN indexes
- Trigram indexes (`gin_trgm_ops`) for fuzzy text search
- DateTime values automatically converted to UTC in `SaveChangesAsync`
- PostGIS extension enabled in `OnModelCreating` for warehouse location Points

**Connection String:**
- Development: `Host=localhost;Port=7890;Database=bookhub;Username=postgres;Password=postgres`
- Docker: Uses `bookhub-sql-dev` service name as host
- Configured in `appsettings.json`, overridable via environment variables

**Migrations** (`App.DAL.EF/Migrations/`):
- `20240407172441_FOOBAR` — Initial schema
- `20251024112416_AddFullTextSearch` — tsvector columns and GIN indexes
- `20260211100741_AddWarehouseLocationAndBookWarehouseSupply` — PostGIS location + inventory tracking
- `20260211143827_AddWarehouseCountry` — Warehouse country field

### Authentication & Authorization

**Dual Authentication System:**
- Cookie-based for MVC UI (ASP.NET Core Identity)
- JWT Bearer for API endpoints
- Configured in `AuthenticationExtensions.AddAppAuthentication()`

**Identity Configuration:**
- Custom user: `AppUser`, Custom role: `AppRole`
- Cookie: `.AspNetCore.Identity.Application`, Secure=Always, SameSite=Lax
- JWT: HmacSha256 signing, ClockSkew=0, configurable expiration (default 120s)

**Roles:** Admin, User (seeded via `appsettings.json`)

### API Versioning

- `Asp.Versioning.Mvc` 8.1.0
- Default version: v1.0, deprecated: v0.9
- Format: `v{Major}.{Minor}`
- Swagger configured via `ConfigureSwaggerOptions` with Bearer security definition
- API DTOs in `App.DTO/v1_0/` with CreateInfo/UpdateInfo patterns

### Search System (HomeController)

Multi-strategy search with fallback chain:
1. PostgreSQL full-text search (`tsvector` + `to_tsquery`)
2. Trigram similarity matching (fuzzy search)
3. Case-insensitive LIKE with 3-character prefix matching
4. Filtering by: authors, genres, publishers, warehouses
5. Sorting by: price, year, rating
6. AJAX partial view support (`_SearchResults.cshtml`)

### Docker Setup

**Development (`docker-compose.yml`):**
- `sql` — PostgreSQL + PostGIS (`db.Dockerfile`) on port 7890, health checks
- `app` — `dotnet watch` with hot-reload, volume-mounted source, HTTPS cert

**Production (`docker-compose.prod.yml`):**
- `sql` — PostgreSQL + PostGIS, localhost-only binding (127.0.0.1:5432)
- `migrator` — EF Core bundle for migrations (`./artifacts/efbundle`)
- `app` — GHCR image, read-only filesystem, tmpfs for /tmp, security hardening:
  - `no-new-privileges`, `cap_drop: ALL`, file descriptor limits
  - Data protection keys on persistent volume

**Dockerfiles:**
- `Dockerfile` — Multi-stage production build (SDK → alpine chiseled runtime)
- `dev.Dockerfile` — Development with `dotnet watch`
- `db.Dockerfile` — PostgreSQL 16.4 with PostGIS 3

### Build Configuration (`Directory.Build.props`)

- Latest C# language version
- Nullable reference types enabled
- Warnings as errors: CS8600, CS8602, CS8603, CS8613, CS8618, CS8625
- Separate bin/obj paths: `obj/local/` (host) vs `obj/container/` (Docker)
- NuGet codegen packages excluded from runtime

### Data Seeding

- Configuration in `appsettings.json` under `SeedData` section
- JSON seed data files in `WebApp/Infrastructure/Data/SeedData/`:
  - `authors.json`, `books.json`, `genres.json`, `publishers.json`, `warehouses.json`
- Seed DTOs in `WebApp/Infrastructure/Data/SeedDTO/`
- Seeds: users with roles, publishers, authors (with images), genres, warehouses (with GPS coordinates), books (with author/genre/warehouse links)
- Executed via `app.SeedAsync()` in `Program.cs`

### Email Services

- MailKit for SMTP (`WebApp/Infrastructure/Email/MailKitEmailSender.cs`)
- Template service (`EmailTemplateService.cs`) with HTML templates:
  - `ConfirmEmail.html`, `ResetPassword.html`
- SMTP config in `appsettings.json` under `Smtp` (Gmail, StartTls)
- Registered as singletons: `IEmailSender`, `IEmailTemplateService`

### Session Management

- Distributed in-memory cache with 20-minute idle timeout
- HttpOnly, Essential, SameSite=Lax, Secure=Always cookies
- Used for shopping cart and user state

### SignalR Real-Time Communication

**Hub:** `WebApp/Hubs/DiscussionHub.cs`
- Endpoint: `/hubs/discussion`
- Requires authentication (`[Authorize]`)
- Group-based broadcasting:
  - `discussion_{id}` — new topics broadcast
  - `topic_{id}` — new messages broadcast
- Methods: `JoinDiscussion`, `LeaveDiscussion`, `JoinTopic`, `LeaveTopic`

**Broadcasting from Controllers:**
- `MessagesController` → `ReceiveMessage` to `topic_{id}` group
- `TopicsController` → `ReceiveTopic` to `discussion_{id}` group
- Always after successful database save

**Client:** `WebApp/wwwroot/js/discussion-hub.js`
- SignalR client 8.0.0 via CDN
- Auto-reconnect with exponential backoff (0, 2, 10, 30 seconds)
- Global instance: `window.discussionHub`

**Notification DTOs:** `WebApp/Models/SignalR/NewMessageNotification.cs`, `NewTopicNotification.cs`

### Frontend

**CSS** (`wwwroot/css/`): `style.css` (main), `theme.css` (dark/light), `auth.css`, `availability.css`, `discussions.css`, `discussion-details.css`, `topic-details.css`, `site.css`

**JavaScript** (`wwwroot/js/`): `site.js`, `theme.js`, `books.js`, `book-availability.js`, `book-title-size-handler.js`, `discussions-index.js`, `discussion-hub.js`, `nav/header/header-auth.js`, `nav/header/header-unauth.js`

**Libraries:** Bootstrap 5.3, jQuery 3.6, jQuery Validation, QRCode.js

**Fonts:** Gilroy (Light, Medium, SemiBold)

**Theme:** Cookie-based dark/light toggle via `data-theme` HTML attribute and CSS custom properties

**Shared Views:** `_Layout.cshtml` (dual layout: authenticated/unauthenticated with admin dropdown), `_LoginPartial.cshtml`, `_ThemeToggle.cshtml`, `_ToastPartial.cshtml`, `_ValidationScriptsPartial.cshtml`

### File Validation

Custom attributes in `WebApp/Helpers/Validation/File/`:
- `AllowedExtensionsAttribute` — Validates file upload extensions
- `MaxFileSizeAttribute` — Validates file upload size

### Health Checks

- Endpoint: `/health`
- Used by Docker health checks
- Returns HTTP 200 when application is healthy

## Important Conventions

### Naming Patterns
- Entities: `App.Domain.Entities.<Name>` (main) or `App.Domain.Address_Tables.<Name>` (junction)
- Repository interfaces: `I<Entity>Repository` (e.g., `ITopicRepository`)
- Service interfaces: `I<Entity>Service` (e.g., `IMessageService`)
- Implementations drop the `I` prefix
- DTOs: same class name across layers (`Message` in DAL.DTO, BLL.DTO, App.DTO)
- API DTOs: `<Entity>CreateInfo`, `<Entity>UpdateInfo` for write operations

### Entity Relationships
- Many-to-many via junction tables with explicit entity classes
- Junction tables have their own Guid ID (inherit `BaseEntityId`)
- `BookWarehouses` includes inventory data (Count, LastSupply) — not a pure junction
- User-owned entities implement `IDomainAppUser<AppUser>`

### Image Storage
- Book covers and author portraits stored as `byte[]` (`imageData` property)
- Seeded from `wwwroot/img/books/` and `wwwroot/img/authors/` directories

### Areas
- **Identity** — Full ASP.NET Core Identity Razor Pages (login, register, 2FA, password reset, account management)
- **Admin** — Placeholder (empty views, `_ViewImports` and `_ViewStart` only)

### Test Projects
- `App.Test` — Integration tests using `CustomWebApplicationFactory` with in-memory database. Tests: API happy flow, MVC happy flow, repository/service tests, controller tests
- `Base.Tests` — Base class unit tests and shared test helpers (`HtmlClientExtension`, `HtmlHelpers`)

### Key NuGet Packages
- `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.2 + `.NetTopologySuite`
- `AutoMapper` 13.0.1
- `Asp.Versioning.Mvc` 8.1.0
- `MailKit` 4.13.0
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.3
- `Swashbuckle.AspNetCore` 6.5.0
- `System.IdentityModel.Tokens.Jwt` 7.1.2
