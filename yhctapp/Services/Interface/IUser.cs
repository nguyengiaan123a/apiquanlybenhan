using yhctapp.Helpper;
using yhctapp.Model.DTO.UserDTO;

namespace yhctapp.Services.Interface
{
    public interface IUser
    {
        public Task<Status> RegisterCustomer(UserVM user);

        public Task<Status> Login(LoginUser user);

        public Task<Status> RegisterAdmin(UserVM user);


        public Task<(int totalpage, List<ListuserVM> listuser)> ListuserVM(int page,int pagesize,string search);

        public Task<Status> DeleteUser(string id);


        public Task<Status> UpdateUser(string id, UserVM user);

        public Task<Status> ChangePassword(string id, ChangePasswordVM model);

        public Task<Status> UpdateProfile(string id, UpdateProfileVM model);

    }
}
