# Demo account

A shared demo login lets interviewers and reviewers explore the live app without registering.

## Credentials

| Field | Value |
|-------|-------|
| **URL** | https://current-au.vercel.app/login |
| **Email** | `demo@current.app` |
| **Password** | `Demo123!` |

## One-time setup (project owner)

The demo user must exist in **Neon** (production database). Passwords are hashed — create the account via the API or UI, not raw SQL.

### Option A — Register on the live site

1. Open https://current-au.vercel.app/register
2. Register with the credentials above
3. You are logged in automatically — create an **Everyday** account (triggers $2,500 welcome credit)

### Option B — Register via API

```bash
curl -X POST https://current-zdw5.onrender.com/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Demo",
    "lastName": "User",
    "email": "demo@current.app",
    "password": "Demo123!"
  }'
```

The response includes a JWT. Log in on the UI and create an account.

### Optional — Admin access (branch / loan approval)

To demo **Branch admin** (`/branch/admin`), promote the user in Neon SQL Editor:

```sql
UPDATE "Users"
SET "Role" = 'Admin'
WHERE "Email" = 'demo@current.app';
```

Log out and log back in so the JWT picks up the new role.
