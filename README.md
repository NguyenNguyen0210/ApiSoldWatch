# ShopNN - Fullstack E-Commerce Platform

ShopNN is a modern, production-ready, fullstack e-commerce web application. The platform features a robust layered backend API built with ASP.NET Core 8, a reactive and state-of-the-art frontend build using React 19 and Vite, a comprehensive suite of unit tests, and fully dockerized multi-container deployment scripts.

---

## 🚀 Key Features

### 💻 Backend (ASP.NET Core Web API)
- **Architecture:** Structured with the Repository-Service pattern for clean separation of concerns.
- **Security:** Secure authentication and authorization using **JWT Tokens** featuring a robust **Refresh Token** rotation mechanism.
- **Payment Integration:** Live integration with **VnPay Payment Gateway** (utilizing HMAC-SHA512 hashing, sorted parameter queries, and callback verification).
- **Global Error Handling:** Implemented a centralized exception handling middleware to return standardized HTTP response wrappers.
- **Database:** Auto-migrated SQL Server with custom seed data configuration.

### ⚛️ Frontend (React & Modern SPA)
- **Build System:** Fast development and builds powered by **Vite**.
- **State Management:** Lightweight and reactive client state management using **Zustand**.
- **Server Cache & Data Fetching:** Highly efficient server-state management, cache invalidation, and background synchronization via **TanStack React Query (v5)**.
- **UI Components:** Styled with **Ant Design (Antd)** for clean, unified, and responsive components.

### 🧪 Quality Assurance (Unit Testing)
- **Coverage:** **88 Unit Tests** targeting core business workflows (Order, Cart, Account, Payment, Product, Category, and Authentication services).
- **Tools:** Implemented with `xUnit`, `Moq` for dependency injection mocking, and `FluentAssertions` for readable assert criteria.
- **Advanced Testing:** Mocking of entity framework database transactions (`IDbContextTransaction`) for complex transactional code verification.

### 🐳 DevOps & Deployment
- **Dockerized System:** Multi-container packaging using `docker-compose.yml`.
- **Database Readiness:** Configured Docker container dependencies with **database healthchecks** (`service_healthy`) to ensure the API only starts after the DB is fully online.
- **Production Routing:** Frontend React production build served via **Nginx** reverse proxy in a containerized environment.

---

## 🛠️ Technology Stack

| Layer | Technologies & Libraries |
| :--- | :--- |
| **Backend API** | ASP.NET Core 8, EF Core, SQL Server, AutoMapper, MailKit |
| **Frontend SPA** | React 19, Vite, Zustand, TanStack React Query (v5), Ant Design, Axios |
| **Testing** | xUnit, Moq, FluentAssertions |
| **DevOps / Infra** | Docker, Docker Compose, Nginx |

---

## 📂 Project Structure

```text
ShopNN/
├── ShopNN/              # Backend ASP.NET Core Web API
├── ShopNN-app/          # Frontend React SPA
├── ShopNN.Tests/        # Unit Tests project using xUnit
├── docker-compose.yml   # Multi-container orchestrator configuration
├── seed_data.sql        # Database initialization script
└── ShopNN.sln           # Visual Studio Solution file
```

---

## ⚡ Quick Start (Docker Compose)

The easiest way to boot the entire system (Database, Backend API, and Frontend App) is using Docker Compose.

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Launching the Application
1. Clone the repository and navigate to the project root:
   ```bash
   cd ShopNN
   ```
2. Run the following command to build and launch all containers:
   ```bash
   docker-compose up -d --build
   ```
3. Docker will spin up three containers:
   - **Database (SQL Server):** Port `1433`
   - **Backend API:** Port `5290` (Swagger will be available at `http://localhost:5290/swagger/index.html`)
   - **Frontend App:** Port `5173` (Access the UI at `http://localhost:5173`)

To stop the containers, simply run:
```bash
docker-compose down
```

---

## 🔧 Local Development Setup

If you prefer to run the components locally without Docker:

### 1. Database Setup
- Ensure you have **SQL Server** running on your local machine.
- Update the connection string under `ConnectionStrings:DefaultConnection` in `ShopNN/appsettings.json`.

### 2. Run Backend API
```bash
cd ShopNN
dotnet restore
dotnet run
```
The API will start listening, and you can access Swagger to test API endpoints directly.

### 3. Run Frontend React SPA
Make sure you have [Node.js](https://nodejs.org/) installed.
```bash
cd ShopNN-app
npm install
npm run dev
```
Open your browser and navigate to `http://localhost:5173`.

---

## 🧪 Running Unit Tests

To run the entire test suite of **88 tests** and verify everything works:

```bash
dotnet test
```

Expected output:
```text
Passed!  - Failed:     0, Passed:    88, Skipped:     0, Total:    88, Duration: ...
```

---

## 📝 Authors

* **Nguyen Nguyen** - *Fullstack Developer*
