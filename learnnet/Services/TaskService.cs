using learnnet.Data;
using learnnet.DTOs;
using learnnet.Entities;
using Microsoft.EntityFrameworkCore;

namespace learnnet.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasks();
        Task<TaskResponseDto?> GetTaskById(int id);
        Task<TaskResponseDto> CreateTask(TaskCreateDto request);
        Task<TaskResponseDto?> UpdateTask(int id, TaskUpdateDto request);
        Task<bool> DeleteTask(int id);
        Task<bool> ClaimTaskPessimistic(int id, int userId);
    }

    /* 
     * TaskService: Nơi chứa logic nghiệp vụ cốt lõi của hệ thống.
     * Code tuân thủ nguyên tắc Dependency Injection (DI) để dễ dàng Unit Test.
     */
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(AppDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Truy vấn tối ưu với AsNoTracking() cho các trường hợp chỉ đọc (Read-only)
        public async Task<IEnumerable<TaskResponseDto>> GetAllTasks()
        {
            _logger.LogInformation("Truy vấn toàn bộ danh sách công việc.");
            return await _context.Tasks
                .AsNoTracking() 
                .Include(t => t.AssignedToUser)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TaskResponseDto?> GetTaskById(int id)
        {
            _logger.LogInformation("Tìm kiếm Task ID: {Id}", id);
            var task = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                _logger.LogWarning("Không tìm thấy Task ID: {Id}", id);
            }

            return task == null ? null : MapToDto(task);
        }

        public async Task<TaskResponseDto> CreateTask(TaskCreateDto request)
        {
            _logger.LogInformation("Bắt đầu tạo công việc mới: {Title}", request.Title);
            
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                Deadline = request.Deadline,
                AssignedToUserId = request.AssignedToUserId,
                // Khởi tạo token cho Optimistic Concurrency
                ConcurrencyToken = Guid.NewGuid()
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tạo thành công Task ID: {Id}", task.Id);
            return MapToDto(task);
        }

        /*
         * Xử lý Cập nhật với cơ chế Optimistic Concurrency (Khóa lạc quan) 
         * và Database Transaction để đảm bảo an toàn dữ liệu.
         */
        public async Task<TaskResponseDto?> UpdateTask(int id, TaskUpdateDto request)
        {
            _logger.LogInformation("Cập nhật Task ID: {Id}", id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null) return null;

                /* 
                 * KIỂM TRA TRANH CHẤP: Nếu Token từ Client gửi lên không khớp với Token trong DB,
                 * nghĩa là đã có người khác sửa dữ liệu này trước đó.
                 */
                if (task.ConcurrencyToken != request.ConcurrencyToken)
                {
                    _logger.LogWarning("Phát hiện tranh chấp dữ liệu (Concurrency) tại Task ID: {Id}", id);
                    throw new DbUpdateConcurrencyException("Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại.");
                }

                task.Title = request.Title;
                task.Description = request.Description;
                task.Status = request.Status;
                task.Priority = request.Priority;
                task.Deadline = request.Deadline;
                task.AssignedToUserId = request.AssignedToUserId;
                
                // Refresh token cho lần sửa tiếp theo
                task.ConcurrencyToken = Guid.NewGuid(); 

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cập nhật thành công Task ID: {Id}", id);

                await _context.Entry(task).Reference(t => t.AssignedToUser).LoadAsync();
                return MapToDto(task);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi xảy ra khi cập nhật Task: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTask(int id)
        {
            _logger.LogInformation("Xóa Task ID: {Id}", id);
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        /*
         * PESSIMISTIC LOCKING (Khóa bi quan):
         * Sử dụng SELECT ... FOR UPDATE để khóa hàng dữ liệu ngay lập tức.
         * Phù hợp cho tính năng "Claim Task" nơi chỉ được phép 1 người duy nhất nhận việc.
         */
        public async Task<bool> ClaimTaskPessimistic(int id, int userId)
        {
            _logger.LogInformation("User {UserId} đang thử nhận quyền thực hiện Task {TaskId}", userId, id);
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Thực hiện câu lệnh SQL thuần để kích hoạt cơ chế khóa hàng của PostgreSQL
                var tasks = await _context.Tasks
                    .FromSqlRaw("SELECT * FROM \"Tasks\" WHERE \"Id\" = {0} FOR UPDATE", id)
                    .ToListAsync();
                
                var task = tasks.FirstOrDefault();

                if (task == null || task.AssignedToUserId != null)
                {
                    _logger.LogWarning("Task {Id} không tồn tại hoặc đã có người nhận.", id);
                    return false;
                }

                task.AssignedToUserId = userId;
                task.Status = Enums.TaskStatus.InProgress;
                task.ConcurrencyToken = Guid.NewGuid();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thực hiện Pessimistic Lock cho Task {Id}", id);
                throw;
            }
        }

        // Manual Mapping từ Entity sang DTO (Production thường dùng AutoMapper nhưng viết tay để bạn dễ hiểu logic)
        private static TaskResponseDto MapToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                Deadline = task.Deadline,
                CreatedAt = task.CreatedAt,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToFullName = task.AssignedToUser?.FullName,
                ConcurrencyToken = task.ConcurrencyToken
            };
        }
    }
}
