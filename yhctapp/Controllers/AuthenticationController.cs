using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticatonController : ControllerBase
    {
        private readonly IUser _user;

        public AuthenticatonController(IUser user)
        {
            _user = user;
        }




        [Authorize]
        [HttpGet("/api/user")]
        public IActionResult GetUser()
        {
            var username = User.Identity?.Name;
            var fullname = User.Claims.FirstOrDefault(c => c.Type == "Fullname")?.Value;
            var phone = User.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;
            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                username,
                fullname,
                phone,
                roles
            });
        }
        [HttpPost]
        [Route("/api/dang-nhap")]
        public async Task<IActionResult> Login(LoginUser user)
        {
            try
            {
             

                var login = await _user.Login(user);

                if (login.Code == 200)
                {
                    // Set JWT as HttpOnly cookie
                    Response.Cookies.Append("jwt_token", login.Message, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Set to true in production (requires HTTPS)
                        SameSite = SameSiteMode.None, // Or Lax, depending on your needs
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    return Ok(new { success = true, message = login.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = login.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("/api/dang-xuat")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Kiểm tra xem cookie có tồn tại không trước khi xóa (tùy chọn nhưng nên có)
                if (Request.Cookies["jwt_token"] != null)
                {
                    // Xóa cookie bằng cách set ngày hết hạn về quá khứ
                    Response.Cookies.Append("jwt_token", "", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Phải giống hệt lúc tạo
                        SameSite = SameSiteMode.None, // Phải giống hệt lúc tạo
                        Expires = DateTimeOffset.UtcNow.AddDays(-1) // Set thời gian về ngày hôm qua
                    });

                    // Hoặc dùng hàm Delete có sẵn của ASP.NET Core (Khuyên dùng)
                    // Response.Cookies.Delete("jwt_token", new CookieOptions
                    // {
                    //     HttpOnly = true,
                    //     Secure = true,
                    //     SameSite = SameSiteMode.None
                    // });
                }

                return Ok(new { success = true, message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi đăng xuất: " + ex.Message });
            }
        }
        [HttpPost]
        [Route("/api/dang-ky-khach-hang")]
        public async Task<IActionResult> Register(UserVM user)
        {
            try
            {
                var register = await _user.RegisterCustomer(user);

                if (register.Code == 200)
                {
                    return Ok(new { success = true, message = register.Message });
                }
                else
                {
                    // Lỗi logic từ Service (ví dụ: trùng username)
                    return BadRequest(new { success = false, message = register.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



    }
}