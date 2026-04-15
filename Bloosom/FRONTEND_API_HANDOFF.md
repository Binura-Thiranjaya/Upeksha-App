# Bloosom Frontend API Handoff

This document is written for frontend developers. It focuses on:
- which endpoints to call
- what request DTOs to send
- what responses to expect
- which routes require auth/roles
- which routes are public
- how delete endpoints behave

## 1) Base Info

- **Base URL:** `https://<your-host>/api/v1`
- **Auth header:** `Authorization: Bearer <jwt-token>`
- **Content type:** `application/json` for normal APIs
- **File uploads:** `multipart/form-data`
- **Swagger:** `/swagger` in Development

## 2) Important Backend Behavior

### Soft delete toggle
All `DELETE` endpoints act as **status toggles**, not permanent delete.

- if `isDeleted = false` -> DELETE sets it to `true`
- if `isDeleted = true` -> DELETE sets it to `false`

Typical response:
```json
{ "id": "guid", "isDeleted": true }
```

### Public routes
These do not require JWT:
- `GET /api/v1/EventCategories`
- `GET /api/v1/FacebookEmbeds`
- `POST /api/v1/Testimonials/submit`

### Missing login endpoint
There is currently **no login/token issuance endpoint** in the controllers. Frontend needs a token from another auth system or a future login API.

## 3) Roles

- `it_admin`
- `admin`
- `manager`
- `staff`

### Role access summary
- `it_admin,admin,manager`: Categories, Products, EventCategories, FacebookEmbeds, Testimonials admin endpoints
- `it_admin,admin,staff`: Orders
- `it_admin`: Settings and Facebook page URL
- any authenticated user: `GET /api/v1/Users`

## 4) Common Response Patterns

### List endpoint pagination
Many list endpoints use this pattern:
```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

Not every controller returns `page` and `pageSize`, so frontend should handle per endpoint.

### Error responses
Common HTTP codes:
- `200` OK
- `201` Created
- `204` No Content
- `400` Bad Request
- `401` Unauthorized
- `403` Forbidden
- `404` Not Found
- `409` Conflict
- `501` Not Implemented

## 5) Entity Shapes Frontend Will See

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

### Product (list/detail projection)
```json
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
```

### Event category
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

### Event image
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

### Facebook embed
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

### User
```json
{
  "id": "guid",
  "fullName": "System Admin",
  "email": "admin@floora.local",
  "passwordHash": "...",
  "role": "it_admin",
  "isActive": true,
  "lastLogin": null,
  "createdAt": "2026-04-15T12:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z",
  "isDeleted": false
}
```

### Site setting
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

### Order
Order responses are full entity objects. The list endpoint returns a summary list, and the detail endpoint includes `items` and `addOns`.

## 6) Request / Response DTOs by Endpoint

## Categories

### GET `/api/v1/Categories`
- **Auth:** `it_admin|admin|manager`
- **Response:** `Category[]`

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
- **Response:** `204`

### DELETE `/api/v1/Categories/{id}`
- **Behavior:** toggle `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

## Products

### GET `/api/v1/Products?search=&categoryId=&page=1&pageSize=20`
- **Auth:** `it_admin|admin|manager`
- **Response:**
```json
{ "items": [], "totalCount": 0, "page": 1, "pageSize": 20 }
```

### GET `/api/v1/Products/{id}`
- **Auth:** `it_admin|admin|manager`
- **Response:** product detail projection

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
- **Response:** `204`

### DELETE `/api/v1/Products/{id}`
- **Behavior:** toggle `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/Products/{id}/images`
- **Status:** not implemented
- **Response:** `501`

### DELETE `/api/v1/Products/{id}/images/{imageId}`
- **Status:** not implemented
- **Response:** `501`

## Event Categories

### GET `/api/v1/EventCategories`
- **Auth:** public
- **Response:** `EventCategory[]`

### POST `/api/v1/EventCategories`
- **Body DTO:** `CreateEventCategoryDto`
```json
{ "name": "Weddings", "description": "Event flowers", "icon": "fa-heart" }
```
- **Response:** created `EventCategory`

### PUT `/api/v1/EventCategories/{id}`
- **Body DTO:** `UpdateEventCategoryDto`
```json
{ "name": "Weddings Premium", "description": "Updated", "icon": "fa-heart" }
```
- **Response:** `204`

### DELETE `/api/v1/EventCategories/{id}`
- **Behavior:** toggle `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/EventCategories/{id}/images`
- **Body type:** `multipart/form-data`
- **Fields:**
  - `file` (required)
  - `sortOrder` (optional, int, default `0`)
  - `caption` (optional, string)
- **Response:** created `EventImage`

### DELETE `/api/v1/EventCategories/{id}/images/{imageId}`
- **Behavior:** toggle `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

## Facebook Embeds

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
- **Response:** `204`

### DELETE `/api/v1/FacebookEmbeds/{id}`
- **Behavior:** toggle `isDeleted`
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
- **Response:** `204`

## Orders

### GET `/api/v1/Orders?search=&status=&page=1&pageSize=20`
- **Auth:** `it_admin|admin|staff`
- **Response:**
```json
{ "items": [], "totalCount": 0, "page": 1, "pageSize": 20 }
```

### GET `/api/v1/Orders/{id}`
- **Auth:** `it_admin|admin|staff`
- **Response:** order detail with `items` and `addOns`

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

## Testimonials

### GET `/api/v1/Testimonials?isApproved=&isFeatured=&page=1&pageSize=20`
- **Auth:** `it_admin|admin|manager`
- **Response:**
```json
{ "items": [], "totalCount": 0 }
```

### POST `/api/v1/Testimonials`
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
- **Response:** `204`

### PATCH `/api/v1/Testimonials/{id}/toggle-approval`
- **Response:**
```json
{ "id": "guid", "isApproved": true }
```

### DELETE `/api/v1/Testimonials/{id}`
- **Behavior:** toggle `isDeleted`
- **Response:**
```json
{ "id": "guid", "isDeleted": true }
```

### POST `/api/v1/Testimonials/submit`
- **Public endpoint**
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

## Settings

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
- **Response:** `204`

### PUT `/api/v1/Settings/{key}`
- **Body DTO:** `UpdateSettingDto`
```json
{ "value": "New value" }
```
- **Response:** `204`

## Users

### GET `/api/v1/Users`
- **Auth:** any authenticated user
- **Response:** `User[]` simplified list projection

### POST `/api/v1/Users`
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

## 7) DTO Summary for Frontend

### Create/Update DTOs
- `CreateCategoryDto`: `name`, `sortOrder`
- `UpdateCategoryDto`: `name`, `sortOrder`
- `CreateProductDto`: `name`, `price`, `description`, `longDescription`, `categoryId`, `isFeatured`, `isAvailable`
- `UpdateProductDto`: same as create
- `CreateEventCategoryDto`: `name`, `description`, `icon`
- `UpdateEventCategoryDto`: `name`, `description`, `icon`
- `CreateFacebookEmbedDto`: `url`, `type`, `label`, `eventCategory`
- `PageUrlDto`: `pageUrl`
- `UpdateOrderStatusDto`: `orderStatus`
- `UpdatePaymentStatusDto`: `paymentStatus`
- `CreateTestimonialDto`: `customerName`, `rating`, `reviewText`, `customerImage`, `isApproved`, `isFeatured`
- `UpdateTestimonialDto`: `customerName`, `rating`, `reviewText`, `isApproved`, `isFeatured`
- `SubmitReviewDto`: `customerName`, `rating`, `reviewText`, `customerImage`
- `BulkUpdateSettingsDto`: `settings[]`
- `SettingPair`: `key`, `value`
- `UpdateSettingDto`: `value`
- `CreateUserDto`: `fullName`, `email`, `password`, `role`

## 8) Frontend Notes

- Always send the JWT token for protected routes.
- For relative upload URLs like `/uploads/file.jpg`, prepend the API host in the frontend.
- `DELETE` means toggle, so the UI should show the current active/inactive state instead of assuming permanent removal.
- Product image routes are scaffolds and currently return `501`.
- Pagination response shapes differ slightly between controllers, so the frontend should not assume every list endpoint returns `pageSize` or the same wrapper.

## 9) Suggested Frontend Contract

If you are building the frontend, the safest contract is:
- create a shared API client
- centralize JWT injection
- normalize list responses into a common shape
- treat delete actions as toggle actions
- keep file upload helpers separate from JSON helpers

---

If you need this as **OpenAPI YAML**, **Postman collection format**, or a **frontend TypeScript interface file**, I can generate that next.

