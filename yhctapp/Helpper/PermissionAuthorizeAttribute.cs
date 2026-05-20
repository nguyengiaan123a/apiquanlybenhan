using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using yhctapp.Data; // Đổi lại đúng namespace chứa MyDbcontext của bạn

public class CheckQuyenAttribute : TypeFilterAttribute
{
    // Yêu cầu truyền vào đường dẫn URL (ví dụ: "/quan-ly-menu")
    public CheckQuyenAttribute(string moduleUrl) : base(typeof(CheckQuyenFilter))
    {
        Arguments = new object[] { moduleUrl };
    }
}

public class CheckQuyenFilter : IAsyncAuthorizationFilter
{
    private readonly MyDbcontext _context;
    private readonly string _moduleUrl;

    public CheckQuyenFilter(MyDbcontext context, string moduleUrl)
    {
        _context = context;
        _moduleUrl = moduleUrl;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Kiểm tra xem user đã có Token chưa (Đã đăng nhập chưa)
        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult(); // Bắn lỗi 401
            return;
        }

        // 2. Lấy UserId từ Token
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        // 3. LOGIC ĐỘNG LÀ Ở ĐÂY: Truy vấn xuống Database xem User có quyền không
        var hasPermission = await (from ur in _context.UserRoles
                                   where ur.UserId == userId

                                   // Nối với bảng phân quyền (RolePermissions)
                                   join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId

                                   // Nối với bảng Menu để lấy thông tin URL
                                   join m in _context.Menus on rp.Id_menu equals m.Id

                                   // So sánh xem URL chức năng của API này có nằm trong danh sách quyền của User không
                                   where m.url == _moduleUrl && m.Status == 1
                                   select m.Id).AnyAsync();

        // 4. Nếu DB không có bản ghi nào khớp -> Chặn cửa
        if (!hasPermission)
        {
            context.Result = new ForbidResult(); // Bắn lỗi 403 (Không đủ quyền)
            return;
        }

        // Nếu qua được xuống đây tức là có quyền, API sẽ tiếp tục chạy bình thường!
    }
}