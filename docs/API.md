# API reference

Base URLs:

| Environment | URL |
|-------------|-----|
| Production | `https://current-zdw5.onrender.com` |
| Local | `http://localhost:5231` |
| Swagger (dev only) | `http://localhost:5231/swagger` |

## Authentication

Public endpoints: `POST /auth/register`, `POST /auth/login`, `GET /health`.

All other endpoints require:

```
Authorization: Bearer <jwt>
```

### Register

```http
POST /auth/register
Content-Type: application/json

{
  "firstName": "Demo",
  "lastName": "User",
  "email": "demo@current.app",
  "password": "Demo123!"
}
```

Returns `201` with `token`, `userId`, `email`, `role`, `expiresAt` and creates a welcome notification.

Duplicate email returns `409`. Password must be at least 8 characters.

### Login

```http
POST /auth/login
Content-Type: application/json

{
  "email": "demo@current.app",
  "password": "Demo123!"
}
```

Returns `200` with the auth payload. Invalid credentials return `401`.

## Health

```http
GET /health
```

Returns `200` with body `Healthy` when the API and database are reachable.

## Endpoints by area

### Users

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/users/me` | Current user profile |
| PUT | `/users/me/profile` | Update name |
| PUT | `/users/me/preferences` | Theme, currency, timezone, locale |
| GET | `/users/{id}` | Own profile only |

### Accounts

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/accounts` | List own accounts |
| GET | `/accounts/{id}` | Get account |
| POST | `/accounts` | Create account (welcome credit on first account) |

Account responses include `bsb` and `accountNumber`. New accounts receive a unique number under BSB `932-000`. Existing accounts are backfilled automatically on deploy.

### Transactions

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/transactions/transfer` | Transfer between own accounts |
| GET | `/transactions` | List own transactions |
| GET | `/transactions/{id}` | Get transaction |

### Payments

Pay by **email** (credits recipient's primary account) or **BSB + account number** (credits that specific account).

```http
POST /payments/send
Idempotency-Key: <uuid>
Content-Type: application/json

{
  "fromAccountId": "...",
  "recipientEmail": "friend@email.com",
  "amount": 50.00,
  "reference": "Lunch"
}
```

Or:

```json
{
  "fromAccountId": "...",
  "recipientBsb": "932-000",
  "recipientAccountNumber": "10000001",
  "amount": 50.00
}
```

Provide either `recipientEmail` or both `recipientBsb` and `recipientAccountNumber`.

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/payments/send` | Pay another user (supports `Idempotency-Key` header) |
| GET | `/payments/sent` | Sent payments |
| GET | `/payments/received` | Received payments |
| GET | `/payments/history` | Combined history |
| GET | `/payments/{transactionId}` | Receipt |

### Contacts

Contacts can be saved by email or by BSB + account number.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/contacts` | List contacts |
| GET | `/contacts/{id}` | Get contact |
| POST | `/contacts` | Create contact |
| PUT | `/contacts/{id}` | Update contact |
| DELETE | `/contacts/{id}` | Delete contact |

### Goals

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/goals` | List goals |
| GET | `/goals/{id}` | Goal detail |
| POST | `/goals` | Create goal (+ goal account) |
| PUT | `/goals/{id}` | Update goal |
| DELETE | `/goals/{id}` | Cancel goal |
| POST | `/goals/{id}/contribute` | Contribute to goal |
| POST | `/goals/{id}/withdraw` | Withdraw from goal |
| GET | `/goals/{id}/history` | Contribution history |

### Analytics

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/analytics/overview` | Dashboard summary |
| GET | `/analytics/cashflow` | Cash flow series |
| GET | `/analytics/networth-history` | Net worth over time |
| GET | `/analytics/categories` | Spending by category |
| GET | `/analytics/goals` | Goal progress summary |
| GET | `/analytics/monthly-summary` | Monthly totals |

### Loans (user)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/loans` | List own loans |
| GET | `/loans/limits` | Borrowing limits for current user |
| GET | `/loans/{id}` | Loan detail |
| POST | `/loans` | Request loan |
| DELETE | `/loans/{id}` | Cancel pending loan |
| POST | `/loans/{id}/repay` | Make repayment |
| GET | `/loans/{id}/repayments` | Repayment history |

### Branch (admin)

Requires `Admin` role.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/branch/treasury` | Treasury balance |
| POST | `/branch/disbursements` | Top up a user account (by email, account id, or BSB + account number) |
| GET | `/branch/loans` | List loans (`?status=Pending`) |
| POST | `/branch/loans/{id}/approve` | Approve and disburse loan |
| POST | `/branch/loans/{id}/reject` | Reject loan |

### Notifications

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/notifications` | List notifications |
| GET | `/notifications/unread-count` | Unread count |
| PATCH | `/notifications/read-all` | Mark all read |
| PATCH | `/notifications/{id}/read` | Mark one read |

## Error responses

| Status | When |
|--------|------|
| 400 | Validation / business rule (`{ "message": "..." }`) |
| 401 | Missing or invalid JWT / invalid login credentials |
| 404 | Resource not found or not owned |
| 409 | Duplicate email on register (verified account) |
| 500 | Unhandled error (generic message in production) |

Payment errors use `{ "code": "...", "message": "..." }`.

## Swagger

Available in **Development** only (`make dev` → `/swagger`). Production does not expose Swagger.
