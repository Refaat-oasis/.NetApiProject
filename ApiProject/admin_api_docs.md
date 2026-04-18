# Admin User Management API Documentation (Detailed)

This documentation provides exact JSON structures for the `AdminController` endpoints.

**Base URL:** `http://localhost:5118/api/Admin`  
**Auth Requirement:** `Authorization: Bearer {token}` (User must have the **Admin** role)

---

## 1. Get All Users
Returns a detailed list of all registered users along with their assigned roles.

*   **Type:** `GET`
*   **Endpoint:** `/users`
*   **Response Body (200 OK):**
```json
[
  {
    "id": "917bc29d-31a2-4497-9bec-fe8d13e0ea95",
    "userName": "admin@test.com",
    "email": "admin@test.com",
    "fullName": "System Admin",
    "address": "Admin Office, HQ",
    "isDeleted": false,
    "roles": ["Admin"]
  },
  {
    "id": "a2b3c4d5-e6f7-8g9h-0i1j-k2l3m4n5o6p7",
    "userName": "john.seller@example.com",
    "email": "john.seller@example.com",
    "fullName": "John Doe",
    "address": "123 Market St, New York",
    "isDeleted": false,
    "roles": ["Seller"]
  },
  {
    "id": "x1y2z3a4-b5c6-d7e8-f9g0-h1i2j3k4l5m6",
    "userName": "customer@test.com",
    "email": "customer@test.com",
    "fullName": "Jane Smith",
    "address": "456 Oak Ave, California",
    "isDeleted": true,
    "roles": ["User"]
  }
]
```

---

## 2. Get Single User Details
Fetch full model information for a specific user.

*   **Type:** `GET`
*   **Endpoint:** `/users/{id}`
*   **Response Body (200 OK):**
```json
{
  "id": "a2b3c4d5-e6f7-8g9h-0i1j-k2l3m4n5o6p7",
  "fullName": "John Doe",
  "address": "123 Market St, New York",
  "isDeleted": false,
  "userName": "john.seller@example.com",
  "email": "john.seller@example.com",
  "phoneNumber": "+1234567890",
  "emailConfirmed": true
}
```

---

## 3. Block/Restrict User
Disables user access by setting `isDeleted` to `true`.

*   **Type:** `PUT`
*   **Endpoint:** `/users/{id}/block`
*   **Response Body (200 OK):**
```json
{
  "message": "User restricted successfully.",
  "user": {
    "id": "x1y2z3a4-b5c6-d7e8-f9g0-h1i2j3k4l5m6",
    "fullName": "Jane Smith",
    "isDeleted": true
  }
}
```

---

## 4. Reactivate User
Restores access for a previously blocked user.

*   **Type:** `PUT`
*   **Endpoint:** `/users/{id}/reactivate`
*   **Response Body (200 OK):**
```json
{
  "message": "User reactivated successfully.",
  "user": {
    "id": "x1y2z3a4-b5c6-d7e8-f9g0-h1i2j3k4l5m6",
    "fullName": "Jane Smith",
    "isDeleted": false
  }
}
```

---

## 5. Create New Admin Account
Creates a brand new user with immediate Admin privileges.

*   **Type:** `POST`
*   **Endpoint:** `/users/admin`
*   **Request Body:**
```json
{
  "fullName": "Super Admin",
  "email": "new.admin@test.com",
  "password": "StrongPassword123!",
  "address": "Central Office"
}
```
*   **Response Body (200 OK):**
```json
{
  "message": "Admin created successfully."
}
```

---

## 6. Update User Role (Role Transition)
Change a user's role and view their previous status.

*   **Type:** `PUT`
*   **Endpoint:** `/users/{id}/role`
*   **Request Body:**
```json
{
  "role": "Seller" 
}
```
*   **Response Body (200 OK):**
```json
{
  "message": "User role updated to Seller.",
  "previousRoles": ["User"],
  "newRole": "Seller"
}
```

---

## Required TypeScript Models (for Angular)

```typescript
export interface UserWithRolesDto {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  address: string;
  isDeleted: boolean;
  roles: string[];
}

export interface UpdateRoleDto {
  role: 'Admin' | 'Seller' | 'User';
}

export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
  address: string;
}
```
