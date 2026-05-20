using yhctapp.Helpper;
using yhctapp.Model.DTO;
using yhctapp.Model.DTO.UserDTO;

namespace yhctapp.Services.Interface.Role
{
    public interface IRole

    {
        public Task<Status> Updaterole(RoleVM entity, string id);

        public Task<Status> Deleterole(string id);

        public Task<Status> Add(RoleVM entity);

        public Task<(int totalpages, IReadOnlyList<RoleVM>)> GetAll(int page, int pagesize, string search);

        public Task<RoleVM> GetById(string id);

        public Task<IReadOnlyList<MenuVM>> GetMenusByRole(string id);
    }
}
