using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Migrations;
using yhctapp.Model.DTO.UserDTO;
// Nhớ using thêm namespace chứa class Menu (Entity) của ông nha, ví dụ:
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [CheckQuyen("/quan-ly-menu")]
    // 1. Thay ControllerBase thành BaseCrudController, 
    // truyền vô kiểu Entity là Menu, kiểu ViewModel là MenuVM
    public class MenuController : BaseCrudController<Menu, MenuVM>
    {
        // 2. Tạo Constructor: Nhận đồ từ DI rồi "tiến cống" lên cho cha bằng chữ base()

        private readonly IMenu _menu;
        public MenuController(IGenericRepository<Menu> repo, IMapper mapper, IMenu menu) : base(repo, mapper)
        {
            _menu = menu;

        }

        // 3. Chỉ cần viết duy nhất hàm GetAll ở đây (vì mỗi bảng search một kiểu khác nhau)
        // Các hàm Add, Update, Delete, GetById thằng cha đã bao xài hết rồi!
        [HttpGet("/api/danh-sach-menu")]
        public async Task<IActionResult> GetAll(int page, int pagesize, string? search = null)
        {
            // Tận dụng _repo và _mapper đã được khai báo protected ở thằng cha
            var (totalPages, data) = await _menu.GetAllPagingAsync(
                page,
                pagesize,
                search
            );


            // Map list Entity ra list DTO
            var mappedData = _mapper.Map<IReadOnlyList<MenuListVM>>(data);

            return Ok(new
            {
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pagesize,
                Data = mappedData
            });
        }
    }
}