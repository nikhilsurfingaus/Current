# Current

A full-stack personal finance platform built with Angular and ASP.NET Core.

## Structure

- `backend/` — ASP.NET Core API
- `frontend/` — Angular UI (Phase 4+)
- `database/` — SQL scripts and seeds
- `docs/` — Project documentation

## Status

**Phase 1:** Backend foundation (Users + Accounts) — complete

See [Release Log](docs/RELEASE_LOG.md) for full phase-by-phase progress.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
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

### Other commands

| Command | Description |
|---------|-------------|
| `make api` | Run API only (assumes DB is up) |
| `make db-up` | Start Postgres and create database |
| `make db-down` | Stop Postgres background service |
| `make migrate` | Apply pending EF migrations |
| `make build` | Build the API |

## API endpoints (Phase 1)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/users` | List all users |
| GET | `/users/{id}` | Get user by ID |
| POST | `/users` | Create a user |
| GET | `/accounts` | List all accounts |
| GET | `/accounts/{id}` | Get account by ID |
| POST | `/accounts` | Create an account |
