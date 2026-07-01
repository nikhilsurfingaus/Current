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

**Status:** Not started

### Goal
Financial core: transfers create balanced debit/credit ledger entries inside a DB transaction.

### Delivered
- [ ] `Transaction` entity
- [ ] `LedgerEntry` entity
- [ ] `POST /transactions/transfer`
- [ ] `GET /transactions`, `GET /transactions/{id}`
- [ ] ACID transfer logic (transaction + rollback on failure)

### Notes
—

---

## Phase 3 — Authentication

**Status:** Not started

### Goal
Secure the API with JWT auth and password hashing.

### Delivered
- [ ] `PasswordHash` and `Role` on `User`
- [ ] `POST /auth/register`
- [ ] `POST /auth/login`
- [ ] JWT middleware and authorization

### Notes
—

---

## Phase 4 — Angular Frontend

**Status:** Not started

### Goal
`current-ui` Angular app: login, dashboard, accounts, transfers, transactions.

### Delivered
- [ ] Angular app (`ng new current-ui --style=scss`)
- [ ] Feature-based structure (`core/`, `shared/`, `features/`, `layouts/`)
- [ ] Auth, dashboard, accounts, transactions pages
- [ ] HttpClient services wired to API

### Notes
—

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