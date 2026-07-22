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

**Status:** Complete  
**Date started:** 2026-07-04  
**Date completed:** 2026-07-05

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
- [x] Dashboard overview page
- [x] HttpClient services wired to API

### Notes
- Part 1 complete: scaffold, theme shell, CORS, folder structure
- Part 2 complete: shared models, `ApiService`, `HttpClient` in `app.config`
- Part 3 complete: `AuthService`, `authInterceptor`, `authGuard`
- Part 4 complete: auth + main layouts, sidebar nav, route structure
- Part 5 complete: login + register forms wired to API, logout, error handling
- Part 6 complete: accounts list + create account UI
- Part 7 complete: transfer form, transactions list, `TransactionService`
- Part 8a complete: dashboard (balance summary, accounts, recent activity)
- Part 8b complete: light split auth layout, form polish (icons, password toggle)
- Part 8c complete: removed `FeaturePlaceholderComponent`, auth mark + input polish
- Light app shell: unified design tokens, sidebar + dashboard mock styling across Phase 4 pages
- Phase 4 complete: full UI flow without Swagger

---

## Phase 5 — Goals

**Status:** Complete  
**Date started:** 2026-07-05  
**Date completed:** 2026-07-05

### Goal
Savings goals and contributions with ledger-backed allocations.

### Delivered
- [x] `GoalStatus` and `ContributionType` enums
- [x] `Goal` and `GoalContribution` entities
- [x] `SourceAccountId` + `GoalAccountId` on goals (goal account created in Part 2)
- [x] Goal DTOs (`Create`, `Update`, `Contribute`, `Withdraw`, responses)
- [x] `GoalMappings` and `GoalContributionMappings`
- [x] `AddGoalsAndGoalContributions` migration
- [x] `IGoalService` / `GoalService` with ledger-backed contribute & withdraw
- [x] `GoalsController` — CRUD, contribute, withdraw, history
- [x] Goal create auto-provisions linked `GoalAccount` (Savings)
- [x] Goals UI — list, create, detail, contribute, withdraw, history
- [x] Dashboard goals widget with active goal progress
- [x] Goal-linked savings accounts hidden from accounts list

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| GET | `/goals` | ✅ |
| GET | `/goals/{id}` | ✅ |
| POST | `/goals` | ✅ |
| PUT | `/goals/{id}` | ✅ |
| DELETE | `/goals/{id}` | ✅ (soft cancel) |
| POST | `/goals/{id}/contribute` | ✅ |
| POST | `/goals/{id}/withdraw` | ✅ |
| GET | `/goals/{id}/history` | ✅ |

### Notes
- Part 1 complete: schema, DTOs, mappings, migration
- Part 2 complete: goal service, controller, transfers + contribution history in one DB transaction
- `IconKey` on goals: preset keys (`default`, `vacation`, `home`, `emergency`, `car`, `gaming`, `investment`, `education`); frontend `GoalIconComponent` + `goal-icon-options.ts`
- Part 3–4 complete: `/goals` list + detail pages, sidebar nav, dashboard widget, goal account filtering

---

## Phase 6 — Analytics

**Status:** Complete  
**Date started:** 2026-07-06  
**Date completed:** 2026-07-08

### Goal
Financial reporting and charts.

### Delivered
- [x] `TransactionCategory` enum (Income, Transfer, Housing, Groceries, etc.)
- [x] `Category`, `Merchant`, `Reference` on `Transactions`
- [x] `AddTransactionAnalyticsFields` migration (existing rows → `Transfer`)
- [x] `IAnalyticsService` / `AnalyticsService` + `AnalyticsController`
- [x] Analytics DTOs (overview, cashflow, net worth history, categories, goals, monthly summary)
- [x] Full aggregation queries (cashflow, categories, net worth history, monthly summary)
- [x] `/analytics` route + sidebar nav + analytics overview page
- [x] Chart.js charts on `/analytics` (cash flow bars, category doughnut, net worth line)
- [x] Dashboard net-worth sparkline from real `networth-history` data

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| GET | `/analytics/overview` | ✅ (real monthly KPI aggregation) |
| GET | `/analytics/cashflow` | ✅ |
| GET | `/analytics/networth-history` | ✅ |
| GET | `/analytics/categories` | ✅ |
| GET | `/analytics/goals` | ✅ |
| GET | `/analytics/monthly-summary` | ✅ |

### Notes
- Part 1 complete: schema, migration, analytics service + controller shell
- Part 2 complete: monthly aggregation logic for overview, cashflow, categories, net worth history, and monthly summary
- New transfers default to `Category = Transfer`; income/expense ratios depend on category distribution
- Part 3 complete: Angular analytics feature page wired to all analytics endpoints
- Part 4 complete: reusable `AppChartComponent`, Chart.js visuals on Analytics + Dashboard sparkline
- Phase 6 complete

---

## Phase 7 — Payments + Security Hardening

**Status:** Complete  
**Date started:** 2026-07-09  
**Date completed:** 2026-07-19

### Goal
Safely move money between users with ledger integrity, idempotency, and ownership rules.

### Delivered
- [x] User-to-user payments via email (`POST /payments/send`)
- [x] Ownership, self-pay block, currency match, insufficient funds checks
- [x] Ledger debit/credit in a DB transaction with rollback
- [x] `IdempotencyKeys` table + `Idempotency-Key` header (safe retries)
- [x] Typed payment error codes (`PaymentException` / `PaymentErrorCode`)
- [x] Payment receipt by id + sent/received/history endpoints
- [x] Frontend: Pay Someone, Payment History, Payment Receipt + sidebar
- [x] Saved contacts — per-user CRUD, payment picker, optional save after payment
- [ ] Docker (optional wrap for this phase)

### Endpoints
| Method | Route | Status |
|--------|-------|--------|
| POST | `/payments/send` | ✅ (requires `Idempotency-Key`) |
| GET | `/payments/sent` | ✅ |
| GET | `/payments/received` | ✅ |
| GET | `/payments/history` | ✅ |
| GET | `/payments/{transactionId}` | ✅ (receipt) |
| GET/POST | `/contacts` | ✅ |
| GET/PUT/DELETE | `/contacts/{contactId}` | ✅ |

### Notes
- Part 1 complete: payment send flow + receipt response
- Part 2 complete: idempotency + standardized payment errors
- Part 3 complete: receipt lookup + sent/received/history
- Part 4 complete: Angular Pay Someone (`/payments/send`), history (`/payments`), receipt (`/payments/:id`)
- Contacts enhancement: manage saved names/emails at `/contacts` and reuse them when paying
- Cross-user payments identified as transactions where from/to accounts belong to different users
- Phase 7 complete (Docker optional / deferred to Phase 9)

---

## Phase 8 — UX Polish & Accessibility

**Status:** Complete  
**Date started:** 2026-07-20  
**Date completed:** 2026-07-22

### Goal
Make every Current workflow intentionally usable on desktop, tablet, mobile, keyboard and screen reader—with polished loading, empty, error and motion states, then user preferences.

### Planned parts
1. Mobile navigation and responsive layouts
2. Mobile tables and chart adaptations
3. Forms and accessibility
4. Loading, empty and error states
5. Subtle animations and reduced-motion support
6. User profile and preferences

### Delivered
- [x] Part 1: slide-out mobile drawer navigation (replaces wrapped sidebar links)
- [x] Part 1: mobile chrome (menu + brand), backdrop, Escape/resize/route close
- [x] Part 1: keep user profile + logout in the drawer on mobile
- [x] Part 1: narrow-phone topbar/footer/page spacing polish
- [x] Part 1: tablet-friendly dashboard/analytics/goals grids
- [x] Part 2: transactions/accounts/goal history table → mobile cards
- [x] Part 2: analytics charts — flip cash flow, trim ticks, hide doughnut legend, mobile summaries
- [x] Part 3: forms + accessibility pass
- [x] Part 4: skeletons / empty / error states
- [x] Part 5: motion + `prefers-reduced-motion`
- [x] Part 6: profile / theme / currency / timezone / locale

### Notes
- Part 1 complete: intentional mobile shell instead of stacking all nav links
- Part 2 complete: mobile card lists for dense tables; chart adaptations inspired by flip/trim/stack patterns
- Parts 3–6 complete: shared skeleton/empty states, form a11y, fade-in cards, settings page with theme/locale/currency preferences
- Drawer chosen over bottom nav because Current has eight primary destinations

