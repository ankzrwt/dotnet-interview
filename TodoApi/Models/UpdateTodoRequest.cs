using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class UpdateTodoRequest
    {
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}
