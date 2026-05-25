using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using yhctapp.Data;
using yhctapp.Model.DTO;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly MyDbcontext _dbcontext;

        public DashboardController(MyDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        private string? GetDepartmentId() =>
            User.Claims.FirstOrDefault(c => c.Type == "IdDepartmentRoom")?.Value;

        private bool IsAdmin() =>
            User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var departmentId = GetDepartmentId();
                var isAdmin = IsAdmin();
                var currentYear = DateTime.Now.Year;

                var query = _dbcontext.DepartmentRooms
                    .AsNoTracking()
                    .Where(dept => dept.Status == 1) // Only active departments
                    .AsQueryable();

                // If not admin, filter by user's department
                if (!isAdmin && !string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(dept => dept.Id == departmentId);
                }

                var stats = await query
                    .Select(dept => new DashboardStatVM
                    {
                        Id = dept.Id,
                        Room = dept.Room,
                        ExpiredCount = dept.DocumentRecords.Count(rec => rec.NamHieuLuc > 0 && rec.ThoiHanLuuTru < 999 && rec.NamHieuLuc + rec.ThoiHanLuuTru < currentYear),
                        SoonToExpireCount = dept.DocumentRecords.Count(rec => rec.NamHieuLuc > 0 && rec.ThoiHanLuuTru < 999 && rec.NamHieuLuc + rec.ThoiHanLuuTru == currentYear),
                        ValidCount = dept.DocumentRecords.Count(rec => rec.NamHieuLuc > 0 && (rec.ThoiHanLuuTru >= 999 || rec.NamHieuLuc + rec.ThoiHanLuuTru > currentYear)),
                        TotalCount = dept.DocumentRecords.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    IsAdmin = isAdmin,
                    UserDepartmentId = departmentId,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi tải dữ liệu thống kê", Error = ex.Message });
            }
        }
    }
}
