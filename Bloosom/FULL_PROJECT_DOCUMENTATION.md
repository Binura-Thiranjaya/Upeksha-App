# Bloosom Full Project Documentation

This document is the full project handoff for backend and frontend developers.
It covers:
- project structure
- runtime behavior
- auth and roles
- every controller
- request DTOs
- response DTOs / payload shapes
- public endpoints
- delete-as-toggle behavior

---

## 1) Project Overview

**Bloosom** is an ASP.NET Core 9 Web API for managing a flower business backend.

### Main features
- product management
- category management
- event category management with image uploads
- testimonials management and public submission
- orders management
- users management
- site settings management
- Facebook embed management

### Tech stack
- .NET 9 / ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer authentication
- BCrypt password hashing
- FluentValidation
- AutoMapper
- Swagger / OpenAPI
- local file storage for uploads

---

## 2) Project Structure

```text
Bloosom/
├── Program.cs
├── Bloosom.csproj
├── appsettings.json
├── appsettings.Development.json
├── README.md
├── API_DOCUMENTATION.md
├── FRONTEND_API_HANDOFF.md
├── FULL_PROJECT_DOCUMENTATION.md
├── Api/
│   └── Controllers/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   ├── Persistence/
│   └── Services/
└── Migrations/
```

---

## 3) Runtime Behavior

### Startup flow
On startup the app:
1. builds the service container
2. applies pending EF Core migrations with `Database.Migrate()`
3. seeds a default admin user if no users exist

### Default seed user
- **Email:** `admin@floara.local`
- **Role:** `it_admin`
- **Password:** `Password123!`

> Change this immediately in production.

### Upload storage
Uploaded event images are stored under:
- `wwwroot/uploads`

Returned upload URLs are relative paths like:
- `/uploads/<generated-file>`

---

## 4) Base API Information

- **Base URL:** `https://<your-host>/api/v1`
- **Content type:** `application/json`
- **File uploads:** `multipart/form-data`
- **Auth header:** `Authorization: Bearer <jwt-token>`
- **Swagger:** `/swagger` in Development

### Important behavior: delete toggles status
For the main entities, `DELETE` does **not** remove data permanently.

Instead, it toggles `IsDeleted`:
- `false -> true`
- `true -> false`

Delete responses usually return:
```json
{ "id": "guid", "isDeleted": true }
```

### Public endpoints
These are anonymous:
- `GET /api/v1/EventCategories`
- `GET /api/v1/FacebookEmbeds`
- `POST /api/v1/Testimonials/submit`

### No login endpoint exists yet
There is currently no auth/login controller in the exposed code.
The frontend must get a JWT from another system or a future auth API.

---

## 5) Roles

Available roles:
- `it_admin`
- `admin`
- `manager`
- `staff`

### Role matrix
- `it_admin, admin, manager`
  - Categories
  - Products
  - EventCategories
  - FacebookEmbeds
  - Testimonials admin endpoints
- `it_admin, admin, staff`
  - Orders
- `it_admin`
  - Settings
  - Facebook page URL endpoints
- any authenticated user
  - `GET /api/v1/Users`

---

## 6) Shared Entity Models

### BaseEntity
Most entities inherit from `BaseEntity`:
- `Id`
- `CreatedAt`
- `UpdatedAt`
- `IsDeleted`

### User
```json
{
  "id": "guid",
  "fullName": "System Admin",
  "email": "admin@floara.local",
  "passwordHash": "...",
  "role": "it_admin",
  "isActive": true,
  "lastLogin": null,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### Category
```json
{
  "id": "guid",
  "name": "Birthday",
  "slug": "birthday",
  "sortOrder": 1,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### Product
```json
{
  "id": "guid",
  "name": "Rose Bouquet",
  "slug": "rose-bouquet",
  "price": 120.5,
  "description": "Short text",
  "longDescription": "Long text",
  "categoryId": "guid",
  "category": null,
  "isFeatured": true,
  "isAvailable": true,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### Order
```json
{
  "id": "guid",
  "orderNumber": "ORD-001",
  "customerName": "John",
  "customerEmail": "john@example.com",
  "customerPhone": "0771234567",
  "deliveryAddress": "Address",
  "deliveryCity": "Colombo",
  "deliveryDate": null,
  "notes": null,
  "subTotal": 1000,
  "deliveryFee": 200,
  "discount": 0,
  "grandTotal": 1200,
  "paymentMethod": "cash",
  "paymentStatus": "pending",
  "orderStatus": "new",
  "source": "website",
  "items": [],
  "addOns": [],
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### EventCategory / EventImage
```json
{
  "id": "guid",
  "name": "Weddings",
  "slug": "weddings",
  "description": "...",
  "icon": "fa-heart",
  "images": [],
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

```json
{
  "id": "guid",
  "eventCategoryId": "guid",
  "imageUrl": "/uploads/file.jpg",
  "caption": "Front stage",
  "sortOrder": 0,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### FacebookEmbed
```json
{
  "id": "guid",
  "url": "https://facebook.com/...",
  "type": "post",
  "label": "Promo",
  "eventCategory": "Weddings",
  "sortOrder": 0,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### Testimonial
```json
{
  "id": "guid",
  "customerName": "Jane",
  "rating": 5,
  "reviewText": "Great service",
  "customerImage": null,
  "isApproved": true,
  "isFeatured": false,
  "sourcePlatform": null,
  "sourceUrl": null,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### SiteSetting
```json
{
  "key": "site_name",
  "value": "Floora",
  "type": "string",
  "id": "guid",
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

---

## 7) Controller-by-Controller API Reference

---

## 7.1 ActivityLogController

### GET `/api/v1/ActivityLog`
- **Auth:** `it_admin|admin|manager`
- **Query params:**
  - `search` optional string
  - `section` optional string
  - `page` default `1`
  - `pageSize` default `50`, max `200`
- **Response:**
```json
{
  "items": [
    {
      "id": "guid",
      "action": "create",
      "section": "products",
      "detail": "Created product Rose Bouquet",
      "userId": null,
      "userName": null,
      "userRole": null,
      "timestamp": "2026-04-15T12:00:00Z",
      "createdAt": "2026-04-15T12:00:00Z",
      "updatedAt": "2026-04-15T12:00:00Z",
      "isDeleted": false
    }
  ],
  "totalCount": 1
}
```

---

## 7.2 CategoriesController

### GET `/api/v1/Categories`
- **Auth:** `it_admin|admin|manager`
- **Response:** `Category[]` where `isDeleted = false`

### POST `/api/v1/Categories`
- **Body DTO:** `CreateCategoryDto`
```json
{ "name": "Birthday", "sortOrder": 1 }
```
- **Response:** created `Category`

### PUT `/api/v1/Categories/{id}`
- **Body DTO:** `UpdateCategoryDto`
```json
{ "name": "Birthday Premium", "sortOrder": 2 }
```
- **Response:** `204 No Content`

### DELETE `/api/v1/Categories/{id}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

---

## 7.3 ProductsController

### GET `/api/v1/Products`
- **Auth:** `it_admin|admin|manager`
- **Query params:**
  - `search`
  - `categoryId`
  - `page` default `1`
  - `pageSize` default `20`, max `100`
- **Response:**
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Rose Bouquet",
      "slug": "rose-bouquet",
      "price": 120.5,
      "description": "Short text",
      "longDescription": "Long text",
      "categoryId": "guid",
      "categoryName": "Birthday",
      "isFeatured": true,
      "isAvailable": true,
      "images": [],
      "createdAt": "2026-04-15T12:00:00Z",
      "updatedAt": "2026-04-15T12:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### GET `/api/v1/Products/{id}`
- **Auth:** `it_admin|admin|manager`
- **Response:** single product projection

### POST `/api/v1/Products`
- **Body DTO:** `CreateProductDto`
```json
{
  "name": "Rose Bouquet",
  "price": 120.5,
  "description": "Short",
  "longDescription": "Long",
  "categoryId": "guid",
  "isFeatured": true,
  "isAvailable": true
}
```
- **Response:**
```json
{ "id": "guid" }
```

### PUT `/api/v1/Products/{id}`
- **Body DTO:** `UpdateProductDto`
- Same payload as create
- **Response:** `204 No Content`

### DELETE `/api/v1/Products/{id}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/Products/{id}/images`
- **Status:** scaffold only
- **Response:** `501 Not Implemented`

### DELETE `/api/v1/Products/{id}/images/{imageId}`
- **Status:** scaffold only
- **Response:** `501 Not Implemented`

---

## 7.4 EventCategoriesController

### GET `/api/v1/EventCategories`
- **Auth:** public
- **Response:** `EventCategory[]` including `images`

### POST `/api/v1/EventCategories`
- **Body DTO:** `CreateEventCategoryDto`
```json
{ "name": "Weddings", "description": "Event flowers", "icon": "fa-heart" }
```
- **Response:** created event category

### PUT `/api/v1/EventCategories/{id}`
- **Body DTO:** `UpdateEventCategoryDto`
```json
{ "name": "Weddings Premium", "description": "Updated", "icon": "fa-heart" }
```
- **Response:** `204 No Content`

### DELETE `/api/v1/EventCategories/{id}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/EventCategories/{id}/images`
- **Content type:** `multipart/form-data`
- **Fields:**
  - `file` required
  - `sortOrder` optional, default `0`
  - `caption` optional
- **Response:** created `EventImage`

### DELETE `/api/v1/EventCategories/{id}/images/{imageId}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

---

## 7.5 FacebookEmbedsController

### GET `/api/v1/FacebookEmbeds`
- **Auth:** public
- **Response:** `FacebookEmbed[]`

### POST `/api/v1/FacebookEmbeds`
- **Body DTO:** `CreateFacebookEmbedDto`
```json
{
  "url": "https://facebook.com/...",
  "type": "post",
  "label": "Promo",
  "eventCategory": "Weddings"
}
```
- **Response:** created embed

### PUT `/api/v1/FacebookEmbeds/{id}`
- **Body DTO:** same as create
- **Response:** `204 No Content`

### DELETE `/api/v1/FacebookEmbeds/{id}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### GET `/facebook-embeds/page-url`
- **Auth:** `it_admin`
- **Response:**
```json
{ "pageUrl": "https://facebook.com/your-page" }
```

### PUT `/facebook-embeds/page-url`
- **Auth:** `it_admin`
- **Body DTO:** `PageUrlDto`
```json
{ "pageUrl": "https://facebook.com/your-page" }
```
- **Response:** `204 No Content`

---

## 7.6 OrdersController

### GET `/api/v1/Orders`
- **Auth:** `it_admin|admin|staff`
- **Query params:**
  - `search` optional
  - `status` optional
  - `page` default `1`
  - `pageSize` default `20`, max `100`
- **Response:**
```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

### GET `/api/v1/Orders/{id}`
- **Auth:** `it_admin|admin|staff`
- **Response:** order with `items` and `addOns`

### PATCH `/api/v1/Orders/{id}/status`
- **Body DTO:** `UpdateOrderStatusDto`
```json
{ "orderStatus": "completed" }
```
- **Response:** updated order

### PATCH `/api/v1/Orders/{id}/payment-status`
- **Body DTO:** `UpdatePaymentStatusDto`
```json
{ "paymentStatus": "paid" }
```
- **Response:** updated order

---

## 7.7 TestimonialsController

### GET `/api/v1/Testimonials`
- **Auth:** `it_admin|admin|manager`
- **Query params:**
  - `isApproved`
  - `isFeatured`
  - `page` default `1`
  - `pageSize` default `20`, max `100`
- **Response:**
```json
{ "items": [], "totalCount": 0 }
```

### POST `/api/v1/Testimonials`
- **Auth:** `it_admin|admin|manager`
- **Body DTO:** `CreateTestimonialDto`
```json
{
  "customerName": "Jane",
  "rating": 5,
  "reviewText": "Great service",
  "customerImage": null,
  "isApproved": true,
  "isFeatured": false
}
```
- **Response:** created testimonial

### PUT `/api/v1/Testimonials/{id}`
- **Auth:** `it_admin|admin|manager`
- **Body DTO:** `UpdateTestimonialDto`
```json
{
  "customerName": "Jane",
  "rating": 4,
  "reviewText": "Updated review",
  "isApproved": true,
  "isFeatured": true
}
```
- **Response:** `204 No Content`

### PATCH `/api/v1/Testimonials/{id}/toggle-approval`
- **Auth:** `it_admin|admin|manager`
- **Response:**
```json
{ "id": "guid", "isApproved": true }
```

### DELETE `/api/v1/Testimonials/{id}`
- **Behavior:** toggles `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/Testimonials/submit`
- **Auth:** public
- **Body DTO:** `SubmitReviewDto`
```json
{
  "customerName": "Guest",
  "rating": 5,
  "reviewText": "Loved it",
  "customerImage": null
}
```
- **Response:**
```json
{ "id": "guid" }
```

---

## 7.8 SettingsController

### GET `/api/v1/Settings`
- **Auth:** `it_admin`
- **Response:** `SiteSetting[]`

### PUT `/api/v1/Settings/bulk`
- **Body DTO:** `BulkUpdateSettingsDto`
```json
{
  "settings": [
    { "key": "site_name", "value": "Floora" },
    { "key": "contact_phone", "value": "+94..." }
  ]
}
```
- **Response:** `204 No Content`

### PUT `/api/v1/Settings/{key}`
- **Body DTO:** `UpdateSettingDto`
```json
{ "value": "New value" }
```
- **Response:** `204 No Content`

---

## 7.9 UsersController

### GET `/api/v1/Users`
- **Auth:** any authenticated user
- **Response:** list of users projected to:
```json
[
  {
    "id": "guid",
    "fullName": "System Admin",
    "email": "admin@floora.local",
    "role": "it_admin",
    "isActive": true,
    "createdAt": "2026-04-15T12:00:00Z",
    "lastLogin": null
  }
]
```

### POST `/api/v1/Users`
- **Auth:** `it_admin|admin`
- **Body DTO:** `CreateUserDto`
```json
{
  "fullName": "New User",
  "email": "user@company.com",
  "password": "Secret123!",
  "role": "staff"
}
```
- **Responses:**
  - `201` with `{ "id": "guid" }`
  - `409` with `{ "error": "Email already exists" }`

---

## 8) DTO Reference Summary

### Category DTOs
- `CreateCategoryDto`: `name`, `sortOrder`
- `UpdateCategoryDto`: `name`, `sortOrder`

### Product DTOs
- `CreateProductDto`: `name`, `price`, `description`, `longDescription`, `categoryId`, `isFeatured`, `isAvailable`
- `UpdateProductDto`: same as create

### Event category DTOs
- `CreateEventCategoryDto`: `name`, `description`, `icon`
- `UpdateEventCategoryDto`: `name`, `description`, `icon`

### Facebook embed DTOs
- `CreateFacebookEmbedDto`: `url`, `type`, `label`, `eventCategory`
- `PageUrlDto`: `pageUrl`

### Orders DTOs
- `UpdateOrderStatusDto`: `orderStatus`
- `UpdatePaymentStatusDto`: `paymentStatus`

### Testimonial DTOs
- `CreateTestimonialDto`: `customerName`, `rating`, `reviewText`, `customerImage`, `isApproved`, `isFeatured`
- `UpdateTestimonialDto`: `customerName`, `rating`, `reviewText`, `isApproved`, `isFeatured`
- `SubmitReviewDto`: `customerName`, `rating`, `reviewText`, `customerImage`

### Settings DTOs
- `BulkUpdateSettingsDto`: `settings[]`
- `SettingPair`: `key`, `value`
- `UpdateSettingDto`: `value`

### Users DTOs
- `CreateUserDto`: `fullName`, `email`, `password`, `role`

---

## 9) Frontend Integration Guidance

### Recommended frontend handling
- centralize API base URL
- attach JWT from auth store/interceptor
- create a shared error handler for 401/403/404/409
- treat delete buttons as **toggle status** buttons
- support both JSON and multipart requests

### Relative image URLs
If API returns `/uploads/file.jpg`, frontend should render it as:
```text
<api-host>/uploads/file.jpg
```

### Pagination
Do not assume identical wrappers for all list endpoints.
Some return `{ items, totalCount, page, pageSize }` and some return only `{ items, totalCount }`.

### Product image endpoints
These are scaffolded and return `501`.
Frontend should show a disabled or placeholder state for product image upload/delete until implemented.

---

## 10) Suggested Improvements

These are not required for the frontend, but useful next steps:
- add auth/login endpoint that issues JWT
- add decimal precision config in `AppDbContext`
- add OpenAPI annotations for richer Swagger docs
- add validation rules for DTOs
- add a strategy for image retention when toggling delete on images

