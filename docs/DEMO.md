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
3. Enter the 6-digit verification code from your email (or from Render logs if SMTP is not configured yet)
4. Log in and create an **Everyday** account (triggers $2,500 welcome credit)

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

Then verify (replace `123456` with the code from email or logs):

```bash
curl -X POST https://current-zdw5.onrender.com/auth/verify-email \
  -H "Content-Type: application/json" \
  -d '{
    "email": "demo@current.app",
    "code": "123456"
  }'
```

Log in on the UI and create an account.

### Optional — Admin access (branch / loan approval)

To demo **Branch admin** (`/branch/admin`), promote the user in Neon SQL Editor:

```sql
UPDATE "Users"
SET "Role" = 'Admin'
WHERE "Email" = 'demo@current.app';
```

Log out and log back in so the JWT picks up the new role.

## What reviewers can try

1. **Dashboard** — balance summary, recent activity
2. **Accounts** — create Everyday / Savings accounts
3. **Transfer** — move money between own accounts
4. **Pay someone** — send to another user's email
5. **Goals** — create a goal, contribute, view history
6. **Analytics** — charts and cash flow
7. **Loans** — request a loan (approve as admin if promoted)
8. **Notifications** — bell icon in top bar
9. **Settings** — theme, currency, timezone

## Reset demo data

To wipe and recreate the demo user, delete the row in Neon (cascades depend on FK constraints — delete accounts/transactions first or drop and re-run migrations only in a dev Neon branch).

For a quick reset of one user, use Neon SQL Editor to delete related rows for that `UserId`, then register again.
