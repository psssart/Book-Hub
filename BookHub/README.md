# BookHub

A modern **ASP.NET Core MVC** web application (built on **.NET 8.0**) for managing books, authors, genres, publishers, and more. BookHub provides a clean, layered architecture, complete with authentication, API endpoints, and Docker support to help you get started quickly and deploy with confidence.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running Locally](#running-locally)
- [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Running Tests](#running-tests)
- [Contributing](#contributing)
- [License](#license)

## Features

- **CRUD Operations** for Books, Authors, Genres, Publishers, Warehouses, Purchases, and User Subscriptions
- **User Authentication & Authorization** via ASP.NET Core Identity
- **Discussion & Messaging** modules for user interaction
- **Ratings & Reviews** to rate and discuss books
- **RESTful API Endpoints** alongside the MVC UI, versioned and documented with Swagger
- **Data Seeding** for initial setup
- **Layered Architecture**: Domain, Data Access (EF Core), Business Logic, and WebApp layers

## Architecture

BookHub follows a clean, domain-driven design with the following layers:

1. **App.Domain** — Domain entities and interfaces
2. **App.DAL.EF** — Entity Framework Core implementation for data access
3. **App.BLL** — Business logic and services
4. **WebApp** — ASP.NET Core MVC project with controllers, views, and API controllers
5. **Infrastructure** — Configuration extensions, Swagger options, and data seeding

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Entity Framework Core Tools](https://docs.microsoft.com/ef/core/cli/dotnet)
- [Docker](https://www.docker.com/) & [Docker Compose]


## Getting Started
1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/BookHub.git
   cd BookHub/BookHub
2. **Configure the database connection**
   * Edit `appsettings.json` or set environment variables:
   ```
   "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookHubDb;Trusted_Connection=True;"
    }
3. **Apply migrations and seed data**
    ```bash 
    dotnet ef database update --project App.DAL.EF
4. **Generate SSL certificates**
    ```bash
   openssl req -x509 -newkey rsa:4096 -sha256 -nodes -keyout key.pem -out cert.pem -days 365 -subj "/CN=localhost" && \
   openssl pkcs12 -export -out https/bookhub.pfx -inkey key.pem -in cert.pem -passout pass:MyPassword && \
   rm key.pem cert.pem

### Docker Deployment
Build and run with single command:
```bash
docker-compose up -d
```

### Running Locally
From the solution root:
```bash
dotnet build
dotnet run --project WebApp/WebApp.csproj
```
Navigate to https://localhost:5001 in your browser.

### API Documentation
API endpoints are documented and available at runtime via Swagger UI:
https://localhost:5001/swagger

### Running Tests
Execute all unit tests:
```bash
dotnet test ./App.Test/App.Test.csproj
dotnet test ./Base.Tests/Base.Tests.csproj
```


## Useful Commands
* **Add EF Core Migration**:

  ```bash
  dotnet ef migrations add InitialCreate --project App.DAL.EF --startup-project WebApp
  ```
### Code Generation
To scaffold MVC controllers, use:
```bash
dotnet aspnet-codegenerator controller -name PublishersController \
    -actions \
    -m App.Domain.Entities.Publisher \
    -dc AppDbContext \
    -outDir Controllers \
    --useDefaultLayout \
    --useAsyncActions \
    --referenceScriptLibraries \
    -f
```
To scaffold API controllers, use:
```bash
dotnet aspnet-codegenerator controller -name ContestsController \
    -m App.Domain.Contest \
    -dc AppDbContext \
    -outDir ApiControllers \
    -api \
    --useAsyncActions \
    -f
```
