# Generative AI Tools Exercise

## Preferred Tool

**Claude Code**

---

# Prompt

```text
Generate a production-ready RESTful API using ASP.NET Core 8 Web API and Entity Framework Core.

Requirements:

- Follow Clean Architecture principles where reasonable.
- Create CRUD endpoints for Task management.
- Assume a User entity already exists.
- Each Task should contain:
    - Id (Guid)
    - Title (required, max 200 chars)
    - Description (optional)
    - Status (Todo, InProgress, Completed)
    - DueDate
    - UserId (Guid)

Generate:

- Entity
- DbContext configuration
- DTOs
- Controller
- Service layer
- Repository pattern (optional)
- FluentValidation validations
- Proper HTTP status codes
- Swagger support
- Global exception handling
- Async methods
- CancellationToken support

The API should expose:

GET /api/tasks
GET /api/tasks/{id}
POST /api/tasks
PUT /api/tasks/{id}
DELETE /api/tasks/{id}

Return clean, readable code following Microsoft best practices.
```

---

# Representative Output

## Entity

```csharp
public class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public Guid UserId { get; set; }
}
```

## Controller

```csharp
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Get(Guid id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var task = await _service.CreateAsync(dto);

        return CreatedAtAction(nameof(Get),
            new { id = task.Id },
            task);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto)
    {
        await _service.UpdateAsync(id, dto);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);

        return NoContent();
    }
}
```

## Validation

```csharp
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow);

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
```

---

# How I Validated the AI Suggestions

I do not accept AI-generated code without reviewing it.

My validation process included:

- Reviewing the generated architecture to ensure it followed ASP.NET Core best practices.
- Building the solution to detect compilation issues.
- Running the API and testing every endpoint using Swagger and Postman.
- Verifying the returned HTTP status codes (`200`, `201`, `204`, `400`, `404`).
- Reviewing dependency injection registrations.
- Ensuring asynchronous methods were used consistently.
- Confirming that the generated API followed REST conventions.
- Running unit tests for the service layer where applicable.

---

# Improvements Made to the Generated Code

The generated code compiled and looked clean, but reviewing it surfaced real issues:

- The generated enum was named `TaskStatus`, which collides with
  `System.Threading.Tasks.TaskStatus` from the BCL. This is visible in the
  Representative Output above. It compiles until the first file that has
  `using System.Threading.Tasks` in scope, then produces ambiguity errors.
  I renamed it to `TaskItemStatus`.
- The enum was serialized as an integer in JSON responses. I added
  `JsonStringEnumConverter` so clients receive `"InProgress"` instead of `1`.
- The `DueDate > DateTime.UtcNow` validation rule was applied to updates as well
  as creation, which meant an overdue task could not be edited at all, not even
  to mark it completed. I restricted the rule to the create path.
- There was no ownership check anywhere: any authenticated user could update or
  delete any other user's task. I added the rule to the service layer (compare
  the task's `UserId` against the id from the JWT claims) so it holds for any
  caller, not just this controller.
- The list query tracked every returned entity. I added `AsNoTracking()` since
  it is a read-only projection.
- The `CreateTaskDto` exposed a client-writable `UserId` (also visible in the
  validator, which validates it). Combined with the missing ownership check,
  this let a client create tasks as any user. I removed it from the contract;
  the owner is always the authenticated caller.

---

# Authentication

Since the exercise assumes a `User` model already exists, I would secure the API using JWT Bearer Authentication.

Rather than trusting the `UserId` supplied by the client, the authenticated user's identifier should be retrieved from the JWT claims.

Example:

```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

This prevents users from accessing or modifying tasks that belong to other users.

---

# Edge Cases Considered

The API was designed to handle several common scenarios:

- Missing required title.
- Due date in the past.
- Invalid GUID values.
- Updating a task that does not exist.
- Deleting an already deleted task.
- Unauthorized access to another user's tasks.
- Empty request bodies.
- Invalid enum values for task status.
- Malformed JSON payloads.

---

# Why I Use Generative AI

I use Generative AI as a productivity tool rather than a replacement for software engineering expertise.

It helps accelerate repetitive tasks such as scaffolding projects, generating boilerplate code, and suggesting implementation patterns. However, I always review the generated code for correctness, security, maintainability, performance, and alignment with the project's coding standards before considering it production-ready.