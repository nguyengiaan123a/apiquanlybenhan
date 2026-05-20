using yhctapp.Model.DTO.AuthorizationDTO;

namespace yhctapp.Services.Interface
{
    public interface IAuthorization
    {
        public Task<List<AuthorizationVM>> GetAuthorizationByUserId(string userId);

    }
}
