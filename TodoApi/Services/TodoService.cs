using Microsoft.Data.Sqlite;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly string _connectionString;

        public TodoService(IConfiguration configuration)
            : this(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todos.db")
        {
        }

        // used in tests
        public TodoService(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Todos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL
                )
            ";
            command.ExecuteNonQuery();
        }

        public Todo CreateTodo(CreateTodoRequest request)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createdAt = DateTime.UtcNow;

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
                VALUES ($title, $description, $isCompleted, $createdAt);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$title", request.Title);
            command.Parameters.AddWithValue("$description", (object?)request.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$isCompleted", request.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("$createdAt", createdAt.ToString("o"));

            var id = Convert.ToInt32(command.ExecuteScalar());

            return new Todo
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted,
                CreatedAt = createdAt
            };
        }

        public List<Todo> GetAllTodos()
        {
            var todos = new List<Todo>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                todos.Add(MapRow(reader));
            }

            return todos;
        }

        public Todo? GetTodoById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRow(reader);
            }

            return null;
        }

        public Todo? UpdateTodo(int id, UpdateTodoRequest request)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Todos
                SET Title = $title, Description = $description, IsCompleted = $isCompleted
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$title", request.Title);
            command.Parameters.AddWithValue("$description", (object?)request.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$isCompleted", request.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("$id", id);

            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
                return null;

            return GetTodoById(id);
        }

        public bool DeleteTodo(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        private static Todo MapRow(SqliteDataReader reader)
        {
            var descOrdinal = reader.GetOrdinal("Description");
            return new Todo
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(descOrdinal) ? null : reader.GetString(descOrdinal),
                IsCompleted = reader.GetInt32(reader.GetOrdinal("IsCompleted")) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
            };
        }
    }
}
