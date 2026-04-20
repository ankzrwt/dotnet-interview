using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _serviceMock;
    private readonly TodoController _controller;

    public TodoControllerTests()
    {
        _serviceMock = new Mock<ITodoService>();
        _controller = new TodoController(_serviceMock.Object);
    }

    [Fact]
    public void CreateTodo_Returns201WithCreatedTodo()
    {
        var request = new CreateTodoRequest { Title = "test todo", Description = "some desc" };
        var returned = new Todo { Id = 1, Title = "test todo", Description = "some desc" };
        _serviceMock.Setup(s => s.CreateTodo(request)).Returns(returned);

        var response = _controller.CreateTodo(request);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(returned, created.Value);
    }

    [Fact]
    public void GetAll_ReturnsListOfTodos()
    {
        var todos = new List<Todo>
        {
            new Todo { Id = 1, Title = "first" },
            new Todo { Id = 2, Title = "second" }
        };
        _serviceMock.Setup(s => s.GetAllTodos()).Returns(todos);

        var response = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(todos, ok.Value);
    }

    [Fact]
    public void GetAll_ReturnsEmptyListWhenNoTodos()
    {
        _serviceMock.Setup(s => s.GetAllTodos()).Returns(new List<Todo>());

        var response = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(response);
        var list = Assert.IsType<List<Todo>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public void GetById_ReturnsTodoWhenFound()
    {
        var todo = new Todo { Id = 5, Title = "my todo" };
        _serviceMock.Setup(s => s.GetTodoById(5)).Returns(todo);

        var response = _controller.GetById(5);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(todo, ok.Value);
    }

    [Fact]
    public void GetById_Returns404WhenMissing()
    {
        _serviceMock.Setup(s => s.GetTodoById(99)).Returns((Todo?)null);

        var response = _controller.GetById(99);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public void Update_ReturnsUpdatedTodo()
    {
        var request = new UpdateTodoRequest { Title = "updated title", IsCompleted = true };
        var updated = new Todo { Id = 3, Title = "updated title", IsCompleted = true };
        _serviceMock.Setup(s => s.UpdateTodo(3, request)).Returns(updated);

        var response = _controller.Update(3, request);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public void Update_Returns404IfTodoGone()
    {
        var request = new UpdateTodoRequest { Title = "nope" };
        _serviceMock.Setup(s => s.UpdateTodo(99, request)).Returns((Todo?)null);

        var response = _controller.Update(99, request);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public void Delete_Returns204OnSuccess()
    {
        _serviceMock.Setup(s => s.DeleteTodo(1)).Returns(true);

        var response = _controller.Delete(1);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public void Delete_Returns404WhenNotFound()
    {
        _serviceMock.Setup(s => s.DeleteTodo(99)).Returns(false);

        var response = _controller.Delete(99);

        Assert.IsType<NotFoundResult>(response);
    }
}
