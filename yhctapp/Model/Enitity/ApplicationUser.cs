using Microsoft.AspNetCore.Identity;

namespace yhctapp.Model.Enitity
{
    public class ApplicationUser : IdentityUser
    {
      
        public string Fullname { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }


    }
}
