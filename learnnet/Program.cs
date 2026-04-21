using learnnet.Data;
using learnnet.Middleware;
using learnnet.Repositories;
using learnnet.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Tự động kiểm tra và tạo bảng (Migrations) + Seed dữ liệu khi khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<learnnet.Data.AppDbContext>();
        
        // 1. Chạy Migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        // 2. Seed dữ liệu nếu bảng Products trống
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new learnnet.Entities.Product
                {
                    Name = "iPhone 15 Pro",
                    Price = 999.99m,
                    Stock = 50,
                    CreatedAt = DateTime.UtcNow,
                    ProductDetail = new learnnet.Entities.ProductDetail
                    {
                        Description = "Chiếc iPhone mạnh mẽ nhất với khung Titan.",
                        Manufacturer = "Apple",
                        WarrantyPeriodMonths = 12
                    }
                },
                new learnnet.Entities.Product
                {
                    Name = "Samsung Galaxy S24 Ultra",
                    Price = 1199.99m,
                    Stock = 30,
                    CreatedAt = DateTime.UtcNow,
                    ProductDetail = new learnnet.Entities.ProductDetail
                    {
                        Description = "Điện thoại AI đỉnh cao với bút S-Pen.",
                        Manufacturer = "Samsung",
                        WarrantyPeriodMonths = 12
                    }
                }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Một lỗi đã xảy ra trong quá trình khởi tạo dữ liệu.");
    }
}

// Use custom exception middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
