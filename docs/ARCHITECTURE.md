# Architecture

## Overview

**Current** is a ledger-based personal finance platform. Money movement is recorded as balanced debit/credit **ledger entries** inside database transactions — not by directly mutating balances without audit trail.

## Production topology

```mermaid
flowchart TB
  subgraph client [Client]
    Browser[Browser]
  end

  subgraph vercel [Vercel]
    UI[Angular 22 SPA]
  end

  subgraph render [Render]
    API[ASP.NET Core 10 API]
  end

  subgraph neon [Neon]
    DB[(PostgreSQL)]
  end

  Browser --> UI
  UI -->|HTTPS + JWT| API
  API -->|EF Core / Npgsql| DB
```

| Layer | Technology | Hosting |
|-------|------------|---------|
| Frontend | Angular 22, Chart.js, SCSS | Vercel |
| API | ASP.NET Core 10, JWT, Serilog | Render (Docker) |
| Database | PostgreSQL, EF Core migrations | Neon |
| CI | GitHub Actions | GitHub |
| Local full stack | Docker Compose | Developer machine |

**Live URLs**

- UI: https://current-au.vercel.app
- API: https://current-zdw5.onrender.com
- Health: `GET /health`

## Backend layers

```
Controllers     → HTTP, auth attributes, request/response DTOs
Services        → Business logic, transactions, validation
Data            → ApplicationDbContext, EF Core mappings
Entities        → Domain models
DTOs / Mappings → API contracts
Middleware      → Exception handling, security headers
Extensions      → CORS, Swagger, Serilog, health checks, DI wiring
```

### Key patterns

- **Dependency injection** — services registered in `ServiceCollectionExtensions`
- **Ownership enforcement** — users only access their own accounts, transactions, goals, loans
- **DB transactions** — transfers, payments, goal contributions, loan disbursements use explicit transactions
- **Double-entry ledger** — each transfer creates a `Transaction` plus matching debit/credit `LedgerEntry` rows
- **Idempotent payments** — `Idempotency-Key` header prevents duplicate sends
- **Branch treasury** — system branch holds float; welcome credit and loan disbursements debit treasury

## Frontend layers

```
features/     → Route-level pages (dashboard, accounts, payments, …)
core/         → Auth guard, interceptor, API service, models
layouts/      → Auth shell, main shell with sidebar / mobile drawer
elements/     → Shared styled components
```

- **JWT** stored client-side; `authInterceptor` attaches `Authorization: Bearer`
- **Route guards** — `authGuard`, `guestGuard`, `adminGuard`
- **Environments** — `environment.development.ts` (localhost API) vs `environment.ts` (Render API for production builds)

## Authentication flow

```mermaid
sequenceDiagram
  participant UI as Angular
  participant API as ASP.NET Core
  participant DB as PostgreSQL

  UI->>API: POST /auth/login
  API->>DB: Verify email + password hash
  API-->>UI: JWT + userId + expiresAt
  UI->>UI: Store token
  UI->>API: GET /users/me (Bearer token)
  API-->>UI: User profile
```

Registration: `POST /auth/register` returns the JWT and welcome notification (same shape as login).

## Financial core (transfer)

```mermaid
sequenceDiagram
  participant S as TransactionService
  participant DB as PostgreSQL

  S->>DB: BEGIN TRANSACTION
  S->>DB: Validate accounts + balance
  S->>DB: Insert Transaction
  S->>DB: Insert LedgerEntry (debit)
  S->>DB: Insert LedgerEntry (credit)
  S->>DB: Update account balances
  S->>DB: COMMIT
```

## Configuration

| Source | Used for |
|--------|----------|
| `appsettings.json` | Base defaults |
| `appsettings.Development.json` | Local `make dev` |
| `appsettings.Production.json` | Render (CORS origins, Serilog) |
| Environment variables | Secrets on Render (connection string, JWT key) |

See [Deployment Guide](DEPLOYMENT.md).

## Testing

- **41 integration tests** (`Current.Api.Tests`) using `WebApplicationFactory` + SQLite in-memory
- Covers auth, transfers, payments, loans, notifications
- Run: `make test`

## Related docs

- [API reference](API.md)
- [ER diagram](ERD.md)
- [Deployment](DEPLOYMENT.md)
- [Demo account](DEMO.md)
- [Release log](RELEASE_LOG.md)
