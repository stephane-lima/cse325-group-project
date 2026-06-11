# cse325-group-project
CSE325 Group Project — **Budget & Expense Tracker** (.NET 10 Blazor Web App)

Group Members:
- Stephane Edmar Lima e Lima
- Stephanie Dacullo Selanoba
- Pastor Zimondi

---

## Running the app locally

```bash
dotnet restore
dotnet run
```

Then open the URL shown in the console (e.g. `https://localhost:5001`).

---

## Login / Authentication (User Guide)

The app uses cookie-based authentication.

**Log in**
1. Click **Log in** in the left navigation, or visit `/login`.
2. Enter your email and password and press **Log in**.
3. A demo account is provided so you can sign in immediately:
   - **Email:** `demo@budgettracker.com`
   - **Password:** `Password123!`

**Create an account**
1. On the login page, click **Create one** (or visit `/register`).
2. Fill in your name, email, and password (min. 6 characters) and submit.
3. You are signed in automatically and returned to the home page.

**Log out**
- Click **Log out** in the navigation. This clears your session and returns you to the login page.

**Protected pages**
- The **Counter** page is marked `[Authorize]` as a demonstration: visiting it while
  signed out redirects you to the login page and returns you there after you sign in.

---

## Notes for the team (Database Setup / User Authentication cards)

The login UI depends only on the `IAccountService` interface
(`Services/AccountService.cs`). The current implementation, `InMemoryAccountService`,
stores users in memory with salted **PBKDF2** password hashes and seeds the demo account.

To move to a real database, implement `IAccountService` against EF Core Identity
and change **one line** in `Program.cs`:

```csharp
builder.Services.AddSingleton<IAccountService, InMemoryAccountService>();
// becomes, e.g.:
// builder.Services.AddScoped<IAccountService, EfCoreAccountService>();
```

No changes to `Login.razor` / `Register.razor` are required.


## Web application site
https://budget-and-expense-tracker.onrender.com


### 1. CRUD Implementation
* **Create:** Users can record transactional data, capturing descriptions, decimal amounts, tracking dates, and financial types (Income/Expense).
* **Read:** Financial data aggregates automatically into a historical ledger and dynamic components, mapping data directly to an interactive **Net Savings Data Chart**
* **Update & Delete:** Built-in table event handlers bind directly to an internal `DeleteTransactionAsync` safety pipeline to ensure seamless ledger adjustments and synchronized dashboard refreshes.

