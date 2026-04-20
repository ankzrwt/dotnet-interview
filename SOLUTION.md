# Solution Documentation

**Candidate Name:** Ankita Rawat
**Completion Date:** 2026-04-19

---

## Changes Made

### New Files Added
- **`TodoApi/Services/ITodoService.cs`** — Service interface so the controller isn't tied to the concrete class.
- **`TodoApi/Models/CreateTodoRequest.cs`** — Request model for creating a todo, has `[Required]` and `[MinLength(1)]` on Title.
- **`TodoApi/Models/UpdateTodoRequest.cs`** — Same idea as above but for updates.
- **`TodoApi.Tests/TodoControllerTests.cs`** — Controller unit tests using Moq, covers the main HTTP response scenarios.

### Modified Files

#### `TodoApi/Models/Todo.cs`
- `string Title` changed to `string Title = string.Empty` to fix nullable warnings.
- `string Description` changed to `string? Description` — it's nullable in the DB so the model should reflect that.

#### `TodoApi/Services/TodoService.cs`
- Implements `ITodoService` now.
- Connection string comes from `IConfiguration` instead of being hardcoded. There's also a second constructor that takes the string directly, mostly for tests.
- Moved the `CREATE TABLE` setup into the constructor — no point having it in two places.
- All SQL queries now use parameterized inputs (`$title`, `$id` etc.) instead of string interpolation — this was the biggest issue, straight up SQL injection risk.
- Fixed the `reader.GetString(2)` crash — checks `IsDBNull` first now since Description can be null.
- Switched from column index numbers to `GetOrdinal("ColumnName")` — index-based access breaks silently if anyone changes the query.
- Pulled out a `MapRow` helper so the reader logic isn't copy-pasted between `GetAllTodos` and `GetTodoById`.
- `UpdateTodo` returns `null` if nothing was updated rather than returning whatever was passed in.

#### `TodoApi/Controllers/TodoController.cs`
- Constructor now takes `ITodoService` — got rid of `new TodoService()` inside every action.
- Route changed from `[Route("api")]` to `[Route("api/todos")]`.
- All the POST-only endpoints replaced with proper HTTP verbs:
  - `POST /api/todos` — create
  - `GET /api/todos` — list all
  - `GET /api/todos/{id}` — get one
  - `PUT /api/todos/{id}` — update
  - `DELETE /api/todos/{id}` — delete (returns 204)
- Create now returns `201 Created` with a Location header.
- Removed the `try/catch` blocks — they were just returning raw exception messages to the client which isn't great.
- Moved the inline request classes out to the Models folder.

#### `TodoApi/Program.cs`
- Removed `InitializeDatabase()` — that's the service's job now.
- Registered `ITodoService`/`TodoService` as Scoped.
- Added `public partial class Program {}` at the bottom in case we want to use `WebApplicationFactory` later.

#### `TodoApi/appsettings.json`
- Added `ConnectionStrings:DefaultConnection`.

#### `TodoApi.Tests/TodoApi.Tests.csproj`
- Added Moq.

#### `TodoApi.Tests/UnitTest1.cs`
- Rewrote all tests. The old ones all hit the same `todos.db` file so they were sharing state and depending on each other's side effects.
- Each test now gets its own temp database file (deleted on cleanup). Used `Pooling=False` in the connection string so SQLite releases the file lock immediately.
- Covers the happy path and failure cases for each method.

---

## Problems Identified

### 1. SQL Injection Vulnerabilities
Every query in `TodoService` used string interpolation to build SQL, for example:
```csharp
command.CommandText = $"SELECT * FROM Todos WHERE Id = {id}";
command.CommandText = $"INSERT INTO Todos ... VALUES ('{todo.Title}', ...)";
```
An attacker could pass `1; DROP TABLE Todos--` as an id or embed quotes in a title to manipulate the query.

### 2. No Dependency Injection
`TodoService` was instantiated with `new TodoService()` inside every controller action. This means:
- The controller is tightly coupled to the concrete class.
- The service cannot be replaced with a mock for testing.
- There is no way to swap implementations (e.g. a different data store) without editing the controller.

### 3. No Service Interface
`TodoService` had no interface, making it impossible to write unit tests for the controller without hitting a real database.

### 4. Wrong HTTP Methods / Non-RESTful Routes
All four endpoints used `POST` and had verb-prefixed names (`createTodo`, `getTodo`, `updateTodo`, `deleteTodo`). REST conventions map operations to HTTP verbs and resource URLs:

| Before | After |
|---|---|
| `POST /api/createTodo` | `POST /api/todos` |
| `POST /api/getTodo` | `GET /api/todos` / `GET /api/todos/{id}` |
| `POST /api/updateTodo` | `PUT /api/todos/{id}` |
| `POST /api/deleteTodo` | `DELETE /api/todos/{id}` |

### 5. Hardcoded Connection String
`private string _connectionString = "Data Source=todos.db"` was hardcoded in `TodoService`. This makes it impossible to point to a different database in tests or production without changing source code.

### 6. Crash on NULL Description
`GetAllTodos` and `GetTodoById` called `reader.GetString(2)` for the `Description` column even though the schema defines it as nullable (`TEXT`). This throws a `System.InvalidCastException` for any row where `Description` is `NULL`.

### 7. Column Access by Magic Index Numbers
`reader.GetInt32(0)`, `reader.GetString(1)` etc. break silently if column order changes, and are unreadable.

### 8. No Input Validation
`CreateTodo` and `UpdateTodo` had no `[Required]` or length checks, so an empty title was accepted.

### 9. Request/Response Models in Controller File
`UpdateTodoRequest`, `GetTodoRequest`, and `DeleteTodoRequest` were all defined at the bottom of `TodoController.cs`. These belong in their own files in the `Models` folder.

### 10. Poor Tests
- Tests used `new TodoService()` which hit the real `todos.db` file — tests shared state and broke each other depending on execution order.
- `TestGetTodo` asserted `todos.Count > 0`, which only passed if previous tests had already written rows.
- `TestEverything` tested multiple unrelated things in one method.
- `Test1()` just asserted `true`.
- Controller tests tried `new TodoController()` which no longer compiles after adding a required constructor parameter.

### 11. Database Initialization Logic in Program.cs
`InitializeDatabase()` was a free function in `Program.cs` that duplicated the schema definition from the service layer. If the schema ever changed, it would need to be updated in two places.

---

## Architectural Decisions

### Dependency Injection via `ITodoService`
Introduced `ITodoService` and registered it with the DI container as `Scoped`. The controller now receives the service through constructor injection. This decouples the controller from any specific implementation and makes the controller testable with a mock.

### Parameterized Queries
Replaced all string-interpolated SQL with named parameters (`$title`, `$id`, etc.) using `command.Parameters.AddWithValue(...)`. This eliminates the SQL injection risk.

### Connection String via `IConfiguration`
`TodoService` now accepts `IConfiguration` through its constructor and reads the connection string from `ConnectionStrings:DefaultConnection`. The default falls back to `todos.db` so nothing breaks if the key is absent.

### Database Initialization in the Service Constructor
Moved `CREATE TABLE IF NOT EXISTS` into the `TodoService` constructor. The table is guaranteed to exist before any method is called, and there is no duplication between `Program.cs` and the service.

### RESTful Routes
Changed the controller route to `api/todos` and mapped each action to the appropriate HTTP verb and URL pattern. `GET /api/todos/{id}` is used as the `CreatedAtAction` location for `POST` responses.

### Nullable Description Handling
The `Todo` model now declares `Description` as `string?`. `MapRow` checks `reader.IsDBNull(...)` before calling `reader.GetString(...)`. Columns are accessed by name via `reader.GetOrdinal(...)` instead of by position.

### Request Models in Own Files
`CreateTodoRequest` and `UpdateTodoRequest` live in `TodoApi/Models/` and carry `[Required]` and `[MinLength(1)]` attributes so ASP.NET model validation rejects bad input before it reaches the service layer.

### Test Isolation
Each `TodoServiceTests` instance creates a unique temp file database (`todo_test_{guid}.db`) and deletes it in `Dispose()`. Tests never share state regardless of execution order or parallelism.

### Controller Unit Tests with Moq
`TodoControllerTests` uses a `Mock<ITodoService>` to test every HTTP response path (200, 201, 204, 404) without touching the database.

---

## Trade-offs

- **Raw ADO.NET kept over ORM**: An ORM like EF Core would reduce boilerplate and eliminate the manual SQL, but swapping the data access layer is out of scope for this exercise. The parameterized query approach is safe and explicit.
- **`Scoped` lifetime for the service**: Scoped means one instance per HTTP request, which is appropriate for a service that holds a connection string but opens/closes connections per method call. `Singleton` would also work here but `Scoped` is the safer default.
- **No integration tests**: The service tests cover the data layer against a real SQLite file. Full HTTP-level integration tests (using `WebApplicationFactory`) would add more coverage but are left as a future improvement.

---

## How to Run

### Prerequisites
- .NET 8.0 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project TodoApi
```

The API starts at `http://localhost:5164`. Open `http://localhost:5164/swagger` for the interactive API docs.

### Test
```bash
dotnet test
```

---

## API Documentation

### Base URL
`http://localhost:5164/api/todos`

---

#### Create a TODO

```
POST /api/todos
Content-Type: application/json

{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false
}
```

Response `201 Created`:
```json
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

#### Get all TODOs

```
GET /api/todos
```

Response `200 OK`:
```json
[
  {
    "id": 1,
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "isCompleted": false,
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

#### Get a TODO by id

```
GET /api/todos/1
```

Response `200 OK`:
```json
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

Response `404 Not Found` when the id does not exist.

---

#### Update a TODO

```
PUT /api/todos/1
Content-Type: application/json

{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread, butter",
  "isCompleted": true
}
```

Response `200 OK` with the updated todo. Response `404 Not Found` when the id does not exist.

---

#### Delete a TODO

```
DELETE /api/todos/1
```

Response `204 No Content` on success. Response `404 Not Found` when the id does not exist.

---

## Future Improvements

- **EF Core migration**: Replace the raw ADO.NET code with EF Core. This removes manual SQL, adds migration support, and makes future schema changes safer.
- **Pagination**: `GET /api/todos` will return every row indefinitely. A `?page=1&size=20` parameter would prevent memory issues at scale.
- **Filtering / search**: Query params like `?isCompleted=true` or `?search=grocery` would make the list endpoint much more useful.
- **Integration tests**: Use `WebApplicationFactory<Program>` with a dedicated test database to cover the full HTTP pipeline including serialization, routing, and validation.
- **Logging**: Add structured logging (Serilog or the built-in `ILogger<T>`) to controller and service methods so failures are observable in production.
- **Error handling middleware**: Instead of returning raw exception messages from `catch` blocks, add a global exception handler that returns a consistent problem-details response and never leaks stack traces to the caller.
- **Authentication**: Right now the API is open to anyone. Bearer token / API-key auth would be a prerequisite before any production deployment.
