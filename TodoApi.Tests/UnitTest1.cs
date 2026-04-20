using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodoServiceTests : IDisposable
{
    private readonly ITodoService _service;
    private readonly string _dbPath;

    public TodoServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"todo_test_{Guid.NewGuid()}.db");
        _service = new TodoService($"Data Source={_dbPath};Pooling=False");
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public void CreateTodo_ShouldReturnTodoWithId()
    {
        var request = new CreateTodoRequest { Title = "fix login bug", Description = "users cant log in on mobile" };

        var result = _service.CreateTodo(request);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("fix login bug", result.Title);
        Assert.Equal("users cant log in on mobile", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public void CreateTodo_CreatedAtShouldBeSetToNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = _service.CreateTodo(new CreateTodoRequest { Title = "some task" });

        Assert.True(result.CreatedAt >= before);
        Assert.True(result.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreateTodo_DescriptionIsOptional()
    {
        var result = _service.CreateTodo(new CreateTodoRequest { Title = "quick task" });

        Assert.NotNull(result);
        Assert.Null(result.Description);
    }

    [Fact]
    public void GetAllTodos_EmptyWhenNothingAdded()
    {
        var result = _service.GetAllTodos();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllTodos_ReturnsEverything()
    {
        _service.CreateTodo(new CreateTodoRequest { Title = "task one" });
        _service.CreateTodo(new CreateTodoRequest { Title = "task two" });
        _service.CreateTodo(new CreateTodoRequest { Title = "task three" });

        var result = _service.GetAllTodos();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetTodoById_ShouldFindExistingTodo()
    {
        var created = _service.CreateTodo(new CreateTodoRequest { Title = "write unit tests" });

        var result = _service.GetTodoById(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("write unit tests", result.Title);
    }

    [Fact]
    public void GetTodoById_ReturnsNullForMissingId()
    {
        var result = _service.GetTodoById(99999);

        Assert.Null(result);
    }

    [Fact]
    public void UpdateTodo_TitleGetsUpdated()
    {
        var created = _service.CreateTodo(new CreateTodoRequest { Title = "old name" });

        var result = _service.UpdateTodo(created.Id, new UpdateTodoRequest { Title = "new name" });

        Assert.NotNull(result);
        Assert.Equal("new name", result.Title);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public void UpdateTodo_CanBeMarkedComplete()
    {
        var created = _service.CreateTodo(new CreateTodoRequest { Title = "do laundry" });

        var result = _service.UpdateTodo(created.Id, new UpdateTodoRequest { Title = "do laundry", IsCompleted = true });

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public void UpdateTodo_ShouldReturnNullIfNotFound()
    {
        var result = _service.UpdateTodo(99999, new UpdateTodoRequest { Title = "whatever" });

        Assert.Null(result);
    }

    [Fact]
    public void UpdateTodo_ChangesShouldPersist()
    {
        var created = _service.CreateTodo(new CreateTodoRequest { Title = "before" });
        _service.UpdateTodo(created.Id, new UpdateTodoRequest { Title = "after", IsCompleted = true });

        var fetched = _service.GetTodoById(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("after", fetched.Title);
        Assert.True(fetched.IsCompleted);
    }

    [Fact]
    public void DeleteTodo_ShouldRemoveIt()
    {
        var created = _service.CreateTodo(new CreateTodoRequest { Title = "temp todo" });

        var deleted = _service.DeleteTodo(created.Id);

        Assert.True(deleted);
        Assert.Null(_service.GetTodoById(created.Id));
    }

    [Fact]
    public void DeleteTodo_ReturnsFalseWhenNotFound()
    {
        var result = _service.DeleteTodo(99999);

        Assert.False(result);
    }

    [Fact]
    public void DeleteTodo_OtherTodosNotAffected()
    {
        var keep = _service.CreateTodo(new CreateTodoRequest { Title = "keep this" });
        var remove = _service.CreateTodo(new CreateTodoRequest { Title = "remove this" });

        _service.DeleteTodo(remove.Id);

        Assert.NotNull(_service.GetTodoById(keep.Id));
    }
}
