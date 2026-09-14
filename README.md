# GamingRigz – Gaming PC Parts Sales & Inventory System 🖥️🎮🛒

A Windows desktop application for managing sales and inventory of gaming rig parts and accessories. GamingRigz handles user accounts, stock management, order creation, order history, sales reporting, and receipt generation for a small gaming hardware shop.

Designed for real-world shop use, this system prioritizes:

- 📦 Accurate, easy-to-update stock tracking
- 🧾 Fast order creation with receipt generation at checkout
- 📊 Clear sales reporting for day-to-day business decisions
- 🔐 Simple, secure account access for shop staff

Current production version: **v1**

🔗 **Live Demo:** _Not applicable — this is a Windows desktop (WinForms) application, not a web app._

## 🌍 Project Overview

GamingRigz is built for a gaming PC parts retailer to manage day-to-day shop operations from a single desktop app — logging in staff, tracking stock levels, creating and archiving orders, reviewing order history, and generating sales reports.

The system helps shop staff:

- Keep an accurate, centralized record of stock on hand
- Create new orders quickly and print/generate receipts
- Look back at past orders and archived records when needed
- Review sales performance through generated reports
- Recover account access via a password retrieval flow

## 🧩 Core Features

- 🔐 **Login, Register, and Password Retrieval** — staff account management with recovery flow
- 📦 **Stock management** (`Stocks.cs`) — track and update gaming rig parts and accessories inventory
- 🧾 **Order creation** (`CreateOrder.cs`) — build new orders from available stock
- 🧮 **Order history** (`OrderHistory.cs`) — review past orders
- 🗄️ **Archive** (`Archive.cs`) — store and retrieve archived records (older orders/stock)
- 📊 **Sales reports** (`SalesReport.cs`) — summarize sales activity
- 🧾 **Receipt generation** (`ReceiptUtility.cs`) — produce receipts for completed orders
- 🗃️ **SQL Server–backed data layer** via `DatabaseHelper.cs` / `DatabaseHelpercs.cs`
- 🚦 **Color-coded stock levels** — green/yellow/red indicators in `Stocks.cs` flag healthy, low, and critical inventory at a glance

## 🧾 Order Flow

```
Login
   ↓
Browse Stock (Stocks)
   ↓
Create Order (CreateOrder) → Product records pulled from Stocks
   ↓
Generate Receipt (ReceiptUtility)
   ↓
Order saved → Order History
   ↓
Older orders/records moved to Archive
   ↓
Sales Report generated from order data
```

| Login Screen | Stocks / Order Screen |
|--------------|------------------------|
| `_(add login screenshot here)_` | `_(add stocks or create-order screenshot here)_` |

## 📋 Database Schema Reference

Schema is defined via SQL Server Database Projects (`Database1/` and `db/`), under `dbo/Tables`:

| Table / Script | Description |
|-----------------|--------------|
| `createaccount.sql` | Staff/user accounts (login credentials) |
| `stocks.sql` | Gaming rig parts & accessories inventory |
| `Cart.sql` | In-progress order/cart items before checkout |
| `archive.sql` | Archived orders/stock records |
| `dbo.Table.sql` | Additional/reference table definition |
| `dbo.orderhistory.data(.sql)` | Order history records/seed data |
| `dbo.stocks*.data.sql` | Stock seed/reference data (multiple revisions) |

> ⚠️ Note: The repo contains two SQL Server Database Projects (`Database1/` and `db/`) with overlapping table definitions — worth consolidating into a single source of truth for the schema going forward.

## 🧱 System Architecture

```
WinForms UI (Login, Stocks, CreateOrder, OrderHistory, SalesReport, Archive)
        ↓
   DatabaseHelper / DatabaseHelpercs (ADO.NET data access)
        ↓
   SQL Server Database (dbo.Tables — accounts, stocks, cart, archive, order history)
```

## 📁 Project Structure

```
GamingRigz/
├── Database1/           # SQL Server Database Project (schema)
├── db/                  # Secondary SQL Server Database Project (overlapping schema — see note above)
├── GamingRigz/           # WinForms application project
│   ├── Properties/ Resources/     # App metadata and icons
│   ├── *.cs / *.Designer.cs / *.resx   # Forms: Login, Register, PasswordRetrieval,
│   │                                    # Stocks, CreateOrder, OrderHistory, Archive, SalesReport
│   ├── DatabaseHelpercs.cs / Product.cs / Program.cs
│   └── App.config / GamingRigz.csproj / packages.config
├── GamingRigz.sln
├── DatabaseHelper.cs / ReceiptUtility.cs
├── SQLQuery1.sql / SQLQuery1(1).sql
├── dbo.*.data(.sql)      # Seed/reference data
└── README.md
```

## 🛠️ Tech Stack

- **Language:** C#
- **UI Framework:** Windows Forms (.NET Framework), with color-coded conditional formatting for stock levels
- **Database:** SQL Server (T-SQL)
- **Schema Management:** SQL Server Database Projects (SSDT) — `Database1/`, `db/`
- **Data Access:** ADO.NET via custom `DatabaseHelper` classes
- **IL Weaving:** Fody (`FodyWeavers.xml`)
- **Package Management:** NuGet (`packages.config`)
- **Version Control:** Git, GitHub

## ▶️ Running Locally

1. Clone the repo
2. Open `GamingRigz.sln` in Visual Studio
3. Restore NuGet packages (Visual Studio will prompt automatically, or run `nuget restore`)
4. Deploy the database schema:
   - Open the `Database1` or `db` SQL Server Database Project
   - Publish it to a local SQL Server / SQL Server Express instance
5. Update the connection string in `App.config` to point to your local database
6. Set `GamingRigz` as the startup project
7. Build and run (F5) to launch the application

## 🚀 Future Roadmap

- ✅ Login, Register, and Password Retrieval
- ✅ Stock management
- ✅ Order creation and receipt generation
- ✅ Order history and archive
- ✅ Sales reporting
- ✅ Color-coded stock level indicators (green/yellow/red)
- 🔜 Consolidate `Database1` and `db` into a single database project
- 🔜 Export sales reports to PDF/Excel
- 🔜 Automated low-stock reorder notifications (beyond the visual indicator)
- 🔜 Role-based access (cashier vs. admin/manager)
- 🔜 Packaged installer (MSI/ClickOnce) for easier deployment

## 🤝 Contributing

Pull requests welcome! Please:

- ❌ Do not commit `desktop.ini` or other OS-generated files (add to `.gitignore`)
- 🔒 Never commit real customer or account data
- 🗄️ Keep schema changes in the database project(s), not as ad-hoc `SQLQuery*.sql` scripts
- 🧪 Test against a local SQL Server instance before submitting changes

## About

A Windows desktop application for managing sales and inventory of gaming rig parts, with account login, stock tracking, order creation, order history, archiving, and sales reporting. 🖥️🎮🛒

### Topics

`csharp` `winforms` `sql-server` `tsql` `inventory-management` `pos`

---

⭐ Stars · 👀 Watchers · 🍴 Forks
