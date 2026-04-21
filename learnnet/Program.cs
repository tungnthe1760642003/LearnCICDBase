using learnnet.Data;
using learnnet.Middleware;
using learnnet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database (PostgreSQL)
// Sử dụng Connection String từ file appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Services (Dependency Injection)
// AddScoped: Một thực thể service được tạo ra cho mỗi request HTTP
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Đăng ký FluentValidation để tự động kiểm tra tính hợp lệ của DTOs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<learnnet.Validators.TaskCreateDtoValidator>();

builder.Services.AddControllers();

// Cấu hình Swagger: Công cụ dùng để test và xem tài liệu API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Cấu hình JWT Authentication (Xác thực người dùng)
// Đọc Key bí mật từ cấu hình để mã hóa và giải mã Token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

/* 
 * 4. Tự động Migrations & Seed dữ liệu mẫu (Sẵn sàng cho Production)
 * Khi ứng dụng khởi động, nó sẽ tự quét xem Database đã cũ chưa để cập nhật.
 */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Thực hiện Migrate tự động (không cần gõ lệnh dotnet ef database update tay)
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        // Tạo tài khoản Admin mặc định nếu DB đang trống
        if (!context.Users.Any())
        {
            var adminUser = new learnnet.Entities.User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Mã hóa mật khẩu chuẩn BCrypt
                FullName = "System Administrator",
                Role = "Admin"
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Tạo Task mẫu ban đầu để bạn dễ theo dõi
            if (!context.Tasks.Any())
            {
                context.Tasks.Add(new learnnet.Entities.TaskItem
                {
                    Title = "Hoàn thành hệ thống Task Management",
                    Description = "Xây dựng đầy đủ tính năng JWT và Concurrency Control.",
                    Status = learnnet.Enums.TaskStatus.InProgress,
                    Priority = learnnet.Enums.TaskPriority.Urgent,
                    Deadline = DateTime.UtcNow.AddDays(7),
                    AssignedToUserId = adminUser.Id,
                    ConcurrencyToken = Guid.NewGuid()
                });
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo Database hoặc Seed dữ liệu.");
    }
}

// 5. Cấu hình Middleware Pipeline (Thứ tự rất quan trọng)

// Middleware xử lý lỗi toàn cục: Phải đặt ở đầu để bắt được mọi lỗi từ các layer sau
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // Xác thực danh tính (Bạn là ai?)
app.UseAuthorization();  // Kiểm tra quyền hạn (Bạn được làm gì?)

app.MapControllers(); // Ánh xạ các Request tới các Controller xử lý tương ứng

app.Run();
