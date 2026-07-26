# Deployment Guide

## Phase 12 progress

| Part | Status |
|------|--------|
| Docker (local) | Done |
| CI/CD (GitHub Actions) | Done |
| Neon (production DB) | You set up — see below |
| Render (API) | Done |
| Vercel (UI) | Done |
| Documentation | Done |

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

## Email verification (Resend)

Signup sends a 6-digit code. The API sends mail via the **Resend HTTP API** (HTTPS). Render blocks outbound SMTP (ports 25/587/465), so do **not** rely on SMTP on Render.

Set env vars on Render only (never commit API keys to git).

### 1. Resend account

1. Sign up at [resend.com](https://resend.com)
2. **API Keys** → create a key (`re_...`)
3. For testing without a custom domain, use sender **`onboarding@resend.dev`** (Resend only delivers to addresses allowed on your Resend account — typically the email you signed up with)

### 2. Render environment variables

Render dashboard → your API service → **Environment** → add:

| Variable | Value |
|----------|--------|
| `Email__Enabled` | `true` |
| `Email__FromAddress` | `onboarding@resend.dev` |
| `Email__FromName` | `Current` |
| `Email__ApiKey` | your Resend API key (`re_...`) |

Use double underscores (`__`) — that maps to `Email:ApiKey` in .NET config.

**Already set SMTP vars?** If you have `Email__SmtpHost=smtp.resend.com` and `Email__SmtpPassword=re_...`, the API will use the HTTP API automatically (no SMTP connection). You can remove the SMTP vars and set `Email__ApiKey` instead.

Save → Render redeploys the API. After deploy, register or tap **Resend code** on the verify page; check inbox and spam.

### 3. Verify it is working

- **Working:** startup log shows `Email verification: Resend API enabled`; verification email arrives
- **Not working:** codes only in Render logs → `Email__ApiKey` missing/wrong, or redeploy not finished

### 4. Custom domain (later)

When you own a domain (e.g. `current.app`):

1. Resend → **Domains** → add domain → add DNS records
2. Change `Email__FromAddress` to e.g. `noreply@yourdomain.com`
3. Redeploy

### Local dev

Leave `Email:Enabled` false in `appsettings.json` — codes print in the API console (`make dev`).

## Demo account

See [Demo account guide](DEMO.md) to register `demo@current.app` on production for portfolio reviewers.

