# Back

ASP.NET Core Web API scaffolded with Entity Framework Core and a clear DDD-oriented layered architecture.

## Layers

- `Back.Domain`: enterprise model, aggregates, value objects, domain events, repository contracts.
- `Back.Application`: use cases, DTOs, service interfaces, orchestration, unit-of-work abstraction.
- `Back.Infrastructure`: EF Core `DbContext`, entity configurations, repository implementations, database wiring.
- `Back.Api`: ASP.NET Core composition root and HTTP controllers.

Dependency direction stays inward:

`Back.Api -> Back.Application -> Back.Domain`

`Back.Api -> Back.Infrastructure -> Back.Application -> Back.Domain`

## Run

```powershell
cd D:\projects\back
dotnet restore
dotnet build .\Back.slnx
dotnet run --project .\src\Back.Api
```

## EF Core migrations

Install the EF CLI once if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Create and apply the first migration:

```powershell
dotnet ef migrations add InitialCreate --project .\src\Back.Infrastructure --startup-project .\src\Back.Api --output-dir Persistence\Migrations
dotnet ef database update --project .\src\Back.Infrastructure --startup-project .\src\Back.Api
```

The default connection string uses SQL Server LocalDB and creates a `BackDb` database.

## Docker

The Docker setup runs the API and SQL Server together. The database is stored in a
named volume, and the API applies existing EF Core migrations when it starts.

```powershell
docker compose up --build
```

Then open Swagger UI at `http://localhost:8080/swagger`.

SQL Server is available to host tools such as SSMS at `localhost,14330`. Its local
development password is read from the ignored `.env` file; `.env.example` documents
the required variable without making `.env` part of source control.

Stop the containers without deleting the database:

```powershell
docker compose down
```

Delete the containers and the database volume for a clean start:

```powershell
docker compose down --volumes
```

## Production Docker

For a production-style Docker run, copy the production environment example and
replace the password with a strong value:

```powershell
copy .env.prod.example .env.prod
```

Then start the production compose file:

```powershell
docker compose --env-file .env.prod -f compose.prod.yaml up -d --build
```

The API listens on `http://localhost:8080`. In a real production deployment, put a
reverse proxy such as Nginx, Caddy, IIS, or a cloud load balancer in front of the
API to terminate HTTPS.

Stop the production containers without deleting the database:

```powershell
docker compose --env-file .env.prod -f compose.prod.yaml down
```

Delete the production database volume only when you intentionally want to remove
all data:

```powershell
docker compose --env-file .env.prod -f compose.prod.yaml down --volumes
```

The included SQL Server service uses `MSSQL_PID=Express`. For a serious production
system, prefer a managed SQL Server or a properly licensed SQL Server edition.

## Sample endpoints

- `GET /api/customers?page=1&pageSize=50`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PATCH /api/customers/{id}/email`
- `DELETE /api/customers/{id}`
