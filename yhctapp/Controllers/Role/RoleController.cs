using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yhctapp.Model.DTO;
using yhctapp.Services.Interface.Role;

namespace yhctapp.Controllers.Role
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckQuyen("/quan-ly-role")]
    public class RoleController : ControllerBase
    {
        private readonly IRole _role;

        public RoleController(IRole role)
        {
            _role = role;
        }

        // 1. Lấy danh sách Role (Phân trang + Tìm kiếm)
        [HttpGet]
        public async Task<IActionResult> GetAll(int page, int pageSize, string search = "")
        {
            var (totalPages, roles) = await _role.GetAll(page, pageSize, search);

            return Ok(new
            {
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = roles
            });
        }

        // 2. Lấy chi tiết một Role theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _role.GetById(id);
            if (result == null)
            {
                return NotFound(new { Message = "Không tìm thấy quyền này!" });
            }
            return Ok(result);
        }

        // 3. Thêm mới một Role
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _role.Add(model);
            if (result.Code == 1)
            {
                return Ok(result); // Trả về 200 kèm thông báo thành công
            }
            return BadRequest(result); // Trả về 400 kèm thông báo lỗi
        }

        // 4. Cập nhật Role theo ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RoleVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _role.Updaterole(model, id);
            if (result.Code == 1)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // 5. Xóa Role theo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _role.Deleterole(id);
            if (result.Code == 1)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // 6. Lấy danh sách Menu được gán cho Role này
        [HttpGet("{id}/menus")]
        public async Task<IActionResult> GetMenus(string id)
        {
            var menus = await _role.GetMenusByRole(id);
            if (menus == null)
            {
                return NotFound(new { Message = "Quyền không tồn tại hoặc không có menu." });
            }
            return Ok(menus);
        }
    }
}