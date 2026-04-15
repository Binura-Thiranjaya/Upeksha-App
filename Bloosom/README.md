# Bloosom

Bloosom is an ASP.NET Core 9 Web API for managing a flower business backend: products, categories, event categories, event images, testimonials, orders, users, site settings, and embedded social content.

## Documentation

- Full project documentation: [`FULL_PROJECT_DOCUMENTATION.md`](FULL_PROJECT_DOCUMENTATION.md)
- Frontend API handoff: [`FRONTEND_API_HANDOFF.md`](FRONTEND_API_HANDOFF.md)
- Endpoint reference: [`API_DOCUMENTATION.md`](API_DOCUMENTATION.md)

## Contents
- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Key Runtime Behavior](#key-runtime-behavior)
- [Getting Started](#getting-started)
- [Database and EF Core](#database-and-ef-core)
- [Authentication and Roles](#authentication-and-roles)
- [API Documentation](#api-documentation)
- [Frontend Integration Notes](#frontend-integration-notes)
- [Troubleshooting](#troubleshooting)

## Overview

The project exposes a versioned REST API under `/api/v1` and uses EF Core with SQL Server for persistence. The app also serves uploaded files from `wwwroot/uploads`.

### Main features
- Product catalog management
- Category management
- Event category management with image uploads
- Testimonials and public testimonial submission
- Order management
- User management
- Site settings management
- Facebook embed management

## Tech Stack

- **Runtime:** .NET 9 / ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Auth:** JWT Bearer authentication
- **Password hashing:** BCrypt
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **API docs:** Swagger / OpenAPI
- **File storage:** Local filesystem (`wwwroot/uploads`)

## Project Structure

```text
Bloosom/
├── Program.cs
├── Bloosom.csproj
├── appsettings.json
├── appsettings.Development.json
├── API_DOCUMENTATION.md
├── Api/
│   └── Controllers/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   ├── Persistence/
│   └── Services/
└── Migrations/
```

### Important folders
- `Api/Controllers`: REST endpoints
- `Domain/Entities`: EF Core entities and base models
- `Infrastructure/Persistence`: `AppDbContext`
- `Infrastructure/Services`: file storage and password hashing services
- `Migrations`: EF Core migration files

## Key Runtime Behavior

### Database startup
On startup, the app:
1. builds the service container,
2. applies pending EF Core migrations with `Database.Migrate()`,
3. seeds a default admin user if the `Users` table is empty.

### Default admin seed
If no users exist, the app seeds:
- **Email:** `admin@floara.local`
- **Role:** `it_admin`
- **Password:** `Password123!`

> Change this immediately in production.

### File uploads
Uploaded event images are stored locally under:
- `wwwroot/uploads`

Returned file URLs are relative paths like:
- `/uploads/<generated-file>`

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server instance
- EF Core tools

### Configure the database
Update the connection string in `appsettings.Development.json` or `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=FloaraDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;TrustServerCertificate=True;"
  }
}
```

### Run the app
```powershell
cd C:\Users\BinuraMadoluwage\Downloads\Projects\Floora\Bloosom\Bloosom
dotnet restore
dotnet build
dotnet run
```

### Create or update migrations
```powershell
cd C:\Users\BinuraMadoluwage\Downloads\Projects\Floora\Bloosom\Bloosom
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Swagger
When running in Development mode, open:
- `https://localhost:<port>/swagger`

## Database and EF Core

### Entity base model
Most entities inherit from `BaseEntity`, which includes:
- `Id`
- `CreatedAt`
- `UpdatedAt`
- `IsDeleted`

### Soft status toggling
This project uses **toggle-based delete actions** for several entities.

Calling a `DELETE` endpoint now toggles `IsDeleted`:
- `false -> true`
- `true -> false`

This applies to:
- categories
- products
- event categories
- event images
- Facebook embeds
- testimonials

The API returns:

```json
{ "id": "guid", "isDeleted": true }
```

### EF migrations
The project already uses EF Core migrations. Startup applies them automatically, so the database schema stays aligned with the codebase.

## Authentication and Roles

### Authentication
JWT Bearer tokens are used for protected endpoints.

### Important note
There is currently **no login/token issuance endpoint** in the controller set provided. The frontend must receive a token from an external auth system or a future auth endpoint.

### Roles
- `it_admin`
- `admin`
- `manager`
- `staff`

Role access is enforced at controller/action level using `[Authorize]`.

## API Documentation

A dedicated API reference is available in:
- `API_DOCUMENTATION.md`

That file contains:
- endpoint list
- payload shapes
- example responses
- role requirements
- public vs protected routes
- frontend integration notes

## Current API Modules

### Categories
Manage floral/event categories.

### Products
Manage product listings and catalog data.

### EventCategories
Manage event-specific categories and image assets.

### FacebookEmbeds
Manage embedded Facebook content and page URL setting.

### Orders
View and update order status/payment status.

### Testimonials
Administer testimonials and allow public submissions.

### Settings
Manage site-wide configuration values.

### Users
List and create application users.

## Frontend Integration Notes

### General
- Send JWT in the `Authorization` header for protected routes.
- Use `application/json` for all JSON requests.
- Use `multipart/form-data` for event image uploads.

### Delete actions
Do **not** assume delete means permanent removal. It toggles `isDeleted`.

### Public endpoints
Use these without auth:
- `GET /api/v1/EventCategories`
- `GET /api/v1/FacebookEmbeds`
- `POST /api/v1/Testimonials/submit`

### Pagination
Most list endpoints support `page` and `pageSize`, but response envelopes differ slightly by controller.

### Image URLs
Returned upload paths are relative. Prefix them with your API host when rendering in the browser.

## Troubleshooting

### Build fails because the app is running
If `dotnet build` cannot overwrite `Bloosom.exe`, stop the currently running app first.

### EF warnings about decimal precision
You may see warnings for decimal fields such as:
- `Product.Price`
- `Order.SubTotal`
- `Order.DeliveryFee`
- `Order.Discount`
- `Order.GrandTotal`
- `OrderItem.UnitPrice`
- `OrderItem.Total`
- `OrderAddOn.Price`

These are not build blockers, but precision configuration in `AppDbContext` is recommended.

### Missing auth endpoint
If the frontend needs login, add a dedicated auth controller that returns JWT tokens.

## Suggested next improvements
- Add a login/auth controller
- Add explicit decimal precision config in `AppDbContext`
- Add OpenAPI annotations for richer Swagger docs
- Add DTO validation rules per endpoint
- Add permanent file retention/cleanup strategy for soft-deleted images

