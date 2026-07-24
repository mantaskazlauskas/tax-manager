# Tax Manager

A small API for managing municipality tax records that are valid for specific periods, and
resolving the applicable tax rate for a municipality on a given date

## Two solutions, one problem

This repo contains **two fully independent implementations** of the same requirements, in their
own folders with their own solution files, Docker setup, and tests. They share no code - each is a
standalone, submittable deliverable:

| | [`TaxManager.Classic`](TaxManager.Classic) | [`TaxManager.Cqrs`](TaxManager.Cqrs) |
|---|---|---|
| API style | MVC Controllers | Minimal APIs |
| Application layer | Interface + service (`ITaxService`) | MediatR commands/queries + handlers, FluentValidation pipeline behavior |
| Style | Traditional / enterprise ASP.NET Core | Modern, pattern-heavy CQRS |

Both use **.NET 10**, **Clean Architecture** (Domain / Application / Infrastructure / Api),
**PostgreSQL + EF Core**, a global exception-handling middleware so unhandled errors never leak
internals to callers, **Docker Compose**, and **xUnit** tests (unit tests for the domain logic,
Testcontainers-backed integration tests for the full HTTP API). Business rules, assumptions, and
the API contract are identical between them - see either subfolder's own README for details and
run instructions.

## The core rule

A tax record covers an inclusive date range and has a period type (Yearly/Monthly/Weekly/Daily).
When several records cover the same date, the **most specific period wins**.

| Municipality | Date | Result |
|---|---|---|
| Copenhagen | Jan 1, 2024 | 0.1 (Daily beats Yearly) |
| Copenhagen | Mar 16, 2024 | 0.2 (Yearly only) |
| Copenhagen | May 2, 2024 | 0.4 (Monthly beats Yearly) |
| Copenhagen | Jul 10, 2024 | 0.2 (Yearly only) |

## Quick start

```sh
cd TaxManager.Classic && docker compose up --build   # API on :5080, Postgres on :5432
# or
cd TaxManager.Cqrs && docker compose up --build      # API on :5081, Postgres on :5433
```

Ports and container/volume names are distinct between the two, so both stacks can run at the same
time if you want to compare them side by side.
