using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    // [CheckQuyen("/quan-ly-tai-khoan")] // Thêm middleware kiểm tra quyền truy cập
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("/api/test")]
        public IActionResult Test() => Ok("Test thành công 3");
    }
}