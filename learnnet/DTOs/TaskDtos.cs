using System.ComponentModel.DataAnnotations;
using learnnet.Enums;

namespace learnnet.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;

        public Enums.TaskPriority Priority { get; set; } = Enums.TaskPriority.Medium;

        public DateTime? Deadline { get; set; }

        public int? AssignedToUserId { get; set; }
    }

    public class TaskUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Enums.TaskStatus Status { get; set; }

        public Enums.TaskPriority Priority { get; set; }

        public DateTime? Deadline { get; set; }

        public int? AssignedToUserId { get; set; }

        [Required]
        public Guid ConcurrencyToken { get; set; }
    }

    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToFullName { get; set; }
        public Guid ConcurrencyToken { get; set; }
    }
}
