# Secure Task Manager API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](./LICENSE)
[![Status](https://img.shields.io/badge/status-active%20development-brightgreen)]()


## Why ths project stands out

| Feature | Implementation |
| :--- | :--- |
| **Stateless JWT Auth** | Access token stored exclusively in `HttpOnly`, `Secure`, `SameSite=Lax` cookies — completely invisible to JavaScript. Zero XSS token-theft surface. |
| **Production-Ready Patterns** | Global exception middleware, FluentValidation, user secrets, EF Core migrations with seed data. |
|**Event-based action control**| events for processing actions with tasks|


## Architecture overwiew

src/  
├── **todoApp.Web**/ # ASP.NET Controllers & Middleware  
├── **todoApp.DI**/ # Dependency Injection Configuration  
├── **todoApp.Core**/ # Core entities, enums, DTOs, Events, Interfaces, Services   
├── **todoApp.Data**/ # EF Core, repositories, JWT provider, Micrations  
└── **todoApp.Tests**/ # Xunit unit and integrations tests


## Key design decisions

- **Business logic is framework-agnostic.** `Domain` and `Application` layers have zero dependencies on ASP.NET or EF Core packages. You could swap the web framework and keep everything intact.

- **Claims-based user identity.** User ID lives in JWT claims, not in request bodies or route parameters. No client can impersonate another user.

### Token Storage: Why Cookies?
We chose `HttpOnly` cookies and eliminated the XSS vector entirely. The remaining CSRF risk is handled through the **double-submit cookie pattern**.

### Authentication Flow
[POST /auth/login] → Valid credentials? → JWT in HttpOnly cookie   
[POST /auth/logout] → Clear cookie server-side   
[GET /tasks] → Middleware extracts claims → Authorized user context


## In  Development

- Habit tracker which transforms simple task manager into a personal analytics platform with daily habit streaks
- Mood tracker with statistic and graph of mood for a month
- More tests!
- Protection from CSRF attacks
- CI/CD pipeline
- Transfer to posgreSQL
- Transfer to https protocol

## Quick Start
```
# clone
git clone https://github.com/dwarfzeker/secure-task-manager-api.git  
cd secure-task-manager-api

# apply secrets
dotnet user-secrets set "Jwt:Key" "your-256-bit-secret-key-here"

#run with docker
docker-compose up # starts frontend and backend by orchestration

```
SwaggerUI available at http://localhost/swagger with full endpoint documentation

## Testing 

|Type|Scope|Tool|
| :--- | :--- |:--- |
Unit Tests	|Domain logic, streak calculation	|xUnit|
Integration Tests|	API endpoints with in-memory DB|	WebApplicationFactory

## Roadmap
* [X] Stateless JWT authentication via HttpOnly cookies

* [X] Clean Architecture layer separation

* [ ] Habit & Mood tracking with statistics

* [ ] CSRF protection (double-submit cookie pattern)

* [ ] Refresh token rotation with database-backed family tracking

* [X] Rate limiting on authentication endpoints

* [ ] Health check endpoints for container orchestration

* [ ] CI/CD pipeline with GitHub Actions

## Built With
Runtime: .NET 9

Framework: ASP.NET Core Web API

ORM: Entity Framework Core

Database: SQLite

Validation: FluentValidation

Auth: Custom JWT provider (no external identity service)

Containerization: Docker + Docker Compose

## Contact   
Built by Polina.

GitHub: @dwarfzeker

Telegram: @polinafurkalo
