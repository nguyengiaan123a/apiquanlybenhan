using yhctapp.Helpper;
using yhctapp.Model.DTO;

namespace yhctapp.Services.Interface.Role
{
    public interface IRolePermisson
    {
        public Task<List<RolePermissonVM>> RolePermisson();
        public Task<Status> AddRolePermisson(List<RolePermissonVM> rolePermissonVMs);
        public Task<Status> UpdateRolePermisson(List<int> id ,List<RolePermissonVM> rolePermissonVM);
        public Task<Status> DeleteRolePermisson(List<int> id);
    }
}
