using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Helpper;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Dùng abstract để ngăn không cho ai gọi API trực tiếp vào cái Base này
    public abstract class BaseCrudController<TEntity, TVM> : ControllerBase
        where TEntity : class
        where TVM : class
    {
        protected readonly IGenericRepository<TEntity> _repo;
        protected readonly IMapper _mapper;

        public BaseCrudController(IGenericRepository<TEntity> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        [HttpGet]
        public virtual async Task<IActionResult> GetAll([FromQuery] int page, [FromQuery] int pageSize )
        {
            // Tránh trường hợp client truyền số âm hoặc = 0 gây lỗi
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Gọi repo lấy dữ liệu (Tạm thời ở Base không dùng filter, orderBy hay include)
            // Nếu bảng con nào cần include/filter, sẽ override lại hàm này.
            var (totalPages, data) = await _repo.GetAll(page, pageSize);

            // Map list Entity ra list VM
            var dataVM = _mapper.Map<IReadOnlyList<TVM>>(data);

            // Trả về JSON chứa cả data và thông tin phân trang

            return Ok(new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = dataVM
            });
        }

        // Dùng từ khóa 'virtual' để nếu bảng nào có logic dị biệt, thằng con có thể ghi đè (override)
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return NotFound(new { Message = "Không tìm thấy dữ liệu" });

            // Map Entity ra VM để giấu bớt cột nhạy cảm nếu có
            var result = _mapper.Map<TVM>(entity);
            return Ok(result);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Add([FromBody] TVM entityVM)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = _mapper.Map<TEntity>(entityVM);
            var result = await _repo.Add(entity);

            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(int id, [FromBody] TVM entityVM)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Lấy data cũ từ DB lên
            var existingEntity = await _repo.GetById(id);
            if (existingEntity == null) return NotFound(new { Message = "Không tìm thấy dữ liệu để cập nhật" });

            // 2. Map dữ liệu mới từ VM đè vào data cũ
            _mapper.Map(entityVM, existingEntity);

            // 3. Update
            var result = await _repo.Update(existingEntity);
            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.Delete(id);
            if (result.Code == 1) return Ok(result);
            return BadRequest(result);
        }

    }
}