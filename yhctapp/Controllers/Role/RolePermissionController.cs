using Microsoft.AspNetCore.Mvc;
using yhctapp.Model.DTO;
using yhctapp.Services.Interface.Role;

namespace yhctapp.Controllers.Role
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermisson _permissonResponsive;

        public RolePermissionController(IRolePermisson permissonResponsive)
        {
            _permissonResponsive = permissonResponsive;
        }

        // =====================================================
        // GET ALL
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _permissonResponsive.RolePermisson();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Code = -1,
                    Message = ex.Message
                });
            }
        }

        // =====================================================
        // ADD
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> AddRolePermission(
            [FromBody] List<RolePermissonVM> rolePermissonVMs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _permissonResponsive
                .AddRolePermisson(rolePermissonVMs);

            if (result.Code != 1)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [HttpPut]
        public async Task<IActionResult> UpdateRolePermission(
            [FromQuery] List<int> ids,
            [FromBody] List<RolePermissonVM> rolePermissonVMs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _permissonResponsive
                .UpdateRolePermisson(ids, rolePermissonVMs);

            if (result.Code != 1)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpDelete]
        public async Task<IActionResult> DeleteRolePermission(
            [FromQuery] List<int> ids)
        {
            var result = await _permissonResponsive
                .DeleteRolePermisson(ids);

            if (result.Code != 1)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}