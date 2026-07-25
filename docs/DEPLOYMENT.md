# Deployment Guide

## Phase 12 progress

| Part | Status |
|------|--------|
| Docker (local) | Done |
| CI/CD (GitHub Actions) | Done |
| Neon (production DB) | You set up — see below |
| Render (API) | Part 4 |
| Vercel (UI) | Part 5 |

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

Set these on **Render** (Part 4), not in code:

| Variable | Example |
|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Neon connection string |
| `Jwt__Key` | Long random secret (32+ chars) |
| `Jwt__Issuer` | `Current.Api` |
| `Jwt__Audience` | `Current.Client` |

---

## Part 4 preview — Render API

- Connect repo → build from `backend/Current.Api/Dockerfile` or `dotnet publish`
- Add env vars above
- API auto-runs migrations on startup (`ApplyMigrationsAsync` in `Program.cs`)

## Part 5 preview — Vercel UI

- Deploy `frontend/current-ui`
- Set `environment.ts` production `apiUrl` to your Render URL
- Configure CORS on API for your Vercel domain
