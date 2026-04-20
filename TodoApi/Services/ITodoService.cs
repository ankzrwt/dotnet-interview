using TodoApi.Models;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Todo CreateTodo(CreateTodoRequest request);
        List<Todo> GetAllTodos();
        Todo? GetTodoById(int id);
        Todo? UpdateTodo(int id, UpdateTodoRequest request);
        bool DeleteTodo(int id);
    }
}
