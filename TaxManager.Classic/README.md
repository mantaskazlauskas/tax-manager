# TaxManager.Classic

Municipality tax management API - "classic" ASP.NET Core style: MVC controllers, a simple
interface + service application layer, EF Core over PostgreSQL. See the [top-level README](../README.md)
for why this exists alongside [`TaxManager.Cqrs`](../TaxManager.Cqrs).

## Architecture

Clean Architecture, four projects:

- **TaxManager.Domain** - `Municipality`/`TaxRecord` entities and the `TaxRateResolver`, the pure
  priority-resolution logic described below. No dependencies.
- **TaxManager.Application** - `ITaxService`/`TaxService`, DTOs, repository interfaces. Manual
  guard-clause validation, no external validation library.
- **TaxManager.Infrastructure** - EF Core `DbContext`, entity configurations, migrations,
  repository implementations.
- **TaxManager.Api** - ASP.NET Core MVC controllers, the global exception-handling middleware,
  Swagger.

## Business rule: resolving a tax rate

A tax record covers an inclusive date range and has a period type (Yearly/Monthly/Weekly/Daily).
When several records cover the same date, the **most specific period wins**: Daily > Weekly >
Monthly > Yearly. This exactly reproduces the example in `requirements.md`:

| Date | Matches | Result |
|---|---|---|
| Jan 1, 2024 | Yearly (0.2) + Daily (0.1) | **0.1** (Daily wins) |
| Mar 16, 2024 | Yearly (0.2) only | **0.2** |
| May 2, 2024 | Yearly (0.2) + Monthly (0.4) | **0.4** (Monthly wins) |
| Jul 10, 2024 | Yearly (0.2) only | **0.2** |

See `TaxManager.Domain/Services/TaxRateResolver.cs` and its unit tests.

## Documented assumptions

- Overlapping ranges of the *same* period type for the *same* municipality are rejected at
  creation/update (400) - there'd be no defined tie-breaker. Overlaps *across* different period
  types are expected; that's what the resolver's priority order is for.
- Municipalities are get-or-created (case-insensitive name match) when a tax record is added -
  there's no separate municipality-management endpoint.
- Querying a date with no applicable record returns `404`, not a `0` rate - "not configured" and
  "0%" are different things.
- `Rate` is `decimal`; dates are `DateOnly`.

## API

| Method | Route | Notes |
|---|---|---|
| `POST` | `/api/tax-records` | `{ municipalityName, periodType, startDate, endDate, rate }` → 201 |
| `PUT` | `/api/tax-records/{id}` | Same body shape (bonus: update) → 200 |
| `GET` | `/api/municipalities/{name}/tax-rate?date=yyyy-MM-dd` | → 200 `{ municipality, date, rate, periodType }` |

`periodType` is one of `Yearly`, `Monthly`, `Weekly`, `Daily`.

Swagger UI is available at `/swagger` when running in Development.

## Error handling

`GlobalExceptionHandler` (`IExceptionHandler`) catches everything that reaches the end of the
pipeline. Known domain exceptions map to a precise 4xx `ProblemDetails` response; anything else is
logged in full server-side and reported to the caller as a generic 500 with no internal detail.

## Running

### Docker Compose (recommended)

```sh
docker compose up --build
```

Starts Postgres + the API (port **5080**), applying EF Core migrations automatically on startup.

```sh
curl -X POST http://localhost:5080/api/tax-records \
  -H "Content-Type: application/json" \
  -d '{"municipalityName":"Copenhagen","periodType":"Yearly","startDate":"2024-01-01","endDate":"2024-12-31","rate":0.2}'

curl "http://localhost:5080/api/municipalities/Copenhagen/tax-rate?date=2024-03-16"
```

### Locally

Requires a Postgres instance reachable via the `ConnectionStrings:TaxManagerDb` in
`src/TaxManager.Api/appsettings.json` (defaults to `localhost:5432`).

```sh
dotnet run --project src/TaxManager.Api
```

## Tests

```sh
dotnet test
```

- `TaxManager.Domain.UnitTests` - the resolver against the requirements table plus edge cases
  (no match, boundary dates, tie precedence). No I/O.
- `TaxManager.Api.IntegrationTests` - full HTTP round trips against a real Postgres instance
  spun up via Testcontainers (independent of docker-compose - Docker must be running).
