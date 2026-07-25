# Current

A full-stack personal finance platform built with Angular and ASP.NET Core.

## Tech stack

[![Angular](https://img.shields.io/badge/Angular-DD0031?style=flat-square&logo=angular&logoColor=white)](https://angular.dev/)
[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white)](https://www.docker.com/)
[![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-2088FF?style=flat-square&logo=githubactions&logoColor=white)](https://github.com/features/actions)

## Deployment

Production runs as three managed services; local dev uses Homebrew Postgres (or Docker).

[![Vercel](https://img.shields.io/badge/Frontend-Vercel-000000?style=flat-square&logo=vercel&logoColor=white)](https://vercel.com/)
[![Render](https://img.shields.io/badge/API-Render-000000?style=flat-square&logo=render&logoColor=white)](https://render.com/)
[![Neon](https://img.shields.io/badge/Database-Neon-00E599?style=flat-square&logo=neondatabase&logoColor=black)](https://neon.tech/)

```mermaid
flowchart LR
  Browser --> Vercel["Vercel<br/>Angular UI"]
  Vercel --> Render["Render<br/>ASP.NET Core API"]
  Render --> Neon["Neon<br/>PostgreSQL"]
```

| Environment | UI | API | Database |
|-------------|----|-----|----------|
| **Production** | [Vercel](https://vercel.com/) — `frontend/current-ui` | [Render](https://render.com/) — Docker | [Neon](https://neon.tech/) |
| **Local dev** | `make ui` → localhost:4200 | `make dev` → localhost:5231 | Homebrew Postgres (`CurrentDb`) |
| **Local Docker** | `make docker-up` → localhost:4200 | same stack | Postgres container |

Production API: `https://current-zdw5.onrender.com`

See [Deployment Guide](docs/DEPLOYMENT.md) for Neon, Render, and Vercel setup. After deploying the UI, add your Vercel URL to API CORS in `CorsExtensions.cs`.

## Structure

- `backend/` — ASP.NET Core API
- `frontend/current-ui/` — Angular app (`current-ui`)
- `brand/` — Logo source files
- `database/` — SQL scripts and seeds
- `docs/` — Project documentation

## Status

**Phase 12 (DevOps):** Docker, CI/CD, Neon, and Render API — complete. Vercel UI — in progress.

See [Release Log](docs/RELEASE_LOG.md) for full phase-by-phase progress.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (Angular CLI via `npm` in `frontend/current-ui`)
- [PostgreSQL 17](https://formulae.brew.sh/formula/postgresql@17) via Homebrew

```bash
brew install postgresql@17
```

Add Postgres to your PATH (add to `~/.zshrc`):

```bash
export PATH="/opt/homebrew/opt/postgresql@17/bin:$PATH"
```

Update `backend/Current.Api/appsettings.Development.json` if your macOS username is not `nikhil`.

## Quick start

From the project root:

```bash
make dev
```

This will:

1. Start PostgreSQL (if not running)
2. Create `CurrentDb` (if it doesn't exist)
3. Apply EF Core migrations
4. Run the API with hot reload

API: http://localhost:5231  
Swagger: http://localhost:5231/swagger

Protected endpoints require a JWT. In Swagger: `POST /auth/login` → copy `token` → **Authorize** → `Bearer <token>`.

### Frontend (Phase 4)

From the project root:

```bash
make ui
```

UI: http://localhost:4200

Install dependencies first (once):

```bash
cd frontend/current-ui && npm install
```

### Other commands

| Command | Description |
|---------|-------------|
| `make api` | Run API only (assumes DB is up) |
| `make db-up` | Start Postgres and create database |
| `make db-down` | Stop Postgres background service |
| `make migrate` | Apply pending EF migrations |
| `make build` | Build the API |
| `make ui` | Run Angular dev server |
| `make build-ui` | Build Angular app |
| `make test` | Run backend integration tests |
| `make docker-up` | Build and start Postgres + API + UI (Docker) |
| `make docker-down` | Stop Docker stack |
| `make docker-logs` | Follow Docker container logs |

## Production database (Neon)

See [Deployment Guide](docs/DEPLOYMENT.md) for Neon setup. After creating a Neon project, run migrations:

```bash
export ConnectionStrings__DefaultConnection='your-neon-connection-string'
make migrate-neon
```

Do not commit real connection strings.

## CI

[![Build](https://github.com/nikhilsurfingaus/Current/actions/workflows/build.yml/badge.svg)](https://github.com/nikhilsurfingaus/Current/actions/workflows/build.yml)

Every push to `master` runs GitHub Actions: build API, run backend tests, build Angular.

## Docker

Run the full stack with one command (requires [Docker Desktop](https://www.docker.com/products/docker-desktop/)):

```bash
make docker-up
```

| Service | URL |
|---------|-----|
| UI | http://localhost:4200 |
| API | http://localhost:5231 |
| Swagger | http://localhost:5231/swagger |

Optional: copy `.env.example` to `.env` to override JWT and ports. Defaults are fine for local Docker.

Stop the stack:

```bash
make docker-down
```


### Phase 1

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/users/me` | Required | Current user profile |
| GET | `/users/{id}` | Required | Own profile only |
| GET | `/accounts` | Required | List own accounts |
| GET | `/accounts/{id}` | Required | Get own account by ID |
| POST | `/accounts` | Required | Create account for current user |

### Phase 2

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/transactions/transfer` | Required | Transfer between own accounts |
| GET | `/transactions` | Required | List own transactions |
| GET | `/transactions/{id}` | Required | Get own transaction by ID |

### Phase 3

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/auth/register` | Public | Register and receive JWT |
| POST | `/auth/login` | Public | Login and receive JWT |
