# Products API Documentation (Admin & Seller Dashboard)

This documentation covers product management, including multi-part form data for image uploads.

**Base URL:** `http://localhost:5118/api/Products`

---

## 1. Get All Products (Admin View)
Returns all products, including those that might be hidden from regular customers.

*   **Type:** `GET`
*   **Endpoint:** `/all`
*   **Auth Requirement:** `Admin` role required.
*   **Response Body (200 OK):** `GetProducts[]`
```json
[
  {
    "id": 1,
    "name": "Smartphone X",
    "description": "High-end smartphone",
    "price": 999.99,
    "stock": 50,
    "categoryId": 1,
    "image": "/images/products/phone_123.jpg"
  }
]
```

---

## 2. Create Product (with Image Upload)
Creates a new product. Requires `multipart/form-data`.

*   **Type:** `POST`
*   **Endpoint:** `/`
*   **Auth Requirement:** `Admin` or `Seller` role.
*   **Content-Type:** `multipart/form-data`
*   **Request Body (FormData):**
    *   `Name`: "Gaming Laptop"
    *   `Description`: "Core i9, 32GB RAM"
    *   `Price`: 2499.00
    *   `Stock`: 10
    *   `CategoryId`: 2
    *   `Image`: [Physical File]
*   **Response Body (201 Created):** `Product` object.

---

## 3. Update Product
Updates an existing product. 

*   **Type:** `PUT`
*   **Endpoint:** `/{id}`
*   **Auth Requirement:** `Admin` or owner `Seller`.
*   **Content-Type:** `multipart/form-data`
*   **Request Body (FormData):**
    *   `Name`: "Updated Name"
    *   `Image`: [New Physical File] (Optional)
*   **Response Body (204 No Content)**

---

## 5. Reactivate Product
Restores a soft-deleted product so it's visible to customers again.

*   **Type:** `PUT`
*   **Endpoint:** `/{id}/reactivate`
*   **Auth Requirement:** `Admin` or owner `Seller`.
*   **Response Body (200 OK):**
```json
{
  "message": "Product reactivated successfully.",
  "product": {
    "id": 1,
    "isDeleted": false
  }
}
```

---

## 4. Delete Product (Soft Delete)
Removes the product from public view and deletes the image file from the server.

*   **Type:** `DELETE`
*   **Endpoint:** `/{id}`
*   **Auth Requirement:** `Admin` or owner `Seller`.
*   **Response Body (204 No Content)**

---

## TypeScript Models

```typescript
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  categoryId: number;
  image: string;
}
```
