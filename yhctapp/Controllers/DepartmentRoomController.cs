using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Model.DTO;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    // [ApiController]
    // [Authorize]
    // [CheckQuyen("/quan-ly-danh-muc-menu")]

    public class DepartmentRoomControlller : ControllerBase
    {
        private readonly IDepartmentRoomRepository _repo;
        private readonly IMapper _mapper;

        public DepartmentRoomControlller(IDepartmentRoomRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (totalPages, data) = await _repo.GetAll(page, pageSize);
            var dataVM = _mapper.Map<IReadOnlyList<DepartmentRoomVM>>(data);

            return Ok(new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = dataVM
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return NotFound(new { Message = "Không tìm thấy dữ liệu" });

            var result = _mapper.Map<DepartmentRoomVM>(entity);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DepartmentRoomVM entityVM)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = _mapper.Map<DepartmentRoom>(entityVM);
            var result = await _repo.Add(entity);

            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DepartmentRoomVM entityVM)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingEntity = await _repo.GetById(id);
            if (existingEntity == null) return NotFound(new { Message = "Không tìm thấy dữ liệu để cập nhật" });

            _mapper.Map(entityVM, existingEntity);

            var result = await _repo.Update(existingEntity);
            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _repo.Delete(id);
            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }
    }
}
