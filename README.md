# Task Scheduling & Daily Productivity Tracking API (.NET 8)

A production-ready RESTful API built with **ASP.NET Core 8**, **Entity Framework Core (SQL Server)**, **ASP.NET Core Identity + JWT**, and **Clean Architecture**.

## Solution Structure

- `src/TaskProductivity.Domain` — Core Entities (`Project`, `TaskItem`, `TimeLog`), Enums, Domain Exceptions, Repository Interfaces.
- `src/TaskProductivity.Application` — Use-case Services, DTOs, FluentValidation Validators, Mapping Extensions.
- `src/TaskProductivity.Infrastructure` — `AppDbContext`, EF Core Entity Configurations & Initial Migration, Repository Implementations, ASP.NET Core Identity & JWT Token Generator.
- `src/TaskProductivity.API` — REST Controllers, Global Exception Middleware (`ProblemDetails`), Validation Filter, Swagger/OpenAPI with JWT Bearer UI.

## Quick Start

1. Update `ConnectionStrings:DefaultConnection` in `src/TaskProductivity.API/appsettings.json` to point to your SQL Server instance.
2. Apply EF Core migrations (or run the API in `Development` mode, which runs `dbContext.Database.MigrateAsync()` automatically):
   ```bash
   dotnet ef database update --project src/TaskProductivity.Infrastructure --startup-project src/TaskProductivity.API
   ```
3. Run the API:
   ```bash
   dotnet run --project src/TaskProductivity.API
   ```
4. Open Swagger UI at `https://localhost:7150/` (or `http://localhost:5150/`), register via `POST /api/auth/register`, copy the `accessToken`, click **Authorize**, and enter `Bearer <your_token>`.
