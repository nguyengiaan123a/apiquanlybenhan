using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Services.Responsive
{
    public class MenuResponsive : GenericRepository<Menu>, IMenu
    {
        private readonly IMapper _mapping;

        // BẮT BUỘC phải có ": base(dbcontext)" ở đuôi
        public MenuResponsive(MyDbcontext dbcontext, IMapper mapping) : base(dbcontext)
        {
            _mapping = mapping;
        }

        public async Task<(int totalPages, List<MenuListVM> data)> GetAllPagingAsync(int page, int pagesize, string search)
        {
            // Dùng AsNoTracking để tối ưu tốc độ đọc (vì ta chỉ lấy lên xem, không update)
            var query = _dbcontext.Catogerymenus.AsNoTracking().AsQueryable();

            // 1. Lọc dữ liệu (Search)
            if (!string.IsNullOrEmpty(search))
            {
                // Giả sử Entity gốc có cột Title
                query = query.Where(x => x.Title.Contains(search));
            }

            // 2. Tính toán phân trang
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pagesize);

            // 3. TỐI ƯU HÓA: Dùng ProjectTo (Không cần Include)
            var data = await query
                .OrderBy(x=>x.Order) // Sắp xếp trước khi cắt trang
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                // Cú pháp thần thánh này sẽ tự động JOIN bảng Menus và lấy dữ liệu
                .Select(x => new MenuListVM
                {
                    Title_Ctmenu = x.Title,
                    // Lấy luôn danh sách Menu con (nếu có)
                    Menus = x.Menus.Select(m => new MenuVM
                    {
                        Id = m.Id,
                        Title = m.Title,
                        url = m.url,
                        Order = m.Order,
                        Status=m.Status,
                        
                    }).OrderBy(x=>x.Order).ToList()
                })
                .ToListAsync();

            return (totalPages, data);
        }
    }
}
