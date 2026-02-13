# NeuroEase — Role-Based Mental Health Practice Platform (Backend)

NeuroEase is a backend-focused project built to demonstrate **production-ready backend engineering** practices:
- Role-based access control (RBAC)
- Clean Architecture / layered design
- Secure authentication (JWT)
- Auditing & logging
- Scalable API patterns
- (Optional) Event-driven notifications

> This repository is intentionally designed as a "European signal project" for backend roles: reliability, maintainability, and security hygiene.

---

## Features (Current / Planned)

### ✅ Current
- REST API (ASP.NET Core)
- Layered architecture (Domain / Application / Infrastructure / API)
- Basic RBAC foundations (roles/permissions)
- Database integration (to be configured via environment variables)

### 🧭 Planned (Recommended for EU interview signal)
- JWT Authentication + Refresh Tokens
- Audit Log (who did what, when)
- Background Jobs (e.g., reminder scheduling)
- Event-driven notifications (RabbitMQ)
- Docker Compose for local development
- GitHub Actions CI (build + test)
- Unit + integration tests

---

## Tech Stack

- **Backend:** C# / ASP.NET Core
- **Database:** PostgreSQL (recommended) / SQL Server (optional)
- **Auth:** JWT
- **DevOps:** Docker, GitHub Actions
- **Messaging (optional):** RabbitMQ
- **Testing:** xUnit

---

## Architecture

This project follows a Clean Architecture-inspired structure:

- `Domain` / `Core` : Entities, Value Objects, Domain rules
- `Application`     : Use-cases, DTOs, Interfaces
- `Infrastructure`  : EF Core, DB, external services
- `API`             : Controllers, middleware, auth, DI

> Goal: isolate business logic from frameworks and infrastructure.

---

## Getting Started (Local)

### Prerequisites
- Docker + Docker Compose
- .NET SDK (if running without Docker)

## Run locally (Docker)

1) Create local env file:
- macOS/Linux:
```bash


cp .env.example .env
