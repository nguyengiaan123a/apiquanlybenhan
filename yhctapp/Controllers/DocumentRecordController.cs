using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
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
        private readonly IImageService _imageService;

        public DocumentRecordController(IDocumentRecordRepository repo, IMapper mapper, IImageService imageService)
        {
            _repo = repo;
            _mapper = mapper;
            _imageService = imageService;
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

        // ===== GET: api/DocumentRecord/{id}/files =====
        [HttpGet("{id}/files")]
        public async Task<IActionResult> GetFiles(int id)
        {
            var files = await _repo.GetFilesByRecordId(id);
            return Ok(files);
        }

        // ===== POST: api/DocumentRecord/{id}/files =====
        [HttpPost("{id}/files")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFiles(int id, [FromForm] List<IFormFile> files)
        {
            var departmentId = GetDepartmentId();
            var isAdmin = IsAdmin();
            var record = await _repo.GetById(id, departmentId, isAdmin);
            if (record == null)
            {
                return NotFound(new { Message = "Không tìm thấy hồ sơ hoặc bạn không có quyền truy cập" });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { Message = "Không có file nào được chọn để tải lên" });
            }

            var statuses = new List<string>();
            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var uniqueFileName = await _imageService.UploadImageAsync(file);
                if (uniqueFileName == null) continue;

                var documentFile = new DocumentFile
                {
                    FileName = file.FileName,
                    FilePath = uniqueFileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    Id_DocumentRecord = id,
                    CreatedDate = DateTime.Now
                };

                var result = await _repo.AddFile(documentFile);
                if (result.Code == 1)
                {
                    statuses.Add($"Thành công: {file.FileName}");
                }
                else
                {
                    statuses.Add($"Thất bại: {file.FileName} - {result.Message}");
                }
            }

            return Ok(new { Message = "Hoàn thành tải lên file", Details = statuses });
        }

        // ===== DELETE: api/DocumentRecord/files/{fileId} =====
        [HttpDelete("files/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var file = await _repo.GetFileById(fileId);
            if (file == null)
            {
                return NotFound(new { Message = "Không tìm thấy file" });
            }

            var departmentId = GetDepartmentId();
            var isAdmin = IsAdmin();
            var record = await _repo.GetById(file.Id_DocumentRecord, departmentId, isAdmin);
            if (record == null)
            {
                return BadRequest(new { Message = "Bạn không có quyền xóa file của hồ sơ này" });
            }

            _imageService.DeleteImage(file.FilePath);

            var result = await _repo.DeleteFile(fileId);
            if (result.Code == 1)
            {
                return Ok(new { Message = result.Message });
            }

            return BadRequest(new { Message = result.Message });
        }
    }
}
