using System.ComponentModel.DataAnnotations;

namespace learnnet.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Vai trò của người dùng (Admin, Member,...)
        public string Role { get; set; } = "Member";

        // Navigation Property: Danh sách các công việc được giao cho người dùng này
        // Giúp Entity Framework thực hiện các phép JOIN dễ dàng hơn.
        public List<TaskItem> AssignedTasks { get; set; } = new();
    }
}
