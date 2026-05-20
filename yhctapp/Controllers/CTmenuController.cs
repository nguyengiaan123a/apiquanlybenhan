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
    [ApiController]
    [Authorize]
    [CheckQuyen("/quan-ly-danh-muc-menu")]

    public class CTmenuController : BaseCrudController<Catogerymenu, CategorymenuVM>
    {
        public CTmenuController(IGenericRepository<Catogerymenu> repo, IMapper mapper) : base(repo, mapper)
        {

        }
        [HttpGet("/api/danh-sach-danh-muc-menu")]
        public async Task<IActionResult> GetAll(int page, int pagesize, string? search = null)
        {
            // Tận dụng _repo và _mapper đã được khai báo protected ở thằng cha
            var (totalPages, data) = await _repo.GetAll(
                page,
                pagesize,
                // Search theo cột Title của bảng Menu
                filter: string.IsNullOrEmpty(search) ? null : x => x.Title.Contains(search),
                // Có thể sắp xếp dữ liệu mới nhất lên đầu (OrderByDescending)
                orderBy: q => q.OrderBy(x => x.Order)
            );

            // Map list Entity ra list DTO
            var mappedData = _mapper.Map<IReadOnlyList<CategorymenuVM>>(data);

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
