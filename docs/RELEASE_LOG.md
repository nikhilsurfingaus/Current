# Release Log

Phase-by-phase progress for **Current** — a ledger-based personal finance platform.

---

## Phase 0 — Learning Foundation

**Status:** Complete  
**Date:** —

### Goal
Learn enough C#, ASP.NET Core, and EF Core to start building.

### Delivered
- [ ] C# fundamentals
- [ ] ASP.NET Core basics
- [ ] Entity Framework Core
- [ ] Dependency injection
- [ ] LINQ
- [ ] EF migrations

### Notes
—

---

## Phase 1 — Backend Foundation

**Status:** Complete  
**Date started:** 2026-07-01  
**Date completed:** 2026-07-01

### Goal
Clean ASP.NET Core API with Users and Accounts — no auth, no transfers, no frontend.

### Delivered
- [x] Repository structure (`frontend/`, `backend/`, `database/`, `docs/`)
- [x] Initial Git commit
- [x] `Current.Api` ASP.NET Core Web API project
- [x] PostgreSQL connected (`CurrentDb`)
- [x] EF Core configured with Npgsql
- [x] `User` and `Account` entities + relationship
- [x] `InitialCreate` migration applied
- [x] `GET/POST /users`, `GET/POST /accounts` endpoints
- [x] Swagger tested
- [x] `Makefile` with `make dev` (Postgres + migrations + API)

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| GET | `/users` | ✅ |
| GET | `/users/{id}` | ✅ |
| POST | `/users` | ✅ |
| GET | `/accounts` | ✅ |
| GET | `/accounts/{id}` | ✅ |
| POST | `/accounts` | ✅ |

### Notes
- API runs at `http://localhost:5231`, Swagger at `/swagger`
- `make dev` starts Postgres on demand, applies migrations, runs `dotnet watch`
- `AccountType` enum: `Everyday`, `Savings`, `Investment`

---

## Phase 2 — Ledger Engine

**Status:** In progress  
**Date started:** 2026-07-02

### Goal
Financial core: transfers create balanced debit/credit ledger entries inside a DB transaction.

### Delivered
- [x] `Common/` folder (`Enums/`, `Constants/`, `Exceptions/`)
- [x] `AccountType` moved to `Common/Enums`
- [x] `TransactionStatus`, `LedgerEntryType` enums
- [x] `Transaction` entity
- [x] `LedgerEntry` entity
- [x] `AddTransactionsAndLedgerEntries` migration
- [x] `TransferRequest`, `TransactionResponse`, `LedgerEntryResponse` DTOs
- [x] `ITransactionService` interface
- [x] Transaction + LedgerEntry mappings
- [x] `TransactionService` (transfer logic)
- [x] ACID transfer logic (transaction + rollback on failure)
- [x] `POST /transactions/transfer`
- [x] `GET /transactions`, `GET /transactions/{id}`

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| POST | `/transactions/transfer` | ✅ |
| GET | `/transactions` | ✅ |
| GET | `/transactions/{id}` | ✅ |

### Notes
- Parts 1–4 complete — ready for end-to-end testing (Part 5)

---

## Phase 3 — Authentication

**Status:** Complete  
**Date started:** 2026-07-03  
**Date completed:** 2026-07-03

### Goal
Secure the API with JWT auth and password hashing.

### Delivered
- [x] `PasswordHash` and `Role` on `User`
- [x] Auth DTOs (`RegisterRequest`, `LoginRequest`, `AuthResponse`)
- [x] `IAuthService` and `AuthService` (register/login + JWT generation)
- [x] `POST /auth/register`
- [x] `POST /auth/login`
- [x] JWT middleware and authorization
- [x] `[Authorize]` on protected endpoints
- [x] Ownership checks (accounts/transactions scoped to current user)
- [x] Swagger Bearer token support
- [x] Typed auth exceptions (`DuplicateEmailException`, `InvalidCredentialsException`)

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| POST | `/auth/register` | ✅ |
| POST | `/auth/login` | ✅ |
| GET | `/users/me` | ✅ |

### Notes
- Part 1 complete: user auth fields added and migrated
- Parts 2–3 complete: auth contracts and service layer ready
- Part 4 complete: JWT bearer authentication wired in startup/DI
- Part 5 complete: AuthController endpoints available in Swagger
- Part 6 complete: endpoints secured + ownership enforced
- Part 7 complete: end-to-end auth and ownership testing passed
- `POST /users` removed — use `/auth/register`
- `POST /accounts` no longer accepts `userId` in body (uses JWT)

---

## Phase 4 — Angular Frontend

**Status:** In progress  
**Date started:** 2026-07-04

### Goal
`current-ui` Angular app: login, dashboard, accounts, transfers, transactions.

### Delivered
- [x] Angular app (`ng new current-ui --style=scss`)
- [x] Feature-based structure (`core/`, `shared/`, `features/`, `layouts/`)
- [x] Deep ocean design tokens (`src/styles/_variables.scss`)
- [x] Environment config (`apiUrl` → `http://localhost:5231`)
- [x] TypeScript models aligned with API DTOs
- [x] `ApiService` + `HttpClient` wired in `app.config`
- [x] Brand assets (`brand/`, `public/brand/`)
- [x] CORS for `http://localhost:4200`
- [ ] Auth, dashboard, accounts, transactions pages
- [ ] HttpClient services wired to API

### Notes
- Part 1 complete: scaffold, theme shell, CORS, folder structure
- Part 2 complete: shared models, `ApiService`, `HttpClient` in `app.config`
- Part 3 complete: `AuthService`, `authInterceptor`, `authGuard`
- Part 4 complete: auth + main layouts, sidebar nav, route structure
- Part 5 complete: login + register forms wired to API, logout, error handling
- Part 6 complete: accounts list + create account UI

---

## Phase 5 — Goals

**Status:** Not started

### Goal
Savings goals and contributions.

### Delivered
- [ ] `Goal` and `GoalContribution` entities
- [ ] Goals CRUD + contribute endpoints
- [ ] Goals UI (list, detail, progress)

### Notes
—

---

## Phase 6 — Analytics

**Status:** Not started

### Goal
Financial reporting and charts.

### Delivered
- [ ] `GET /analytics/net-worth`
- [ ] `GET /analytics/spending`
- [ ] `GET /analytics/cashflow`
- [ ] `GET /analytics/balance-history`
- [ ] Dashboard charts and widgets

### Notes
—

---

## Phase 7 — Production Ready

**Status:** Not started

### Goal
Enterprise polish and deployment.

### Delivered
- [ ] FluentValidation
- [ ] Serilog + global exception middleware
- [ ] Pagination and caching
- [ ] Docker
- [ ] Deploy: Vercel (frontend), Render (API), Neon (PostgreSQL)

### Notes
—