# Entity relationship diagram

Core domain model for **Current**. All monetary movement flows through `Transaction` + `LedgerEntry`.

## High-level ERD

```mermaid
erDiagram
  User ||--o{ Account : owns
  User ||--o{ Goal : owns
  User ||--o{ Loan : requests
  User ||--o{ Contact : has
  User ||--o{ Notification : receives
  User ||--o{ IdempotencyKey : uses

  Account ||--o{ Transaction : from_account
  Account ||--o{ Transaction : to_account
  Account ||--o{ LedgerEntry : has

  Transaction ||--|{ LedgerEntry : contains
  Transaction ||--o| GoalContribution : records

  Goal ||--o| Account : goal_account
  Goal ||--o| Account : source_account
  Goal ||--o{ GoalContribution : has

  Branch ||--|| Account : treasury

  Loan }o--|| User : borrower
  Loan }o--|| Branch : via
  Loan }o--|| Account : funded_into
  Loan }o--o| Transaction : disbursement
  Loan ||--o{ LoanRepayment : has

  LoanRepayment }o--|| Transaction : payment

  User {
    uuid Id PK
    string Email UK
    string PasswordHash
    string Role
    string PreferredCurrency
  }

  Account {
    uuid Id PK
    uuid UserId FK
    string Name
    string AccountType
    decimal CurrentBalance
    string Currency
  }

  Transaction {
    uuid Id PK
    uuid FromAccountId FK
    uuid ToAccountId FK
    decimal Amount
    string Category
    string Status
  }

  LedgerEntry {
    uuid Id PK
    uuid TransactionId FK
    uuid AccountId FK
    string EntryType
    decimal Amount
  }

  Goal {
    uuid Id PK
    uuid UserId FK
    uuid SourceAccountId FK
    uuid GoalAccountId FK
    decimal TargetAmount
    decimal CurrentAmount
    string Status
  }

  Loan {
    uuid Id PK
    uuid UserId FK
    uuid FundedAccountId FK
    decimal Principal
    decimal OutstandingPrincipal
    string Status
  }

  Notification {
    uuid Id PK
    uuid UserId FK
    string Title
    string Body
    string NotificationType
    bool IsRead
  }
```

## Entity summary

| Entity | Purpose |
|--------|---------|
| **User** | Registered user; auth, profile, preferences, role (`User` / `Admin`) |
| **Account** | User wallet (Everyday, Savings, Investment); holds balance |
| **Transaction** | Money movement record between two accounts |
| **LedgerEntry** | Debit or credit line tied to a transaction (double-entry) |
| **Goal** | Savings target with linked goal account |
| **GoalContribution** | Deposit or withdrawal against a goal |
| **Contact** | Saved payee (name + email) |
| **IdempotencyKey** | Dedupes payment requests per user |
| **Branch** | System branch (Current HQ) with treasury account |
| **Loan** | User loan request → approval → active repayment lifecycle |
| **LoanRepayment** | Payment applied against loan principal/interest |
| **Notification** | In-app alerts (security, payments, system) |

## Ledger invariant

For every completed transfer or payment:

- Sum of **debits** = sum of **credits**
- `Transaction.Amount` matches each leg
- Account balances updated in the same DB transaction

## Branch / loans

- **Treasury** — seeded with system float; debited for welcome credits and approved loans
- **Welcome credit** — $2,500 on first user-created account (configurable in `Branch` options)
- **Loans** — pending → approved (treasury disbursement) → active → repaid / rejected / cancelled
