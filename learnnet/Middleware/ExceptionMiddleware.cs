using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace learnnet.Middleware
{
    /* 
     * GLOBAL EXCEPTION HANDLING (Xử lý lỗi toàn cục):
     * Đây là một Middleware "chốt chặn cuối cùng" để bắt mọi lỗi xảy ra trong ứng dụng.
     * Nó giúp hệ thống luôn trả về định dạng JSON thống nhất khi có sự cố.
     */
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp tới các Layer xử lý tiếp theo (Controller, Service)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Khi có bất kỳ lỗi nào xảy ra ở bất kỳ đâu, nó sẽ "bắn" về đây để xử lý tập trung
                _logger.LogError(ex, "Một lỗi không mong muốn đã xảy ra: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // Phân loại lỗi để trả về StatusCode HTTP phù hợp (Đúng chuẩn RESTful)
            var statusCode = exception switch
            {
                // Lỗi tranh chấp dữ liệu (409 Conflict)
                DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
                // Lỗi quyền truy cập (401 Unauthorized)
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                // Lỗi tìm kiếm (404 Not Found)
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                // Các lỗi hệ thống khác (500 Internal Server Error)
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                // Trong môi trường Development thì hiện cả chi tiết lỗi (StackTrace) để Dev dễ sửa
                Detail = _env.IsDevelopment() ? exception.StackTrace?.ToString() : "Vui lòng liên hệ quản trị viên."
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
