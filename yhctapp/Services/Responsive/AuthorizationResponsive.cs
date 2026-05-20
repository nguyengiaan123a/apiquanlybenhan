using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using yhctapp.Model.DTO.AuthorizationDTO;
using yhctapp.Services.Interface;
using yhctapp.Data;

public class AuthorizationResponsive : IAuthorization
{
    private readonly MyDbcontext _context;

    public AuthorizationResponsive(MyDbcontext context)
    {
        _context = context;
    }

    public async Task<List<AuthorizationVM>> GetAuthorizationByUserId(string userId)
    {
        try
        {
            // BƯỚC 1: Lấy dữ liệu phẳng (flat data) từ DB
            // Tạo ra một object ẩn danh (Anonymous type) để hứng dữ liệu tạm
            var query = from ur in _context.UserRoles.AsNoTracking()
                        where ur.UserId == userId

                        join rp in _context.RolePermissions.AsNoTracking()
                             on ur.RoleId equals rp.RoleId

                        join m in _context.Menus.AsNoTracking().OrderBy(x=>x.Order)
                             on rp.Id_menu equals m.Id

                        join c in _context.Catogerymenus.AsNoTracking().OrderBy(x => x.Order)
                             on m.Id_menu equals c.Id

                        // Status == 1 theo như code hiện tại của bạn
                        where m.Status == 1 && c.Status == 1 

                        select new
                        {
                            CategoryTitle = c.Title,
                            CategoryThumbnail = c.Thumnail,
                            Order_ct = c.Order,
                            MenuTitle = m.Title,
                            MenuUrl = m.url,
                            Methumnail=m.Thumnail,
                            Order_menu= m.Order,
                        };

            // Dùng Distinct() để loại bỏ các quyền trùng lặp và tải dữ liệu về RAM
            var flatList = await query.Distinct().OrderBy(x=>x.Order_ct).ToListAsync();

            // BƯỚC 2: Gom nhóm (GroupBy) dữ liệu để tạo thành cấu trúc Cha - Con
            var result = flatList
                // Gom nhóm theo Tiêu đề và Thumbnail của Danh mục
                .GroupBy(x => new { x.CategoryTitle, x.CategoryThumbnail })
                .Select(g => new AuthorizationVM
                {
                    Title_ctmenu = g.Key.CategoryTitle,
                    thumnail = g.Key.CategoryThumbnail,

                    // Lặp qua các item trong nhóm để tạo List<MenuAuthoVM>
                    Menus = g.Select(m => new MenuAuthoVM
                    {
                        Title_menu = m.MenuTitle,
                        url = m.MenuUrl,
                        thumnail=m.Methumnail,
                        Order_menu = m.Order_menu,

                    }).ToList()
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy dữ liệu phân quyền: {ex.Message}");
        }
    }
}