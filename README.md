# SummerProject1

ASP.NET Core (.NET 8) backend for a VIN check web app.

## Features

- 3 logical sources: `Auctions`, `Inspections`, `Listings`
- User can run **one** source or **all** sources
- Stripe Checkout payment flow
- Checks execute **only after payment** (Stripe webhook)
- SQL Server (local Docker) + Azure SQL (production)
- EF Core migrations
- Background worker to run provider jobs
- Dummy providers with deterministic pseudo-results based on VIN
- Swagger/OpenAPI

## Quick start (local)

### 1) Prereqs
- .NET 8 SDK
- Docker Desktop

### 2) Start SQL Server

```bash
docker compose up -d
```

### 3) Configure env vars

Copy `.env.example` to `.env` (optional) or export environment variables.

Required variables:
- `ConnectionStrings__Default` (or use appsettings.Development.json)
- `Stripe__SecretKey`
- `Stripe__WebhookSecret`
- `Stripe__SuccessUrl`
- `Stripe__CancelUrl`

### 4) Run migrations

```bash
dotnet tool restore

dotnet ef database update --project src/SummerProject1.Infrastructure --startup-project src/SummerProject1.Api
```

### 5) Run API

```bash
dotnet run --project src/SummerProject1.Api
```

Open Swagger: `http://localhost:5080/swagger`

## Stripe notes

- The backend creates a Stripe Checkout Session for each check.
- After payment, Stripe calls `POST /api/stripe/webhook`.
- The webhook marks the check as paid and enqueues background execution.

For local webhook testing, use Stripe CLI and set `Stripe__WebhookSecret` accordingly.

## Project structure

- `src/SummerProject1.Api` - Web API
- `src/SummerProject1.Infrastructure` - EF Core DbContext, migrations, providers

