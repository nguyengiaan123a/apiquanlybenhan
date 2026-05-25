using System.Reflection.Metadata;
using System.Security.Claims;
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

    public class DocumentgroupController : BaseCrudController<DocumentGroup, DocumentVM>
    {
        public DocumentgroupController(IGenericRepository<DocumentGroup> repo, IMapper mapper) : base(repo, mapper)
        {

        }
        public override async Task<IActionResult> Add([FromBody] DocumentVM entityVM)  // ✅ Đúng
        {
            try
            {
                var roomIdString = User.Claims.FirstOrDefault(c => c.Type == "IdDepartmentRoom")?.Value;
                entityVM.Id_DepartmentRoom = roomIdString; // Gán giá trị Id_DepartmentRoom từ claim
                var entity = _mapper.Map<DocumentGroup>(entityVM);
                var result = await _repo.Add(entity);
                if (result.Code == 1)
                {
                       return Ok(new { Message = result.Message, Data = result });
                }
                else
                {
                    return BadRequest(new { Message = result.Message, Data = result });
                }
             
            }catch (Exception ex)
            {
                return BadRequest(new { Message = "Thêm mới thất bại", Error = ex.Message });
            }


        }
    

    }
}
