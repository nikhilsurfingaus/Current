# Current v1.0.0

**Release date:** 2026-07-26

First production release of **Current** — a ledger-based personal finance platform.

## Live

| | URL |
|---|-----|
| **App** | https://current-au.vercel.app |
| **API** | https://current-zdw5.onrender.com |
| **Health** | https://current-zdw5.onrender.com/health |

**Demo:** `demo@current.app` / `Demo123!` — [setup guide](DEMO.md)

## Highlights

### Product
- User accounts with JWT authentication
- Double-entry ledger for transfers and payments
- Savings goals with contribution history
- Analytics (cash flow, categories, net worth)
- Peer-to-peer payments with idempotency
- Loan requests with admin approval workflow
- In-app notifications
- Mobile-responsive Angular UI with theme support

### Engineering
- ASP.NET Core 10 API with EF Core + PostgreSQL
- Angular 22 frontend
- 41 integration tests
- Docker Compose for local full stack
- GitHub Actions CI on every push
- Deployed: Vercel (UI) + Render (API) + Neon (DB)
- Serilog structured logging
- `GET /health` with database check
- Global exception handling and security headers
- Configuration via appsettings + environment variables

## Stack

```
Angular (Vercel) → ASP.NET Core (Render) → PostgreSQL (Neon)
```

## Documentation

- [Architecture](ARCHITECTURE.md)
- [API reference](API.md)
- [ER diagram](ERD.md)
- [Deployment](DEPLOYMENT.md)
- [Full release log](RELEASE_LOG.md)

## Local development

```bash
make dev    # API + Postgres
make ui     # Angular
make test   # Integration tests
```

## What's next

Post-v1.0 ideas: custom domain, screenshot assets in README, loan automation cron, rate limiting, E2E frontend tests.
