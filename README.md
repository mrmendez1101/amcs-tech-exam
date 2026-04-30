# Marketplace API

A 2-tier C# REST API for a job marketplace connecting customers to contractors. Built for scale (10M customers, 100K contractors) using .NET 8, Dapper, and PostgreSQL.

## Prerequisites

| Tool | Purpose |
|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Required for both run modes below |
| .NET 8 SDK | Required only if running the API outside Docker |

---

## Option A - Run everything in Docker (recommended)

The `Dockerfile` and `docker-compose.yml` are in the **repo root**. All commands below must be run from that root folder (`amcs-tech-exam/`).

```bash
# 1. Clone and enter the repo root
git clone <repo-url>
cd amcs-tech-exam

# 2. Build the image and start all services
docker compose up --build
```

That single command:
- Builds the API image from `Dockerfile`
- Starts Postgres 15, waits for it to be healthy
- Runs all DbUp migrations automatically on API startup
- Starts Adminer (DB browser)

**What's running after startup:**

| Service | URL | Credentials |
|---|---|---|
| API | http://localhost:8080 | - |
| Swagger UI | http://localhost:8080/swagger | - |
| Adminer (DB browser) | http://localhost:8081 | Server: `postgres`, User/Pass/DB: `marketplace` |
| Postgres | localhost:5432 | User/Pass/DB: `marketplace` |

**Verify it's working:**

```bash
# Should return {"items":[],"total":0}
curl "http://localhost:8080/customers?page=1&pageSize=20"
```

Or open http://localhost:8080/swagger in your browser.

**Subsequent runs (image already built):**

```bash
docker compose up
```

**Stop and keep the database:**

```bash
docker compose down
```

**Full reset (drop all data):**

```bash
docker compose down -v
docker compose up --build
```

---

## Option B - Run the API locally with dotnet run

Use this when you want faster iteration without rebuilding the Docker image.

```bash
# 1. Start only Postgres in Docker (from the repo root)
cd amcs-tech-exam
docker compose up postgres -d

# 2. Run the API locally (from the API project folder)
cd src/Job.Marketplace.API
dotnet run
```

The API starts on http://localhost:8080. The connection string in `appsettings.Development.json` already points to `localhost:5432` with the same credentials as the Docker Compose setup, so no extra configuration is needed.

Stop Postgres when done:

```bash
# From the repo root
docker compose down
```

---

## API endpoint reference

All responses are JSON. Errors follow [RFC 7807 ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807).

### Customers

```bash
# Search by last name prefix or exact id (paginated)
curl "http://localhost:8080/customers?term=smi&page=1&pageSize=20"

# Get one
curl "http://localhost:8080/customers/<customer-id>"
```

### Contractors

```bash
# Search by name
curl "http://localhost:8080/contractors?term=acme"

# Get one
curl "http://localhost:8080/contractors/<contractor-id>"
```

### Jobs

```bash
# Create
curl -X POST http://localhost:8080/jobs \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "<customer-id>",
    "startDate": "2026-05-01",
    "dueDate":   "2026-05-15",
    "budget":    1500.00,
    "description": "Repaint the back fence"
  }'

# Read
curl "http://localhost:8080/jobs/<job-id>"

# Update
curl -X PUT http://localhost:8080/jobs/<job-id> \
  -H "Content-Type: application/json" \
  -d '{ "budget": 1800.00, "description": "Repaint and reseal" }'

# Delete
curl -X DELETE "http://localhost:8080/jobs/<job-id>"
```

### Job offers

```bash
# Contractor submits an offer on an open job
curl -X POST "http://localhost:8080/jobs/<job-id>/offers" \
  -H "Content-Type: application/json" \
  -d '{ "contractorId": "<contractor-id>", "price": 1700.00 }'

# Customer accepts an offer
curl -X POST "http://localhost:8080/jobs/<job-id>/offers/<offer-id>/accept"
```

---

## Running tests

Docker must be running for integration tests (Testcontainers spins up its own Postgres container).

```bash
# From the repo root

# Unit tests only (no Docker required)
dotnet test tests/Job.Marketplace.UnitTests

# Integration tests (Docker required)
dotnet test tests/Job.Marketplace.IntegrationTests

# Everything
dotnet test
```

---

## Seed data

After the API is up (migrations must have run first):

```bash
# Demo seed - small dataset for local exploration
docker exec -i marketplace_postgres \
  psql -U marketplace -d marketplace < db/seed/seed-demo.sql

# Bulk seed - 100K+ rows for EXPLAIN ANALYZE and load testing
docker exec -i marketplace_postgres \
  psql -U marketplace -d marketplace < db/seed/seed-bulk.sql
```

Clear seeded data without dropping the schema:

```bash
docker exec -i marketplace_postgres psql -U marketplace -d marketplace \
  -c "TRUNCATE job_offers, jobs, contractors, customers RESTART IDENTITY CASCADE;"
```

---

## Architecture

### Vertical Slice Architecture

The codebase is organised by **feature** rather than by technical layer. Each slice owns its own request, validator, handler, SQL queries, and endpoint registration.

```
src/
  Job.Marketplace.Domain/              # entities, value objects, business rules
    Customer.cs  Contractor.cs  Job.cs  JobOffer.cs  JobStatus.cs

  Job.Marketplace.Infrastructure/      # cross-cutting concerns (separate class library)
    DependencyInjection.cs             # AddInfrastructure() - wires everything in one call
    Endpoints/                         # IEndpoint interface (static abstract Map)
    Persistence/                       # IDbConnectionFactory, NpgsqlConnectionFactory, DapperConfig
    Migrations/                        # DbUp runner + versioned .sql scripts (embedded resources)
    Errors/                            # ProblemDetails extensions
    Time/                              # IClock, SystemClock

  Job.Marketplace.API/
    Program.cs                         # builder.Services.AddInfrastructure(...) + slice registrations
    GlobalUsings.cs
    Features/
      Customers/
        Search/                        # SearchCustomersRequest/Response/Queries/Handler/Endpoint
        GetById/                       # GetCustomerByIdQueries/Handler/Endpoint
      Contractors/
        Search/                        # SearchContractorsRequest/Response/Queries/Handler/Endpoint
      Jobs/
        Create/  GetById/  Update/  Delete/
      JobOffers/
        Create/  Accept/

tests/
  Job.Marketplace.UnitTests/           # handler + validator unit tests (NSubstitute mocks)
  Job.Marketplace.IntegrationTests/    # endpoint tests against real Postgres (Testcontainers)
```

### Why Dapper over EF Core

- **Predictable SQL**. At 10M rows, query plans matter. Dapper means the exact SQL you wrote is what runs.
- **No proxying**. Domain entities can be sealed with private setters and factory methods.
- **Hand-rolled migrations via DbUp**. Schema is reviewable as plain `.sql` files in source control. Indexes (including the `pg_trgm` GIN index for prefix search) are explicit and version-controlled.
- **Trade-off**: more boilerplate per slice (write the SQL yourself). VSA contains that boilerplate to one file per feature.

### Persistence layer

`Job.Marketplace.Infrastructure/Persistence/IDbConnectionFactory.cs` produces an open `NpgsqlConnection` per call. Slice-local `Queries` classes use that connection plus parameterised Dapper calls. There is no generic repository.

Migrations live in `Job.Marketplace.Infrastructure/Migrations/Scripts/` as embedded `.sql` resources and run on app startup via DbUp. Failed migrations crash the API so that broken schema cannot ship.

Infrastructure services are registered via a single extension method - `builder.Services.AddInfrastructure(builder.Configuration)` - defined in `DependencyInjection.cs`. This also calls `DapperConfig.Apply()` so `Program.cs` stays clean.

---

## Design decisions and trade-offs

| Decision | Rationale | Trade-off |
|---|---|---|
| Vertical Slice Architecture | Feature-aligned, easy to test and reason about. | Some duplication across slices. Acceptable. |
| Dapper instead of EF Core | Predictable SQL at 10M-row scale, simpler domain model. | More boilerplate per slice; manual schema migrations. |
| DbUp for migrations | Plain `.sql` files; reviewable, version-controlled. | No model diffing; you write `ALTER TABLE` by hand. |
| Postgres `pg_trgm` GIN index | Enables fast prefix search at scale. | Larger index size. |
| FluentValidation | Validators are testable in isolation; clean separation from DTOs. | Extra dependency. Earned. |
| Slice-local `IQueries` interfaces | Handler tests mock data access without mocking `IDbConnection`. | One small interface per slice. Acceptable cost. |
| `Job.Marketplace.Infrastructure` project | Isolates cross-cutting concerns behind `AddInfrastructure()`. `Program.cs` stays clean. | One extra project reference. |
| Minimal APIs over controllers | Less ceremony, fits VSA naturally. | Less familiar to teams accustomed to MVC. |

### What is deliberately not implemented

- **Authentication / authorisation**. Out of scope per spec.
- **Distributed cache (Redis)**. The bonus calls for an LRU specifically.
- **Soft delete on Customers / Contractors**. Jobs use a status enum; entity-wide soft-delete adds complexity beyond the spec.

---

## Roadmap

What is implemented:

- [x] Phase 1: Customers / Contractors search, Jobs CRUD, Job Offers create + accept.
- [x] Phase 1: Validation, ProblemDetails errors, pagination, indexes.
- [x] Phase 1: Unit tests + integration tests via Testcontainers.
- [x] Phase 1: Dockerised API + Postgres + Adminer via `docker compose`.
- [ ] Phase 2 bonus: LRU (Lease Recently Used) accounts cache. $\color{red}{\textbf{Not yet implemented.}}$ 

---

## Project structure (top-level)

```
.
+-- docker-compose.yml           # runs API + Postgres + Adminer
+-- Dockerfile                   # multi-stage build for the API
+-- .dockerignore
+-- Job.Marketplace.sln
+-- README.md
+-- src/
|   +-- Job.Marketplace.Domain/
|   +-- Job.Marketplace.Infrastructure/
|   |   +-- DependencyInjection.cs
|   +-- Job.Marketplace.API/
|       +-- Features/
+-- tests/
    +-- Job.Marketplace.UnitTests/
    +-- Job.Marketplace.IntegrationTests/
```

## License

MIT
