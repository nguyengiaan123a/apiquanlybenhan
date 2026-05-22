using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Model.DTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentRecordController : ControllerBase
    {
        private readonly IDocumentRecordRepository _repo;
        private readonly IMapper _mapper;

        public DocumentRecordController(IDocumentRecordRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // ===== Helper: Lấy thông tin phòng ban và quyền từ JWT =====
        private string? GetDepartmentId() =>
            User.Claims.FirstOrDefault(c => c.Type == "IdDepartmentRoom")?.Value;

        private bool IsAdmin() =>
            User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));

        // ===== GET: api/DocumentRecord?page=1&pageSize=15&search=... =====
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1 , [FromQuery] int pageSize = 15, [FromQuery] string? search = null,
        [FromQuery] string? Id_DocumentGroup = null , [FromQuery] string ? Id_DepartmentRoom = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;
            var departmentId = GetDepartmentId();
            var isAdmin = IsAdmin();
            var (totalPages, data) = await _repo.GetAll(page, pageSize, departmentId, isAdmin, search , Id_DocumentGroup,Id_DepartmentRoom);
            return Ok(new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = data
            });
        }

        // ===== GET: api/DocumentRecord/{id} =====
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var departmentId = GetDepartmentId();
            var isAdmin = IsAdmin();

            var result = await _repo.GetById(id, departmentId, isAdmin);
            if (result == null)
                return NotFound(new { Message = "Không tìm thấy hồ sơ" });

            return Ok(result);
        }

        // ===== POST: api/DocumentRecord =====
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DocumentRecordVM vm)
        {
            try
            {
                var entity = _mapper.Map<DocumentRecord>(vm);

                // Tự gán phòng ban từ JWT claim
                var departmentId = GetDepartmentId();
                if (!IsAdmin() || string.IsNullOrEmpty(vm.Id_DepartmentRoom))
                {
                    entity.Id_DepartmentRoom = departmentId ?? "";
                }

                var result = await _repo.Add(entity);
                if (result.Code == 1)
                    return Ok(new { Message = result.Message, Data = result });

                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Thêm mới thất bại", Error = ex.Message });
            }
        }

        // ===== PUT: api/DocumentRecord/{id} =====
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DocumentRecordVM vm)
        {
            try
            {
                var departmentId = GetDepartmentId();
                var isAdmin = IsAdmin();

                var entity = _mapper.Map<DocumentRecord>(vm);

                var result = await _repo.Update(id, entity, departmentId, isAdmin);
                if (result.Code == 1)
                    return Ok(new { Message = result.Message });

                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Cập nhật thất bại", Error = ex.Message });
            }
        }

        // ===== DELETE: api/DocumentRecord/{id} =====
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var departmentId = GetDepartmentId();
                var isAdmin = IsAdmin();

                var result = await _repo.Delete(id, departmentId, isAdmin);
                if (result.Code == 1)
                    return Ok(new { Message = result.Message });

                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Xóa thất bại", Error = ex.Message });
            }
        }
    }
}
