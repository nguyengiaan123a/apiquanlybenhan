using Microsoft.AspNetCore.Identity;

namespace yhctapp.Model.Enitity
{
    public class ApplicationUser : IdentityUser
    {
      
        public string Fullname { get; set; }

        public string IdDepartmentRoom { get; set;}

        public  DepartmentRoom DepartmentRoom { get; set;}


    }
}
