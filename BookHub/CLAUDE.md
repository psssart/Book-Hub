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

# Access application at https://localhost:5001
# Swagger UI available at https://localhost:5001/swagger (Development only)
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

BookHub is a .NET 8.0 ASP.NET Core MVC application using a clean, layered architecture with the following structure:

### Layer Hierarchy (Base → App → WebApp)

The solution uses a **two-tier inheritance pattern** where generic base classes (`Base.*`) are extended by application-specific classes (`App.*`):

1. **Base Layer** (Generic/Reusable):
   - `Base.Contracts.Domain` - Base domain interfaces (IDomainEntityId)
   - `Base.Contracts.DAL` - Generic repository interfaces (IEntityRepository, IUnitOfWork)
   - `Base.Contracts.BLL` - Generic service interfaces (IEntityService, IBLL)
   - `Base.DAL.EF` - Generic EF Core repository implementations
   - `Base.BLL` - Generic business logic service implementations

2. **App Layer** (Application-Specific):
   - `App.Domain` - Domain entities (Book, Author, Genre, Publisher, etc.)
   - `App.Contracts.DAL` - Application repository contracts (IAppUnitOfWork, custom repositories)
   - `App.DAL.EF` - EF Core implementation (AppDbContext, AppUOW, migrations)
   - `App.Contracts.BLL` - Application business logic contracts (IAppBLL, custom services)
   - `App.BLL` - Business logic services (AppBLL implementation)
   - `App.DTO` - Data transfer objects for API
   - `App.BLL.DTO` & `App.DAL.DTO` - Layer-specific DTOs

3. **WebApp Layer**:
   - `WebApp` - ASP.NET Core MVC project with:
     - `Controllers/` - MVC controllers for web UI
     - `ApiControllers/` - RESTful API controllers (versioned)
     - `Areas/` - Identity and Admin areas
     - `Infrastructure/` - Configuration, extensions, email services
     - `ViewModels/` - View-specific models

### Key Architectural Patterns

**Unit of Work + Repository Pattern:**
- `IAppUnitOfWork` provides access to all repositories
- Each repository inherits from `IEntityRepository<TEntity>` in Base layer
- Custom repositories (e.g., `ITopicRepository`, `IMessageRepository`) extend base repository for specific needs

**Service Layer Pattern:**
- `IAppBLL` provides access to all services (mirrors UnitOfWork at BLL level)
- Services inherit from `IEntityService<TEntity>` which extends `IEntityRepository<TEntity>`
- Services add business logic on top of repository operations

**AutoMapper for Object Mapping:**
- Three AutoMapper profiles: `App.DAL.EF.AutoMapperProfile`, `App.BLL.AutoMapperProfile`, `WebApp.Helpers.AutoMapperProfile`
- Maps between Domain → DAL DTOs → BLL DTOs → API DTOs

**Dependency Injection Flow:**
- `Program.cs` registers: `AppDbContext` → `IAppUnitOfWork` (AppUOW) → `IAppBLL` (AppBLL)
- Controllers inject `IAppBLL` to access services
- Services use mappers to transform between layers

### Database Configuration

**PostgreSQL with Advanced Search:**
- Uses Npgsql provider for PostgreSQL
- Full-text search with `tsvector` columns on Book and Author entities
- Trigram indexes (`gin_trgm_ops`) for fuzzy text search on titles and names
- DateTime values automatically converted to UTC in `SaveChangesAsync`

**Connection String:**
- Development: `Host=localhost;Port=7890;Database=bookhub;Username=postgres;Password=postgres`
- Docker: Uses service name `bookhub-sql-dev` as host
- Configured in `appsettings.json` and overridable via environment variables

### Authentication & Authorization

**Dual Authentication System:**
- Cookie-based authentication for MVC UI (ASP.NET Core Identity)
- JWT Bearer authentication for API endpoints
- Both configured in `AuthenticationExtensions.AddAppAuthentication()`

**Identity Configuration:**
- Custom user: `AppUser` (in `App.Domain.Identity`)
- Custom role: `AppRole`
- Custom user-role: `AppUserRole`
- Refresh tokens: `AppRefreshToken`

**JWT Settings:**
- Configured in `appsettings.json` under `JWT` section
- Tokens validated with issuer, audience, and signing key
- ClockSkew set to zero for exact expiration

### API Versioning

- Uses `Asp.Versioning.Mvc` and `Asp.Versioning.Mvc.ApiExplorer`
- Default version: v1.0
- Version format: `v{version}` (e.g., v1, v2)
- Swagger UI documents all API versions
- Configured in `ApiExtensions.AddVersioningAndSwagger()`

### Docker Setup

**Development Compose (`docker-compose.yml`):**
- PostgreSQL service on port 7890
- App service with hot-reload (`dotnet watch`)
- Volume mounts for live code updates
- HTTPS certificate from `/https/bookhub.pfx`
- Health checks for both services

**Build Configuration (`Directory.Build.props`):**
- Separate bin/obj directories for container vs local development
- Prevents file locking conflicts between host and container
- Nullable reference types enabled with strict warnings as errors
- Container builds use `/obj/container` and `/bin/container`
- Local builds use `/obj/local` and `/bin/local`

### Data Seeding

- Configured in `appsettings.json` under `SeedData` section
- Seeds users (admin and regular users), publishers, authors, genres, warehouses, and books
- Executed on app startup via `app.SeedAsync()` in `Program.cs`
- Implementation in `WebApp/Infrastructure/Data/`

### Email Services

- Uses MailKit for email sending
- SMTP configuration in `appsettings.json` under `Smtp` section
- Template-based emails stored in `WebApp/Infrastructure/Email/EmailTemplates/`
- Services: `IEmailSender` (MailKitEmailSender) and `IEmailTemplateService`

## Important Conventions

### Naming Patterns

- Repository interfaces: `I<Entity>Repository` (e.g., `ITopicRepository`)
- Service interfaces: `I<Entity>Service` (e.g., `IMessageService`)
- Implementations match interface names without 'I' prefix
- DTOs follow layer naming: `<Entity>DTO` in respective DTO projects

### Database Entity Relationships

- Many-to-many relationships use junction tables (e.g., `BookGenre`, `BookAuthor`, `BookWarehouses`)
- All entities inherit from `IDomainEntityId` (Guid-based IDs)
- Soft deletes supported via base repository methods with `userId` parameter

### API Controllers

- Located in `WebApp/ApiControllers/`
- Organized by resource (e.g., `MessagesController`, `RatingsController`)
- Identity-related endpoints in `ApiControllers/Identity/`
- Use DTOs from `App.DTO` for request/response models
- Inject `IAppBLL` and use AutoMapper for transformations

### Areas

- **Identity**: ASP.NET Core Identity UI (login, register, etc.)
- **Admin**: Administrative functionality (placeholder for admin controllers/views)

### Health Checks

- Endpoint: `/health`
- Used by Docker health checks
- Returns HTTP 200 when application is healthy
