# Bloosom API Documentation

## Base
- Base URL: `https://<your-host>/api/v1`
- Content type: `application/json`
- Auth for protected endpoints: `Authorization: Bearer <jwt-token>`
- Swagger (development): `/swagger`

## Important behavior
- All "delete" endpoints are soft status toggles.
- Calling a DELETE endpoint toggles `isDeleted` (`false -> true` or `true -> false`).
- DELETE responses now return `200 OK` with:

```json
{ "id": "guid", "isDeleted": true }
```

## Auth and roles
- `it_admin,admin,manager`: Categories, Products, EventCategories, FacebookEmbeds, Testimonials admin endpoints
- `it_admin,admin,staff`: Orders
- `it_admin`: Settings, Facebook page URL endpoints
- Any authenticated user: `GET /api/v1/Users`
- Public: `GET /api/v1/EventCategories`, `GET /api/v1/FacebookEmbeds`, `POST /api/v1/Testimonials/submit`

## Common status codes
- `200` success with response body
- `201` created
- `204` success without body
- `400` bad request
- `401` unauthorized
- `403` forbidden
- `404` not found
- `409` conflict
- `501` not implemented

---

## Categories
### GET `/api/v1/Categories`
- Role: `it_admin|admin|manager`
- Returns non-deleted categories sorted by `sortOrder`.

### POST `/api/v1/Categories`
- Role: `it_admin|admin|manager`
- Body:
```json
{ "name": "Birthday", "sortOrder": 1 }
```
- Response: `201` with created category.

### PUT `/api/v1/Categories/{id}`
- Role: `it_admin|admin|manager`
- Body:
```json
{ "name": "Birthday Premium", "sortOrder": 2 }
```
- Response: `204` or `404`.

### DELETE `/api/v1/Categories/{id}`
- Role: `it_admin|admin|manager`
- Toggles `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

---

## Products
### GET `/api/v1/Products?search=&categoryId=&page=1&pageSize=20`
- Role: `it_admin|admin|manager`
- Query params:
  - `search` optional
  - `categoryId` optional GUID
  - `page` default `1`
  - `pageSize` default `20`, max `100`
- Response:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Rose Bouquet",
      "slug": "rose-bouquet",
      "price": 120.5,
      "description": "...",
      "longDescription": "...",
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
- Role: `it_admin|admin|manager`
- Response: single product projection.

### POST `/api/v1/Products`
- Role: `it_admin|admin|manager`
- Body:
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
- Response: `201` with `{ "id": "guid" }`.

### PUT `/api/v1/Products/{id}`
- Role: `it_admin|admin|manager`
- Body: same as create.
- Response: `204` or `404`.

### DELETE `/api/v1/Products/{id}`
- Role: `it_admin|admin|manager`
- Toggles `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

### POST `/api/v1/Products/{id}/images`
- Role: `it_admin|admin|manager`
- Not implemented.
- Response: `501`.

### DELETE `/api/v1/Products/{id}/images/{imageId}`
- Role: `it_admin|admin|manager`
- Not implemented.
- Response: `501`.

---

## Event Categories
### GET `/api/v1/EventCategories`
- Public.
- Returns non-deleted event categories with images.

### POST `/api/v1/EventCategories`
- Role: `it_admin|admin|manager`
- Body:
```json
{ "name": "Weddings", "description": "Event flowers", "icon": "fa-heart" }
```
- Response: `201` with created event category.

### PUT `/api/v1/EventCategories/{id}`
- Role: `it_admin|admin|manager`
- Body: same as create.
- Response: `204` or `404`.

### DELETE `/api/v1/EventCategories/{id}`
- Role: `it_admin|admin|manager`
- Toggles `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

### POST `/api/v1/EventCategories/{id}/images`
- Role: `it_admin|admin|manager`
- Content type: `multipart/form-data`
- Fields:
  - `file` required
  - `sortOrder` optional (default `0`)
  - `caption` optional
- Response: `201` with created image.

### DELETE `/api/v1/EventCategories/{id}/images/{imageId}`
- Role: `it_admin|admin|manager`
- Toggles image `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

---

## Facebook Embeds
### GET `/api/v1/FacebookEmbeds`
- Public.
- Returns non-deleted embeds ordered by `sortOrder`.

### POST `/api/v1/FacebookEmbeds`
- Role: `it_admin|admin|manager`
- Body:
```json
{ "url": "https://facebook.com/...", "type": "post", "label": "Promo", "eventCategory": "Weddings" }
```
- Response: `201` with created embed.

### PUT `/api/v1/FacebookEmbeds/{id}`
- Role: `it_admin|admin|manager`
- Body: same as create.
- Response: `204` or `404`.

### DELETE `/api/v1/FacebookEmbeds/{id}`
- Role: `it_admin|admin|manager`
- Toggles `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

### GET `/facebook-embeds/page-url`
- Role: `it_admin`
- Response:
```json
{ "pageUrl": "https://facebook.com/your-page" }
```

### PUT `/facebook-embeds/page-url`
- Role: `it_admin`
- Body:
```json
{ "pageUrl": "https://facebook.com/your-page" }
```
- Response: `204`.

---

## Orders
### GET `/api/v1/Orders?search=&status=&page=1&pageSize=20`
- Role: `it_admin|admin|staff`
- Query:
  - `search` optional
  - `status` optional
  - `page` default `1`
  - `pageSize` default `20`, max `100`
- Response:
```json
{ "items": [], "totalCount": 0, "page": 1, "pageSize": 20 }
```

### GET `/api/v1/Orders/{id}`
- Role: `it_admin|admin|staff`
- Returns order with `items` and `addOns`.

### PATCH `/api/v1/Orders/{id}/status`
- Role: `it_admin|admin|staff`
- Body:
```json
{ "orderStatus": "completed" }
```
- Response: updated order.

### PATCH `/api/v1/Orders/{id}/payment-status`
- Role: `it_admin|admin|staff`
- Body:
```json
{ "paymentStatus": "paid" }
```
- Response: updated order.

---

## Testimonials
### GET `/api/v1/Testimonials?isApproved=&isFeatured=&page=1&pageSize=20`
- Role: `it_admin|admin|manager`
- Response:
```json
{ "items": [], "totalCount": 0 }
```

### POST `/api/v1/Testimonials`
- Role: `it_admin|admin|manager`
- Body:
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
- Response: `201` with created testimonial.

### PUT `/api/v1/Testimonials/{id}`
- Role: `it_admin|admin|manager`
- Body:
```json
{
  "customerName": "Jane",
  "rating": 4,
  "reviewText": "Updated review",
  "isApproved": true,
  "isFeatured": true
}
```
- Response: `204` or `404`.

### PATCH `/api/v1/Testimonials/{id}/toggle-approval`
- Role: `it_admin|admin|manager`
- Response:
```json
{ "id": "guid", "isApproved": true }
```

### DELETE `/api/v1/Testimonials/{id}`
- Role: `it_admin|admin|manager`
- Toggles `isDeleted`.
- Response: `200` with `{ id, isDeleted }`.

### POST `/api/v1/Testimonials/submit`
- Public.
- Body:
```json
{
  "customerName": "Guest",
  "rating": 5,
  "reviewText": "Loved it",
  "customerImage": null
}
```
- Response: `201` with `{ "id": "guid" }`.

---

## Settings
### GET `/api/v1/Settings`
- Role: `it_admin`
- Response: list of settings.

### PUT `/api/v1/Settings/bulk`
- Role: `it_admin`
- Body:
```json
{
  "settings": [
    { "key": "site_name", "value": "Floora" },
    { "key": "contact_phone", "value": "+94..." }
  ]
}
```
- Response: `204`.

### PUT `/api/v1/Settings/{key}`
- Role: `it_admin`
- Body:
```json
{ "value": "New value" }
```
- Response: `204` or `404`.

---

## Users
### GET `/api/v1/Users`
- Auth: any authenticated user
- Response:
```json
[
  {
    "id": "guid",
    "fullName": "System Admin",
    "email": "admin@floara.local",
    "role": "it_admin",
    "isActive": true,
    "createdAt": "2026-04-15T12:00:00Z",
    "lastLogin": null
  }
]
```

### POST `/api/v1/Users`
- Role: `it_admin|admin`
- Body:
```json
{
  "fullName": "New User",
  "email": "user@company.com",
  "password": "Secret123!",
  "role": "staff"
}
```
- Responses:
  - `201` with `{ "id": "guid" }`
  - `409` with `{ "error": "Email already exists" }`

