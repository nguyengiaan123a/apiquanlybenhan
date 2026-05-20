using Microsoft.AspNetCore.Identity;

namespace yhctapp.Model.Enitity
{
    public class Approle :IdentityRole
    {
        public Approle()
        {
            RolePermissions = new List<RolePermission>();
        }
        public List<RolePermission> RolePermissions { get; set; }
    }
}
