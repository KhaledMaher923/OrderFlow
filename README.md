# 📦 OrderFlow: Enterprise Inventory & Order Management System

<div align="center">

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18+-61DAFB?style=flat&logo=react&logoColor=black)](https://react.dev/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Status](https://img.shields.io/badge/Status-Completed-success.svg)]()

**OrderFlow** is a robust, full-stack enterprise inventory and order management platform engineered to streamline stock control, automate sales invoice processing, and provide real-time business insights. Developed as a graduation project for the **Digital Egypt Pioneers Initiative (DEPI)**.

</div>

---

## 🚀 Key Features

* **Advanced Inventory Management:** Complete CRUD operations for products, categories, and stock items with automated SKU tracking.
* **Automated Order Processing & Invoicing:** Seamless generation of sales invoices, dynamic calculation of order items, and real-time stock deduction upon checkout.
* **Stock Movement & Audit History:** Comprehensive tracking of stock adjustments, returns handling, and low-stock alerts.
* **Supplier & Purchase Tracking:** End-to-end management of suppliers, purchase orders, and stock replenishment workflows.
* **Role-Based Access Control (RBAC):** Secure authentication and authorization utilizing **ASP.NET Identity** / JWT tokens with role-specific dashboard views.
* **Real-Time Dashboards & Analytics:** Interactive visual data for sales performance, inventory turnover, and transaction history.

---

## 🛠️ Tech Stack

### **Backend**
* **Framework:** ASP.NET Core Web API (.NET 8)
* **ORM:** Entity Framework Core (EF Core)
* **Database:** Microsoft SQL Server
* **Authentication:** ASP.NET Core Identity / JWT

### **Frontend**
* **Library:** React.js (Vite)
* **Styling:** Tailwind CSS / Modern UI Components
* **State Management & Routing:** React Router, Context API / Axios for HTTP requests

---

## 📐 Architecture & Design Patterns

* **Clean / Layered Architecture:** Clear separation of concerns across API Controllers, Business Logic Services, and Data Access Repositories.
* **Repository & Unit of Work Patterns:** Ensures maintainability, testability, and transactional data integrity across complex order-fulfillment workflows.
* **RESTful API Principles:** Structured endpoint design with robust error handling and response wrapping.

---

## 📁 Project Structure

```text
OrderFlow/
│
├── backend/                  # ASP.NET Core Web API Solution
│   ├── OrderFlow.API/        # Controllers, Middleware, Program.cs
│   ├── OrderFlow.Core/       # Domain Entities, Interfaces, DTOs
│   ├── OrderFlow.Services/   # Business Logic & Service Implementations
│   └── OrderFlow.Infrastructure/ # EF Core Context, Migrations, Repositories
│
├── frontend/                 # React Single Page Application
│   ├── src/
│   │   ├── components/       # Reusable UI elements & layouts
│   │   ├── pages/            # Dashboard, Products, Orders, Suppliers
│   │   ├── services/         # API integration modules (Axios)
│   │   └── App.jsx
│   └── package.json
│
└── README.md
