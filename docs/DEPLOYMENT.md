# Deployment Guide

## Phase 12 progress

| Part | Status |
|------|--------|
| Docker (local) | Done |
| CI/CD (GitHub Actions) | Done |
| Neon (production DB) | You set up — see below |
| Render (API) | Done |
| Vercel (UI) | Done |

---

## Part 3 — Neon production database

### Step 1: Create Neon account

1. Go to [https://neon.tech](https://neon.tech) and sign up (free tier).
2. Create a project, e.g. **`current-prod`**.
3. Region: pick one close to you (e.g. `ap-southeast-1` for Australia).

### Step 2: Get connection string

In Neon dashboard → **Connection details**:

- Enable **Connection pooling** (optional for serverless; direct is fine for Render).
- Copy the **connection string** — it looks like:

```
postgresql://user:password@ep-xxxx.ap-southeast-1.aws.neon.tech/neondb?sslmode=require
```

Convert to .NET format (or use the URI as-is — Npgsql accepts both):

```
Host=ep-xxxx.ap-southeast-1.aws.neon.tech;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require;Trust Server Certificate=true
```

**Never commit this string to Git.**

### Step 3: Store locally (for migrations only)

Create a file **outside the repo** or use your shell:

```bash
# In ~/.zshrc or a one-off export (do NOT commit)
export ConnectionStrings__DefaultConnection='Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true'
```

Or save to `~/.current-neon.env` (any path **not** in the repo):

```bash
echo "export ConnectionStrings__DefaultConnection='YOUR_NEON_STRING'" >> ~/.current-neon.env
source ~/.current-neon.env
```

### Step 4: Run migrations against Neon

From project root:

```bash
make migrate-neon
```

Or manually:

```bash
cd backend/Current.Api
dotnet ef database update
```

(with `ConnectionStrings__DefaultConnection` set in your environment)

You should see all EF migrations apply. Neon dashboard → **Tables** will show `Users`, `Accounts`, etc.

### Step 5: Verify

```bash
# Replace with your Neon connection string (one-off, not saved in repo)
psql "$DATABASE_URL" -c '\dt'
```

Or use Neon's **SQL Editor** in the dashboard: `SELECT * FROM "__EFMigrationsHistory";`

### What NOT to do

- Do not `pg_dump` your local Mac database into Neon (schema conflicts, wrong owner).
- Do not paste connection strings in chat, GitHub issues, or commits.
- Local `make dev` still uses Homebrew Postgres — Neon is **production only**.

---

## Environment variables (production)

Set these on **Render**, not in committed code. Secrets never go in `appsettings.json`.

### Required

| Variable | Example | Purpose |
|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables production config + middleware |
| `ConnectionStrings__DefaultConnection` | Neon .NET connection string | PostgreSQL |
| `Jwt__Key` | Random 32+ char secret | JWT signing |
| `Jwt__Issuer` | `Current.Api` | JWT issuer |
| `Jwt__Audience` | `Current.Client` | JWT audience |

### CORS (Vercel frontend)

Production origins are read from config (`Cors:AllowedOrigins`). Defaults are in `appsettings.Production.json`. Override on Render when your Vercel URL changes:

| Variable | Example |
|----------|---------|
| `Cors__AllowedOrigins__0` | `http://localhost:4200` |
| `Cors__AllowedOrigins__1` | `https://current-au.vercel.app` |

Add `__2`, `__3`, etc. for extra origins (custom domains).

**Local dev** (`make dev` + `make ui`) allows any `localhost` origin automatically — no CORS env vars needed.

### Config file hierarchy

| File | When used |
|------|-----------|
| `appsettings.json` | Base defaults |
| `appsettings.Development.json` | `make dev`, local API |
| `appsettings.Production.json` | Render (`ASPNETCORE_ENVIRONMENT=Production`) |
| Environment variables | Override any setting (Render dashboard) |

---

## Part 4 — Render API

- Connect repo → build from `backend/Current.Api/Dockerfile`
- Add env vars above
- API auto-runs migrations on startup (`ApplyMigrationsAsync` in `Program.cs`)
- **Health Check Path:** `/health` (Settings → Health Checks)

Test after deploy:

```bash
curl -i https://your-service.onrender.com/health
```

Expected: `200 OK` with body `Healthy` when the database is reachable.

## Part 5 — Vercel UI

- Deploy `frontend/current-ui` (root directory, output `dist/current-ui/browser`)
- Production `apiUrl` in `environment.ts` points at your Render URL
- CORS origins in `appsettings.Production.json` or Render env vars (`Cors__AllowedOrigins__*`)
