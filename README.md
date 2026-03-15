# 🛒 E-Commerce RESTful API

A secure, scalable, and production-oriented **RESTful API** for an e-commerce platform built using **ASP.NET Core Web API (.NET 8)**.

The system provides a complete backend infrastructure capable of handling authentication, product catalog management, shopping cart operations, and secure order processing while maintaining strong architectural and security principles.

---

# 📌 Project Overview

This project demonstrates the design and development of a modern **E-Commerce backend system** built with a strong focus on:

- Security
- Performance
- Clean Architecture
- Maintainability
- Scalability

The API implements complex business logic required for real-world e-commerce systems including:

- Advanced authentication
- Product and category management
- Dynamic user carts
- Order placement and transaction handling

---

# 🏗️ Architecture

The application follows a strict **3-Tier Architecture** to ensure clear separation of responsibilities and maintain clean system design.

### 1️⃣ Presentation Layer
Handles incoming HTTP requests and responses.

Components:
- Controllers
- API endpoints
- Request validation

### 2️⃣ Business Logic Layer (BLL)
Contains the core application logic and services responsible for implementing business rules.

Components:
- Services
- DTOs
- Business validations

### 3️⃣ Data Access Layer (DAL)
Responsible for interacting with the database.

Components:
- Repositories
- Stored procedure calls
- Data mapping

---

# 🧠 Design Patterns Used

The project uses several industry-standard design patterns to improve code quality and maintainability.

### Repository Pattern
Provides abstraction between the business logic and database access.

Benefits:
- Loose coupling
- Easier testing
- Cleaner architecture

### Data Transfer Objects (DTOs)
Used to control how data is received and returned from the API.

Benefits:
- Prevent over-posting vulnerabilities
- Control API response data
- Improve maintainability

### Dependency Injection (DI)
All services are injected using ASP.NET Core's built-in dependency injection container.

---

# 🔐 Security & Authentication

Security is one of the core focuses of the system.
### API Rate Limiting

Implemented **API Rate Limiting** to protect the system from abuse, brute-force attacks, and excessive requests by limiting the number of requests a client can make within a specified time window.

### JWT Authentication
Implemented stateless authentication using **JSON Web Tokens (JWT)**.

### Refresh Token Rotation
Implemented a professional refresh token mechanism where:

- A new refresh token is issued on each refresh
- The previous token becomes invalid

### Password Security
Passwords are securely stored using:
BCrypt Hashing

This protects against:

- Brute-force attacks
- Rainbow table attacks

### Refresh Token Protection
Refresh tokens are hashed using:
SHA-256

before storing them in the database.

### Role-Based Access Control (RBAC)

Two system roles are implemented:

- **Admin**
- **User**

Admin privileges include:

- Product management
- Category management
- User management

### Resource-Based Authorization

Additional authorization checks prevent **IDOR (Insecure Direct Object Reference)** attacks.

Example:
- Users can only access **their own cart**
- Users can only access **their own orders**

---

# 🗄️ Database & Performance Optimization

### Database System

Microsoft SQL Server

### Data Access Strategy

Instead of using traditional ORMs, the system uses:

ADO.NET + Stored Procedures

Benefits:

- Maximum performance
- Full control over queries
- Guaranteed protection against SQL Injection

### Pagination

Server-side pagination is implemented for large datasets.

Examples:

- Products list
- Categories list
- Users list

Pagination metadata is returned in response headers:
X-Pagination

---

# 📊 Monitoring & Logging

The project integrates **Serilog** for structured logging.

Captured logs include:

- Application events
- Warnings
- Errors
- Exceptions
### Rate Limiting

Rate limiting has been implemented to improve system reliability and prevent API abuse by restricting the number of requests allowed per client within a specific timeframe.

Logs are written to:
Console
Daily Rolling Log Files

Example:
Logs/log-2026-03-09.txt

This ensures reliable monitoring and easier debugging in production environments.

---

# 📦 Core Modules

## 🔐 Authentication & Identity

Features:

- User Registration
- Secure Login
- JWT Access Tokens
- Refresh Token Rotation
- Secure Logout

---

## 👤 Users Management

Admin can:

- View users
- Create users
- Update users
- Delete users

---

## 📦 Product Catalog

Includes:

- Product creation (Admin)
- Product update (Admin)
- Product deletion (Admin)
- Public product browsing

---

## 🗂 Category Management

Includes:

- Category creation
- Category update
- Category deletion
- Public category browsing

---

## 🛒 Shopping Cart System

Each authenticated user has their own cart.

Features:

- Add product to cart
- Update quantity
- Remove item
- Clear cart
- Cart summary

---

## 📦 Order Processing

Handles the full order lifecycle.

Features:

- Place order
- Validate stock availability
- Create order items
- Cancel order
- Secure order management

---

# 📡 API Endpoints Overview

## Authentication

| Method | Endpoint | Description |
|------|------|------|
| POST | /api/auth/register | Create user account |
| POST | /api/auth/login | User login |
| POST | /api/auth/refresh | Refresh JWT token |
| POST | /api/auth/logout | Secure logout |

---

## Products

| Method | Endpoint | Access |
|------|------|------|
| GET | /api/products | Public |
| GET | /api/products/{id} | Public |
| POST | /api/products | Admin |
| PUT | /api/products/{id} | Admin |
| DELETE | /api/products/{id} | Admin |

---

## Categories

| Method | Endpoint | Access |
|------|------|------|
| GET | /api/categories | Public |
| GET | /api/categories/{id} | Public |
| POST | /api/categories | Admin |
| PUT | /api/categories/{id} | Admin |
| DELETE | /api/categories/{id} | Admin |

---

## Cart

| Method | Endpoint | Access |
|------|------|------|
| GET | /api/cart/my-cart | User |
| POST | /api/cart/add | User |
| PUT | /api/cart/update | User |
| DELETE | /api/cart/remove | User |
| DELETE | /api/cart/clear | User |

---

## Orders

| Method | Endpoint | Access |
|------|------|------|
| POST | /api/orders/place | User |
| POST | /api/orders/cancel/{id} | User |

---

# 📂 Project Structure
EcommerceApi
│
├── EcommerceApi
│ ├── Controllers
│ ├── Middleware
│ └── Program.cs
│
├── EcommerceBusinessLayer
│ ├── Services
│ ├── Interfaces
│ └── DTOs
│
├── EcommerceDataLayer
│ ├── Repositories
│ ├── Database
│ └── Helpers
│
└── SQL
├── Tables.sql
└── StoredProcedures.sql

---

# 💻 Technology Stack

### Backend

- ASP.NET Core Web API (.NET 8)
- C#

### Database

- Microsoft SQL Server
- T-SQL
- ADO.NET

### Security

- JWT Authentication
- BCrypt Password Hashing
- SHA-256 Token Hashing
- Role-Based Authorization
- Resource-Based Authorization
- API Rate Limiting
### Logging

- Serilog

### Documentation

- Swagger / OpenAPI

---

# 🚀 Getting Started

### 1️⃣ Setup Database

Run the SQL scripts inside:
EcommerceApi
│
├── EcommerceApi
│ ├── Controllers
│ ├── Middleware
│ └── Program.cs
│
├── EcommerceBusinessLayer
│ ├── Services
│ ├── Interfaces
│ └── DTOs
│
├── EcommerceDataLayer
│ ├── Repositories
│ ├── Database
│ └── Helpers
│
└── SQL
├── Tables.sql
└── StoredProcedures.sql
3️⃣ Run The Application
dotnet build
dotnet run
4️⃣ Test Using Swagger

Navigate to:
/swagger
to explore and test the API.
🔮 Future Improvements

Planned enhancements:

Payment integration (Stripe)

Product image upload

Order tracking system

Email notifications

Redis caching

Docker containerization

