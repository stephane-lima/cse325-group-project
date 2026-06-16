# cse325-group-project
CSE325 Group Project — **Budget & Expense Tracker** (.NET 10 Blazor Web App)

Group Members:
- Stephane Edmar Lima e Lima
- Stephanie Dacullo Selanoba
- Pastor Zimondi

# Budget & Expense Tracker — User Guide

This README is a concise user guide for the deployed Budget & Expense Tracker web app.
Access the live site at: https://budget-and-expense-tracker.onrender.com

Quick overview
- What it does: track income and expenses, maintain a transaction ledger, and manage savings goals.
- Key pages: Dashboard, Transactions, Financial Goals, Account (Login / Register).

Demo account (try instantly)
- Email: demo@budgettracker.com
- Password: Password123!

Using the app (web)

1) Sign in
- Visit the site and click **Log in** (top-right or from the menu).
- Enter your email and password, then submit.

2) Create an account
- From the login screen click **Create one** or open `/register`.
- Provide a display name, valid email and a password (minimum 6 characters).
- After successful registration you are automatically signed in.

3) Dashboard (Home)
- The Dashboard displays Total Net Balance, Total Incomes, Total Expenses and a short list of recent transactions.
- Two charts visualize cash flow and cumulative savings over time.

4) Transactions
- Open **Transactions** to add, view, or remove transaction entries. Use the Quick Add Transaction form to submit a transaction with:
  - Transaction Label, Type (Income / Expense), Category, Amount and Date.
- Transactions appear in the history table
- Use the Remove button to delete an entry.

5) Financial Goals
- Create savings goals with a target amount, starting saved amount, and due date.
- Add or remove saved amounts for a goal using the input next to each goal.
- The Current Goals and Completed Goals card display the target amount, the saved amount, the date (due/completed), and a progress bar that reflects the percent completion.

Account & security notes
- The site uses cookie-based authentication. Signing out clears your session until you sign in again.
- Passwords are stored as salted PBKDF2 hashes (server-side) 
- The app never stores plain-text passwords.

Troubleshooting (common issues)
- Can't log in: verify email and password (try the demo account first). If you just registered, ensure registration succeeded and try again.
- Page errors: refresh the page or try signing out and in again. If the error persists, contact the site administrator.
- Missing charts: ensure your browser allows loading https://cdn.jsdelivr.net (used for Chart.js).

Privacy & data
- This app stores data in a database for each account. If you use the public demo instance, seeded demo data is available for testing and may be reset periodically.

Support & contact
- For questions or to report issues, open an issue in the project repository or contact the maintainers (see project metadata).

### Developer Notes (Code Documentation)

- The project includes XML-style comments on the public models and services to make generating API docs simpler.
- Generate XML docs by adding `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to the .csproj and building.
- Key code locations:
  - `Data/AppDbContext.cs` — EF Core context and seed data.
  - `Services/AccountService.cs` — Registration, credential validation, and claims principal creation.
  - `Components/Account/Register.razor` and `Components/Account/Login.razor` — Authentication UI and form bindings.


### 1. CRUD Implementation
* **Create:** Users can record transactional data, capturing descriptions, decimal amounts, tracking dates, and financial types (Income/Expense).
* **Read:** Financial data aggregates automatically into a historical ledger and dynamic components, mapping data directly to an interactive **Net Savings Data Chart**
* **Update & Delete:** Built-in table event handlers bind directly to an internal `DeleteTransactionAsync` safety pipeline to ensure seamless ledger adjustments and synchronized dashboard refreshes.

