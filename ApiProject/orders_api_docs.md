# Orders API Documentation (Admin Dashboard)

This documentation covers managing customer orders, tracking totals, and updating statuses.

**Base URL:** `http://localhost:5118/api/Orders`

---

## 1. Get All Orders (Admin View)
Returns a complete history of all orders in the system.

*   **Type:** `GET`
*   **Endpoint:** `/admin/all`
*   **Auth Requirement:** `Admin` role required.
*   **Response Body (200 OK):** `OrderResponseDto[]`
```json
[
  {
    "id": 501,
    "orderDate": "2024-04-18T10:30:00",
    "total": 1250.50,
    "status": "Processing",
    "paymentMethod": "CreditCard",
    "shippingAddress": "123 Main St, NY",
    "items": [
      {
        "productId": 1,
        "productName": "Smartphone X",
        "quantity": 1,
        "price": 999.99,
        "subtotal": 999.99
      }
    ]
  }
]
```

---

## 2. Get Order Details
Fetch full details for a specific order.

*   **Type:** `GET`
*   **Endpoint:** `/admin/{id}`
*   **Auth Requirement:** `Admin` role required.
*   **Response Body (200 OK):** `OrderResponseDto`

---

## 3. Update Order Status
Change the status of an order (e.g., from "Processing" to "Shipped").

*   **Type:** `PUT`
*   **Endpoint:** `/admin/{id}/status`
*   **Auth Requirement:** `Admin` role required.
*   **Request Body:**
```json
{
  "status": "Shipped"
}
```
*   **Response Body (200 OK):**
```json
{
  "message": "Order status updated successfully",
  "status": "Shipped"
}
```

---

## Required TypeScript Models

```typescript
export interface OrderResponseDto {
  id: number;
  orderDate: string;
  total: number;
  status: string; // 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled'
  paymentMethod: string;
  shippingAddress: string;
  items: OrderItemResponseDto[];
}

export interface OrderItemResponseDto {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  subtotal: number;
}
```
