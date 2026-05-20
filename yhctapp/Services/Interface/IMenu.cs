using yhctapp.Model.DTO.UserDTO;

namespace yhctapp.Services.Interface
{
    public interface IMenu
    {
        public Task<(int totalPages, List<MenuListVM> data)> GetAllPagingAsync(
            int page,
            int pagesize,
            string search);
    }
}
