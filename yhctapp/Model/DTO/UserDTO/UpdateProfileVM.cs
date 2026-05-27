using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO.UserDTO
{
    public class UpdateProfileVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
    }
}
