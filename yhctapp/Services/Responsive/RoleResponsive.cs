using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.DTO;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface.Role;

namespace yhctapp.Services.Responsive
{
    public class RoleResponsive : IRole
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MyDbcontext _context;

        public RoleResponsive(RoleManager<IdentityRole> roleManager, MyDbcontext context)
        {
            _roleManager = roleManager;
            _context = context;


        }
        public async Task<Status> Add(RoleVM entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity.name))
                {
                    return new Status { Code = 0, Message = "Tên quyền không được để trống!" };
                }
                bool roleExists = await _roleManager.RoleExistsAsync(entity.name);
                if (roleExists)
                {
                    return new Status { Code = 0, Message = "Quyền này đã tồn tại trong hệ thống!" };
                }
                var newRole = new IdentityRole(entity.name);
                var result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    return new Status { Code = 1, Message = "Thêm Role thành công!" };
                }
                else
                {
                    // Lấy tất cả các lỗi từ Identity (ví dụ: tên chứa ký tự không hợp lệ) để báo cho người dùng
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new Status { Code = 0, Message = $"Lỗi từ hệ thống: {errors}" };
                }

            }
            catch (Exception ex)
            {
                // Ghi log lỗi (Nên dùng ILogger ở đây trong thực tế)
                return new Status { Code = -1, Message = $"Lỗi Exception: {ex.Message}" };

            }

        }

        public async Task<Status> Deleterole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return new Status { Code = 0, Message = "Không tìm thấy quyền này!" };
                }

                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return new Status { Code = 1, Message = "Xóa quyền thành công!" };
                }

                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new Status { Code = 0, Message = $"Lỗi khi xóa: {errors}" };
            }
            catch (Exception ex)
            {
                // Lưu ý: Có thể dính Exception nếu Role này đang được gán cho User (ràng buộc khóa ngoại)
                return new Status { Code = -1, Message = $"Lỗi Exception: {ex.Message}" };
            }
        }

        public async Task<(int totalpages, IReadOnlyList<RoleVM>)> GetAll(int page, int pagesize, string search)
        {
            // Bắt đầu với IQueryable để tối ưu câu query dưới DB
            var query = _roleManager.Roles.AsQueryable();

            // 1. Áp dụng bộ lọc tìm kiếm (nếu có)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name.Contains(search));
            }

            // 2. Tính tổng số dòng và tổng số trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pagesize);

            // 3. Phân trang và map dữ liệu (Chỉ lấy đúng số dòng cần thiết)
            var roles = await query
                .OrderBy(r => r.Name) // Nên order trước khi phân trang để dữ liệu ổn định
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Select(r => new RoleVM
                {
                    Id = r.Id,
                    name = r.Name
                })
                .ToListAsync();

            return (totalPages, roles);
        }

        public async Task<RoleVM> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            // Map dữ liệu từ IdentityRole sang ViewModel của bạn
            return new RoleVM
            {
                Id = role.Id,
                name = role.Name
            };
        }
        public async Task<Status> Updaterole(RoleVM entity, string id)
        {
            try
            {
                // Tìm Role trong DB
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return new Status { Code = 0, Message = "Không tìm thấy quyền này để cập nhật!" };
                }

                // Cập nhật thông tin
                role.Name = entity.name;
                // Có thể cập nhật thêm NormalizedName hoặc các trường khác nếu custom

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return new Status { Code = 1, Message = "Cập nhật thành công!" };
                }

                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new Status { Code = 0, Message = $"Lỗi cập nhật: {errors}" };
            }
            catch (Exception ex)
            {
                return new Status { Code = -1, Message = $"Lỗi Exception: {ex.Message}" };
            }
        }

        public async Task<IReadOnlyList<MenuVM>> GetMenusByRole(string id)
        {
            var roleExists = await _roleManager.Roles.AnyAsync(r => r.Id == id);
            if (!roleExists)
            {
                return new List<MenuVM>();
            }

            var menus = await _context.Set<RolePermission>()
                .AsNoTracking()
                .Where(x => x.RoleId == id)
                .Join(
                    _context.Menus.AsNoTracking(),
                    permission => permission.Id_menu,
                    menu => menu.Id,
                    (permission, menu) => menu)
                .OrderBy(x => x.Order)
                .Select(x => new MenuVM
                {
                    Id = x.Id,
                    Title = x.Title,
                    url = x.url,
                    Thumnail = x.Thumnail,
                    Order = x.Order,
                    Status = x.Status
                })
                .ToListAsync();

            return menus;
        }
    }
}
