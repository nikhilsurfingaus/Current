# Current

A full-stack personal finance platform built with Angular and ASP.NET Core.

## Structure

- `backend/` — ASP.NET Core API
- `frontend/` — Angular UI (Phase 4+)
- `database/` — SQL scripts and seeds
- `docs/` — Project documentation

## Status

**Phase 3:** Authentication — complete  
**Phase 4:** Angular frontend — next

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

Protected endpoints require a JWT. In Swagger: `POST /auth/login` → copy `token` → **Authorize** → `Bearer <token>`.

### Other commands

| Command | Description |
|---------|-------------|
| `make api` | Run API only (assumes DB is up) |
| `make db-up` | Start Postgres and create database |
| `make db-down` | Stop Postgres background service |
| `make migrate` | Apply pending EF migrations |
| `make build` | Build the API |

## API endpoints

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
