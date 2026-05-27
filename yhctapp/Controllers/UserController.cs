using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Services.Interface;
using System.Security.Claims;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
     [ApiController]
    // [Authorize]
    // [CheckQuyen("/quan-ly-tai-khoan")] // Thêm middleware kiểm tra quyền truy cập
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }





        [HttpPost]
        [Route("/api/dang-ky-tai-khoan")]
        public async Task<IActionResult> RegisterAdmin(UserVM user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                // Đã sửa lỗi gọi nhầm hàm RegisterCustomer ở đây
                var register = await _user.RegisterAdmin(user);

                if (register.Code == 200)
                {
                    return Ok(new { success = true, message = register.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = register.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("/api/danh-sach-tai-khoan")]
        // [Authorize(Roles = "ADMIN")] // Mở comment nếu cần phân quyền
        public async Task<IActionResult> GetListUsers([FromQuery] int page = 1, [FromQuery] int pagesize = 10, [FromQuery] string search = "")
        {
            try
            {
                var result = await _user.ListuserVM(page, pagesize, search);

                return Ok(new
                {
                    success = true,
                    totalPage = result.totalpage,
                    data = result.listuser
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("/api/cap-nhat-tai-khoan/{id}")]
        // [Authorize] 
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserVM user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                var updateResult = await _user.UpdateUser(id, user);

                if (updateResult.Code == 200)
                {
                    return Ok(new { success = true, message = updateResult.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = updateResult.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("/api/xoa-tai-khoan/{id}")]
        // [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var deleteResult = await _user.DeleteUser(id);

                // Check theo logic status.Code = 1 (Thành công) của bạn trong hàm DeleteUser
                if (deleteResult.Code == 1)
                {
                    return Ok(new { success = true, message = deleteResult.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = deleteResult.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("/api/cap-nhat-thong-tin")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Không xác định được người dùng." });
                }

                var updateResult = await _user.UpdateProfile(userId, model);

                if (updateResult.Code == 200)
                {
                    return Ok(new { success = true, message = updateResult.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = updateResult.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("/api/doi-mat-khau")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Không xác định được người dùng." });
                }

                var result = await _user.ChangePassword(userId, model);

                if (result.Code == 200)
                {
                    return Ok(new { success = true, message = result.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}