using Microsoft.EntityFrameworkCore;
using learnnet.Entities;

namespace learnnet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* 
             * TỐI ƯU HIỆU NĂNG (DATABASE TUNING):
             * Việc đánh Index giúp Database tìm kiếm nhanh hơn, giảm tải cho CPU của DB Server.
             */

            // Index đơn cho các trường thường xuyên lọc/tìm kiếm
            modelBuilder.Entity<TaskItem>().HasIndex(t => t.Status);
            modelBuilder.Entity<TaskItem>().HasIndex(t => t.Deadline);
            modelBuilder.Entity<TaskItem>().HasIndex(t => t.AssignedToUserId);

            /* 
             * [ADVANCED] COMPOSITE INDEX (Index kết hợp):
             * Dùng khi bạn thường xuyên thực hiện truy vấn lọc theo cả Status VÀ Priority cùng lúc.
             * Ví dụ: Lấy tất cả Task đang "In Progress" mà có mức độ "Urgent".
             */
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => new { t.Status, t.Priority });

            /* 
             * [ADVANCED] PARTIAL INDEX (Index bộ phận):
             * Chỉ đánh Index cho các Task CHƯA hoàn thành (Status != Done).
             * Vì trong thực tế, chúng ta thường truy vấn các task đang làm việc nhiều hơn task đã xong.
             * Việc này giúp Index nhỏ hơn, tiết kiệm bộ nhớ và ghi dữ liệu nhanh hơn.
             */
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.Status)
                .HasFilter("\"Status\" != 3"); // 3 là giá trị Enum của 'Done'

            // Cấu hình quan hệ (Relationships)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu xóa User, Task vẫn tồn tại nhưng để trống người nhận
        }
    }
}
