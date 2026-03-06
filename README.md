# 📚 BookHub
https://bookhub.null-land.com/
> A stylish and feature-rich online bookstore with forums, ratings, and warehouse maps.

---

## ✨ What is BookHub?

BookHub is a full-stack web application where users can browse, purchase, and discuss books — all in one place. Think of it as a bookstore meets Goodreads meets a community forum.

---

## 🚀 Features

📖 **Book Catalog** — Browse books with advanced search, filtering by genre/author/publisher, and sorting by price, year, or rating

🔍 **Smart Search** — Full-text search with fuzzy matching so typos won't stop you from finding what you need

💬 **Discussion Forums** — Create discussions about books, genres, or authors with real-time messaging powered by SignalR

⭐ **Ratings & Reviews** — Rate books and leave reviews for the community

🗺️ **Warehouse Maps** — See book availability across warehouses with interactive maps and geospatial data

🛒 **Purchases & Cart** — Buy books and track your reading history

🔔 **Subscriptions** — Subscribe to books and stay updated

🌗 **Dark/Light Theme** — Toggle between themes with a single click

👤 **User Accounts** — Full authentication with registration, login, 2FA, and password recovery

🔐 **Admin Panel** — Manage the entire catalog, users, and warehouses

📡 **REST API** — Versioned API with JWT authentication and Swagger documentation

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| 🖥️ Backend | .NET 8.0, ASP.NET Core MVC |
| 🗄️ Database | PostgreSQL 16 + PostGIS 3 |
| 🔄 ORM | Entity Framework Core 8 |
| 🔑 Auth | ASP.NET Core Identity + JWT Bearer |
| ⚡ Real-time | SignalR |
| 📧 Email | MailKit (SMTP) |
| 🎨 Frontend | Bootstrap 5.3, jQuery, Razor Views |
| 🐳 DevOps | Docker & Docker Compose |

---

## 📐 Architecture

BookHub follows a clean layered architecture across **18 projects**:

```
┌─────────────────────────────────────────┐
│              🌐 WebApp                  │
│    MVC Controllers · API · SignalR      │
├─────────────────────────────────────────┤
│              ⚙️ App.BLL                 │
│       Business Logic & Services         │
├─────────────────────────────────────────┤
│            💾 App.DAL.EF                │
│    Entity Framework · Repositories      │
├─────────────────────────────────────────┤
│            📦 App.Domain                │
│     Entities · Identity · DTOs          │
├─────────────────────────────────────────┤
│             🧱 Base.*                   │
│   Generic Contracts & Implementations   │
└─────────────────────────────────────────┘
```

---

## 🏁 Quick Start

### 🐳 With Docker (recommended)

```bash
git clone https://github.com/psssart/Book-Hub.git
cd Book-Hub/BookHub
docker-compose up -d
```

Open **https://localhost** and you're ready to go! 🎉

### 💻 Running Locally

```bash
git clone https://github.com/psssart/Book-Hub.git
cd Book-Hub/BookHub
docker-compose up -d sql          # start the database
dotnet ef database update --project App.DAL.EF --startup-project WebApp
dotnet run --project WebApp/WebApp.csproj
```

Open **https://localhost:5001** 🎉

> 📖 For detailed setup, database configuration, and all available commands, see the [technical README](BookHub/README.md).

---

## 🧪 Testing

```bash
dotnet test
```

Integration tests use an in-memory database — no external dependencies needed.

---

## 📄 License

See [LICENSE.txt](LICENSE.txt) for details.
