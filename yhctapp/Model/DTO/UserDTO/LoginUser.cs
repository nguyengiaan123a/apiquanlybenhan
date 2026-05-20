using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO.UserDTO
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Số diện thoại không được để trống")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password không được để trống")]

        public string Password { get; set; } = null!;
    }
}
