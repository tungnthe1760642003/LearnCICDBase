using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using learnnet.Enums;

namespace learnnet.Entities
{
    /* 
     * TaskItem: Thực thể đại diện cho một công việc trong hệ thống.
     */
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Trạng thái công việc (Todo, InProgress, Review, Done)
        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;

        // Mức độ ưu tiên (Low, Medium, High, Urgent)
        public Enums.TaskPriority Priority { get; set; } = Enums.TaskPriority.Medium;

        // Thời hạn hoàn thành công việc
        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Mối quan hệ: Một công việc được giao cho một người dùng nhất định
        public int? AssignedToUserId { get; set; }
        
        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; }

        /* 
         * [CONCURRENCY CONTROL]
         * ConcurrencyToken là "chìa khóa" để thực hiện Optimistic Locking.
         * Thuộc tính này sẽ được Entity Framework kiểm tra mỗi lần Update để tránh tình trạng 
         * hai người cùng sửa một bản ghi dẫn đến mất mát dữ liệu (Lost Update).
         */
        [ConcurrencyCheck]
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
    }
}
