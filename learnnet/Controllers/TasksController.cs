using learnnet.DTOs;
using learnnet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace learnnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /* 
     * TasksController: Cổng giao tiếp RESTful API cho các tính năng quản lý công việc.
     * [Authorize]: Đảm bảo mọi Endpoint trong này đều yêu cầu Token hợp lệ.
     * [ApiController]: Tự động hóa việc kiểm tra Validation của DTOs.
     */
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /* 
         * TRUY VẤN (READ): Lấy danh sách toàn bộ công việc.
         * Thường được dùng cho màn hình Dashboard tổng quát.
         */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            var tasks = await _taskService.GetAllTasks();
            return Ok(tasks);
        }

        // Lấy chi tiết một công việc theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _taskService.GetTaskById(id);
            if (task == null) return NotFound(new { Message = "Công việc không tồn tại." });
            return Ok(task);
        }

        /* 
         * TẠO MỚI (CREATE): Thêm một công việc vào hệ thống.
         * Dữ liệu được truyền trong Body của Request dưới dạng JSON.
         */
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto request)
        {
            var task = await _taskService.CreateTask(request);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /* 
         * CẬP NHẬT (UPDATE): Sửa đổi thông tin công việc hiện có.
         * [PUT] yêu cầu gửi toàn bộ đối tượng cập nhật lên.
         */
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, TaskUpdateDto request)
        {
            try
            {
                var task = await _taskService.UpdateTask(id, request);
                if (task == null) return NotFound(new { Message = "Công việc không tồn tại." });
                return Ok(task);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        // Xóa một công việc
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTask(id);
            if (!result) return NotFound(new { Message = "Công việc không tồn tại." });
            return NoContent();
        }

        /* 
         * NHẬN VIỆC (CLAIM): Sử dụng Pessimistic Locking.
         * Đây là tính năng đặc biệt để chứng minh kỹ năng xử lý hệ thống tải cao (High Concurrency).
         */
        [HttpPost("{id}/claim")]
        public async Task<IActionResult> ClaimTask(int id)
        {
            /* 
             * Lấy UserId từ JWT Token của người dùng hiện tại (lấy từ Claims).
             * NameIdentifier thường chứa ID của User được mã hóa trong Token.
             */
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // Gọi Service để thực hiện nhận việc với cơ chế khóa hàng DB
            var result = await _taskService.ClaimTaskPessimistic(id, userId);

            if (!result)
            {
                return BadRequest(new { Message = "Không thể nhận việc. Có thể công việc đã được người khác nhận hoặc không tồn tại." });
            }

            return Ok(new { Message = "Bạn đã nhận việc thành công!" });
        }
    }
}
