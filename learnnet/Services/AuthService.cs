using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using learnnet.Data;
using learnnet.DTOs;
using learnnet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace learnnet.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto request);
        Task<AuthResponseDto> Login(LoginDto request);
    }

    /* 
     * AuthService: Quản lý đăng ký và đăng nhập.
     * Đây là layer bảo mật quan trọng nhất của ứng dụng.
     */
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new AuthResponseDto { Success = false, Message = "Username đã tồn tại." };
            }

            // MÃ HÓA MẬT KHẨU: Tuyệt đối không lưu mật khẩu dạng text thô.
            // BCrypt tự động thêm Salt để chống lại các cuộc tấn công Rainbow Table.
            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Role = "Member"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto { Success = true, Message = "Đăng ký thành công." };
        }

        public async Task<AuthResponseDto> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // KIỂM TRA MẬT KHẨU: So sánh hash trong DB với mật khẩu người dùng nhập vào.
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponseDto { Success = false, Message = "Sai username hoặc mật khẩu." };
            }

            // Nếu đúng mật khẩu, tạo JWT Token để trả về cho người dùng
            string token = CreateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Đăng nhập thành công.",
                Token = token,
                Username = user.Username
            };
        }

        /* 
         * TẠO JWT TOKEN:
         * Token này chứa thông tin định danh người dùng (Claims) và được ký bằng Secret Key.
         */
        private string CreateToken(User user)
        {
            // Claims: Các thông tin "tuyên bố" về người dùng được nhúng vào Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Lấy Secret Key từ appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            // Ký Token bằng thuật toán HmacSha512
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1), // Token có hiệu lực trong 1 ngày
                    signingCredentials: creds
                );

            // Xuất Token ra chuỗi String để gửi cho Client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
