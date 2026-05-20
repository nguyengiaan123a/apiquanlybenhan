using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yhctapp.Model.DTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Controllers
{
    // trang quản lý phân quyền 
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [CheckQuyen("/quan-ly-role")]

    public class PermissonController : BaseCrudController<RolePermission, RolePermissonVM>
    {
        public PermissonController(IGenericRepository<RolePermission> repo, IMapper mapper) : base(repo, mapper)
        {
        }
    }
}
