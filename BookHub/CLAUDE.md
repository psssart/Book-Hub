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

### SignalR Real-Time Communication

**Overview:**
- ASP.NET Core 8.0 built-in SignalR for WebSocket-based real-time updates
- JavaScript client library 8.0.0 loaded via CDN
- Enables instant message and topic updates without page refresh
- Progressive enhancement approach - forms work without JavaScript

**Hub Architecture:**
- **Single unified hub**: `WebApp/Hubs/DiscussionHub.cs`
- Hub endpoint: `/hubs/discussion`
- Requires authentication via `[Authorize]` attribute
- Supports both Cookie (MVC) and JWT Bearer (API) authentication

**Group Management Strategy:**
- **Discussion-level groups**: `discussion_{discussionId}` - broadcasts new topics to all viewers of a discussion
- **Topic-level groups**: `topic_{topicId}` - broadcasts new messages to all viewers of a topic
- Groups prevent unnecessary broadcasts to unrelated clients
- Auto-rejoin groups on reconnection (handled by client-side manager)

**Broadcasting from Controllers:**

When creating new messages or topics, controllers broadcast to SignalR groups:

```csharp
// In MessagesController - inject IHubContext<DiscussionHub>
private readonly IHubContext<DiscussionHub> _hubContext;

// After saving message
var notification = new NewMessageNotification { /* ... */ };
await _hubContext.Clients
    .Group($"topic_{topicId}")
    .SendAsync("ReceiveMessage", notification);
```

```csharp
// In TopicsController - inject IHubContext<DiscussionHub>
private readonly IHubContext<DiscussionHub> _hubContext;

// After saving topic
var notification = new NewTopicNotification { /* ... */ };
await _hubContext.Clients
    .Group($"discussion_{discussionId}")
    .SendAsync("ReceiveTopic", notification);
```

**Notification Models:**
- `WebApp/Models/SignalR/NewMessageNotification.cs` - Message broadcast DTO
- `WebApp/Models/SignalR/NewTopicNotification.cs` - Topic broadcast DTO
- Keep DTOs minimal (only essential data for UI updates)

**Client-Side Integration:**

Connection manager: `WebApp/wwwroot/js/discussion-hub.js`
- Global instance: `window.discussionHub`
- Automatic reconnection with exponential backoff (0, 2, 10, 30 seconds)
- Methods: `initialize()`, `joinDiscussion(id)`, `joinTopic(id)`, `onReceiveMessage(callback)`, `onReceiveTopic(callback)`

Usage in views:
```javascript
// Topics/Details.cshtml - Join topic and listen for messages
await window.discussionHub.initialize();
await window.discussionHub.joinTopic(topicId);
window.discussionHub.onReceiveMessage(function(notification) {
    // Update UI with new message
});

// Discussions/Details.cshtml - Join discussion and listen for topics
await window.discussionHub.initialize();
await window.discussionHub.joinDiscussion(discussionId);
window.discussionHub.onReceiveTopic(function(notification) {
    // Update UI with new topic
});
```

**Configuration in Program.cs:**
```csharp
// Register SignalR services (line ~88)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// CORS configuration requires AllowCredentials for SignalR (line ~67)
.AllowCredentials(); // Required for SignalR

// Map hub endpoint (line ~162)
app.MapHub<WebApp.Hubs.DiscussionHub>("/hubs/discussion");
```

**Key Implementation Files:**
- `WebApp/Hubs/DiscussionHub.cs` - SignalR hub with group management
- `WebApp/Models/SignalR/` - Notification DTOs
- `WebApp/wwwroot/js/discussion-hub.js` - Client connection manager
- `WebApp/Controllers/MessagesController.cs` - Broadcasts messages
- `WebApp/Controllers/TopicsController.cs` - Broadcasts topics
- `WebApp/Views/Topics/Details.cshtml` - Real-time message UI
- `WebApp/Views/Discussions/Details.cshtml` - Real-time topic UI
- `WebApp/Views/Shared/_Layout.cshtml` - SignalR script includes

**Important Notes:**
- All SignalR broadcasts happen AFTER successful database saves (hybrid approach)
- Broadcasting is done through controllers, NOT directly from views
- Maintains BLL pattern - no breaking changes to business logic
- Connection logging enabled via `ILogger<DiscussionHub>`
- Network failures auto-reconnect; groups are automatically rejoined
- Works in production with proper CORS configuration (`AllowCredentials`)

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
