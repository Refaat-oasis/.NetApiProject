# Categories API Documentation (Detailed)

This documentation covers the endpoints available in the `CategoriesController` for managing product categories, including image uploads.

**Base URL:** `http://localhost:5118/api/Categories`

---

## 1. Get All Categories
Returns a list of all categories with their names and image URLs.

*   **Type:** `GET`
*   **Endpoint:** `/`
*   **Response Body (200 OK):** `GetCategory[]`
```json
[
  {
    "id": 1,
    "name": "Electronics",
    "imageUrl": "images/categories/electronics_123.jpg"
  },
  {
    "id": 2,
    "name": "Fashion",
    "imageUrl": "images/categories/default.jpg"
  }
]
```

---

## 2. Get Category with Products
Fetch a specific category along with all products belonging to it.

*   **Type:** `GET`
*   **Endpoint:** `/{id}/products`
*   **Response Body (200 OK):** `GetCategoryWithProducts`
```json
{
  "id": 1,
  "name": "Electronics",
  "imageUrl": "images/categories/electronics_123.jpg",
  "products": [
    {
      "id": 101,
      "name": "Smartphone X",
      "price": 999.99,
      "description": "Latest flagship model",
      "imageUrl": "images/products/phone_abc.jpg"
    }
  ]
}
```

---

## 3. Create Category (with Image Upload)
Creates a new category. This endpoint requires `multipart/form-data` (form-data).

*   **Type:** `POST`
*   **Endpoint:** `/`
*   **Auth Requirement:** `Admin` role required.
*   **Content-Type:** `multipart/form-data`
*   **Request Body (FormData):**
    *   `Name`: "Home Appliances"
    *   `Image`: [Physical File] (Optional)
*   **Response Body (201 Created):**
```json
{
  "id": 3,
  "name": "Home Appliances",
  "imageUrl": "images/categories/home_appliances_456.jpg"
}
```

---

## 4. Update Category (with Image Replacement)
Updates an existing category. If a new image is provided, the old one is automatically deleted from the server.

*   **Type:** `PUT`
*   **Endpoint:** `/{id}`
*   **Auth Requirement:** `Admin` role required.
*   **Content-Type:** `multipart/form-data`
*   **Request Body (FormData):**
    *   `Name`: "Gaming"
    *   `Image`: [New Physical File] (Optional)
*   **Response Body (204 No Content):** No body returned on success.

---

## 5. Delete Category
Deletes the category and removes its associated image file from the server (unless it's the `default.jpg`).

*   **Type:** `DELETE`
*   **Endpoint:** `/{id}`
*   **Auth Requirement:** `Admin` role required.
*   **Response Body (204 No Content):** No body returned on success.

---

## Required TypeScript Models (for Angular)

```typescript
export interface Category {
  id: number;
  name: string;
  imageUrl: string;
}

export interface CategoryWithProducts extends Category {
  products: any[]; // Replace with Product interface if available
}

/**
 * When sending data for Create/Update, use FormData:
 * 
 * const formData = new FormData();
 * formData.append('Name', categoryName);
 * if (selectedFile) {
 *   formData.append('Image', selectedFile);
 * }
 */
```

### Note on Image URLs:
To display images in Angular, prepend your backend base URL:
`<img [src]="'http://localhost:5118/' + category.imageUrl">`
