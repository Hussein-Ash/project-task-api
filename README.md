# Project Task API

A REST API for managing **Projects** and their **Tasks**. A project has many tasks; deleting a project deletes its tasks. The API exposes six endpoints, returns [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) for every error, and ships with a Docker Compose setup so it runs with one command.

---

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| API | ASP.NET Core with controllers |
| ORM | EF Core 10.0.4 |
| Database | PostgreSQL 17 |
| Driver | Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 |
| Validation | DataAnnotations |
| API docs | Microsoft.AspNetCore.OpenApi + Scalar 2.16.16 |
| Testing | xUnit v3, Moq, Shouldly |

---

## Project structure

```
src/
├── ProjectTaskApi.Domain/          entities and domain exceptions; no dependencies at all
├── ProjectTaskApi.Application/     DTOs, services, repository interfaces; no EF Core
├── ProjectTaskApi.Infrastructure/  DbContext, entity configuration, repository implementations
└── ProjectTaskApi.Api/             controllers, exception handler, composition root
tests/
└── ProjectTaskApi.UnitTests/       service tests with mocked repositories
```

Dependencies point inward: `Api` → `Infrastructure` → `Application` → `Domain`. `Domain` references nothing.

The rule that gives this shape its value is that **`Application` never references EF Core**. It declares `IProjectRepository` and `ITaskRepository`; `Infrastructure` implements them. That is what allows the services to be tested against mocks with no database, and it is enforceable — `dotnet list src/ProjectTaskApi.Application package --include-transitive` shows no EF Core.

`Api` referencing `Infrastructure` is deliberate: as the composition root it needs the concrete types in order to register them.

---

## Getting started

### With Docker (recommended)

Requires Docker Desktop.

```bash
docker compose up
```

That builds the API, starts PostgreSQL, waits for it to pass its healthcheck, applies migrations, and serves the API.

| | |
|---|---|
| API | http://localhost:8080 |
| API reference (Scalar) | http://localhost:8080/scalar/v1 |
| OpenAPI document | http://localhost:8080/openapi/v1.json |
| PostgreSQL | `localhost:5433` |

To stop, and to discard the database volume as well:

```bash
docker compose down -v
```

### Running locally

Requires the .NET 10 SDK and a reachable PostgreSQL.

Start just the database, then run the API against it:

```bash
docker compose up -d db
dotnet run --project src/ProjectTaskApi.Api
```

The API listens on http://localhost:8080. The connection string lives under `ConnectionStrings:Default` in `src/ProjectTaskApi.Api/appsettings.json`.

> **Why port 5433?** The container publishes PostgreSQL on **5433**, not the default 5432. If something is already bound to 5432 on your machine — a locally installed PostgreSQL service, most commonly — Docker reports the mapping but never acquires the port, and every connection silently reaches the wrong server. Using 5433 removes that failure mode. Inside the Compose network the API still reaches the database on 5432 via the `db` hostname, so this affects local runs only.

---

## Database initialization

The schema is managed by EF Core migrations, committed under `src/ProjectTaskApi.Infrastructure/Migrations`.

**Automatically:** on startup, **in the Development environment only**, the API applies any pending migrations. `docker-compose.yml` sets `ASPNETCORE_ENVIRONMENT=Development`, so a fresh `docker compose up` produces a working schema with no extra step.

This is scoped to Development on purpose. Auto-migrating on production startup races between instances and removes the opportunity to review a schema change before it lands. Outside Development, apply migrations explicitly:

```bash
dotnet ef database update --project src/ProjectTaskApi.Infrastructure --startup-project src/ProjectTaskApi.Api
```

That needs the EF tools, if you do not already have them:

```bash
dotnet tool install --global dotnet-ef
```

### Schema

```
projects
  id           uuid          PK
  name         varchar(200)  NOT NULL
  created_at   timestamptz   NOT NULL

tasks
  id           uuid          PK
  project_id   uuid          NOT NULL  FK -> projects(id) ON DELETE CASCADE
  title        varchar(200)  NOT NULL
  completed    boolean       NOT NULL DEFAULT false
  created_at   timestamptz   NOT NULL

  INDEX ix_tasks_project_id ON tasks(project_id)
```

---

## API endpoints

Six endpoints, at the paths given in the brief, with no `/api` prefix.

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/projects` | List projects, paginated |
| `POST` | `/projects` | Create a project |
| `GET` | `/projects/{id}` | Get a project with its tasks |
| `POST` | `/projects/{id}/tasks` | Create a task for a project |
| `PUT` | `/tasks/{id}` | Replace a task |
| `DELETE` | `/tasks/{id}` | Delete a task |

### `GET /projects`

| Parameter | Type | Default | Range |
|---|---|---|---|
| `page` | int | 1 | ≥ 1 |
| `pageSize` | int | 20 | 1–100 |

Ordered by `created_at` descending, so pages do not overlap.

```bash
curl "http://localhost:8080/projects?page=1&pageSize=20"
```

```json
{
  "items": [
    {
      "id": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
      "name": "Website Redesign",
      "createdAt": "2026-07-26T18:17:10.361675+00:00"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

### `POST /projects`

```bash
curl -X POST http://localhost:8080/projects \
  -H "Content-Type: application/json" \
  -d '{"name":"Website Redesign"}'
```

`201 Created`, with `Location: /projects/{id}`.

```json
{
  "id": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
  "name": "Website Redesign",
  "createdAt": "2026-07-26T18:17:10.361675+00:00"
}
```

### `GET /projects/{id}`

| Parameter | Type | Behaviour |
|---|---|---|
| `completed` | bool? | Omitted returns all tasks; `true` only completed; `false` only incomplete |

```bash
curl "http://localhost:8080/projects/019f9fa5-2dd9-72d8-a119-15e387b0ea02?completed=true"
```

```json
{
  "id": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
  "name": "Website Redesign",
  "createdAt": "2026-07-26T18:17:10.361675+00:00",
  "tasks": [
    {
      "id": "019f9fa5-a857-79b3-af7f-937945c809e6",
      "projectId": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
      "title": "Ship it",
      "completed": true,
      "createdAt": "2026-07-26T18:17:41.719335+00:00"
    }
  ]
}
```

### `POST /projects/{id}/tasks`

```bash
curl -X POST http://localhost:8080/projects/019f9fa5-2dd9-72d8-a119-15e387b0ea02/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Design homepage"}'
```

`201 Created`, with `Location: /tasks/{id}`. New tasks always start incomplete, so `completed` is not accepted here.

```json
{
  "id": "019f9fa5-a851-715e-853c-f8f15cc03ab3",
  "projectId": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
  "title": "Design homepage",
  "completed": false,
  "createdAt": "2026-07-26T18:17:41.713372+00:00"
}
```

### `PUT /tasks/{id}`

A full replacement, so **both** `title` and `completed` are required.

```bash
curl -X PUT http://localhost:8080/tasks/019f9fa5-a851-715e-853c-f8f15cc03ab3 \
  -H "Content-Type: application/json" \
  -d '{"title":"Design homepage v2","completed":true}'
```

```json
{
  "id": "019f9fa5-a851-715e-853c-f8f15cc03ab3",
  "projectId": "019f9fa5-2dd9-72d8-a119-15e387b0ea02",
  "title": "Design homepage v2",
  "completed": true,
  "createdAt": "2026-07-26T18:17:41.713372+00:00"
}
```

### `DELETE /tasks/{id}`

```bash
curl -X DELETE http://localhost:8080/tasks/019f9fa5-a851-715e-853c-f8f15cc03ab3
```

`204 No Content`.

### Status codes

| Endpoint | Success | Failure |
|---|---|---|
| `GET /projects` | 200 | 400 |
| `POST /projects` | 201 | 400 |
| `GET /projects/{id}` | 200 | 404 |
| `POST /projects/{id}/tasks` | 201 | 400, 404 |
| `PUT /tasks/{id}` | 200 | 400, 404 |
| `DELETE /tasks/{id}` | 204 | 404 |

---

## Error responses

Every error is Problem Details, so clients parse one shape regardless of what failed. A single `IExceptionHandler` maps domain exceptions to status codes, which is why no controller contains a `try`/`catch`.

### 404

Messages name both the entity type and the id that was not found.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Project with ID '019f9999-0000-7000-8000-00000000dead' was not found.",
  "instance": "/projects/019f9999-0000-7000-8000-00000000dead"
}
```

### 400

Validation failures carry a per-field `errors` object.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

### 500

Unrecognised exceptions are logged in full and answered with a generic body carrying no stack trace or internal detail, in every environment.

---

## Running tests

```bash
dotnet test
```

15 tests covering every path through both services, including all failure modes. Repositories are mocked, so no database is required.

The mocks are strict: an unexpected repository call fails the test rather than passing quietly. Several tests assert on calls that must *not* happen — no project is saved when the name invariant fails, and no task is saved against a project that does not exist.

---

## Assumptions

**Architecture.** Layered, with dependencies pointing inward. At six endpoints a single project would genuinely suffice; the separation is here to make the dependency rule explicit and to keep the service layer testable without EF Core. Deliberately absent for the same reason: MediatR, CQRS, AutoMapper, FluentValidation, a generic `IRepository<T>`, and Unit of Work. Six endpoints do not justify them.

**Routes** are exactly as the brief specifies, with no `/api` prefix, even though a prefix is a common convention.

**`TaskItem`, not `Task`.** The entity avoids colliding with `System.Threading.Tasks.Task`, which would force awkward qualification throughout an async codebase. The table is still `tasks` and the route is still `/tasks`.

**Identifiers are UUID v7** (`Guid.CreateVersion7()`), which is time-ordered, so inserts append to the index instead of fragmenting it — the usual objection to random UUID keys in PostgreSQL.

**Names and titles are trimmed, and whitespace-only values are rejected** with 400. `RequiredAttribute` trims before testing for emptiness, so `"   "` fails validation at the model-binding layer. The domain factories independently trim and reject empty values, so the invariant holds even for callers that bypass the API surface.

**New tasks always start incomplete.** `completed` is not accepted when creating a task.

**`PUT` is a full replacement**, so both `title` and `completed` are required. `completed` is modelled as a nullable bool specifically so that omitting it fails validation — a non-nullable bool cannot express absence, and a missing value would silently deserialize to `false`, quietly clearing the flag and making `PUT` a partial update in disguise.

**Tasks cannot move between projects.** `projectId` is immutable after creation and is not accepted on update.

**Deleting a project cascades** to its tasks, enforced by the foreign key rather than by application code.

**Migrations apply automatically in Development only**, never in production.

**Timestamps** are stored as `timestamptz` and serialized as ISO 8601 with offset.

**Paging defaults to 20 and is capped at 100** to bound response size. Results are sorted by `created_at` descending; paging without a deterministic sort returns overlapping rows between pages.

**The task completion filter is a query parameter** on `GET /projects/{id}` rather than a separate `GET /projects/{id}/tasks` endpoint. The brief asks for six endpoints, so the API exposes six.

**No authentication.** The brief does not ask for it, and adding it would mean inventing a security model the assignment does not describe.
